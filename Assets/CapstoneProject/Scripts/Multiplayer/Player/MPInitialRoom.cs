using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

[RequireComponent(typeof(PhotonView))]
public class MPInitialRoom : MonoBehaviourPunCallbacks, IPunObservable
{
    [SerializeField] Transform[] NetPos;
    [SerializeField] GameObject OnlinePlayer;
    PhotonView pv;
    int playerID;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
    }

    void Start()
    {
        pv = GetComponent<PhotonView>();
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Welcome!");
            if (PhotonNetwork.InRoom)
            {
                Debug.Log(string.Format("Your game has started in room {0}!", PhotonNetwork.CurrentRoom.Name));
                StartCoroutine(DelaySpawn());
            }
        }
        else
        {
            Debug.Log("This scene will only work when connected to Photon Network.");
        }
    }

    //Initial Player goes first...
    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(0.2f); // wait a frame or two
        SpawnPlayer();
        Debug.Log("Spawned player: " + playerID);
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
        PhotonNetwork.LeaveRoom();
        PhotonNetwork.LeaveLobby();
        PhotonNetwork.Disconnect();
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        PhotonNetwork.LocalPlayer.TagObject = null;
        SceneManager.LoadScene("_TitleScreen");
    }


    void SpawnPlayer()
    {
        if (OnlinePlayer == null)
        {
            Debug.LogError("Player prefab is missing in inspector!");
            return;
        }

        if (NetPos == null || NetPos.Length == 0)
        {
            Debug.LogError("No spawn points assigned!");
            return;
        }

        // Prevent double-spawning if the player already exists
        if (PhotonNetwork.LocalPlayer.TagObject != null)
        {
            Debug.Log("Player already spawned, skipping.");
            return;
        }

        playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("playerID: " + playerID);

        Transform spawnLocation; //this gets the position of location transform

        if (playerID == 1)
        {
            spawnLocation = NetPos[0];
        }
        else if (playerID == 2 && NetPos.Length > 1)
        {
            spawnLocation = NetPos[1];
        }
        else
        {
            int randomIndex = UnityEngine.Random.Range(0, NetPos.Length);
            spawnLocation = NetPos[randomIndex];
        }

        // The prefab MUST be in a folder called "Resources"
        // Instantiate networked player
        GameObject newPlayer = PhotonNetwork.Instantiate(OnlinePlayer.name, spawnLocation.position, spawnLocation.rotation);

        // Store a reference so Photon knows this player exists
        PhotonNetwork.LocalPlayer.TagObject = newPlayer;

        Debug.Log("Spawned player " + playerID + " at " + spawnLocation.name);
    }

    public override void OnJoinedRoom()
    {
        string InitialEntryMSG = "[" + DateTime.Now.ToString() + "] You're in: " + PhotonNetwork.CurrentRoom.Name + "[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]";
        if (PhotonNetwork.IsMasterClient)
        {
            Debug.Log("[" + DateTime.Now.ToString() + "] You have entered room \"" + PhotonNetwork.CurrentRoom.Name + "\" as the leader.[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]");
            SpawnPlayer();
        }
        else
        {
            Debug.Log("[" + DateTime.Now.ToString() + "] You have entered room \"" + PhotonNetwork.CurrentRoom.Name + "\" as a member.[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]");
            SpawnPlayer();
        }
    }
}
