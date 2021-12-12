using System;
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


    public List<PlayerDataSlot> playerPosList = new List<PlayerDataSlot>();
    [SerializeField] private GameObject playerPrefab;

    private PlayerData newPlayerData;
    private PhotonView myPV;
    
    [Header("Private Panel")] 
    [SerializeField] TMP_InputField inputFieldLobbyName;
    [SerializeField] private GameObject privateGamePanel;

    private void Start()
    {
        myPV = GetComponent<PhotonView>();
    }

    public void OnClickPlay()
    {
        if (usernameInput.text != "")
        {
            PhotonNetwork.NickName = usernameInput.text;
            GameObject.FindObjectOfType<GameController>().UpdatePlayerName();
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
        if (inputFieldLobbyName.text.Contains("_privateGame") == false && inputFieldLobbyName.text == "")
        {
            PhotonNetwork.CreateRoom(usernameInput.text + "'s Lobby", new RoomOptions() {MaxPlayers = 4}, null);
        }
    }
    

    public override void OnJoinedRoom()
    {
        Debug.Log("Joined A Room..");
        //Turn off pre game screen
        preGamePanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(true);
        privateGamePanelState(false);

        GameObject newPlayerObject =
            PhotonNetwork.Instantiate(playerPrefab.name, playerPrefab.transform.position, Quaternion.identity);
        newPlayerData = newPlayerObject.GetComponent<PlayerData>();
        PhotonView newPlayerPV = newPlayerObject.GetComponent<PhotonView>();
        //newPlayerPV.RPC("SyncPlayerSlots",RpcTarget.AllBuffered);
    }

    public override void OnCreatedRoom()
    {
        Debug.Log("Created A Room..");
        //Turn off pre game screen
        preGamePanel.gameObject.SetActive(false);
        gamePanel.gameObject.SetActive(true);
        privateGamePanelState(false);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Debug.Log("New Player Entered The Room..");
        // newlyJoinedPlayer = newPlayer;

        if (PhotonNetwork.IsMasterClient)
        {
            myPV.RPC(nameof(CheckAndStartGameIfEnoughPlayersAvailable), RpcTarget.AllBuffered);
        }
    }


    [PunRPC]
    public void CheckAndStartGameIfEnoughPlayersAvailable()
    {
        if (PhotonNetwork.PlayerList.Length >= GameController.Instance.minimumPlayerToPlay)
        {
            FindObjectOfType<GameController>().StopGameCountDown();
            FindObjectOfType<GameController>().StartGameAfterAllPlayerJoin();
        }
        else
        {
            FindObjectOfType<GameController>().StopGameCountDown();
        }
    }

    public PlayerDataSlot GetFreePlayerDataSlot()
    {
        foreach (var item in playerPosList)
        {
            if (item.isTaken == false)
            {
                item.isTaken = true;
                item.gameObject.SetActive(true);
                return item;
            }
        }

        return null;
    }
    public void OnClickHostPrivate()
    {
        if (usernameInput.text != "")
        {
            OnClickPlay();
            PhotonNetwork.CreateRoom(inputFieldLobbyName.text + "_privateGame", new RoomOptions() {MaxPlayers = 4, IsVisible = false},typedLobby: null);
        }
    }
    
    public void OnClickJoinPrivate()
    {
        if (usernameInput.text != "")
        {
            OnClickPlay();
            PhotonNetwork.JoinRoom(inputFieldLobbyName.text);
        } 
    }
    public void privateGamePanelState(bool state)
    {
        privateGamePanel.SetActive(state);
        inputFieldLobbyName.text = "";
    }
}