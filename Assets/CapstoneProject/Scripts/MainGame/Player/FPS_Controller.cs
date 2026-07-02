using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPS_Controller : MonoBehaviour
{
    [Header("Movement Settings")]
    public float walkingSpeed = 5f;
    public float runningSpeed = 8f;
    public float jumpForce = 5f;
    public float gravity = 20f;

    [Header("Camera Reference")]
    public Camera playerCamera;

    [Header("Camera Rotation")]
    public float lookSpeed = 2.0f;
    public float lookXLimit = 45.0f;

    [Header("Input Actions")]
    public InputActionReference moveAction;
    public InputActionReference lookAction;
    public InputActionReference jumpAction;
    public InputActionReference runAction;

    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    [Header("Movement Condition")]
    public bool canMove = true;

    [HideInInspector]
    public bool shopOpen = false;
    [HideInInspector]
    public bool disassemblyMode = false;

    void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        runAction.action.Enable();
    }

    void OnDisable()
    {
        moveAction.action.Disable();
        lookAction.action.Disable();
        jumpAction.action.Disable();
        runAction.action.Disable();
    }

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Cursor Handling
        if (shopOpen || disassemblyMode)
        {
            Cursor.lockState =
                CursorLockMode.None;

            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState =
                CursorLockMode.Locked;

            Cursor.visible = false;
        }

        // Movement Directions
        Vector3 forward = transform.forward;
        Vector3 right = transform.right;

        // Read Inputs
        Vector2 moveInput = moveAction.action.ReadValue<Vector2>();
        Vector2 lookInput = lookAction.action.ReadValue<Vector2>();

        bool isRunning = runAction.action.IsPressed();

        float speed = isRunning ? runningSpeed : walkingSpeed;

        float curSpeedX = canMove ? speed * moveInput.y : 0;
        float curSpeedY = canMove ? speed * moveInput.x : 0;

        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping
        if (jumpAction.action.triggered &&
            canMove &&
            characterController.isGrounded)
        {
            moveDirection.y = jumpForce;
        }
        else
        {
            moveDirection.y = movementDirectionY;
        }

        // Gravity
        if (!characterController.isGrounded)
        {
            moveDirection.y -= gravity * Time.deltaTime;
        }

        // Apply Movement
        characterController.Move(moveDirection * Time.deltaTime);

        // Mouse Look
        if (canMove && !shopOpen)
        {
            rotationX += -lookInput.y * lookSpeed;
            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            playerCamera.transform.localRotation =
                Quaternion.Euler(rotationX, 0, 0);

            transform.Rotate(0, lookInput.x * lookSpeed, 0);
        }
    }
}