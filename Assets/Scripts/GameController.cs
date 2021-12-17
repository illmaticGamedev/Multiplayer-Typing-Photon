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
    public int restartingSeconds;
    
    [Header("Paragraph Related Objects")]
    [TextArea(1,20)]
    [SerializeField] private string[] paragraphs;
    [SerializeField] private TMP_Text textToType;
    [SerializeField] private List<string> wordsInCurrentParagraph = new List<string>();
    [SerializeField] private TMP_InputField inputField;
    [SerializeField] private TMP_Text highlightedWord;
    [SerializeField] private GameObject paragraphWritingAnimatingBar;
    [Header("Game Status/Properties")]
    private bool typingStarted = false;
    [SerializeField] private bool countdownStarted = false;
    [SerializeField] private bool gameStarted = false;
    [SerializeField] private float currentPercentage;
    private int currentWordCheckingIndex = 0;
    private int initialWordCount;
    private int wordsCompleted;
    private float timeCounter;
    private float currentWPM = 0;
    private int waitingTimePassed = 0;
    
    [Header("Reference Data")]
    [SerializeField] private Color correctTextColorWhenTyping;
    [SerializeField] private Color wrongTextColorWhenTyping;

    [Header("Player Data")]
    [SerializeField] public PlayerData myPlayer;
    [SerializeField] public TMP_InputField playerNameTextInputText;
    [SerializeField] private GameObject playerNameInputAnimatingBar;
    [SerializeField] public string myPlayerName;

    [Header("AllPanels")]
    [SerializeField] public GameObject waitingPanel;

    [Header("Win/Lose Panel Setup")] 
    [SerializeField] GameObject winLosePanelParent;
    [SerializeField] GameObject winPanel;
    [SerializeField] private TMP_Text winPanelStatus;
    [SerializeField] GameObject losePanel;
   // [SerializeField] private TMP_Text loseNumberText;
    [SerializeField] private TMP_Text losePanelStatus;
    [SerializeField] private TMP_Text restartingInSecondsText;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private AudioClip keyboardClip;

    private int randomParagraphIndexSelectedByMaster;


    [Header("BUG ISSUE - CLEARING POSSIBLE CACHE FROM ALL INPUT OBJECTS ")]
    [SerializeField] TMP_InputField bugFixInputFieldMain;
    [SerializeField] TMP_Text bugFixInputFieldChildTextObject;
    private void Awake()
    {
        Instance = this;
        inputField.interactable = false;
    }

    private void ClearPossibleCacheFromInputFields()
    {
        bugFixInputFieldMain.text = "";
        bugFixInputFieldChildTextObject.text = "";
        bugFixInputFieldChildTextObject.ClearMesh();
        inputField.interactable = true;
        inputField.Select();
    }
    
    public void typeAnimatingBarState(bool state)
    {
        paragraphWritingAnimatingBar.SetActive(state);
        playerNameInputAnimatingBar.SetActive(state);
    }
    private void Start()
    {
        _audioSource.clip = keyboardClip;
        playerNameTextInputText.Select();
    }
   public void ChooseRandomParagraph(int index)
    {
        textToType.text = paragraphs[index];
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
                InvokeRepeating(nameof(IncreaseTimeBySecondAndUpdateValues), 1, 1);
            }

            // Check if the its the right word
            if (wordsInCurrentParagraph.Count > 0 &&
                inputField.text == wordsInCurrentParagraph[currentWordCheckingIndex])
            {
                wordsInCurrentParagraph.Remove(inputField.text);

                if (textToType.text.Length == inputField.text.Length - 1)
                {
                    textToType.text = textToType.text.Remove(0, inputField.text.Length - 1);

                    currentPercentage = 100;
                }
                else
                {
                    textToType.text = textToType.text.Remove(0, inputField.text.Length);
                }

                inputField.text = "";
                wordsCompleted += 1;
                Debug.Log(wordsCompleted + " : " + initialWordCount);
                if (wordsCompleted == initialWordCount)
                {
                    if (ConnectToServer.Instance.CheckIfAnyoneElseCompletedParagraph())
                    {
                        ThrowWinLosePanel(false);
                    }
                    else
                    {
                        ThrowWinLosePanel(true);
                    }
                    
                    winLosePanelParent.SetActive(true);
                    myPlayer.pw.RPC("UpdateValues",RpcTarget.All,(int) currentWPM, 100);
                    myPlayer.pw.RPC("ParagraphCompleted", RpcTarget.All);
                }
            }


            //Check if user is mispelling the word
            if (wordsInCurrentParagraph.Count > 0 &&
                wordsInCurrentParagraph[currentWordCheckingIndex].Contains(inputField.text))
            {
                inputField.textComponent.color = correctTextColorWhenTyping;
            }
            else
            {
                inputField.textComponent.color = wrongTextColorWhenTyping;
            }
        }
        else
        {
            inputField.text = "";
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
        if (countdownStarted == false && gameStarted == false)
        {
            countdownStarted = true;
            waitingPanel.SetActive(true);
            InvokeRepeating(nameof(AddSeconds),0,1);
        }
    }
    public void waitingPanelTextChange(string text)
    {
        waitingPanel.GetComponentInChildren<TMP_Text>().text = text;
    }
    public void StopGameCountDown()
    {
        countdownStarted = false;
        gameStarted = false;
        waitingPanel.gameObject.SetActive(false);
        PhotonNetwork.CurrentRoom.IsOpen = true;
        waitingTimePassed = 0;
        CancelInvoke(nameof(AddSeconds));
    }
    public void AddSeconds()
    {
        waitingTimePassed += 1;
        waitingTimePassed = Mathf.Clamp(waitingTimePassed,0, waitingTimeAfterMinimumPlayers);
        waitingPanelTextChange("Waiting.... " + (waitingTimeAfterMinimumPlayers - waitingTimePassed) + "s");
        if (waitingTimePassed == waitingTimeAfterMinimumPlayers)
        {
            PhotonNetwork.CurrentRoom.IsOpen = false;
            gameStarted = true;
            waitingPanel.gameObject.SetActive(false);
            ClearPossibleCacheFromInputFields();
            CancelInvoke(nameof(AddSeconds));
        }
    }

    #region ThrowWinLosePanel
    public void ThrowWinLosePanel(bool gameWon)
    {
        if (gameWon)
        {
            winPanelStatus.text = "You had the highest WPM of " + (int)currentWPM + ".";
            winPanel.gameObject.SetActive(true);
        }
        else
        {
            losePanelStatus.text = "Your WPM :  " + (int)currentWPM + ".";
            //loseNumberText.text = "";
            losePanel.gameObject.SetActive(true); 
        }
        restartingInSecondsText.text = "Restarting In " + restartingSeconds + "...";
        
        InvokeRepeating(nameof(RestartingInSecondsText),0f,1f);
    }

    void RestartingInSecondsText()
    {
        restartingInSecondsText.text = "Restarting In " + restartingSeconds + "...";
        restartingSeconds -= 1;
        
        if (restartingSeconds == 0)
        {
            HardRestartGame();
        }
    }

    public void HardRestartGame()
    {
        PhotonNetwork.Disconnect();
        Application.LoadLevel(Application.loadedLevel);
    }
   #endregion
   
    public void PlayClickSound()
   {
       _audioSource.Play();
   }

    public void SelectRandomParagraph()
   {
       if (PhotonNetwork.IsMasterClient)
       {
           randomParagraphIndexSelectedByMaster = UnityEngine.Random.Range(0,paragraphs.Length);
           ConnectToServer.Instance.photonView.RPC("SyncParagraphsFromMaster",RpcTarget.AllBuffered,randomParagraphIndexSelectedByMaster);
       }
   }
}
