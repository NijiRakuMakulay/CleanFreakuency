using Photon.Pun;
using Photon.Realtime;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class MPInitialRoom : MonoBehaviourPunCallbacks, IPunObservable
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Welcome!");
            if (PhotonNetwork.InRoom)
            {
                Debug.Log(string.Format("Your game has started in room {0}!", PhotonNetwork.CurrentRoom.Name));
            }
        }
        else
        {
            Debug.Log("This scene will only work when connected to Photon Network.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //Local Player -> Send Data

        }
        else if (stream.IsReading)
        {
            //Remote Player -> Receive Data
        }
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Debug.LogWarning("Someone left!");
        //PhotonNetwork.LeaveRoom();
    }

    public override void OnJoinedRoom()
    {
        string InitialEntryMSG = "[" + DateTime.Now.ToString() + "] You're in: " + PhotonNetwork.CurrentRoom.Name + "[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]";
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[" + DateTime.Now.ToString() + "] You have entered room \"" + PhotonNetwork.CurrentRoom.Name + "\" as the leader.[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]");
        }
        else
        {
            Debug.Log("[" + DateTime.Now.ToString() + "] You have entered room \"" + PhotonNetwork.CurrentRoom.Name + "\" as a member.[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]");
        }
    }
}
