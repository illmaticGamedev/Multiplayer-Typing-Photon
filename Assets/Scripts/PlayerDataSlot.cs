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
    public bool isMine = false;
    public bool paragraphCompleted = false;
    [SerializeField] private Image WPMBackground;
    [SerializeField] private Sprite GreenWPMBackground;

    private void Start()
    {
        UpdateValues(0,0);
    }

    public void SetColorForMyPlayer(Color color)
    {
        playerNameText.color = color;
        sliderValue.fillRect.GetComponent<Image>().color = new Color(0.65f, 0.9f, 0.34f);
        WPMBackground.sprite = GreenWPMBackground;
    }

    public void UpdateValues(int wpm, int slider)
    {
        if (paragraphCompleted == false)
        {
            sliderValue.value = slider;
            playerWPM.text = wpm + " WPM";
        }
    }
}
