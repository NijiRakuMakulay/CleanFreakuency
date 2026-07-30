using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(HingeJoint))]
[RequireComponent(typeof(PhotonView))]
public class MultiplayerDoubleDoor : MonoBehaviourPunCallbacks, IPunObservable
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

    PhotonView pv;
    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        hinge = GetComponent<HingeJoint>();
        pv = GetComponent<PhotonView>();

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