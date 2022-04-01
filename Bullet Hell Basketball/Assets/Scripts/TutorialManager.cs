using System;
using UnityEngine;
using UnityEngine.UI;

public enum ControlType{
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

    public int currentMessageIndex;

    void Start(){
        functions = new TutorialEvent[4];

        functions[0] = M;
    }

    public void DisplayNextMessage(){
        functions[0].Invoke();
    }

    public void M(){
        gameManager.ResetPlayersAndBall();
    }
}
