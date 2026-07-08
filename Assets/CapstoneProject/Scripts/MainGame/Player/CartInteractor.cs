using UnityEngine;
using UnityEngine.InputSystem;

public class CartInteractor : MonoBehaviour
{
    [Header("References")]
    public Camera playerCamera;

    [Header("Interaction")]
    public float interactRange = 4f;
    public LayerMask cartLayer;

    [Header("Input")]
    public InputActionReference interactAction;
    public InputActionReference scrollAction;

    private CartController currentCart;

    private void OnEnable()
    {
        if (interactAction != null)
            interactAction.action.Enable();

        if (scrollAction != null)
            scrollAction.action.Enable();
    }

    private void OnDisable()
    {
        if (currentCart != null)
            currentCart.StopControlling();

        if (interactAction != null)
            interactAction.action.Disable();

        if (scrollAction != null)
            scrollAction.action.Disable();
    }

    private void Update()
    {
        if (interactAction != null && interactAction.action.WasPressedThisFrame())
        {
            if (currentCart != null)
            {
                DropCart();
            }
            else
            {
                TryGrabCart();
            }
        }

        if (currentCart != null && scrollAction != null)
        {
            Vector2 scrollValue = scrollAction.action.ReadValue<Vector2>();

            if (Mathf.Abs(scrollValue.y) > 0.01f)
            {
                currentCart.AdjustDistance(scrollValue.y * 0.01f);
            }
        }
    }

    private void TryGrabCart()
    {
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, interactRange, cartLayer))
        {
            CartController cart = hit.collider.GetComponentInParent<CartController>();

            if (cart != null)
            {
                currentCart = cart;
                currentCart.StartControlling(playerCamera.transform);
            }
        }
    }

    private void DropCart()
    {
        currentCart.StopControlling();
        currentCart = null;
    }
}