using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PhotonView))]
public class FPS_Controller : MonoBehaviourPunCallbacks, IPunObservable
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

    PhotonView pv;
    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    public override void OnEnable()
    {
        moveAction.action.Enable();
        lookAction.action.Enable();
        jumpAction.action.Enable();
        runAction.action.Enable();
    }

    public override void OnDisable()
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
        networkPosition = transform.position;
        networkRotation = transform.rotation;
        pv = GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.LogWarning("Operating in Multiplayer Mode");
            }
        }
        else
        {
            Debug.LogWarning("Operating in Singleplayer Mode");
        }
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                if (pv.IsMine)
                {
                    PlayerControls();
                    playerCamera.enabled = true;
                }
                else
                {
                    playerCamera.enabled = false;
                    transform.position = networkPosition;
                    transform.rotation = networkRotation;
                    //transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                    //transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
                }
            }
        }
        else
        {
            PlayerControls();
        }
        
    }

    void PlayerControls()
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

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) // Local player → send data
        {
            stream.SendNext(transform.position);
            stream.SendNext(transform.rotation);
        }
        else // Remote player → receive data
        {
            networkPosition = (Vector3)stream.ReceiveNext();
            networkRotation = (Quaternion)stream.ReceiveNext();
        }
    }
}