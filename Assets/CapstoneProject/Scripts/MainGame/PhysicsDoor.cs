using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
public class PhysicsDoor : MonoBehaviour
{
    [Header("Door Limits")]
    public float minAngle = -100f;
    public float maxAngle = 0f;

    [Header("Physics")]
    public float mass = 8f;
    public float linearDamping = 1f;
    public float angularDamping = 2f;

    private Rigidbody rb;
    private HingeJoint hinge;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();

        rb.mass = mass;
        rb.linearDamping = linearDamping;
        rb.angularDamping = angularDamping;

        rb.useGravity = false;
        rb.isKinematic = false;
        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        rb.constraints =
            RigidbodyConstraints.FreezePositionY |
            RigidbodyConstraints.FreezeRotationX |
            RigidbodyConstraints.FreezeRotationZ;

        hinge.axis = Vector3.up;
        hinge.anchor = Vector3.zero;
        hinge.useLimits = true;

        JointLimits limits = hinge.limits;
        limits.min = minAngle;
        limits.max = maxAngle;
        hinge.limits = limits;
    }
}