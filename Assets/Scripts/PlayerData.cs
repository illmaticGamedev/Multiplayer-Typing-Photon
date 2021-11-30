using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerData : MonoBehaviour
{
    public int currentWPM;
    public string playerName;
    public int progressValue;
    
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private Slider progressValueText;
    [SerializeField] private TMP_Text currentWPMText;
    private void Start()
    {
        // if isLocal
        if (true)
        {
            FindObjectOfType<GameController>().myPlayer = this;
            FindObjectOfType<GameController>().playerNameTextInputText.text = playerName;
            playerNameText.text = playerName;
            
        }
    }
    
    public void UpdateValues(int newCurrentWPM, int newProgressValue)
    {
        currentWPMText.text = newCurrentWPM.ToString() + " WPM";
        progressValueText.value = newProgressValue;

        currentWPM = newCurrentWPM;
        progressValue = newProgressValue;
    }
}
