using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PickupController : MonoBehaviour
{
    [Header("Pickup Settings")]
    public float pickupRange = 5f;

    [Header("Hold Point")]
    public Transform holdPoint;

    [Header("Hold Distance")]
    public float holdDistance = 2f;
    public float minHoldDistance = 1.5f;
    public float maxHoldDistance = 8f;
    public float scrollSpeed = 4f;

    [Header("Movement")]
    public float moveSpeed = 20f;
    public float rotateSpeed = 15f;

    [Header("Layer")]
    public LayerMask pickupLayer;

    [Header("UI")]
    public TextMeshProUGUI interactionText;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip pickupSound;
    public AudioClip holdSound;
    public AudioClip dropSound;

    [Header("Input Actions")]
    public InputActionReference interactAction;
    public InputActionReference scrollAction;

    [Header("References")]
    public FPS_Controller playerController;

    private Rigidbody heldObject;
    private ObjectHighlight currentHighlight;
    private AudioSource holdAudioSource;

    void OnEnable()
    {
        interactAction.action.Enable();
        scrollAction.action.Enable();
    }

    void OnDisable()
    {
        interactAction.action.Disable();
        scrollAction.action.Disable();
    }

    void Start()
    {
        interactionText.gameObject.SetActive(false);

        holdAudioSource = gameObject.AddComponent<AudioSource>();
        holdAudioSource.loop = true;
        holdAudioSource.playOnAwake = false;
        holdAudioSource.clip = holdSound;

        holdPoint.localPosition =
            new Vector3(0, 0, holdDistance);
    }

    void Update()
    {
        // Prevent pickup while shop is open
        if (playerController != null && playerController.shopOpen)
        {
            return;
        }

        HandleRaycast();

        if (heldObject != null)
        {
            MoveObject();
            HandleScroll();

            // Click again to drop
            if (interactAction.action.triggered)
            {
                DropObject();
            }
        }
        else
        {
            // Click to pickup
            if (interactAction.action.triggered)
            {
                TryPickup();
            }
        }
    }

    void HandleRaycast()
    {
        Ray ray =
            new Ray(transform.position, transform.forward);

        RaycastHit hit;

        // Remove previous highlight
        if (currentHighlight != null)
        {
            currentHighlight.RemoveHighlight();
            currentHighlight = null;
        }

        interactionText.gameObject.SetActive(false);

        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            ObjectHighlight highlight =
                hit.collider.GetComponent<ObjectHighlight>();

            TrashItem trash =
                hit.collider.GetComponent<TrashItem>();

            Rigidbody rb =
                hit.collider.GetComponent<Rigidbody>();

            // Highlight if not held
            if (highlight != null && rb != heldObject)
            {
                currentHighlight = highlight;
                currentHighlight.Highlight();
            }

            // Show UI if not held
            if (trash != null && rb != heldObject)
            {
                interactionText.gameObject.SetActive(true);

                interactionText.text =
                    trash.itemName +
                    "\nValue: " +
                    trash.value +
                    "\n\nLeft Click to Pick Up";
            }
        }
    }

    void TryPickup()
    {
        Ray ray =
            new Ray(transform.position, transform.forward);

        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, pickupRange, pickupLayer))
        {
            Rigidbody rb =
                hit.collider.GetComponent<Rigidbody>();

            if (rb != null)
            {
                heldObject = rb;

                holdDistance = Mathf.Clamp(
                    holdDistance,
                    minHoldDistance,
                    maxHoldDistance
                );

                holdPoint.localPosition =
                    new Vector3(0, 0, holdDistance);

                // Remove highlight
                ObjectHighlight highlight =
                    heldObject.GetComponent<ObjectHighlight>();

                if (highlight != null)
                {
                    highlight.RemoveHighlight();
                }

                heldObject.useGravity = false;
                heldObject.linearDamping = 10f;
                heldObject.angularDamping = 10f;
                heldObject.freezeRotation = true;

                if (audioSource != null && pickupSound != null)
                {
                    audioSource.PlayOneShot(pickupSound);
                }

                if (holdSound != null)
                {
                    holdAudioSource.Play();
                }
            }
        }
    }

    void MoveObject()
    {
        Vector3 desiredPosition =
            holdPoint.position;

        RaycastHit hit;

        Vector3 direction =
            desiredPosition - transform.position;

        float distance =
            direction.magnitude;

        // Wall collision
        if (
            Physics.Raycast(
                transform.position,
                direction.normalized,
                out hit,
                distance,
                ~0,
                QueryTriggerInteraction.Ignore
            )
        )
        {
            if (hit.rigidbody != heldObject)
            {
                desiredPosition =
                    hit.point -
                    direction.normalized * 0.3f;
            }
        }

        heldObject.position =
            Vector3.Lerp(
                heldObject.position,
                desiredPosition,
                moveSpeed * Time.deltaTime
            );

        heldObject.rotation =
            Quaternion.Lerp(
                heldObject.rotation,
                transform.rotation,
                rotateSpeed * Time.deltaTime
            );
    }

    void HandleScroll()
    {
        Vector2 scrollInput =
            scrollAction.action.ReadValue<Vector2>();

        float scroll = scrollInput.y;

        if (Mathf.Abs(scroll) > 0.01f)
        {
            holdDistance += scroll * scrollSpeed;

            holdDistance = Mathf.Clamp(
                holdDistance,
                minHoldDistance,
                maxHoldDistance
            );

            holdPoint.localPosition =
                new Vector3(
                    0,
                    0,
                    holdDistance
                );
        }
    }

    void DropObject()
    {
        heldObject.useGravity = true;
        heldObject.linearDamping = 1f;
        heldObject.angularDamping = 0.05f;
        heldObject.freezeRotation = false;

        if (audioSource != null && dropSound != null)
        {
            audioSource.PlayOneShot(dropSound);
        }

        if (holdAudioSource.isPlaying)
        {
            holdAudioSource.Stop();
        }

        heldObject = null;
    }

    public void DropHeldExternally()
    {
        if (heldObject == null)
            return;

        heldObject.useGravity = true;
        heldObject.linearDamping = 1f;
        heldObject.angularDamping = 0.05f;
        heldObject.freezeRotation = false;

        if (holdAudioSource.isPlaying)
        {
            holdAudioSource.Stop();
        }

        heldObject = null;
    }
}