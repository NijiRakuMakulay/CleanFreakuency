using UnityEngine;

public enum EnemyStealTargetType
{
    None,
    PlayerHeldItem,
    CartItem
}

public struct EnemyStealTarget
{
    public EnemyStealTargetType Type { get; private set; }
    public Transform TargetTransform { get; private set; }
    public PickupController PickupController { get; private set; }
    public CartStorageZone CartStorageZone { get; private set; }

    public bool IsValid
    {
        get
        {
            return Type != EnemyStealTargetType.None && TargetTransform != null;
        }
    }

    public static EnemyStealTarget None
    {
        get
        {
            return new EnemyStealTarget
            {
                Type = EnemyStealTargetType.None,
                TargetTransform = null,
                PickupController = null,
                CartStorageZone = null
            };
        }
    }

    public static EnemyStealTarget FromPlayer(PickupController pickupController)
    {
        if (pickupController == null)
            return None;

        return new EnemyStealTarget
        {
            Type = EnemyStealTargetType.PlayerHeldItem,
            TargetTransform = pickupController.transform,
            PickupController = pickupController,
            CartStorageZone = null
        };
    }

    public static EnemyStealTarget FromCart(CartStorageZone cartStorageZone)
    {
        if (cartStorageZone == null)
            return None;

        return new EnemyStealTarget
        {
            Type = EnemyStealTargetType.CartItem,
            TargetTransform = cartStorageZone.transform,
            PickupController = null,
            CartStorageZone = cartStorageZone
        };
    }
}