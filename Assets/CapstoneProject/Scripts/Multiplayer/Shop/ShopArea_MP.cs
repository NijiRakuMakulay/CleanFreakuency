using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class ShopArea_MP : MonoBehaviourPunCallbacks
{
    public ShopUI_MP shopUI;
    PhotonView pv;
    private List<TrashItem> itemsInside = new List<TrashItem>();

    void Start() { pv = GetComponent<PhotonView>(); }

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
    [PunRPC]
    void Sell()
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

        MoneyManager_MP.Instance.AddMoney(total);

        shopUI.UpdateUI(itemsInside);

        Debug.Log(
            "Sold for ₱" + total
        );
    }
}