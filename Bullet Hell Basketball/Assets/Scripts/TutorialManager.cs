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

    public GameObject instructionStart;
    public GameObject instructionBackspace;

    public GameObject bulletProps;

    public BulletManager[] bulletManagers;

    public Text controlChangeAlert;

    private String[] controlTypes = new String[] { "Gamepad", "Keyboard 1", "Keyboard 2" };

    public float controlChangeAlertTimeMax = 2;

    public float controlChangeAlertTimeCurrent;

    void Start()
    {
        controlChangeAlertTimeCurrent = controlChangeAlertTimeMax;
        functions = new TutorialEvent[messages.Length];

        functions[4] = ResetPlayersAndBall;
        functions[11] = GiveBallToDummy;
        functions[13] = StartBullets;
        functions[14] = HideBulletProps;
        functions[16] = SpawnPowerups;
        messageIndexDisplay.text = (currentMessageIndex + 1) + "/" + messages.Length;
        DisplayMessage();
    }

    public void DisplayNextMessage()
    {
        messages[currentMessageIndex].SetActive(false);
        currentMessageIndex++;
        if (currentMessageIndex == messages.Length)
        {
            //switch to game scene, they are done the tutorial
            gameManager.BackToMenu();
            return;
        }
        messageIndexDisplay.text = (currentMessageIndex + 1) + "/" + messages.Length;
        DisplayMessage();
    }

    public void ChangeControlType(ControlType type)
    {
        this.controlType = type;
        controlChangeAlertTimeCurrent = 0;
        controlChangeAlert.text = "Control scheme changed to " + controlTypes[(int)type];
        UpdateControlTypeDisplay();
    }

    public void UpdateControlTypeDisplay()
    {
        if (messages[currentMessageIndex].transform.childCount >= 3)
        {
            for (int i = 0; i < messages[currentMessageIndex].transform.childCount; i++)
            {
                messages[currentMessageIndex].transform.GetChild(i).gameObject.SetActive(false);
            }
            messages[currentMessageIndex].transform.GetChild((int)controlType).gameObject.SetActive(true);
        }
        if (controlType == ControlType.Gamepad)
        {
            instructionStart.SetActive(true);
            instructionBackspace.SetActive(false);
        }
        else
        {
            instructionStart.SetActive(false);
            instructionBackspace.SetActive(true);
        }
    }

    public void DisplayMessage()
    {
        messages[currentMessageIndex].SetActive(true);

        //check children for different control displays (gamepad, kb1, kb2)
        UpdateControlTypeDisplay();

        if (functions[currentMessageIndex] != null)
            functions[currentMessageIndex].Invoke();

    }

    public void ResetPlayersAndBall()
    {
        gameManager.ResetPlayersAndBall();
    }

    public void StartBullets()
    {
        for (int i = 0; i < bulletManagers.Length; i++)
        {
            bulletManagers[i].gameObject.SetActive(true);
        }
        bulletProps.SetActive(true);
    }

    public void HideBulletProps()
    {
        bulletProps.SetActive(false);
    }

    public void GiveBallToDummy()
    {
        gameManager.playerScriptsTeam1[0].GrabBall();
    }

    public void SpawnPowerups()
    {
        Vector2[] positions = new Vector2[] { new Vector2(-9.5f, 17.4f), new Vector2(9.5f, 17.4f), new Vector2(-10, 3.5f), new Vector2(0, 3.5f), new Vector2(10, 3.5f) };
        for (int i = 0; i < positions.Length; i++)
        {
            gameManager.SpawnSpecificPowerup((PowerupType) i, positions[i]);
        }
    }
}
