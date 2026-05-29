using UnityEngine;
using TMPro;
using UnityEngine.InputSystem;

public class PlayerInteract : MonoBehaviour
{
    PlayerAction PAct;
    InputAction Player_Move;
    InputAction Player_Jump;
    InputAction Player_Run;
    InputAction Player_Interact;
    InputAction Player_LMB;
    InputAction Player_RMB;
    InputAction ShowCursor;

    public float interactDistance = 3f;
    public TextMeshProUGUI interactionText;
    public InventorySystem inventory;

    void Awake() { PAct = new PlayerAction(); }

    void OnEnable()
    {
        Debug.Log("Simulation started.");
        Player_Move = PAct.PlayableCharacter.Move;
        Player_Jump = PAct.PlayableCharacter.Jump;
        Player_Run = PAct.PlayableCharacter.Run;
        Player_Interact = PAct.PlayableCharacter.Interact;
        Player_LMB = PAct.PlayableCharacter.LeftClick;
        Player_RMB = PAct.PlayableCharacter.RightClick;
        ShowCursor = PAct.UserInterface.ShowCursor;
        Player_Move.Enable();
        Player_Jump.Enable();
        Player_Run.Enable();
        Player_Interact.Enable();
        Player_LMB.Enable();
        Player_RMB.Enable();
        ShowCursor.Enable();
    }

    void OnDisable()
    {
        Debug.Log("Simulation ended.");
        Player_Move.Disable();
        Player_Jump.Disable();
        Player_Run.Disable();
        Player_Interact.Disable();
        Player_LMB.Disable();
        Player_RMB.Disable();
        ShowCursor.Disable();
    }

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

                if (Player_Interact.IsPressed())
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