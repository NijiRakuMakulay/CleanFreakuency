using UnityEngine;

public class ShopTrigger_MP : MonoBehaviour
{
    public ShopUI_MP shopUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.OpenShop(other.gameObject);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.AutoCloseShop(other.gameObject);
        }
    }
}