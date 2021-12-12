using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameController : MonoBehaviour
{
    public static GameController Instance;

    [Header("Game Settings")] 
    [SerializeField] int waitingTimeAfterMinimumPlayers;
    public int minimumPlayerToPlay;
    
    [TextArea(1,20)]
    [SerializeField] private string[] paragraphs;
    [SerializeField] private TMP_Text textToType;
    [SerializeField] private List<string> wordsInCurrentParagraph = new List<string>();
    [SerializeField] private TMP_InputField inputField;
   
    [SerializeField] private bool countdownStarted = false;
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

    [SerializeField] public PlayerData myPlayer;
    [SerializeField] public TMP_InputField playerNameTextInputText;
    [SerializeField] public string myPlayerName;


    [SerializeField] private GameObject waitingPanel;


    
    private bool typingStarted = false;
    private float timeCounter;
    private float currentWPM = 0;
    public int currentWaitingTime = 0;

    private void Awake()
    {
        Instance = this;
    }

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
    public void UpdatePlayerName()
    {
        myPlayerName = playerNameTextInputText.text;
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
                InvokeRepeating(nameof(IncreaseTimeBySecondAndUpdateValues),1,1);
            }
            
            // Check if the its the right word
            if (inputField.text== wordsInCurrentParagraph[currentWordCheckingIndex])
            {
                wordsInCurrentParagraph.Remove(inputField.text);

                if (textToType.text.Length == inputField.text.Length - 1)
                {
                    textToType.text = textToType.text.Remove(0,inputField.text.Length-1);
                    
                    currentPercentage = 100;
                  //  mySliderScore.value = currentPercentage;
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

       // UpdateSliderScore();
       // CalculateWPM();

       // if (myPlayer != null)
       // {
       //     myPlayer.pw.RPC("UpdateValues",RpcTarget.AllBuffered,(int)currentWPM,(int)currentPercentage);
       // }
          
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
        //mySliderScore.value = currentPercentage;
    }
    void CalculateWPM()
    {
        if (wordsCompleted != 0 && timeCounter != 0)
        {
            float secondsToMinute = timeCounter / 60;
            currentWPM = wordsCompleted / secondsToMinute;
           // currentWPMText.text = (int)currentWPM + " WPM";
            
            if(myPlayer != null)
                myPlayer.currentWPM = (int) currentWPM;
        }
    }
    void IncreaseTimeBySecondAndUpdateValues()
    {
        timeCounter += 1;
        UpdateSliderScore();
        CalculateWPM();
        if (myPlayer != null)
        {
            myPlayer.pw.RPC("UpdateValues",RpcTarget.AllBuffered,(int)currentWPM,(int)currentPercentage);
        }
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
    public void StartGameAfterAllPlayerJoin()
    {
        if (countdownStarted == false)
        {
            countdownStarted = true;
            waitingPanel.gameObject.SetActive(true);
            InvokeRepeating(nameof(AddSeconds),1,1);
        }
    }
    public void StopGameCountDown()
    {
        countdownStarted = false;
        gameStarted = false;
        waitingPanel.gameObject.SetActive(false);
        PhotonNetwork.CurrentRoom.IsOpen = true;
        currentWaitingTime = 0;
        CancelInvoke(nameof(AddSeconds));
    }
    void AddSeconds()
    {
        currentWaitingTime += 1;
        currentWaitingTime = Mathf.Clamp(currentWaitingTime,0, waitingTimeAfterMinimumPlayers);
        waitingPanel.GetComponentInChildren<TMP_Text>().text = "Waiting.... " + (waitingTimeAfterMinimumPlayers - currentWaitingTime) + "s";
        
        if (currentWaitingTime == waitingTimeAfterMinimumPlayers)
        {
            gameStarted = true;
            PhotonNetwork.CurrentRoom.IsOpen = false;
            waitingPanel.gameObject.SetActive(false);
        }
    }
   
    
    
}
