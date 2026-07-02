using UnityEngine;
using UnityEngine.AI;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

#if PHOTON_UNITY_NETWORKING
public class EnemyTrashThiefAI : MonoBehaviourPun
#else
public class EnemyTrashThiefAI : MonoBehaviour
#endif
{
    private enum EnemyState
    {
        Roam,
        ChasePlayer,
        FleeWithTrash
    }

    [Header("References")]
    public Transform enemyHoldPoint;
    public Transform roamCenter;

    [Header("Carry Settings")]
    public Vector3 carryLocalPosition = Vector3.zero;
    public Vector3 carryLocalRotation = Vector3.zero;
    public bool forceCarryPositionEveryFrame = true;

    [Header("Roaming")]
    public float roamRadius = 15f;
    public float roamSpeed = 3.5f;
    public float roamPointReachedDistance = 1.2f;

    [Header("Stealing")]
    public float detectionRadius = 12f;
    public float stealDistance = 1.5f;
    public float chaseSpeed = 5f;

    [Header("Fleeing")]
    public float fleeSpeed = 6f;
    public float fleeDistance = 10f;
    public float tossForce = 5f;
    public float tossUpForce = 2f;
    public float maxCarryTime = 4f;

    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Roam;

    private PickupController targetPickup;
    private GameObject stolenItem;

    private float carryTimer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

#if PHOTON_UNITY_NETWORKING
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            if (agent != null)
            {
                agent.enabled = false;
            }

            return;
        }
#endif

        if (roamCenter == null)
        {
            roamCenter = transform;
        }

        GoToRandomRoamPoint();
    }

    void Update()
    {
        if (stolenItem != null && forceCarryPositionEveryFrame)
        {
            KeepStolenItemAttached();
        }

#if PHOTON_UNITY_NETWORKING
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            return;
        }
#endif

        if (agent == null || !agent.enabled)
            return;

        if (stolenItem != null)
        {
            FleeWithTrashBehavior();
            return;
        }

        PickupController bestTarget =
            FindBestPlayerHoldingTrash();

        if (bestTarget != null)
        {
            ChasePlayerBehavior(bestTarget);
        }
        else
        {
            RoamBehavior();
        }
    }

    PickupController FindBestPlayerHoldingTrash()
    {
        PickupController bestTarget = null;
        float closestDistance = Mathf.Infinity;

        foreach (PickupController pickup in PickupController.ActivePickups)
        {
            if (pickup == null)
                continue;

            if (!pickup.IsHoldingItem)
                continue;

            float distance =
                Vector3.Distance(
                    transform.position,
                    pickup.transform.position
                );

            if (
                distance <= detectionRadius &&
                distance < closestDistance
            )
            {
                closestDistance = distance;
                bestTarget = pickup;
            }
        }

        return bestTarget;
    }

    void RoamBehavior()
    {
        currentState = EnemyState.Roam;
        targetPickup = null;

        agent.speed = roamSpeed;

        if (
            !agent.pathPending &&
            agent.remainingDistance <= roamPointReachedDistance
        )
        {
            GoToRandomRoamPoint();
        }
    }

    void ChasePlayerBehavior(PickupController pickup)
    {
        if (pickup == null)
            return;

        currentState = EnemyState.ChasePlayer;
        targetPickup = pickup;

        agent.speed = chaseSpeed;
        agent.SetDestination(pickup.transform.position);

        float distanceToPlayer =
            Vector3.Distance(
                transform.position,
                pickup.transform.position
            );

        if (distanceToPlayer <= stealDistance)
        {
            TryStealItem(pickup);
        }
    }

    void TryStealItem(PickupController pickup)
    {
        if (pickup == null || !pickup.IsHoldingItem)
            return;

        stolenItem =
            pickup.ForceStealHeldObject(enemyHoldPoint);

        if (stolenItem == null)
            return;

#if PHOTON_UNITY_NETWORKING
        if (PhotonNetwork.InRoom)
        {
            int itemViewID =
                GetPhotonViewID(stolenItem);

            if (
                itemViewID != -1 &&
                photonView != null
            )
            {
                photonView.RPC(
                    nameof(RPC_AttachStolenItem),
                    RpcTarget.All,
                    itemViewID
                );
            }
            else
            {
                SetItemHeldByEnemy(stolenItem);
            }
        }
        else
        {
            SetItemHeldByEnemy(stolenItem);
        }
#else
        SetItemHeldByEnemy(stolenItem);
#endif

        carryTimer = 0f;

        Vector3 fleeTarget =
            GetRandomDropPointAwayFromPlayer(
                pickup.transform
            );

        agent.speed = fleeSpeed;
        agent.SetDestination(fleeTarget);

        currentState = EnemyState.FleeWithTrash;

        Debug.Log("Enemy stole an item and is running away!");
    }

    void FleeWithTrashBehavior()
    {
        currentState = EnemyState.FleeWithTrash;
        agent.speed = fleeSpeed;

        carryTimer += Time.deltaTime;

        KeepStolenItemAttached();

        if (
            !agent.pathPending &&
            agent.remainingDistance <= roamPointReachedDistance
        )
        {
            TossStolenItem();
            return;
        }

        if (carryTimer >= maxCarryTime)
        {
            TossStolenItem();
            return;
        }
    }

    void SetItemHeldByEnemy(GameObject item)
    {
        if (item == null || enemyHoldPoint == null)
            return;

        Rigidbody rb =
            item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }

        Collider[] colliders =
            item.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }

