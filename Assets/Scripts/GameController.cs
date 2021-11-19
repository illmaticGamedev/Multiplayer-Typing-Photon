using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [SerializeField] private string[] paragraphs;
    [SerializeField] private TMP_Text textToType;
    [SerializeField] private List<string> wordsInCurrentParagraph = new List<string>();
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Slider mySliderScore;
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private int currentWordCheckingIndex = 0;
    private void Start()
    {
        ChooseRandomParagraph();
    }

    void ChooseRandomParagraph()
    {
        int randomIndex = UnityEngine.Random.Range(0,wordsInCurrentParagraph.Count);
        textToType.text = paragraphs[randomIndex];
        string tempTextToTypeCopy = textToType.text;
        
        ExtractWords(tempTextToTypeCopy);
    }


    public void OnInputTextChange()
    {
        if (gameStarted)
        {
            if (inputField.text == wordsInCurrentParagraph[currentWordCheckingIndex])
            {
                wordsInCurrentParagraph.Remove(inputField.text);
                inputField.text = "";
            }
        }
    }

    void ExtractWords(string tempTextToTypeCopy)
    {
        string currentWord = "";
        foreach (var item in tempTextToTypeCopy)
        {
            if (item != ' ')
            {
                currentWord += item;

                if (item == tempTextToTypeCopy[tempTextToTypeCopy.Length-1])
                {
                    wordsInCurrentParagraph.Add(currentWord);
                    currentWord = "";
                }
            }
            else
            {
                wordsInCurrentParagraph.Add(currentWord);
                currentWord = "";
            }
        }
    }
}
