using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using TMPro;
using UnityEngine.UI;
public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public TMP_InputField usernameInput;
    public TMP_Text buttonText;

    [SerializeField] private GameObject preGamePanel;
    [SerializeField] private GameObject gamePanel;


    public List<Transform> playerPosList = new List<Transform>();
    [SerializeField] private GameObject playerPrefab;
    public void OnClickQuickPlay()
    {
        if (usernameInput.text != "")
        {
            PhotonNetwork.NickName = usernameInput.text;
            buttonText.text = "Connecting....";
            PhotonNetwork.ConnectUsingSettings();
        }
    }

    public override void OnConnectedToMaster()
    {
        AfterConnectedToMaster();
    }

    public void AfterConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
        Debug.Log("Connected To Master..");
    }

    public override void OnJoinedLobby()
    {
        PhotonNetwork.JoinRandomRoom();
        Debug.Log("Joined A Lobby..");
    }

    public override void OnJoinRandomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        Debug.Log("Join Room Failed - " + message);
        PhotonNetwork.CreateRoom(usernameInput.text + "'s Lobby", new RoomOptions() {MaxPlayers = 4},null);
    }
    
    public override void OnJoinedRoom()
    {
        Debug.Log("Joined A Room..");
        //Turn off pre game screen
        preGamePanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(true);
        UpdatePlayerList(PhotonNetwork.LocalPlayer);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created A Room..");
        //Turn off pre game screen
        preGamePanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(true);
        UpdatePlayerList(PhotonNetwork.LocalPlayer);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList(newPlayer);
    }

    public void UpdatePlayerList(Player newPlayer)
    {
        foreach (var item in playerPosList)
        {
            if (item.childCount == 0)
            {
                GameObject newPlayerPrefab = Instantiate(playerPrefab, item.transform);
                newPlayerPrefab.GetComponent<PlayerData>().playerName = newPlayer.NickName;
                break;
            }
        }
    }
}
