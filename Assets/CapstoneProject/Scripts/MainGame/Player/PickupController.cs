using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using System.Collections.Generic;
#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

#if PHOTON_UNITY_NETWORKING
[RequireComponent(typeof(PhotonView))]
public class PickupController : MonoBehaviourPunCallbacks, IPunObservable
#else
public class PickupController : MonoBehaviour
#endif
{
    public static List<PickupController> ActivePickups = new List<PickupController>();

    [Header("Pickup Settings")]
    public float pickupRange = 5f;

    [Header("Hold Point")]
    public Transform holdPoint;

    [Header("Hold Distance")]
    public float holdDistance = 2f;
    public float minHoldDistance = 1.5f;
    public float maxHoldDistance = 8f;
    public float scrollSpeed = 4f;

    [Header("Movement")]
    public float moveSpeed = 20f;
    public float rotateSpeed = 15f;

    [Header("Hold Centering")]
    [Tooltip("Keeps the visible/collider center of the held object in front of the camera, instead of using the object's pivot.")]
    public bool centerHeldObjectByBounds = true;

    [Tooltip("Distance from walls/obstacles while holding an object.")]
    public float obstaclePadding = 0.3f;

    [Header("Held Rotation")]
    [Tooltip("Use this if some objects face the wrong way while held. Try Y = 90, -90, or 180.")]
    public Vector3 heldRotationOffset = Vector3.zero;

    [Header("Layer")]
    public LayerMask pickupLayer;

    [Header("UI")]
    public TextMeshProUGUI interactionText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip holdSound;
    public AudioClip dropSound;

    [Header("Input Actions")]
    public InputActionReference interactAction;
    public InputActionReference scrollAction;

    [Header("References")]
    public FPS_Controller playerController;

    private Rigidbody heldObject;
    private ObjectHighlight currentHighlight;
    private AudioSource holdAudioSource;

    private bool inputEnabledByThisController;
#if PHOTON_UNITY_NETWORKING
    PhotonView pv;
    private bool networkIsHoldingItem;
    private int networkHeldObjectViewID = -1;
#endif

    public bool IsHoldingItem
    {
        get
        {
#if PHOTON_UNITY_NETWORKING
            if (PhotonNetwork.InRoom && !IsLocalPlayer())
            {
                return networkIsHoldingItem;
            }
#endif
            return heldObject != null;
        }
    }

    public GameObject HeldObject
    {
        get
        {
            if (heldObject != null)
            {
                return heldObject.gameObject;
            }
#if PHOTON_UNITY_NETWORKING
            if (
                PhotonNetwork.InRoom &&
                networkHeldObjectViewID != -1
            )
            {
                PhotonView itemView =
                    PhotonView.Find(networkHeldObjectViewID);

                if (itemView != null)
                {
                    return itemView.gameObject;
                }
            }
#endif
            return null;
        }
    }

    public int HeldObjectViewID
    {
        get
        {
#if PHOTON_UNITY_NETWORKING
            if (heldObject != null)
            {
                PhotonView itemView =
                    heldObject.GetComponent<PhotonView>();

                if (itemView == null)
                {
                    itemView =
                        heldObject.GetComponentInParent<PhotonView>();
                }

                if (itemView != null)
                {
                    return itemView.ViewID;
                }
            }

            return networkHeldObjectViewID;
#else
            return -1;
#endif
        }
    }

    public override void OnEnable()
    {
        if (!ActivePickups.Contains(this))
        {
            ActivePickups.Add(this);
        }

        if (IsLocalPlayer())
        {
            EnableInputActions();
        }
    }

    public override void OnDisable()
    {
        ActivePickups.Remove(this);

        StopHoldingEffects();
        DisableInputActions();
    }

    void Start()
    {
        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
        
        holdAudioSource = gameObject.AddComponent<AudioSource>();
        holdAudioSource.loop = true;
        holdAudioSource.playOnAwake = false;
        holdAudioSource.clip = holdSound;
        pv = GetComponent<PhotonView>();
        if (holdPoint != null)
        {
            holdPoint.localPosition =
                new Vector3(0, 0, holdDistance);
        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            if (!IsLocalPlayer())
            {
                return;
            }

            if (playerController != null && playerController.shopOpen)
            {
                return;
            }

            HandleRaycast();

            if (heldObject != null)
            {
                MoveObject();
                HandleScroll();

                if (
                    interactAction != null &&
                    interactAction.action.triggered
                )
                {
                    DropObject();
                }
            }
            else
            {
                if (
                    interactAction != null &&
                    interactAction.action.triggered
                )
                {
                    TryPickup();
                }
            }
        }
    }

    void EnableInputActions()
    {
        if (inputEnabledByThisController)
            return;

        if (interactAction != null)
        {
            interactAction.action.Enable();
        }

        if (scrollAction != null)
        {
            scrollAction.action.Enable();
        }

        inputEnabledByThisController = true;
    }

    void DisableInputActions()
    {
        if (!inputEnabledByThisController)
            return;

        if (interactAction != null)
        {
            interactAction.action.Disable();
        }

        if (scrollAction != null)
        {
            scrollAction.action.Disable();
        }

        inputEnabledByThisController = false;
    }

    bool IsLocalPlayer()
    {
#if PHOTON_UNITY_NETWORKING
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                if (pv == null) { return true; }
                else { return pv.IsMine; }
            }
        }
