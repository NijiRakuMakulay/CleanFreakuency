using Photon.Pun;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ShopUI_MP : MonoBehaviourPunCallbacks
{
    public GameObject panel;

    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI itemListText;
    public TextMeshProUGUI totalValueText;

    public ShopArea_MP shopArea;

    public void OpenShop(GameObject customer)
    {
        panel.SetActive(true);

        customer.GetComponent<FPS_Controller>().shopOpen = true;

        UpdateUI(shopArea.GetItems());
    }

    public void AutoCloseShop(GameObject customer)
    {
        panel.SetActive(false);
        customer.GetComponent<FPS_Controller>().shopOpen = false;
    }

    public void ManualCloseShop()
    {
        panel.SetActive(false);
    }

    public void UpdateUI(
        List<TrashItem> items
    )
    {
        int total = 0;

        itemListText.text = "";

        foreach (TrashItem item in items)
        {
            if (item == null)
                continue;

            itemListText.text +=
                item.itemName +
                " - ₱" +
                item.value +
                "\n";

            total += item.value;
        }

        itemCountText.text =
            "Items: " + items.Count;

        totalValueText.text =
            "Total: ₱" + total;
    }

    public void SellAll()
    {
        if (shopArea.GetItems().Count == 0)
        {
            Debug.Log("Nothing to sell.");

            return;
        }

        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                shopArea.photonView.RPC("Sell", RpcTarget.All);
            }
        }
    }
}