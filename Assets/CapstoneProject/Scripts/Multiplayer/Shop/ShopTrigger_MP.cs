using Photon.Pun;
using UnityEngine;

public class ShopTrigger_MP : MonoBehaviourPunCallbacks
{
    public ShopUI_MP shopUI;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                shopUI.OpenShop(other.gameObject);
            }
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                if (shopUI.panel.activeInHierarchy) { other.GetComponent<FPS_Controller>().shopOpen = true; }
                else { other.GetComponent<FPS_Controller>().shopOpen = false; }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (other.GetComponent<PhotonView>().IsMine)
            {
                shopUI.AutoCloseShop(other.gameObject);
            }
        }
    }
}