using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
using UnityEngine.UI;

public class GameLogicHelper : MonoBehaviour
{
    
    public string nextLevel = "SampleScene";
    public string reloadLevel = "SampleScene";

    public GameObject pauseMessageTitle;
    public GameObject pauseMessage;
    public GameObject pauseMessageEsc;

    public GameObject gameMessageTitle;
    public GameObject gameMessage;

    private TextMeshProUGUI pauseMessageTitleText;
    private TextMeshProUGUI pauseMessageText;
    private TextMeshProUGUI gameMessageTitleText;
    private TextMeshProUGUI gameMessageText;
   
    public GameObject playerObject;
    private float messageDisplayTime = -999f;
    private float messageEraseTime = 1f;
    
    public GameObject playerShip;
    
    public GameObject sliderObject;
    public GameObject sliderTextObject;
    private Slider slider;
    private TextMeshProUGUI sliderText;

    PlayerShip ship;

    Player player;
    private bool showedDeath = false; 
    private bool showedPass = false; 
    private float restartTime = 999999f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    //
    bool triedDebug = false;
    void Start()
    {
        player = playerObject.GetComponent<Player>();
        pauseMessageTitleText = pauseMessageTitle.GetComponent<TextMeshProUGUI>();
        pauseMessageText = pauseMessage.GetComponent<TextMeshProUGUI>();
        gameMessageTitleText = gameMessageTitle.GetComponent<TextMeshProUGUI>();
        gameMessageText = gameMessage.GetComponent<TextMeshProUGUI>();
        ship = playerShip.GetComponent<PlayerShip>();
        slider = sliderObject.GetComponent<Slider>();
        sliderText = sliderTextObject.GetComponent<TextMeshProUGUI>();

    }

    // Update is called once per frame
    void Update()
    {
        if (Time.time > messageEraseTime)
        {
            HideGameMessage();
        }
        if (player.curPauseState == Player.PauseState.NONE)
        {
           HidePauseMessage(); 
        }

        if(Time.time > restartTime && showedDeath)
        {
            RestartLevel();
        }
        else if(Time.time > restartTime && showedPass)
        {
            GoToNextLevel();
        }
        if (ship.curHealth <= 0 && !showedDeath)
        {
            ShowFail("DEATH NOTICE", "You died, Press [Enter] to try again!");
        }
    }
    
    public void ShowFail(string title, string message)
    {
        DisplayPauseMessage(title,message);
        restartTime = Time.time + 0.2f;
        showedDeath = true;
    }

    public void ShowPass(string title, string message)
    {
        DisplayPauseMessage(title,message);
        restartTime = Time.time + 0.2f;
        showedPass = true;
    }
    
    public void GoToNextLevel()
    {
        SceneManager.LoadScene(nextLevel);
    }
    public void RestartLevel()
    {
        SceneManager.LoadScene(reloadLevel);
    }
    public void DisplayPauseMessage(string title, string message)
    {
        player.SetGamePause(true);
        pauseMessageTitle.SetActive(true);
        pauseMessage.SetActive(true);
        pauseMessageEsc.SetActive(true);
        pauseMessageTitleText.text = title;
        pauseMessageText.text = message;

    }

    public void HidePauseMessage()
    {
        pauseMessageTitle.SetActive(false);
        pauseMessage.SetActive(false);
        pauseMessageEsc.SetActive(false);

    }

    public void UpdateProgress(float newValue, string newSliderText)
    {
        sliderObject.SetActive(true); 
        sliderTextObject.SetActive(true);
        slider.value = newValue; 
        sliderText.text = newSliderText; 
    }

    public void DisplayGameMessage(string title, string message, float durationSec)
    {
        
        gameMessageTitle.SetActive(true);
        gameMessage.SetActive(true);

        gameMessageTitleText.text = title;
        gameMessageText.text = message;
        messageDisplayTime = Time.time;
        messageEraseTime = messageDisplayTime + durationSec;

    }

    public void HideGameMessage()
    {
        gameMessageTitle.SetActive(false);
        gameMessage.SetActive(false);
    }


}