#endif
        return true;
    }

    void HandleRaycast()
    {
        Ray ray =
            new Ray(transform.position, transform.forward);

        RaycastHit hit;

        if (currentHighlight != null)
        {
            currentHighlight.RemoveHighlight();
            currentHighlight = null;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }

        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            ObjectHighlight highlight =
                hit.collider.GetComponentInParent<ObjectHighlight>();

            TrashItem trash =
                hit.collider.GetComponentInParent<TrashItem>();

            Rigidbody rb =
                GetRigidbodyFromHit(hit);

            if (highlight != null && rb != heldObject)
            {
                currentHighlight = highlight;
                currentHighlight.Highlight();
            }

            if (trash != null && rb != heldObject)
            {
                if (interactionText != null)
                {
                    interactionText.gameObject.SetActive(true);

                    interactionText.text =
                        trash.itemName +
                        "\nValue: " +
                        trash.value +
                        "\n\nLeft Click to Pick Up";
                }
            }
        }
    }

    void TryPickup()
    {
        Ray ray =
            new Ray(transform.position, transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            Rigidbody rb =
                GetRigidbodyFromHit(hit);

            if (rb != null)
            {
                heldObject = rb;
#if PHOTON_UNITY_NETWORKING
                if (PhotonNetwork.InRoom)
                {
                    PhotonView itemView =
                        heldObject.GetComponent<PhotonView>();

                    if (itemView == null)
                    {
                        itemView =
                            heldObject.GetComponentInParent<PhotonView>();
                    }

                    if (itemView != null)
                    {
                        itemView.TransferOwnership(
                            PhotonNetwork.LocalPlayer
                        );

                        networkHeldObjectViewID =
                            itemView.ViewID;
                    }
                }
#endif
                holdDistance = Mathf.Clamp(
                    holdDistance,
                    minHoldDistance,
                    maxHoldDistance
                );

                if (holdPoint != null)
                {
                    holdPoint.localPosition =
                        new Vector3(0, 0, holdDistance);
                }

                ObjectHighlight highlight =
                    heldObject.GetComponentInParent<ObjectHighlight>();

                if (highlight != null)
                {
                    highlight.RemoveHighlight();
                }

                heldObject.isKinematic = false;
                heldObject.useGravity = false;
                heldObject.linearDamping = 10f;
                heldObject.angularDamping = 10f;
                heldObject.freezeRotation = true;
                heldObject.collisionDetectionMode =
                    CollisionDetectionMode.ContinuousDynamic;

                if (audioSource != null && pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

                if (
                    holdSound != null &&
                    holdAudioSource != null &&
                    !holdAudioSource.isPlaying
                )
                {
                    holdAudioSource.Play();
                }
            }
        }
    }

    void MoveObject()
    {
        if (heldObject == null)
            return;

        Vector3 desiredCenterPosition;

        if (holdPoint != null)
        {
            desiredCenterPosition = holdPoint.position;
        }
        else
        {
            desiredCenterPosition =
                transform.position + transform.forward * holdDistance;
        }

        Vector3 direction =
            desiredCenterPosition - transform.position;

        float distance =
            direction.magnitude;

        if (
            distance > 0.01f &&
            Physics.Raycast(
                transform.position,
                direction.normalized,
                out RaycastHit hit,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            if (hit.rigidbody != heldObject)
            {
                desiredCenterPosition =
                    hit.point -
                    direction.normalized * obstaclePadding;
            }
        }

        Vector3 desiredPivotPosition =
            GetDesiredPivotPositionFromCenter(desiredCenterPosition);

        Quaternion targetRotation =
            Quaternion.LookRotation(transform.forward, Vector3.up) *
            Quaternion.Euler(heldRotationOffset);

        heldObject.MovePosition(
            Vector3.Lerp(
                heldObject.position,
                desiredPivotPosition,
                moveSpeed * Time.deltaTime
            )
        );

        heldObject.MoveRotation(
            Quaternion.Lerp(
                heldObject.rotation,
                targetRotation,
                rotateSpeed * Time.deltaTime
            )
        );
    }

    Vector3 GetDesiredPivotPositionFromCenter(Vector3 desiredCenterPosition)
    {
        if (heldObject == null)
            return desiredCenterPosition;

        if (!centerHeldObjectByBounds)
            return desiredCenterPosition;

        Vector3 objectCenter =
            GetHeldObjectBoundsCenter();

        Vector3 pivotToCenterOffset =
            objectCenter - heldObject.position;

        return desiredCenterPosition - pivotToCenterOffset;
    }

    Vector3 GetHeldObjectBoundsCenter()
    {
        if (heldObject == null)
            return Vector3.zero;

        Collider[] colliders =
            heldObject.GetComponentsInChildren<Collider>();

        bool hasBounds = false;
        Bounds bounds = new Bounds();

        foreach (Collider col in colliders)
        {
            if (col == null)
                continue;

            if (!col.enabled)
                continue;

            if (col.isTrigger)
                continue;

            if (!hasBounds)
            {
                bounds = col.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(col.bounds);
            }
        }

        if (hasBounds)
            return bounds.center;

        Renderer[] renderers =
            heldObject.GetComponentsInChildren<Renderer>();

        foreach (Renderer renderer in renderers)
        {
            if (renderer == null)
                continue;

            if (!renderer.enabled)
                continue;

            if (!hasBounds)
            {
                bounds = renderer.bounds;
                hasBounds = true;
            }
            else
            {
                bounds.Encapsulate(renderer.bounds);
            }
        }

        if (hasBounds)
            return bounds.center;

        return heldObject.position;
    }

    Rigidbody GetRigidbodyFromHit(RaycastHit hit)
    {
        Rigidbody rb = hit.collider.attachedRigidbody;

        if (rb == null)
        {
            rb = hit.collider.GetComponentInParent<Rigidbody>();
        }

        return rb;
    }

    void HandleScroll()
    {
        if (scrollAction == null)
            return;

        Vector2 scrollInput =
            scrollAction.action.ReadValue<Vector2>();

        float scroll = scrollInput.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            holdDistance += scroll * scrollSpeed;

            holdDistance = Mathf.Clamp(
                holdDistance,
                minHoldDistance,
                maxHoldDistance
            );

            if (holdPoint != null)
            {
                holdPoint.localPosition =
                    new Vector3(
                        0,
                        0,
                        holdDistance
                    );
            }
        }
    }

    void DropObject()
    {
        if (heldObject == null)
            return;

        heldObject.isKinematic = false;
        heldObject.useGravity = true;
        heldObject.linearDamping = 1f;
        heldObject.angularDamping = 0.05f;
        heldObject.freezeRotation = false;

        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }

        StopHoldingEffects();

        heldObject = null;
#if PHOTON_UNITY_NETWORKING
        networkHeldObjectViewID = -1;
        networkIsHoldingItem = false;
#endif
    }

    public void DropHeldExternally()
    {
        if (heldObject == null)
            return;

        heldObject.isKinematic = false;
        heldObject.useGravity = true;
        heldObject.linearDamping = 1f;
        heldObject.angularDamping = 0.05f;
        heldObject.freezeRotation = false;

        StopHoldingEffects();

        heldObject = null;
#if PHOTON_UNITY_NETWORKING
        networkHeldObjectViewID = -1;
        networkIsHoldingItem = false;
#endif
    }

    public GameObject ForceStealHeldObject(
        Transform thiefHoldPoint
    )
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                return ForceStealHeldObjectPhoton();
            }
        }

        return ForceStealHeldObjectSingleplayer(
            thiefHoldPoint
        );
    }

    GameObject ForceStealHeldObjectSingleplayer(
        Transform thiefHoldPoint
    )
    {
        if (heldObject == null)
            return null;

        GameObject stolenObject =
            heldObject.gameObject;

        StopHoldingEffects();

        heldObject = null;

        Rigidbody rb =
            stolenObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            if (!rb.isKinematic)
            {
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            rb.useGravity = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }

        if (thiefHoldPoint != null)
        {
            stolenObject.transform.SetParent(thiefHoldPoint);
            stolenObject.transform.localPosition = Vector3.zero;
            stolenObject.transform.localRotation =
                Quaternion.identity;
        }

        Debug.Log(
            "Enemy stole item from player: " +
            stolenObject.name
        );

        return stolenObject;
    }

