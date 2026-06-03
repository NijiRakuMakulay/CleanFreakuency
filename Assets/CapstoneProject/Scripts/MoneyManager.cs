using UnityEngine;
using TMPro;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance;

    public int currentMoney = 0;

    public TextMeshProUGUI moneyText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        UpdateUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;

        UpdateUI();
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;

            UpdateUI();

            return true;
        }

        return false;
    }

    void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText.text =
                "₱ " + currentMoney;
        }
    }
}