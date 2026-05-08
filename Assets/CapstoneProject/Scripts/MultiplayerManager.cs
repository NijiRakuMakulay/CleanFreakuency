using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    RoomOptions MultiplayerOptions;
    int RoomID;
    int PlayerID;
    string RoomName;
    TextMeshProUGUI ConnectingText;
    TextMeshProUGUI TitleText;
    TextMeshProUGUI PlayerText;
    CanvasGroup LobbyMenu;
    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PlayerID = Random.Range(0, 9999999);
        PhotonNetwork.NickName = "Player" + PlayerID;
        ConnectingText = GameObject.Find("ConnectingState").GetComponent<TextMeshProUGUI>();
        TitleText = GameObject.Find("MPMan_TitleText").GetComponent<TextMeshProUGUI>();
        PlayerText = GameObject.Find("YourName").GetComponent<TextMeshProUGUI>();
        LobbyMenu = GameObject.Find("LobbyMenu").GetComponent<CanvasGroup>();
        ConnectingText.enabled = true;
        LobbyMenu.alpha = 0.0f;
        LobbyMenu.interactable = false;
        LobbyMenu.blocksRaycasts = false;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Connecting to master server...");
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            PlayerText.enabled = true;
            PlayerText.text = "Username: " + PhotonNetwork.NickName;
            if (PhotonNetwork.InLobby)
            {
                TitleText.text = "Multiplayer - Lobby";
                LobbyMenu.alpha = 1.0f;
                LobbyMenu.interactable = true;
                LobbyMenu.blocksRaycasts = true;
            }
            if (PhotonNetwork.InRoom)
            {
                TitleText.text = "Multiplayer - In Room: " + PhotonNetwork.CurrentRoom.Name + "[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + " players]";
                LobbyMenu.alpha = 0.0f;
                LobbyMenu.interactable = false;
                LobbyMenu.blocksRaycasts = false;
            }
        }
        else
        {
            PlayerText.enabled = false;
            ConnectingText.enabled = true;
            LobbyMenu.alpha = 0.0f;
            LobbyMenu.interactable = false;
            LobbyMenu.blocksRaycasts = false;
        }
    }

    public void RoomMake()
    {
        RoomID = Random.Range(0, 100000);
        RoomName = "Room" + RoomID;
        Debug.Log("Generating room: "+RoomName);
        PhotonNetwork.CreateRoom(RoomName, MultiplayerOptions);
    }

    public void RoomScan()
    {
        Debug.Log("Looking for rooms...");
        PhotonNetwork.JoinRandomRoom();
    }

    public override void OnConnectedToMaster()
    {
        Debug.Log("Connected! Joining lobby now.");
        ConnectingText.enabled = false;
        MultiplayerOptions = new RoomOptions();
        MultiplayerOptions.MaxPlayers = 4;
        PhotonNetwork.JoinLobby();
    }

    public override void OnJoinedLobby()
    {
        Debug.Log("Welcome to the Multiplayer Lobby!");
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created: " + PhotonNetwork.CurrentRoom.Name);
    }


    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        Debug.LogError("Can you try joining THAT room?");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("You're in: " + PhotonNetwork.CurrentRoom.Name + "[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]");
        foreach (Player human in PhotonNetwork.PlayerList)
        {
            Debug.Log("Player " + human.ActorNumber + ": " + human.NickName);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        Debug.LogError("Let's get started by creating a room!");
    }
}
