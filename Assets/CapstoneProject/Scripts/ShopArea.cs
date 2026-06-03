using UnityEngine;
using System.Collections.Generic;

public class ShopArea : MonoBehaviour
{
    public ShopUI shopUI;

    private List<TrashItem> itemsInside = new List<TrashItem>();

    void OnTriggerEnter(Collider other)
    {
        TrashItem item =
            other.GetComponent<TrashItem>();

        if (
            item != null &&
            !itemsInside.Contains(item)
        )
        {
            itemsInside.Add(item);

            shopUI.UpdateUI(itemsInside);
        }
    }

    void OnTriggerExit(Collider other)
    {
        TrashItem item =
            other.GetComponent<TrashItem>();

        if (
            item != null &&
            itemsInside.Contains(item)
        )
        {
            itemsInside.Remove(item);

            shopUI.UpdateUI(itemsInside);
        }
    }

    public List<TrashItem> GetItems()
    {
        itemsInside.RemoveAll(
            item => item == null
        );

        return itemsInside;
    }

    public void SellItems()
    {
        int total = 0;

        foreach (TrashItem item in GetItems())
        {
            if (item == null)
                continue;

            total += item.value;

            Destroy(item.gameObject);
        }

        itemsInside.Clear();

        MoneyManager.Instance.AddMoney(total);

        shopUI.UpdateUI(itemsInside);

        Debug.Log(
            "Sold for ₱" + total
        );
    }
}