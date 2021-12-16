using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Realtime;
using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using TMPro;
using UnityEngine.UI;
using Random = Unity.Mathematics.Random;

public class ConnectToServer : MonoBehaviourPunCallbacks
{
    public static ConnectToServer Instance;
    
    public TMP_InputField usernameInput;
    public TMP_Text playButtonText;

    [SerializeField] private GameObject preGamePanel;
    [SerializeField] private GameObject gamePanel;

    public List<PlayerDataSlot> playerList = new List<PlayerDataSlot>();
    [SerializeField] private GameObject playerPrefab;

    private PlayerData newPlayerData;
    private PhotonView myPV;
    private int numOfPlayersFinished;
    
    [Header("Private Panel")] 
    [SerializeField] TMP_InputField inputFieldLobbyName;
    [SerializeField] private GameObject privateGamePanel;
    
    private void Start()
    {
        Instance = this;
        myPV = GetComponent<PhotonView>();
    }

    public void OnClickPlay()
    {
        if (usernameInput.text != "")
        {
            PhotonNetwork.NickName = usernameInput.text;
            FindObjectOfType<GameController>().UpdatePlayerName();
            playButtonText.text = "Connecting....";
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
        //playButtonText.text = "QUICK PLAY";
        if (inputFieldLobbyName.text.Contains("_privateGame") == false && inputFieldLobbyName.text == "")
        {
            PhotonNetwork.CreateRoom(usernameInput.text + "'s Lobby" + UnityEngine.Random.Range(0,5000), new RoomOptions() {MaxPlayers = 4, IsOpen = true}, null);
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
        playButtonText.text = "QUICK PLAY";
        GameController.Instance.waitingPanelTextChange("Waiting for other players... ");
        GameController.Instance.waitingPanel.SetActive(true);
        GameController.Instance.SelectRandomParagraph();
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        playButtonText.text = "QUICK PLAY";
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

    [PunRPC]
    public void SyncParagraphsFromMaster(int Index)
    {
        GameController.Instance.ChooseRandomParagraph(Index);
    }
    
    public PlayerDataSlot GetFreePlayerDataSlot()
    {
        foreach (var item in playerList)
        {
            if (item.isTaken == false)
            {
                item.isTaken = true;
                item.gameObject.SetActive(true);
                return item;
            }
        }
        
        Debug.LogError("PlayerDataSlot Null");
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
    public bool CheckIfAnyoneElseCompletedParagraph()
    {
        foreach (var item in playerList)
        {
            if (item.paragraphCompleted == true && item.isMine == false)
            {
                return true;
            }
        }

        return false;
    }
}