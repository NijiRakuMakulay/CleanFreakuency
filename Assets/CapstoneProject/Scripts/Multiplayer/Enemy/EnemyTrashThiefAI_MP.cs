using UnityEngine;
using UnityEngine.AI;
using Photon.Pun;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTrashTargetScanner))]
[RequireComponent(typeof(EnemyTrashStealer_MP))]
[RequireComponent(typeof(EnemyItemCarrier_MP))]
[RequireComponent(typeof(EnemyRoamNavigator))]
[RequireComponent(typeof(PhotonView))]
public class EnemyTrashThiefAI_MP : MonoBehaviourPunCallbacks, IPunObservable
{
    private enum EnemyState
    {
        Roam,
        Chase,
        Flee
    }

    [Header("Components")]
    public EnemyTrashTargetScanner targetScanner;
    public EnemyTrashStealer_MP trashStealer;
    public EnemyItemCarrier_MP itemCarrier;
    public EnemyRoamNavigator roamNavigator;

    [Header("Movement")]
    public float roamSpeed = 3.5f;
    public float chaseSpeed = 5f;
    public float fleeSpeed = 6f;

    [Header("Stealing")]
    public float playerStealDistance = 1.5f;
    public float cartStealDistance = 2f;

    [Header("Fleeing")]
    public float fleeDistance = 8f;
    public float maxCarryTime = 4f;
    public float tossForce = 5f;
    public float tossUpForce = 2f;

    private NavMeshAgent agent;
    private EnemyState currentState = EnemyState.Roam;
    private EnemyStealTarget currentTarget = EnemyStealTarget.None;

    private float carryTimer;
    GameObject EnemyManager;
    PhotonView pv;
    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        EnemyManager = GameObject.Find("OnlineEnemySpawner");
        pv = GetComponent<PhotonView>();
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                if (agent != null)
                {
                    networkPosition = transform.position;
                    networkRotation = transform.rotation;
                    agent.enabled = true;
                }
                enabled = true;
                return;
            }
            if (pv.IsMine)
            {
                StartRoaming();
            }
            else
            {
                transform.position = networkPosition;
                transform.rotation = networkRotation;
                //transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                //transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
            }

        }
    }

    void Update()
    {
        if (pv.IsMine)
        {
            if (agent == null || !agent.enabled)
                return;

            if (itemCarrier != null && itemCarrier.HasItem)
            {
                FleeBehavior();
                return;
            }

            EnemyStealTarget target = targetScanner.FindBestTarget();

            if (target.IsValid)
            {
                ChaseBehavior(target);
            }
            else
            {
                RoamBehavior();
            }
        }
        else
        {
            transform.position = networkPosition;
            transform.rotation = networkRotation;
            //transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
            //transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
        }
    }

    private void RoamBehavior()
    {
        if (currentState != EnemyState.Roam)
        {
            StartRoaming();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= roamNavigator.roamPointReachedDistance)
        {
            roamNavigator.GoToRandomRoamPoint(agent);
        }
    }

    private void ChaseBehavior(EnemyStealTarget target)
    {
        if (!target.IsValid)
        {
            StartRoaming();
            return;
        }

        currentState = EnemyState.Chase;
        currentTarget = target;

        agent.speed = chaseSpeed;
        agent.SetDestination(target.TargetTransform.position);

        float distanceToTarget = Vector3.Distance(
            transform.position,
            target.TargetTransform.position
        );

        float requiredStealDistance = GetStealDistance(target);

        if (distanceToTarget <= requiredStealDistance)
        {
            TryStealCurrentTarget();
        }
    }

    private void FleeBehavior()
    {
        currentState = EnemyState.Flee;
        agent.speed = fleeSpeed;

        carryTimer += Time.deltaTime;

        bool reachedFleePoint =
            !agent.pathPending &&
            agent.remainingDistance <= roamNavigator.roamPointReachedDistance;

        bool carriedTooLong = carryTimer >= maxCarryTime;

        if (reachedFleePoint || carriedTooLong)
        {
            TossStolenItem();
        }
    }

    private void TryStealCurrentTarget()
    {
        bool stoleItem = false;

        if (currentTarget.Type == EnemyStealTargetType.PlayerHeldItem)
        {
            stoleItem = trashStealer.TryStealFromPlayer(currentTarget.PickupController);
        }
        else if (currentTarget.Type == EnemyStealTargetType.CartItem)
        {
            stoleItem = trashStealer.TryStealFromCart(currentTarget.CartStorageZone);
        }

        if (!stoleItem)
        {
            currentTarget = EnemyStealTarget.None;
            StartRoaming();
            return;
        }

        StartFleeingFrom(currentTarget.TargetTransform);
    }

    private void StartRoaming()
    {
        if (agent == null || roamNavigator == null) return;

        currentState = EnemyState.Roam;
        currentTarget = EnemyStealTarget.None;

        agent.speed = roamSpeed;

        roamNavigator.GoToRandomRoamPoint(agent);
    }

    private void StartFleeingFrom(Transform targetToAvoid)
    {
        if (targetToAvoid == null)
        {
            StartRoaming();
            return;
        }

        currentState = EnemyState.Flee;
        carryTimer = 0f;

        agent.speed = fleeSpeed;

        Vector3 fleePoint = roamNavigator.GetFleePointAwayFrom(
            targetToAvoid,
            transform,
            fleeDistance * 0.5f
        );

        agent.SetDestination(fleePoint);
    }

    private void TossStolenItem()
    {
        if (itemCarrier == null || !itemCarrier.HasItem)
        {
            StartRoaming();
            return;
        }

        Vector3 tossDirection = transform.forward;
        tossDirection.y = 0f;

        if (tossDirection.sqrMagnitude <= 0.01f)
        {
            tossDirection = transform.right;
        }

        tossDirection.Normalize();

        Vector3 finalForce =
            tossDirection * tossForce +
            Vector3.up * tossUpForce;

        itemCarrier.TossCurrentItem(finalForce);

        StartRoaming();
    }

    private float GetStealDistance(EnemyStealTarget target)
    {
        if (target.Type == EnemyStealTargetType.CartItem)
            return cartStealDistance;

        return playerStealDistance;
    }
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Local player → send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else // Remote player → receive data
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}