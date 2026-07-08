using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class PhysicsDoor2 : MonoBehaviour
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

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();

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
}