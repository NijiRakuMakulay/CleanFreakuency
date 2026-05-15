using Photon.Pun;
using UnityEngine;

public class ReadySync : MonoBehaviourPunCallbacks, IPunObservable
{
    bool IsPlayerReady;
    bool NetReady;
    PhotonView PV;

    void Awake() { PV = GetComponent<PhotonView>(); PhotonNetwork.AutomaticallySyncScene = true; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        NetReady = IsPlayerReady;
    }

    void Update()
    {
        if (PV.IsMine) { Debug.Log("You are controlling"); } else { Debug.Log("You are observing"); }
    }

    public void ReadyToggle()
    {
        if (!IsPlayerReady) { IsPlayerReady = true; } else { IsPlayerReady = false; }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //Local Player -> Send Data
            stream.SendNext(IsPlayerReady);

        }
        else if (stream.IsReading)
        {
            //Remote Player -> Receive Data
            NetReady = (bool)stream.ReceiveNext();
        }
    }
}