#if PHOTON_UNITY_NETWORKING
        PhotonTransformView photonTransformView =
            item.GetComponent<PhotonTransformView>();

        if (photonTransformView != null)
        {
            photonTransformView.enabled = false;
        }

        PhotonRigidbodyView photonRigidbodyView =
            item.GetComponent<PhotonRigidbodyView>();

        if (photonRigidbodyView != null)
        {
            photonRigidbodyView.enabled = false;
        }
#endif

        item.transform.SetParent(enemyHoldPoint, false);
        item.transform.localPosition = carryLocalPosition;
        item.transform.localRotation = Quaternion.Euler(carryLocalRotation);

        stolenItem = item;

        KeepStolenItemAttached();
    }

    void KeepStolenItemAttached()
    {
        if (stolenItem == null || enemyHoldPoint == null)
            return;

        if (stolenItem.transform.parent != enemyHoldPoint)
        {
            stolenItem.transform.SetParent(enemyHoldPoint, false);
        }

        stolenItem.transform.localPosition = carryLocalPosition;
        stolenItem.transform.localRotation = Quaternion.Euler(carryLocalRotation);

        Rigidbody rb =
            stolenItem.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            rb.useGravity = false;
            rb.isKinematic = true;
            rb.freezeRotation = true;
        }
    }

    void TossStolenItem()
    {
        if (stolenItem == null)
            return;

        GameObject itemToToss =
            stolenItem;

        stolenItem = null;

        Vector3 tossDirection =
            transform.forward;

        tossDirection.y = 0f;

        if (tossDirection == Vector3.zero)
        {
            tossDirection = transform.right;
        }

        tossDirection.Normalize();

        Vector3 finalForce =
            tossDirection * tossForce +
            Vector3.up * tossUpForce;

#if PHOTON_UNITY_NETWORKING
        if (PhotonNetwork.InRoom)
        {
            int itemViewID =
                GetPhotonViewID(itemToToss);

            if (
                itemViewID != -1 &&
                photonView != null
            )
            {
                photonView.RPC(
                    nameof(RPC_TossStolenItem),
                    RpcTarget.All,
                    itemViewID,
                    finalForce
                );
            }
            else
            {
                TossItemObject(
                    itemToToss,
                    finalForce,
                    true
                );
            }
        }
        else
        {
            TossItemObject(
                itemToToss,
                finalForce,
                true
            );
        }
#else
        TossItemObject(
            itemToToss,
            finalForce,
            true
        );
#endif

        Debug.Log("Enemy tossed the stolen item!");

        targetPickup = null;

        GoToRandomRoamPoint();

        currentState = EnemyState.Roam;
    }

    void TossItemObject(
        GameObject item,
        Vector3 force,
        bool applyForce
    )
    {
        if (item == null)
            return;

        item.transform.SetParent(null, true);

#if PHOTON_UNITY_NETWORKING
        PhotonTransformView photonTransformView =
            item.GetComponent<PhotonTransformView>();

        if (photonTransformView != null)
        {
            photonTransformView.enabled = true;
        }

        PhotonRigidbodyView photonRigidbodyView =
            item.GetComponent<PhotonRigidbodyView>();

        if (photonRigidbodyView != null)
        {
            photonRigidbodyView.enabled = true;
        }
#endif

        Collider[] colliders =
            item.GetComponentsInChildren<Collider>();

        foreach (Collider col in colliders)
        {
            col.enabled = true;
        }

        Rigidbody rb =
            item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.freezeRotation = false;

            rb.collisionDetectionMode =
                CollisionDetectionMode.ContinuousDynamic;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            if (applyForce)
            {
                rb.AddForce(
                    force,
                    ForceMode.Impulse
                );
            }
        }
    }

    void GoToRandomRoamPoint()
    {
        if (agent == null || !agent.enabled)
            return;

        Vector3 randomPoint;

        if (
            GetRandomNavMeshPoint(
                roamCenter.position,
                roamRadius,
                out randomPoint
            )
        )
        {
            agent.SetDestination(randomPoint);
        }
    }

    Vector3 GetRandomDropPointAwayFromPlayer(
        Transform playerTransform
    )
    {
        Vector3 bestPoint =
            transform.position;

        float bestDistance =
            -1f;

        for (int i = 0; i < 30; i++)
        {
            Vector3 randomPoint;

            bool found =
                GetRandomNavMeshPoint(
                    roamCenter.position,
                    roamRadius,
                    out randomPoint
                );

            if (!found)
                continue;

            float distanceFromPlayer =
                Vector3.Distance(
                    randomPoint,
                    playerTransform.position
                );

            float distanceFromEnemy =
                Vector3.Distance(
                    randomPoint,
                    transform.position
                );

            if (
                distanceFromPlayer > bestDistance &&
                distanceFromEnemy >= fleeDistance * 0.5f
            )
            {
                bestDistance = distanceFromPlayer;
                bestPoint = randomPoint;
            }
        }

        return bestPoint;
    }

    bool GetRandomNavMeshPoint(
        Vector3 center,
        float radius,
        out Vector3 result
    )
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection =
                Random.insideUnitSphere * radius;

            randomDirection += center;

            NavMeshHit hit;

            if (
                NavMesh.SamplePosition(
                    randomDirection,
                    out hit,
                    radius,
                    NavMesh.AllAreas
                )
            )
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }

#if PHOTON_UNITY_NETWORKING
    int GetPhotonViewID(GameObject obj)
    {
        if (obj == null)
            return -1;

        PhotonView view =
            obj.GetComponent<PhotonView>();

        if (view == null)
        {
            view =
                obj.GetComponentInParent<PhotonView>();
        }

        if (view == null)
            return -1;

        return view.ViewID;
    }

    [PunRPC]
    void RPC_AttachStolenItem(int itemViewID)
    {
        PhotonView itemView =
            PhotonView.Find(itemViewID);

        if (itemView == null)
            return;

        stolenItem =
            itemView.gameObject;

        SetItemHeldByEnemy(stolenItem);
    }

    [PunRPC]
    void RPC_TossStolenItem(
        int itemViewID,
        Vector3 force
    )
    {
        PhotonView itemView =
            PhotonView.Find(itemViewID);

        if (itemView == null)
            return;

        bool shouldApplyForce =
            PhotonNetwork.IsMasterClient;

        TossItemObject(
            itemView.gameObject,
            force,
            shouldApplyForce
        );

        if (stolenItem == itemView.gameObject)
        {
            stolenItem = null;
        }
    }
#endif
}