using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum ControlType
{
    Gamepad,
    Keyboard1,
    Keyboard2
}

public class TutorialManager : MonoBehaviour
{

    public ControlType controlType;

    public GameManager gameManager;


    public delegate void TutorialEvent();

    public TutorialEvent[] functions;


    public GameObject[] messages;

    private int currentMessageIndex = 0;

    public Text messageIndexDisplay;

    public GameObject bulletProps;

    void Start()
    {
        functions = new TutorialEvent[messages.Length];

        functions[0] = ResetPlayersAndBall;
        functions[3] = ResetPlayersAndBall;
        functions[10] = GiveBallToDummy;
        functions[12] = StartBullets;
        functions[13] = HideBulletProps;
        DisplayMessage();
    }

    public void DisplayNextMessage()
    {
        messages[currentMessageIndex].SetActive(false);
        currentMessageIndex++;
        if (currentMessageIndex == messages.Length)
        {
            //switch to game scene, they are done the tutorial
            Destroy(FindObjectOfType<AudioManager>().gameObject);
            SceneManager.LoadScene(0);
            return;
        }
        messageIndexDisplay.text = (currentMessageIndex + 1) + "/" + messages.Length;
        DisplayMessage();
    }

    public void DisplayMessage()
    {
        messages[currentMessageIndex].SetActive(true);

        //check children for different control displays (gamepad, kb1, kb2)
        if (messages[currentMessageIndex].transform.childCount >= 3)
        {
            messages[currentMessageIndex].transform.GetChild((int)controlType).gameObject.SetActive(true);
        }

        if(functions[currentMessageIndex] != null)
            functions[currentMessageIndex].Invoke();

    }

    public void ResetPlayersAndBall()
    {
        gameManager.ResetPlayersAndBall();
    }

    public void StartBullets()
    {
        BulletManager[] bulletManagers = FindObjectsOfType<BulletManager>();

        for (int i = 0; i < bulletManagers.Length; i++)
        {
            bulletManagers[i].gameObject.SetActive(true);
        }

        bulletProps.SetActive(true);
    }

    public void HideBulletProps(){
        bulletProps.SetActive(false);
    }

    public void GiveBallToDummy(){
        gameManager.player2Script.GrabBall();
    }
}
