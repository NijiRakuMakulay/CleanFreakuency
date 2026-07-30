using Photon.Pun;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class DisassemblyManager_MP : MonoBehaviourPunCallbacks, IPunObservable
{
    public Camera tableCamera;

    public InputActionReference clickAction;

    public DisassemblyTable_MP table;
    private DisassemblyPart hoveredPart;

    [Header("Normal Rewards")]
    public List<GameObject> rewardParts;

    [Header("Reward Spawn Points")]
    public List<Transform> rewardSpawnPoints;

    [Header("Secret Reward")]
    public GameObject bonusReward;

    [Header("Secret Sequence")]
    public List<string> secretSequence;

    private List<DisassemblyPart> parts = new List<DisassemblyPart>();

    private List<string> clickHistory = new List<string>();

    private GameObject currentObject;

    PhotonView worker;
    PhotonView pv;

    void Awake() { pv = GetComponent<PhotonView>(); }

    public override void OnEnable()
    {
        if (clickAction != null)
        {
            clickAction.action.Enable();

            Debug.Log(
                "Click action enabled"
            );
        }
    }

    public override void OnDisable()
    {
        if (clickAction != null)
        {
            clickAction.action.Disable();
        }
    }

    public void BeginDisassembly(GameObject obj, PhotonView pv)
    {
        Debug.Log("Disassembling...");
        worker = pv;
        currentObject = obj;
        if (worker.IsMine)
        {
            Debug.Log("Begin disassembly on: " + obj.name);
            parts.Clear();
            clickHistory.Clear();
            parts.AddRange( obj.GetComponentsInChildren<DisassemblyPart>() );
        }
    }

    void Update()
    {
        if (currentObject == null || !tableCamera.gameObject.activeSelf)
            return;

        HandleHover();

        if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            Debug.Log("Mouse click detected");

            HandleClick();
        }
    }

    void HandleClick()
    {
        if (worker.IsMine)
        {
            Debug.Log("Click detected");

            Ray ray = tableCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            RaycastHit hit;

            int mask = ~LayerMask.GetMask("DisassemblyTable");

            if (Physics.Raycast(ray, out hit, 100f, mask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log(
                    "Hit: " +
                    hit.collider.name
                );

                DisassemblyPart part =
                    hit.collider.GetComponent
                    <DisassemblyPart>();

                if (part == null)
                {
                    part = hit.collider.GetComponentInParent<DisassemblyPart>();
                }

                if (part == null)
                {
                    part = hit.collider.GetComponentInChildren<DisassemblyPart>();
                }

                if (part == null)
                {
                    Debug.Log("No DisassemblyPart");

                    return;
                }

                RemovePart(part);
            }
        }
    }

    void RemovePart(DisassemblyPart part)
    {
        if (!parts.Contains(part))
            return;

        Debug.Log("Removed: " + part.partID);

        clickHistory.Add(part.partID);

        parts.Remove(part);

        if (part.removeVisual != null)
        {
            part.removeVisual.SetActive(false);
        }

        Collider col = part.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }

        if (parts.Count == 0)
        {
            FinishDisassembly();
        }
    }

    void FinishDisassembly()
    {
        if (worker.IsMine)
        {
            int spawnIndex = 0;

            foreach (GameObject reward in rewardParts)
            {
                SpawnReward(reward,spawnIndex);
                spawnIndex++;
            }

            bool secretUnlocked = clickHistory.SequenceEqual(secretSequence);

            if (secretUnlocked && bonusReward != null)
            {
                SpawnReward(bonusReward,spawnIndex);
                Debug.Log("Secret reward unlocked!");
            }
            if (PhotonNetwork.IsConnected)
            {
                if (PhotonNetwork.InRoom)
                {
                    pv.RPC("RemoveCompactTrash", RpcTarget.All);
                }
            }
            Debug.Log("Disassembled!");
            
        }
        
        //Destroy(currentObject);
        currentObject = null;
        worker = null;
        table.EndDisassembly();
    }

    [PunRPC]void RemoveCompactTrash()
    {
        Destroy(currentObject);
    }

    void SpawnReward( GameObject rewardPrefab, int spawnIndex )
    {
        if (rewardPrefab == null)
        {
            Debug.LogWarning("Reward prefab is missing.");
            return;
        }

        if (rewardSpawnPoints == null || rewardSpawnPoints.Count == 0)
        {
            Debug.LogWarning("No reward spawn points assigned. Spawning at current object position instead.");
            PhotonNetwork.InstantiateRoomObject(rewardPrefab.name,currentObject.transform.position,Quaternion.identity);
            //Instantiate(rewardPrefab,currentObject.transform.position,Quaternion.identity);

            return;
        }

        if (spawnIndex >= rewardSpawnPoints.Count)
        {
            Debug.LogWarning("Not enough reward spawn points. Add more spawn points or reduce the number of rewards.");
            return;
        }

        Transform spawnPoint = rewardSpawnPoints[spawnIndex];

        if (spawnPoint == null)
        {
            Debug.LogWarning("Reward spawn point is missing in the Inspector.");
            return;
        }

        GameObject spawnedReward = PhotonNetwork.Instantiate(rewardPrefab.name, spawnPoint.position,spawnPoint.rotation);
        //GameObject spawnedReward = Instantiate(rewardPrefab, spawnPoint.position,spawnPoint.rotation);

        Rigidbody rb = spawnedReward.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }

    void HandleHover()
    {
        if (worker.IsMine)
        {
            Ray ray = tableCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

            RaycastHit hit;

            if (hoveredPart != null)
            {
                ObjectHighlight oldHighlight =
                    hoveredPart.GetComponent<ObjectHighlight>();

                if (oldHighlight != null)
                {
                    oldHighlight.RemoveHighlight();
                }

                hoveredPart = null;
            }

            int mask = ~LayerMask.GetMask("DisassemblyTable");

            if (Physics.Raycast(ray, out hit, 100f, mask, QueryTriggerInteraction.Ignore))
            {
                Debug.Log("Hover hit: " + hit.collider.name);

                DisassemblyPart part = hit.collider.GetComponent<DisassemblyPart>();

                if (part == null)
                {
                    part = hit.collider.GetComponentInParent<DisassemblyPart>();
                }

                if (part == null)
                {
                    part = hit.collider.GetComponentInChildren<DisassemblyPart>();
                }

                if (part != null)
                {
                    hoveredPart = part;

                    Debug.Log("Hovering: " + part.name);

                    ObjectHighlight highlight =
                        part.GetComponent<ObjectHighlight>();

                    if (highlight != null)
                    {
                        Debug.Log("Highlight found");

                        highlight.Highlight();
                    }
                    else
                    {
                        Debug.Log("No ObjectHighlight");
                    }
                }
            }
        }
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //Local Player -> Send Data
        }
        else if (stream.IsReading)
        {
            //Remote Player -> Receive Data
        }
    }
}