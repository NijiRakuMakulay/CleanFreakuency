using UnityEngine;

public class EnemyTrashStealer_MP : MonoBehaviour
{
    [Header("References")]
    public EnemyItemCarrier_MP itemCarrier;

    [Header("Steal Rules")]
    public LayerMask forbiddenStealLayers;

    private void Awake()
    {
        if (itemCarrier == null)
        {
            itemCarrier = GetComponent<EnemyItemCarrier_MP>();
        }

        int cartLayer = LayerMask.NameToLayer("Cart");

        if (cartLayer != -1)
        {
            forbiddenStealLayers.value |= 1 << cartLayer;
        }
    }

    public bool TryStealFromPlayer(PickupController pickupController)
    {
        if (pickupController == null)
            return false;

        if (!pickupController.IsHoldingItem)
            return false;

        if (itemCarrier == null || itemCarrier.holdPoint == null)
            return false;

        GameObject rawItem = pickupController.ForceStealHeldObject(itemCarrier.holdPoint);

        if (rawItem == null)
            return false;

        GameObject stealableRoot = itemCarrier.GetStealableRoot(rawItem);

        bool isValid =
            itemCarrier.IsValidStealableObject(
                stealableRoot,
                forbiddenStealLayers
            );

        if (!isValid)
        {
            Debug.LogWarning("Enemy tried to steal an invalid object. Check if the cart is still on the Pickup layer.");

            itemCarrier.ReleaseItemWithoutForce(rawItem);
            return false;
        }

        itemCarrier.AttachItem(stealableRoot);
        return true;
    }

    public bool TryStealFromCart(CartStorageZone cartStorageZone)
    {
        if (cartStorageZone == null)
            return false;

        if (!cartStorageZone.HasItems())
            return false;

        if (itemCarrier == null)
            return false;

        TrashItem item = cartStorageZone.GetRandomItemInside();

        if (item == null)
            return false;

        GameObject itemObject = item.gameObject;

        bool isValid =
            itemCarrier.IsValidStealableObject(
                itemObject,
                forbiddenStealLayers
            );

        if (!isValid)
        {
            Debug.LogWarning("Enemy found an invalid cart item. The cart itself should not have TrashItem.");
            cartStorageZone.RemoveItem(item);
            return false;
        }

        cartStorageZone.RemoveItem(item);
        itemCarrier.AttachItem(itemObject);

        return true;
    }
}