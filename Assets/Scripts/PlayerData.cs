using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public int currentWPM;
    public string playerName;
    public int progressValue;
    public PlayerDataSlot myDataSlot;

    public PhotonView pw;
    
    private void Start()
    {
        pw = GetComponent<PhotonView>();
        // if isLocal
        if (pw.IsMine)
        {
            FindObjectOfType<GameController>().myPlayer = this;
            playerName = FindObjectOfType<GameController>().myPlayerName;
            Debug.Log("Player Name Changed");
            pw.RPC("SyncPlayerSlots",RpcTarget.AllBuffered,PhotonNetwork.NickName);
            myDataSlot.SetColorForName(Color.green);
        }
    }
    
    [PunRPC]
    public void UpdateValues(int newCurrentWPM, int newProgressValue)
    {
        currentWPM = newCurrentWPM;
        progressValue = newProgressValue;

        if (myDataSlot)
        {
            myDataSlot.UpdateValues(currentWPM,progressValue);
        }
    }

    [PunRPC]
    public void SyncPlayerSlots(string newPlayerName)
    {
        myDataSlot = FindObjectOfType<ConnectToServer>().GetFreePlayerDataSlot();
        myDataSlot.playerNameText.text = newPlayerName;
    }

 
    
}
