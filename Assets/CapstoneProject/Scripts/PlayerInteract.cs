using UnityEngine;
using TMPro;

public class PlayerInteract : MonoBehaviour
{
    public float interactDistance = 3f;

    public TextMeshProUGUI interactionText;
    public InventorySystem inventory;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance))
        {
            TrashItem trash = hit.collider.GetComponent<TrashItem>();

            if (trash != null)
            {
                interactionText.gameObject.SetActive(true);

                interactionText.text =
                    "Press E to Pick Up\n" +
                    trash.itemName +
                    "\nAmount: " + trash.amount;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    inventory.AddTrash(trash.amount);

                    Destroy(trash.gameObject);

                    interactionText.gameObject.SetActive(false);
                }
            }
            else
            {
                interactionText.gameObject.SetActive(false);
            }
        }
        else
        {
            interactionText.gameObject.SetActive(false);
        }
    }
}