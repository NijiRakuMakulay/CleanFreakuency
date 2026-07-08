using UnityEngine;
using UnityEngine.AI;

#if PHOTON_UNITY_NETWORKING
using Photon.Pun;
#endif

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(EnemyTrashTargetScanner))]
[RequireComponent(typeof(EnemyTrashStealer))]
[RequireComponent(typeof(EnemyItemCarrier))]
[RequireComponent(typeof(EnemyRoamNavigator))]
#if PHOTON_UNITY_NETWORKING
public class EnemyTrashThiefAI : MonoBehaviourPun
#else
public class EnemyTrashThiefAI : MonoBehaviour
#endif
{
    private enum EnemyState
    {
        Roam,
        Chase,
        Flee
    }

    [Header("Components")]
    public EnemyTrashTargetScanner targetScanner;
    public EnemyTrashStealer trashStealer;
    public EnemyItemCarrier itemCarrier;
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
    private bool initialized;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();

        if (targetScanner == null)
            targetScanner = GetComponent<EnemyTrashTargetScanner>();

        if (trashStealer == null)
            trashStealer = GetComponent<EnemyTrashStealer>();

        if (itemCarrier == null)
            itemCarrier = GetComponent<EnemyItemCarrier>();

        if (roamNavigator == null)
            roamNavigator = GetComponent<EnemyRoamNavigator>();

        initialized = ValidateComponents();
    }

    private void Start()
    {
        if (!initialized)
            return;

#if PHOTON_UNITY_NETWORKING
        if (PhotonNetwork.InRoom && !PhotonNetwork.IsMasterClient)
        {
            if (agent != null)
                agent.enabled = false;

            enabled = false;
            return;
        }
#endif

        StartRoaming();
    }

    private void Update()
    {
        if (!initialized)
            return;

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

    private bool ValidateComponents()
    {
        if (agent == null)
        {
            Debug.LogError($"{name}: Missing NavMeshAgent.");
            enabled = false;
            return false;
        }

        if (targetScanner == null)
        {
            Debug.LogError($"{name}: Missing EnemyTrashTargetScanner.");
            enabled = false;
            return false;
        }

        if (trashStealer == null)
        {
            Debug.LogError($"{name}: Missing EnemyTrashStealer.");
            enabled = false;
            return false;
        }

        if (itemCarrier == null)
        {
            Debug.LogError($"{name}: Missing EnemyItemCarrier.");
            enabled = false;
            return false;
        }

        if (roamNavigator == null)
        {
            Debug.LogError($"{name}: Missing EnemyRoamNavigator.");
            enabled = false;
            return false;
        }

        return true;
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
        if (!initialized || agent == null || roamNavigator == null)
            return;

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
}