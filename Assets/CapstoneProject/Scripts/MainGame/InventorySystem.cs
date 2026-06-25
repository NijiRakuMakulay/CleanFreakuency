using UnityEngine;
using TMPro;

public class InventorySystem : MonoBehaviour
{
    public int totalTrash = 0;

    public TextMeshProUGUI inventoryText;

    void Start()
    {
        UpdateUI();
    }

    public void AddTrash(int amount)
    {
        totalTrash += amount;

        UpdateUI();
    }

    void UpdateUI()
    {
        inventoryText.text = "Trash Collected: " + totalTrash;
    }
}