using UnityEngine;
using Photon.Pun;

public class EnemyItemCarrier_MP : MonoBehaviourPunCallbacks
{
    [Header("Hold Point")]
    public Transform holdPoint;

    [Header("Carry Settings")]
    public Vector3 carryLocalPosition = Vector3.zero;
    public Vector3 carryLocalRotation = Vector3.zero;
    public bool forceCarryPositionEveryFrame = true;

    public GameObject CurrentItem { get; private set; }

    public bool HasItem
    {
        get { return CurrentItem != null; }
    }

    private void LateUpdate()
    {
        if (CurrentItem != null && forceCarryPositionEveryFrame)
        {
            KeepItemAttached();
        }
    }

    public void AttachItem(GameObject item)
    {
        if (item == null)
            return;

        if (PhotonNetwork.InRoom)
        {
            int viewID = GetPhotonViewID(item);

            if (viewID != -1 && photonView != null)
            {
                photonView.RPC(nameof(RPC_AttachItem), RpcTarget.All, viewID);
                return;
            }
        }

        AttachItemLocal(item);
    }

    public void TossCurrentItem(Vector3 force)
    {
        if (CurrentItem == null)
            return;

        GameObject itemToToss = CurrentItem;
        CurrentItem = null;

        if (PhotonNetwork.InRoom)
        {
            int viewID = GetPhotonViewID(itemToToss);

            if (viewID != -1 && photonView != null)
            {
                photonView.RPC(nameof(RPC_TossItem), RpcTarget.All, viewID, force);
                return;
            }
        }

        ReleaseItemLocal(itemToToss, force, true);
    }

    public void ReleaseItemWithoutForce(GameObject item)
    {
        if (item == null)
            return;

        ReleaseItemLocal(item, Vector3.zero, false);

        if (CurrentItem == item)
        {
            CurrentItem = null;
        }
    }

    public GameObject GetStealableRoot(GameObject item)
    {
        if (item == null)
            return null;

        TrashItem trashItem = item.GetComponent<TrashItem>();

        if (trashItem == null)
        {
            trashItem = item.GetComponentInParent<TrashItem>();
        }

        if (trashItem != null)
            return trashItem.gameObject;

        return item;
    }

    public bool IsValidStealableObject(GameObject item, LayerMask forbiddenStealLayers)
    {
        if (item == null)
            return false;

        TrashItem trashItem = item.GetComponent<TrashItem>();

        if (trashItem == null)
        {
            trashItem = item.GetComponentInParent<TrashItem>();
        }

        if (trashItem == null)
            return false;

        return !IsObjectOrParentInForbiddenLayer(trashItem.transform, forbiddenStealLayers);
    }

    private void AttachItemLocal(GameObject item)
    {
        if (item == null || holdPoint == null)
            return;

        CurrentItem = item;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        PrepareRigidbodyForCarry(rb);

        SetItemColliders(item, false);
        SetPhotonSyncComponents(item, false);

        item.transform.SetParent(holdPoint, false);
        item.transform.localPosition = carryLocalPosition;
        item.transform.localRotation = Quaternion.Euler(carryLocalRotation);

        KeepItemAttached();
    }

    private void KeepItemAttached()
    {
        if (CurrentItem == null || holdPoint == null)
            return;

        if (CurrentItem.transform.parent != holdPoint)
        {
            CurrentItem.transform.SetParent(holdPoint, false);
        }

        CurrentItem.transform.localPosition = carryLocalPosition;
        CurrentItem.transform.localRotation = Quaternion.Euler(carryLocalRotation);

        Rigidbody rb = CurrentItem.GetComponent<Rigidbody>();
        PrepareRigidbodyForCarry(rb);
    }

    private void ReleaseItemLocal(GameObject item, Vector3 force, bool applyForce)
    {
        if (item == null)
            return;

        item.transform.SetParent(null, true);

        SetPhotonSyncComponents(item, true);
        SetItemColliders(item, true);

        Rigidbody rb = item.GetComponent<Rigidbody>();
        PrepareRigidbodyForRelease(rb, force, applyForce);

        if (CurrentItem == item)
        {
            CurrentItem = null;
        }
    }

    private void PrepareRigidbodyForCarry(Rigidbody rb)
    {
        if (rb == null)
            return;

        /*
         * Important:
         * Do NOT set linearVelocity or angularVelocity while the Rigidbody is kinematic.
         * That causes this warning:
         * "Setting linear velocity of a kinematic body is not supported."
         */

        if (!rb.isKinematic)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.isKinematic = true;
    }

    private void PrepareRigidbodyForRelease(Rigidbody rb, Vector3 force, bool applyForce)
    {
        if (rb == null)
            return;

        /*
         * Important:
         * Set isKinematic to false first.
         * Then it is safe to change velocity and apply force.
         */

        rb.isKinematic = false;
        rb.useGravity = true;
        rb.freezeRotation = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;

        rb.WakeUp();

        if (applyForce)
        {
            rb.AddForce(force, ForceMode.Impulse);
        }
    }

    private void SetItemColliders(GameObject item, bool enabled)
    {
        if (item == null)
            return;

        Collider[] colliders = item.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = enabled;
        }
    }

    private bool IsObjectOrParentInForbiddenLayer(Transform checkTransform, LayerMask forbiddenStealLayers)
    {
        while (checkTransform != null)
        {
            bool isForbidden =
                (forbiddenStealLayers.value & (1 << checkTransform.gameObject.layer)) != 0;

            if (isForbidden)
                return true;

            checkTransform = checkTransform.parent;
        }

        return false;
    }

    private void SetPhotonSyncComponents(GameObject item, bool enabled)
    {
        if (item == null)
            return;

        PhotonTransformView photonTransformView = item.GetComponent<PhotonTransformView>();

        if (photonTransformView != null)
        {
            photonTransformView.enabled = enabled;
        }

        PhotonRigidbodyView photonRigidbodyView = item.GetComponent<PhotonRigidbodyView>();

        if (photonRigidbodyView != null)
        {
            photonRigidbodyView.enabled = enabled;
        }
    }

    private int GetPhotonViewID(GameObject obj)
    {
        if (obj == null)
            return -1;

        PhotonView view = obj.GetComponent<PhotonView>();

        if (view == null)
        {
            view = obj.GetComponentInParent<PhotonView>();
        }

        if (view == null)
            return -1;

        return view.ViewID;
    }

    [PunRPC]
    private void RPC_AttachItem(int itemViewID)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);

        if (itemView == null)
            return;

        AttachItemLocal(itemView.gameObject);
    }

    [PunRPC]
    private void RPC_TossItem(int itemViewID, Vector3 force)
    {
        PhotonView itemView = PhotonView.Find(itemViewID);

        if (itemView == null)
            return;

        bool shouldApplyForce = PhotonNetwork.IsMasterClient;

        ReleaseItemLocal(itemView.gameObject, force, shouldApplyForce);

        if (CurrentItem == itemView.gameObject)
        {
            CurrentItem = null;
        }
    }
}