#if PHOTON_UNITY_NETWORKING
    GameObject ForceStealHeldObjectPhoton()
    {
        int itemViewID = HeldObjectViewID;

        if (itemViewID == -1)
        {
            Debug.LogWarning(
                "Cannot steal item. Held object has no PhotonView."
            );

            return null;
        }

        PhotonView itemView =
            PhotonView.Find(itemViewID);

        if (itemView == null)
        {
            Debug.LogWarning(
                "Cannot find held item PhotonView."
            );

            return null;
        }

        if (
            photonView != null &&
            photonView.Owner != null &&
            !photonView.IsMine
        )
        {
            photonView.RPC(
                nameof(RPC_ClearHeldItemBecauseStolen),
                photonView.Owner,
                itemViewID
            );
        }
        else
        {
            ClearHeldItemBecauseStolen(itemViewID);
        }

        networkIsHoldingItem = false;
        networkHeldObjectViewID = -1;

        itemView.TransferOwnership(
            PhotonNetwork.MasterClient
        );

        return itemView.gameObject;
    }

    [PunRPC]
    void RPC_ClearHeldItemBecauseStolen(
        int stolenItemViewID
    )
    {
        ClearHeldItemBecauseStolen(
            stolenItemViewID
        );
    }

    void ClearHeldItemBecauseStolen(
        int stolenItemViewID
    )
    {
        if (heldObject != null)
        {
            int currentHeldViewID =
                HeldObjectViewID;

            if (
                currentHeldViewID == stolenItemViewID ||
                currentHeldViewID == -1
            )
            {
                StopHoldingEffects();

                heldObject = null;
            }
        }

        networkIsHoldingItem = false;
        networkHeldObjectViewID = -1;

        Debug.Log(
            "Held item cleared because enemy stole it."
        );
    }

    public void OnPhotonSerializeView(
        PhotonStream stream,
        PhotonMessageInfo info
    )
    {
        if (stream.IsWriting)
        {
            bool holding =
                heldObject != null;

            int heldViewID =
                -1;

            if (heldObject != null)
            {
                PhotonView itemView =
                    heldObject.GetComponent<PhotonView>();

                if (itemView == null)
                {
                    itemView =
                        heldObject.GetComponentInParent<PhotonView>();
                }

                if (itemView != null)
                {
                    heldViewID = itemView.ViewID;
                }
            }

            stream.SendNext(holding);
            stream.SendNext(heldViewID);
        }
        else
        {
            networkIsHoldingItem =
                (bool)stream.ReceiveNext();

            networkHeldObjectViewID =
                (int)stream.ReceiveNext();
        }
    }
#endif

    void StopHoldingEffects()
    {
        if (
            holdAudioSource != null &&
            holdAudioSource.isPlaying
        )
        {
            holdAudioSource.Stop();
        }

        if (currentHighlight != null)
        {
            currentHighlight.RemoveHighlight();
            currentHighlight = null;
        }

        if (interactionText != null)
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}