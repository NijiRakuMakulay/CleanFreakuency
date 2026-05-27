using UnityEngine;

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

    [Header("Controller Properties")]
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;
    private float rotationX = 0f;

    [Header("Movement Condition")]
    public bool canMove = true;

    void Start()
    {
        characterController = GetComponent<CharacterController>();

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Cursor Toggle
        if (Input.GetKeyDown(KeyCode.Z))
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        if (Input.GetMouseButtonDown(0))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Movement Directions
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Running
        bool isRunning = Input.GetKey(KeyCode.LeftShift);

        // Movement Input
        float curSpeedX = canMove
            ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Vertical")
            : 0;

        float curSpeedY = canMove
            ? (isRunning ? runningSpeed : walkingSpeed) * Input.GetAxis("Horizontal")
            : 0;

        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping
        if (Input.GetButton("Jump") && canMove && characterController.isGrounded)
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
        if (canMove)
        {
            rotationX += -Input.GetAxis("Mouse Y") * lookSpeed;

            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            playerCamera.transform.localRotation =
                Quaternion.Euler(rotationX, 0, 0);

            transform.rotation *=
                Quaternion.Euler(0, Input.GetAxis("Mouse X") * lookSpeed, 0);
        }
    }
}