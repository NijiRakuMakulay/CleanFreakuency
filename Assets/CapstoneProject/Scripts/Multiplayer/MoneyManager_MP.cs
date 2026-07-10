using Photon.Pun;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class MoneyManager_MP : MonoBehaviourPunCallbacks, IPunObservable
{
    public static MoneyManager_MP Instance;
    PhotonView pv;
    int currentMoney = 0;
    int netMoney = 0;

    public TextMeshProUGUI[] moneyText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        pv = GetComponent<PhotonView>();
        UpdateUI();
    }

    void Update()
    {
        if (PhotonNetwork.IsConnected)
        {
            if (PhotonNetwork.InRoom) { UpdateUI(); }
        }
        
    }

    public void AddMoney(int amount)
    {
        if (pv.IsMine)
        {
            currentMoney += amount;
            UpdateUI();
        }
    }

    public bool SpendMoney(int amount)
    {
        if (currentMoney >= amount)
        {
            currentMoney -= amount;

            UpdateUI();

            return true;
        }
        return false;
    }

    void UpdateUI()
    {
        if (moneyText != null)
        {
            moneyText[0].text = "₱ " + currentMoney;
            moneyText[1].text = "₱ " + netMoney;
            moneyText[2].text = "₱ " + netMoney;
            moneyText[3].text = "₱ " + netMoney;
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            //Local Player -> Send Data
            stream.SendNext(currentMoney);
        }
        else if (stream.IsReading)
        {
            //Remote Player -> Receive Data
            netMoney = (int)stream.ReceiveNext();
        }
    }
}