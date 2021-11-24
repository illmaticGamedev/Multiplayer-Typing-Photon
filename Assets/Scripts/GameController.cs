using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    [TextArea(1,20)]
    [SerializeField] private string[] paragraphs;
    [SerializeField] private TMP_Text textToType;
    [SerializeField] private List<string> wordsInCurrentParagraph = new List<string>();
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private Slider mySliderScore;
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private int currentWordCheckingIndex = 0;
    [SerializeField] private TMP_Text highlightedWord;
    [SerializeField] private TMP_Text currentWPMText;
    [SerializeField] private Button restartGameBtn;
    [SerializeField] int initialWordCount;
    [SerializeField] int wordsCompleted;
    [SerializeField] private float currentPercentage;

    [SerializeField] private Color correctTextColorWhenTyping;
    [SerializeField] private Color wrongTextColorWhenTyping;
    private bool typingStarted = false;
    private float timeCounter;
    private float currentWPM = 0;
    private void Start()
    {
        ChooseRandomParagraph();
        restartGameBtn.onClick.AddListener((() => Application.LoadLevel(Application.loadedLevel)));
    }

    void ChooseRandomParagraph()
    {
        int randomIndex = UnityEngine.Random.Range(0,paragraphs.Length);
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
            //Start Timer
            if (typingStarted == false)
            {
                typingStarted = true;
                InvokeRepeating(nameof(IncreaseTimeBySecond),1,1);
            }
            
            // Check if the its the right word
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
            
            
            //Check if user is mispelling the word
            if (wordsInCurrentParagraph[currentWordCheckingIndex].Contains(inputField.text))
            {
                inputField.textComponent.color = correctTextColorWhenTyping;
            }
            else
            {
                inputField.textComponent.color = wrongTextColorWhenTyping;
            }
        }

        UpdateSliderScore();
        CalculateWPM();
    }

    void ExtractWords(string tempTextToTypeCopy)
    {
        string currentWord = "";
        foreach (var item in tempTextToTypeCopy)
        {
            if (item != ' ')
            {
                currentWord += item;
                
                if (item == tempTextToTypeCopy[tempTextToTypeCopy.Length-1] && currentWord != "")
                {
                    currentWord = RemoveOrReplaceWeirdCharactersInWord(currentWord);
                    wordsInCurrentParagraph.Add(currentWord + " ");
                    currentWord = "";
                }
            }
            else
            {
                if (currentWord != "")
                {
                    currentWord = RemoveOrReplaceWeirdCharactersInWord(currentWord);
                    wordsInCurrentParagraph.Add(currentWord+" ");
                    currentWord = "";
                }
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

    void CalculateWPM()
    {
        if (wordsCompleted != 0 && timeCounter != 0)
        {
            float secondsToMinute = timeCounter / 60;
            currentWPM = wordsCompleted / secondsToMinute;
            currentWPMText.text = (int)currentWPM + " WPM";
        }
    }

    void IncreaseTimeBySecond()
    {
        timeCounter += 1;
    }

    
    // Not sure what the fuck is happening here right now but will figure it out later
    string RemoveOrReplaceWeirdCharactersInWord(string word)
    {
        if (word.Contains("’"))
        {
          word =  word.Replace("’", "'");
        }
        
        // if (word.Contains("“"))
        // {
        //     word =  word.Replace("“", "\"");
        //     word =  word.Replace("\"", "\"");
        //     
        // }
        return word;
    }
}
