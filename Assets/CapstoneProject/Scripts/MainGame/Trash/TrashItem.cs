using Photon.Pun;
using UnityEngine;

#if PHOTON_UNITY_NETWORKING
[RequireComponent(typeof(PhotonView))]
#endif

#if PHOTON_UNITY_NETWORKING
public class TrashItem : MonoBehaviourPun, IPunObservable
#else
public class TrashItem : MonoBehaviour
#endif
{
    [Header("Basic Info")]
    public string itemName;
    public int value;
#if PHOTON_UNITY_NETWORKING
    PhotonView pv;
    [Header("Network sync variables")]
    private Vector3 networkPosition;
    private Quaternion networkRotation;


    void Awake()
    {
        networkPosition = transform.position;
        networkRotation = transform.rotation;
    }
    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom)
            {
                Debug.Log(string.Format("Tracking object: {0}", itemName));
            }
        }
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
#endif
}