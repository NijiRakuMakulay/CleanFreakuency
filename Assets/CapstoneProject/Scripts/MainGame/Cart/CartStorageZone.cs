using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class CartStorageZone : MonoBehaviour
{
    [Header("Only these layers count as cart items")]
    public LayerMask pickupLayer;

    private readonly Dictionary<TrashItem, int> itemsInside =
        new Dictionary<TrashItem, int>();

    private readonly List<TrashItem> cleanupList =
        new List<TrashItem>();

    private void Awake()
    {
        Collider zoneCollider = GetComponent<Collider>();
        zoneCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        TrashItem item = other.GetComponentInParent<TrashItem>();

        if (item == null)
            return;

        bool colliderIsPickup =
            IsInPickupLayer(other.gameObject);

        bool itemRootIsPickup =
            IsInPickupLayer(item.gameObject);

        if (!colliderIsPickup && !itemRootIsPickup)
            return;

        if (!itemsInside.ContainsKey(item))
        {
            itemsInside[item] = 0;
        }

        itemsInside[item]++;
    }

    private void OnTriggerExit(Collider other)
    {
        TrashItem item = other.GetComponentInParent<TrashItem>();

        if (item == null)
            return;

        if (!itemsInside.ContainsKey(item))
            return;

        itemsInside[item]--;

        if (itemsInside[item] <= 0)
        {
            itemsInside.Remove(item);
        }
    }

    public bool HasItems()
    {
        CleanupMissingItems();
        return itemsInside.Count > 0;
    }

    public TrashItem GetRandomItemInside()
    {
        CleanupMissingItems();

        if (itemsInside.Count == 0)
            return null;

        cleanupList.Clear();

        foreach (TrashItem item in itemsInside.Keys)
        {
            cleanupList.Add(item);
        }

        int randomIndex = Random.Range(0, cleanupList.Count);
        return cleanupList[randomIndex];
    }

    public void RemoveItem(TrashItem item)
    {
        if (item == null)
            return;

        if (itemsInside.ContainsKey(item))
        {
            itemsInside.Remove(item);
        }
    }

    private void CleanupMissingItems()
    {
        cleanupList.Clear();

        foreach (TrashItem item in itemsInside.Keys)
        {
            if (item == null || !item.gameObject.activeInHierarchy)
            {
                cleanupList.Add(item);
            }
        }

        foreach (TrashItem item in cleanupList)
        {
            itemsInside.Remove(item);
        }
    }

    private bool IsInPickupLayer(GameObject obj)
    {
        return (pickupLayer.value & (1 << obj.layer)) != 0;
    }
}