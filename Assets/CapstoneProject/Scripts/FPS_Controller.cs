using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
public class FPS_Controller : MonoBehaviour
{
    PlayerAction PAct;
    InputAction Player_Move;
    InputAction Player_Jump;
    InputAction Player_Run;
    InputAction Player_Interact;
    InputAction Player_LMB;
    InputAction Player_RMB;
    InputAction ShowCursor;
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

    [Header("Look Sensitivity")]
    const float MinLookSensitivity = 0.1f;
    const float MaxLookSensitivity = 1.0f;
    [Range(0.1f, 1.0f)]public float LookSensitivity = 0.25f;

    void Awake()
    {
        characterController = GetComponent<CharacterController>();
        PAct = new PlayerAction();
    }
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

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        //Cursor Toggle
        if (ShowCursor.IsPressed())
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
        else
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        // Movement Directions
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);
        Vector3 MoveVector;
        MoveVector = Player_Move.ReadValue<Vector3>();

        // Running
        bool isRunning;
        if (Player_Run.IsPressed()) { isRunning = true; } else { isRunning = false; }

        // Movement Input
        float curSpeedX = canMove
            ? (isRunning ? runningSpeed : walkingSpeed) * MoveVector.z
            : 0;

        float curSpeedY = canMove
            ? (isRunning ? runningSpeed : walkingSpeed) * MoveVector.x
            : 0;

        float movementDirectionY = moveDirection.y;

        moveDirection = (forward * curSpeedX) + (right * curSpeedY);

        // Jumping
        if (Player_Jump.IsPressed() && canMove && characterController.isGrounded)
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
            rotationX += -(Mouse.current.delta.ReadValue().y * LookSensitivity) * lookSpeed;

            rotationX = Mathf.Clamp(rotationX, -lookXLimit, lookXLimit);

            playerCamera.transform.localRotation =
                Quaternion.Euler(rotationX, 0, 0);

            transform.rotation *=
                Quaternion.Euler(0, (Mouse.current.delta.ReadValue().x * LookSensitivity) * lookSpeed, 0);
        }

        /*
         * Old Version
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
        *
        */
    }
}