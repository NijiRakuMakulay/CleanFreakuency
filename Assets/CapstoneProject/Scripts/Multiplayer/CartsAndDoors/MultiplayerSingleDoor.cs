using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(PhotonView))]
public class MultiplayerSingleDoor : MonoBehaviourPunCallbacks, IPunObservable
{
    [Header("Hinge Marker")]
    [Tooltip("Empty object placed exactly where the door hinge should be.")]
    public Transform hingePoint;

    [Tooltip("Usually keep this as 0,1,0 for a normal vertical door hinge.")]
    public Vector3 worldHingeAxis = Vector3.up;

    [Header("Door Limits")]
    public bool useLimits = true;
    public float minAngle = -100f;
    public float maxAngle = 0f;

    [Header("Physics")]
    public float mass = 8f;
    public float linearDamping = 1f;
    public float angularDamping = 4f;

    [Header("Stability")]
    public int solverIterations = 12;
    public int solverVelocityIterations = 12;

    private Rigidbody rb;
    private HingeJoint hinge;
    private Rigidbody fixedAnchorBody;

    PhotonView pv;
    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        pv = GetComponent<PhotonView>();

        if (hingePoint == null)
        {
            Debug.LogError(name + ": Missing HingePoint reference.");
            enabled = false;
            return;
        }

        SetupRigidbody();
        CreateFixedAnchorBody();
        SetupHingeJoint();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                if (!pv.IsMine)
                {
                    transform.position = networkPosition;
                    transform.rotation = networkRotation;
                    //transform.position = Vector3.Lerp(transform.position, networkPosition, Time.deltaTime * 10f);
                    //transform.rotation = Quaternion.Lerp(transform.rotation, networkRotation, Time.deltaTime * 10f);
                }
            }
        }
    }

    private void SetupRigidbody()
    {
        rb.mass = mass;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints =
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        rb.solverIterations = solverIterations;
        rb.solverVelocityIterations = solverVelocityIterations;
    }

    private void CreateFixedAnchorBody()
    {
        GameObject anchorObject = new GameObject(name + "_FixedHingeAnchor");

        anchorObject.transform.position = hingePoint.position;
        anchorObject.transform.rotation = Quaternion.identity;

        if (transform.parent != null)
        {
            anchorObject.transform.SetParent(transform.parent, true);
        }

        fixedAnchorBody = anchorObject.AddComponent<Rigidbody>();
        fixedAnchorBody.useGravity = false;
        fixedAnchorBody.isKinematic = true;
    }

    private void SetupHingeJoint()
    {
        Vector3 hingeWorldPosition = hingePoint.position;
        Vector3 hingeWorldAxis = worldHingeAxis.normalized;

        hinge.connectedBody = fixedAnchorBody;

        hinge.autoConfigureConnectedAnchor = false;

        hinge.anchor = transform.InverseTransformPoint(hingeWorldPosition);
        hinge.connectedAnchor =
            fixedAnchorBody.transform.InverseTransformPoint(hingeWorldPosition);

        hinge.axis = transform.InverseTransformDirection(hingeWorldAxis);

        hinge.useLimits = useLimits;
        hinge.useSpring = false;
        hinge.useMotor = false;

        JointLimits limits = hinge.limits;
        limits.min = minAngle;
        limits.max = maxAngle;
        limits.bounciness = 0f;
        limits.bounceMinVelocity = 0f;
        hinge.limits = limits;

        hinge.enableCollision = false;
        hinge.enablePreprocessing = false;
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