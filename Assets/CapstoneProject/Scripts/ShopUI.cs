using UnityEngine;
using TMPro;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public GameObject panel;

    public TextMeshProUGUI itemCountText;
    public TextMeshProUGUI itemListText;
    public TextMeshProUGUI totalValueText;

    public ShopArea shopArea;

    public FPS_Controller playerController;

    public void OpenShop()
    {
        panel.SetActive(true);

        playerController.shopOpen = true;

        UpdateUI(shopArea.GetItems());
    }

    public void CloseShop()
    {
        panel.SetActive(false);

        playerController.shopOpen = false;
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

        shopArea.SellItems();
    }
}