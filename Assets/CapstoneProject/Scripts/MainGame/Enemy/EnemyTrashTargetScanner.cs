using UnityEngine;

public class EnemyTrashTargetScanner : MonoBehaviour
{
    [Header("Player Detection")]
    public bool canStealFromPlayers = true;
    public float playerDetectionRadius = 12f;

    [Header("Cart Detection")]
    public bool canStealFromCarts = true;
    public float cartDetectionRadius = 12f;
    public CartStorageZone[] cartStorageZones;

    public EnemyStealTarget FindBestTarget()
    {
        EnemyStealTarget playerTarget = FindBestPlayerTarget();

        if (playerTarget.IsValid)
            return playerTarget;

        EnemyStealTarget cartTarget = FindBestCartTarget();

        if (cartTarget.IsValid)
            return cartTarget;

        return EnemyStealTarget.None;
    }

    private EnemyStealTarget FindBestPlayerTarget()
    {
        if (!canStealFromPlayers)
            return EnemyStealTarget.None;

        PickupController bestPickup = null;
        float closestDistance = Mathf.Infinity;

        foreach (PickupController pickup in PickupController.ActivePickups)
        {
            if (pickup == null)
                continue;

            if (!pickup.IsHoldingItem)
                continue;

            float distance = Vector3.Distance(transform.position, pickup.transform.position);

            if (distance <= playerDetectionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                bestPickup = pickup;
            }
        }

        return EnemyStealTarget.FromPlayer(bestPickup);
    }

    private EnemyStealTarget FindBestCartTarget()
    {
        if (!canStealFromCarts)
            return EnemyStealTarget.None;

        RefreshCartZonesIfNeeded();

        if (cartStorageZones == null || cartStorageZones.Length == 0)
            return EnemyStealTarget.None;

        CartStorageZone bestCart = null;
        float closestDistance = Mathf.Infinity;

        foreach (CartStorageZone cartZone in cartStorageZones)
        {
            if (cartZone == null)
                continue;

            if (!cartZone.HasItems())
                continue;

            float distance = Vector3.Distance(transform.position, cartZone.transform.position);

            if (distance <= cartDetectionRadius && distance < closestDistance)
            {
                closestDistance = distance;
                bestCart = cartZone;
            }
        }

        return EnemyStealTarget.FromCart(bestCart);
    }

    private void RefreshCartZonesIfNeeded()
    {
        if (cartStorageZones != null && cartStorageZones.Length > 0)
            return;

        cartStorageZones = FindObjectsByType<CartStorageZone>(FindObjectsSortMode.None);
    }
}