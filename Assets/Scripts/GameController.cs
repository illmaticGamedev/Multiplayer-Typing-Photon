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
    [SerializeField] private TMP_Text highlightedWord;
    
    
    [SerializeField] int initialWordCount;
    [SerializeField] int wordsCompleted;
    [SerializeField] private float currentPercentage;
    private void Start()
    {
        ChooseRandomParagraph();
    }

    void ChooseRandomParagraph()
    {
        int randomIndex = UnityEngine.Random.Range(0,wordsInCurrentParagraph.Count);
        textToType.text = paragraphs[randomIndex];
        string tempTextToTypeCopy = textToType.text;
        wordsCompleted = 0;
        ExtractWords(tempTextToTypeCopy);
        UpdateSliderScore();
    }


    public void OnInputTextChange()
    {
        RecheckHighlightedWord();
        
        if (gameStarted)
        {
            if (inputField.text== wordsInCurrentParagraph[currentWordCheckingIndex])
            {
                wordsInCurrentParagraph.Remove(inputField.text);

                if (textToType.text.Length == inputField.text.Length - 1)
                {
                    textToType.text = textToType.text.Remove(0,inputField.text.Length-1);
                    
                    currentPercentage = 100;
                    mySliderScore.value = currentPercentage;
                }
                else
                {
                    textToType.text = textToType.text.Remove(0,inputField.text.Length);
                }

                inputField.text = "";
                wordsCompleted += 1;
            }
        }

        UpdateSliderScore();
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
                    wordsInCurrentParagraph.Add(currentWord + " ");
                    currentWord = "";
                }
            }
            else
            {
                wordsInCurrentParagraph.Add(currentWord+" ");
                currentWord = "";
            }
        }

        initialWordCount = wordsInCurrentParagraph.Count;
        RecheckHighlightedWord();
    }


    void RecheckHighlightedWord()
    {
        if (textToType.text == "")
        {
            highlightedWord.text = "";
        }
        else
        {
            highlightedWord.text = wordsInCurrentParagraph[0]; 
        }
    }

    void UpdateSliderScore()
    {
        if (currentPercentage < 100)
        {
            currentPercentage  = (wordsCompleted * 100)/ initialWordCount;
        }
        mySliderScore.value = currentPercentage;
    }
}
