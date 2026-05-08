using UnityEngine;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine.UI;
public class MultiplayerManager : MonoBehaviourPunCallbacks
{
    RoomOptions MultiplayerOptions;
    int RoomID;
    int PlayerID;
    string username;
    string CurrentRoomName;
    string CreateRoomID;
    string JoinRoomID;
    TextMeshProUGUI ConnectingText;
    TextMeshProUGUI TitleText;
    TextMeshProUGUI LocalPlayerText;
    int playerIndex;
    TextMeshProUGUI[] MultiplayerText;
    TextMeshProUGUI[] MultiplayerStatus;
    TextMeshProUGUI CreateIDEntry;
    TextMeshProUGUI JoinIDEntry;
    TextMeshProUGUI NewNameText;
    TextMeshProUGUI LobbyMessage;
    CanvasGroup LobbyMenu;
    CanvasGroup RoomMenu;
    Button ManualCreateButton;
    Button AutoCreateButton;
    Button ManualJoinButton;
    Button AutoJoinButton;
    Button ChangeNameButton;

    void Awake()
    {
        CurrentRoomName = "";
        PhotonNetwork.AutomaticallySyncScene = true;
        PlayerID = Random.Range(0, 9999999);
        username = "Player" + PlayerID;
        PhotonNetwork.NickName = username;
        ConnectingText = GameObject.Find("ConnectingState").GetComponent<TextMeshProUGUI>();
        TitleText = GameObject.Find("MPMan_TitleText").GetComponent<TextMeshProUGUI>();
        LocalPlayerText = GameObject.Find("YourName").GetComponent<TextMeshProUGUI>();
        NewNameText = GameObject.Find("NewName").GetComponent<TextMeshProUGUI>();
        LobbyMenu = GameObject.Find("LobbyMenu").GetComponent<CanvasGroup>();
        RoomMenu = GameObject.Find("RoomMenu").GetComponent<CanvasGroup>();
        CreateIDEntry = GameObject.Find("CID").GetComponent<TextMeshProUGUI>();
        JoinIDEntry = GameObject.Find("JID").GetComponent<TextMeshProUGUI>();
        LobbyMessage = GameObject.Find("LobbyMessage").GetComponent<TextMeshProUGUI>();
        ConnectingText.enabled = true;
        LobbyMenu.alpha = 0.0f;
        LobbyMenu.interactable = false;
        LobbyMenu.blocksRaycasts = false;
        RoomMenu.alpha = 0.0f;
        RoomMenu.interactable = false;
        RoomMenu.blocksRaycasts = false;

        MultiplayerText = new TextMeshProUGUI[4];
        MultiplayerText[0] = GameObject.Find("Player1Text").GetComponent<TextMeshProUGUI>();
        MultiplayerText[1] = GameObject.Find("Player2Text").GetComponent<TextMeshProUGUI>();
        MultiplayerText[2] = GameObject.Find("Player3Text").GetComponent<TextMeshProUGUI>();
        MultiplayerText[3] = GameObject.Find("Player4Text").GetComponent<TextMeshProUGUI>();

        MultiplayerStatus = new TextMeshProUGUI[4];
        MultiplayerStatus[0] = GameObject.Find("Player1Status").GetComponent<TextMeshProUGUI>();
        MultiplayerStatus[1] = GameObject.Find("Player2Status").GetComponent<TextMeshProUGUI>();
        MultiplayerStatus[2] = GameObject.Find("Player3Status").GetComponent<TextMeshProUGUI>();
        MultiplayerStatus[3] = GameObject.Find("Player4Status").GetComponent<TextMeshProUGUI>();

        ManualCreateButton = GameObject.Find("CreateRoom").GetComponent<Button>();
        AutoCreateButton = GameObject.Find("AutoCreateRoom").GetComponent<Button>();
        ManualJoinButton = GameObject.Find("JoinRoom").GetComponent<Button>();
        AutoJoinButton = GameObject.Find("JoinRandom").GetComponent<Button>();
        ChangeNameButton = GameObject.Find("ChangeName").GetComponent<Button>();
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
        if (CreateIDEntry.text.Length <= 1) { ManualCreateButton.interactable = false; } else { ManualCreateButton.interactable = true; }
        if (JoinIDEntry.text.Length <= 1) { ManualJoinButton.interactable = false; } else { ManualJoinButton.interactable = true; }
        if (NewNameText.text.Length <= 1) { ChangeNameButton.interactable = false; } else { ChangeNameButton.interactable = true; }
        if (PhotonNetwork.IsConnected)
        {
            LocalPlayerText.enabled = true;
            LocalPlayerText.text = "Username: " + PhotonNetwork.NickName;
            if (PhotonNetwork.InLobby)
            {
                TitleText.text = "Multiplayer - Lobby";
                LobbyMenu.alpha = 1.0f;
                LobbyMenu.interactable = true;
                LobbyMenu.blocksRaycasts = true;
                RoomMenu.alpha = 0.0f;
                RoomMenu.interactable = false;
                RoomMenu.blocksRaycasts = false;
            }
            if (PhotonNetwork.InRoom)
            {
                TitleText.text = "Multiplayer - In Room: " + PhotonNetwork.CurrentRoom.Name + "[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + " players]";
                LobbyMenu.alpha = 0.0f;
                LobbyMenu.interactable = false;
                LobbyMenu.blocksRaycasts = false;
                RoomMenu.alpha = 1.0f;
                RoomMenu.interactable = true;
                RoomMenu.blocksRaycasts = true;
                for(int x=0; x<PhotonNetwork.CurrentRoom.MaxPlayers; x++)
                {
                    MultiplayerText[x].text = " ";
                    MultiplayerStatus[x].text = "NOT PRESENT";
                }
                playerIndex = 0;
                foreach (Player human in PhotonNetwork.PlayerList)
                {
                    if (human.NickName == PhotonNetwork.NickName)
                    {
                        MultiplayerText[playerIndex].text = human.NickName + "(YOU)";
                    }
                    MultiplayerText[playerIndex].text = human.NickName;
                    MultiplayerStatus[playerIndex].text = "PRESENT";
                    playerIndex++;
                }
            }
        }
        else
        {
            LocalPlayerText.enabled = false;
            ConnectingText.enabled = true;
            LobbyMenu.alpha = 0.0f;
            LobbyMenu.interactable = false;
            LobbyMenu.blocksRaycasts = false;
            RoomMenu.alpha = 0.0f;
            RoomMenu.interactable = false;
            RoomMenu.blocksRaycasts = false;
        }
    }

    public void UpdateInputCID()
    {
        CreateRoomID = CreateIDEntry.text;
    }

    public void UpdateInputJID()
    {
        JoinRoomID = JoinIDEntry.text;
    }

    public void ConfirmNameChange()
    {
        username = NewNameText.text;
        PhotonNetwork.NickName = username;
    }

    public void RoomMake()
    {
        CurrentRoomName = "Room ID: " + CreateRoomID;
        Debug.Log("Generating room: " + CurrentRoomName);
        PhotonNetwork.CreateRoom(CurrentRoomName, MultiplayerOptions);
    }

    public void RoomJoin()
    {
        CurrentRoomName = "Room ID: " + JoinRoomID;
        PhotonNetwork.JoinRoom(CurrentRoomName);
    }

    public void RandomRoomMake()
    {
        RoomID = Random.Range(0, 100000);
        CurrentRoomName = "Room ID: " + RoomID;
        Debug.Log("Generating room: "+ CurrentRoomName);
        PhotonNetwork.CreateRoom(CurrentRoomName, MultiplayerOptions);
    }

    public void RoomScan()
    {
        Debug.Log("Looking for rooms...");
        PhotonNetwork.JoinRandomRoom();
    }

    public void RoomExit()
    {
        PhotonNetwork.LeaveRoom();
        CurrentRoomName = "";
    }

    //From Photon.Pun
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
        if (message == "A game with the specified id already exist.")
        {
            LobbyMessage.text = string.Format("[{0}] There is a room like that! Can you please try joining THAT room?", message);
            Debug.LogError(LobbyMessage.text);
        }
        else
        {
            LobbyMessage.text = string.Format("[{0}] Unknown error occurred. Please try again.", message);
            Debug.LogError(LobbyMessage.text);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("You're in: " + PhotonNetwork.CurrentRoom.Name + "[" + PhotonNetwork.CurrentRoom.PlayerCount + "/" + PhotonNetwork.CurrentRoom.MaxPlayers + "]");
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        if(message == "Game does not exist")
        {
            LobbyMessage.text = string.Format("[{0}] Where is that room? We don't have that yet! Can you please try creating THAT room instead?", message);
            Debug.LogError(LobbyMessage.text);
        }
        else
        {
            LobbyMessage.text = string.Format("[{0}] Unknown error occurred. Please try again.", message);
            Debug.LogError(LobbyMessage.text);
        }
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        if (message == "No match found")
        {
            LobbyMessage.text = string.Format("[{0}] Where is that room? We don't have that yet! Can you please try creating THAT room instead?", message);
            Debug.LogError(LobbyMessage.text);
        }
        else
        {
            LobbyMessage.text = string.Format("[{0}] Unknown error occurred. Please try again.", message);
            Debug.LogError(LobbyMessage.text);
        }
    }
}
