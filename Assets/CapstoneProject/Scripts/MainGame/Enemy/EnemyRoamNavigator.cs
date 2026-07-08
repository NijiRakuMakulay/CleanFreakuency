using UnityEngine;
using UnityEngine.AI;

public class EnemyRoamNavigator : MonoBehaviour
{
    [Header("Roaming")]
    public Transform roamCenter;
    public float roamRadius = 15f;
    public float roamPointReachedDistance = 1.2f;

    private void Awake()
    {
        if (roamCenter == null)
        {
            roamCenter = transform;
        }
    }

    public void GoToRandomRoamPoint(NavMeshAgent agent)
    {
        if (agent == null || !agent.enabled)
            return;

        if (GetRandomNavMeshPoint(roamCenter.position, roamRadius, out Vector3 point))
        {
            agent.SetDestination(point);
        }
    }

    public Vector3 GetFleePointAwayFrom(Transform avoidTarget, Transform enemyTransform, float minimumEnemyDistance)
    {
        Vector3 bestPoint = enemyTransform.position;
        float bestDistanceFromTarget = -1f;

        for (int i = 0; i < 30; i++)
        {
            bool found = GetRandomNavMeshPoint(roamCenter.position, roamRadius, out Vector3 point);

            if (!found)
                continue;

            float distanceFromTarget = Vector3.Distance(point, avoidTarget.position);
            float distanceFromEnemy = Vector3.Distance(point, enemyTransform.position);

            if (distanceFromTarget > bestDistanceFromTarget && distanceFromEnemy >= minimumEnemyDistance)
            {
                bestDistanceFromTarget = distanceFromTarget;
                bestPoint = point;
            }
        }

        return bestPoint;
    }

    private bool GetRandomNavMeshPoint(Vector3 center, float radius, out Vector3 result)
    {
        for (int i = 0; i < 30; i++)
        {
            Vector3 randomDirection = Random.insideUnitSphere * radius;
            randomDirection += center;

            bool found = NavMesh.SamplePosition(
                randomDirection,
                out NavMeshHit hit,
                radius,
                NavMesh.AllAreas
            );

            if (found)
            {
                result = hit.position;
                return true;
            }
        }

        result = center;
        return false;
    }
}