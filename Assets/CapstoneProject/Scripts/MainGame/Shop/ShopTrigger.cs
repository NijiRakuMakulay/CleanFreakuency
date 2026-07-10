using UnityEngine;

public class ShopTrigger : MonoBehaviour
{
    public ShopUI shopUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            shopUI.OpenShop(other.gameObject);
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (shopUI.panel.activeInHierarchy)
            {
                other.GetComponent<FPS_Controller>().shopOpen = true;
            }
            else
            {
                other.GetComponent<FPS_Controller>().shopOpen = false;
            }
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