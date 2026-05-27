using UnityEngine;
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

    private Rigidbody heldObject;

    private ObjectHighlight currentHighlight;

    private AudioSource holdAudioSource;

    void Start()
    {
        interactionText.gameObject.SetActive(false);

        holdAudioSource = gameObject.AddComponent<AudioSource>();

        holdAudioSource.loop = true;

        holdAudioSource.playOnAwake = false;

        holdAudioSource.clip = holdSound;

        // Initialize hold point
        holdPoint.localPosition =
            new Vector3(0, 0, holdDistance);
    }

    void Update()
    {
        HandleRaycast();

        if (heldObject != null)
        {
            MoveObject();

            HandleScroll();

            // Left click again to drop
            if (Input.GetMouseButtonDown(0))
            {
                DropObject();
            }
        }
        else
        {
            // Left click to pick up
            if (Input.GetMouseButtonDown(0))
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

            // Highlight ONLY if object is not held
            if (
                highlight != null &&
                rb != heldObject
            )
            {
                currentHighlight = highlight;

                currentHighlight.Highlight();
            }

            // Show UI ONLY if object is not held
            if (
                trash != null &&
                rb != heldObject
            )
            {
                interactionText.gameObject.SetActive(true);

                interactionText.text =
                    trash.itemName +
                    "\nValue: " +
                    trash.amount +
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

                // Reset hold distance properly
                holdDistance = Mathf.Clamp(
                    holdDistance,
                    minHoldDistance,
                    maxHoldDistance
                );

                holdPoint.localPosition =
                    new Vector3(0, 0, holdDistance);

                // Remove highlight immediately
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

                audioSource.PlayOneShot(pickupSound);

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

        // Wall collision check
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
            // Ignore held object collision
            if (hit.rigidbody != heldObject)
            {
                desiredPosition =
                    hit.point - direction.normalized * 0.3f;
            }
        }

        // Smooth movement
        heldObject.position = Vector3.Lerp(
            heldObject.position,
            desiredPosition,
            moveSpeed * Time.deltaTime
        );

        // Smooth rotation
        heldObject.rotation = Quaternion.Lerp(
            heldObject.rotation,
            transform.rotation,
            rotateSpeed * Time.deltaTime
        );
    }

    void HandleScroll()
    {
        float scroll =
            Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0)
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

        audioSource.PlayOneShot(dropSound);

        // Stop hold sound
        if (holdAudioSource.isPlaying)
        {
            holdAudioSource.Stop();
        }

        heldObject = null;
    }
}