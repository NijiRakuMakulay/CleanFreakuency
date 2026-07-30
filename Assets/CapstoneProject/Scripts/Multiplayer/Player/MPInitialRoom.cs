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
    [SerializeField] Transform[] ItemPos;
    [SerializeField] GameObject[] ItemPrefabList;
    [SerializeField] int[] ItemToSpawn;
    [SerializeField] Transform[] DoorPos;
    [SerializeField] GameObject[] DoorPrefabList;
    [SerializeField] int[] DoorToSpawn;
    [SerializeField] GameObject CartPrefab;
    [SerializeField] Transform CartSpawnPos;
    [SerializeField] GameObject[] Enemies;
    [SerializeField] Transform[] EnemySpawnPos;
    [SerializeField] Transform[] EnemyRoamPos;
    PhotonView pv;
    int playerID;

    void Awake()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NetworkingClient.LoadBalancingPeer.DisconnectTimeout = 30000;
        PhotonNetwork.KeepAliveInBackground = 240.0f;
        pv = GetComponent<PhotonView>();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (PhotonNetwork.IsConnected)
        {
            Debug.Log("Welcome!");
            if (PhotonNetwork.InRoom)
            {
                if (PhotonNetwork.IsMasterClient) { InitialLevelLoad(); }
                Debug.Log(string.Format("Your game has started in room {0}!", PhotonNetwork.CurrentRoom.Name));
                StartCoroutine(DelaySpawn());
            }
            else
            {
                Debug.Log("This scene will only work when connected to Photon Network.");
            }
        }
    }
    void InitialLevelLoad()
    {
        int itemIndex = 0;
        int doorIndex = 0;
        int enemyIndex = 0;

        Debug.Log($"Spawning cart...");
        PhotonNetwork.InstantiateRoomObject(CartPrefab.name, CartSpawnPos.position, CartSpawnPos.rotation);

        foreach (int ItemType in ItemToSpawn)
        {
            Debug.Log($"Spawning items...{itemIndex}");
            PhotonNetwork.InstantiateRoomObject(ItemPrefabList[ItemType].name, ItemPos[itemIndex].position, ItemPos[itemIndex].rotation);
            itemIndex++;
        }

        foreach (int doorType in DoorToSpawn)
        {
            Debug.Log($"Spawning doors...{doorType}");
            PhotonNetwork.InstantiateRoomObject(DoorPrefabList[doorType].name, DoorPos[doorIndex].position, DoorPos[doorIndex].rotation);
            doorIndex++;
        }

        foreach (GameObject enemy in Enemies)
        {
            GameObject lightbulb;
            Debug.Log($"Spawning enemies...{enemyIndex}");
            lightbulb = PhotonNetwork.InstantiateRoomObject(enemy.name, EnemySpawnPos[enemyIndex].position, EnemySpawnPos[enemyIndex].rotation);
            lightbulb.GetComponent<EnemyRoamNavigator>().roamCenter = EnemyRoamPos[enemyIndex];
            enemyIndex++;
        }
    }

    //Initial Player goes first...
    IEnumerator DelaySpawn()
    {
        yield return new WaitForSeconds(0.5f); // wait 0.5 seconds
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
