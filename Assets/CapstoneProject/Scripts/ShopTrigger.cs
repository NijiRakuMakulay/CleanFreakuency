using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopUI shopUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.OpenShop();
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.CloseShop();
        }
    }
}