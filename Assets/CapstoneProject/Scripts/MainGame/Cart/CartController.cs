using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class CartController : MonoBehaviour
{
    [Header("Cart Movement")]
    public float holdDistance = 2.5f;
    public float minHoldDistance = 1.5f;
    public float maxHoldDistance = 5f;
    public float scrollSpeed = 1f;

    [Header("Physics Pulling")]
    public float pullForce = 70f;
    public float dampingForce = 8f;
    public float maxSpeed = 12f;
    public float rotationSpeed = 10f;

    [Header("Forward Push Assist")]
    [Tooltip("Extra force that helps the cart move forward faster when the player pushes forward.")]
    public float forwardAssistForce = 55f;

    [Tooltip("How far the cart must lag behind the hold point before forward assist starts.")]
    public float forwardAssistStartDistance = 0.15f;

    [Tooltip("Higher value makes the cart catch up harder when it is far from the hold point.")]
    public float forwardAssistMultiplier = 1.5f;

    [Header("Facing Direction")]
    [Tooltip("Enable this so the panel side of the cart faces the player while held.")]
    public bool panelSideFacesPlayer = true;

    [Tooltip("Change this if the wrong side faces the player. Try 0, 90, -90, or 180.")]
    public float panelFacingYawOffset = 0f;

    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip grabSound;
    public AudioClip holdSound;
    public AudioClip releaseSound;

    [Range(0f, 1f)]
    public float holdSoundVolume = 0.6f;

    private Rigidbody rb;
    private Transform playerCamera;
    private bool isBeingControlled;

    private AudioSource holdAudioSource;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();

        rb.useGravity = true;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        SetupAudio();
    }

    private void OnDisable()
    {
        StopHoldSound();
    }

    private void SetupAudio()
    {
        if (audioSource == null)
        {
            audioSource = GetComponent<AudioSource>();
        }

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        audioSource.playOnAwake = false;

        holdAudioSource = gameObject.AddComponent<AudioSource>();
        holdAudioSource.playOnAwake = false;
        holdAudioSource.loop = true;
        holdAudioSource.volume = holdSoundVolume;
        holdAudioSource.clip = holdSound;
    }

    public void StartControlling(Transform cameraTransform)
    {
        playerCamera = cameraTransform;

        if (!isBeingControlled)
        {
            PlayOneShot(grabSound);
            StartHoldSound();
        }

        isBeingControlled = true;
    }

    public void StopControlling()
    {
        if (!isBeingControlled)
            return;

        isBeingControlled = false;
        playerCamera = null;

        StopHoldSound();
        PlayOneShot(releaseSound);
    }

    public void AdjustDistance(float scrollInput)
    {
        if (!isBeingControlled)
            return;

        holdDistance += scrollInput * scrollSpeed;
        holdDistance = Mathf.Clamp(holdDistance, minHoldDistance, maxHoldDistance);
    }

    private void FixedUpdate()
    {
        if (!isBeingControlled || playerCamera == null)
            return;

        Vector3 cameraForward =
            Vector3.ProjectOnPlane(playerCamera.forward, Vector3.up).normalized;

        if (cameraForward.sqrMagnitude < 0.01f)
            return;

        MoveCartTowardHoldPosition(cameraForward);
        RotateCartTowardPlayer(cameraForward);
        LimitCartSpeed();
    }

    private void MoveCartTowardHoldPosition(Vector3 cameraForward)
    {
        Vector3 targetPosition =
            playerCamera.position + cameraForward * holdDistance;

        targetPosition.y = rb.position.y;

        Vector3 toTarget = targetPosition - rb.position;
        toTarget.y = 0f;

        Vector3 horizontalVelocity = rb.linearVelocity;
        horizontalVelocity.y = 0f;

        Vector3 pullForceVector =
            toTarget * pullForce -
            horizontalVelocity * dampingForce;

        rb.AddForce(pullForceVector, ForceMode.Acceleration);

        ApplyForwardPushAssist(cameraForward, toTarget);
    }

    private void ApplyForwardPushAssist(Vector3 cameraForward, Vector3 toTarget)
    {
        float forwardLag =
            Vector3.Dot(toTarget, cameraForward);

        if (forwardLag <= forwardAssistStartDistance)
            return;

        float assistStrength =
            Mathf.Clamp01(forwardLag * forwardAssistMultiplier);

        Vector3 assistForce =
            cameraForward * forwardAssistForce * assistStrength;

        rb.AddForce(assistForce, ForceMode.Acceleration);
    }

    private void RotateCartTowardPlayer(Vector3 cameraForward)
    {
        if (!panelSideFacesPlayer)
            return;

        Vector3 directionToPlayer = -cameraForward;

        Quaternion targetRotation =
            Quaternion.LookRotation(directionToPlayer, Vector3.up) *
            Quaternion.Euler(0f, panelFacingYawOffset, 0f);

        Quaternion smoothedRotation =
            Quaternion.Slerp(
                rb.rotation,
                targetRotation,
                rotationSpeed * Time.fixedDeltaTime
            );

        rb.MoveRotation(smoothedRotation);
    }

    private void LimitCartSpeed()
    {
        Vector3 currentVelocity = rb.linearVelocity;

        Vector3 flatVelocity =
            new Vector3(
                currentVelocity.x,
                0f,
                currentVelocity.z
            );

        if (flatVelocity.magnitude <= maxSpeed)
            return;

        Vector3 limitedFlatVelocity =
            flatVelocity.normalized * maxSpeed;

        rb.linearVelocity =
            new Vector3(
                limitedFlatVelocity.x,
                currentVelocity.y,
                limitedFlatVelocity.z
            );
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource == null || clip == null)
            return;

        audioSource.PlayOneShot(clip);
    }

    private void StartHoldSound()
    {
        if (holdAudioSource == null || holdSound == null)
            return;

        holdAudioSource.clip = holdSound;
        holdAudioSource.volume = holdSoundVolume;

        if (!holdAudioSource.isPlaying)
        {
            holdAudioSource.Play();
        }
    }

    private void StopHoldSound()
    {
        if (holdAudioSource == null)
            return;

        if (holdAudioSource.isPlaying)
        {
            holdAudioSource.Stop();
        }
    }
}