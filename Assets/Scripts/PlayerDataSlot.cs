using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerDataSlot : MonoBehaviour
{

    public TMP_Text playerNameText;
    public TMP_Text playerWPM;
    public Slider sliderValue;
    public bool isTaken = false;

    public void SetColorForName(Color color)
    {
        playerNameText.color = color;
    }

    public void UpdateValues(int wpm, int slider)
    {
        sliderValue.value = slider;
        playerWPM.text = wpm + " WPM";
    }
}
