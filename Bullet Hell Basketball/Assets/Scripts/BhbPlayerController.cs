using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Control
{
    Up,
    Down,
    Left,
    Right,
    Jump,
    Jump2,
    Action,
    Pause
}

public class BhbPlayerController : NeonHeightsCharacterController
{

    private KeyCode[] player1Controls = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.B, KeyCode.N, KeyCode.Escape };
    private KeyCode[] player2Controls = { KeyCode.P, KeyCode.Semicolon, KeyCode.L, KeyCode.Quote, KeyCode.RightControl, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.Escape };
    public int playerNumber;

    public void Init(int playerNumber){
        this.playerNumber = playerNumber;
    }


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (GetControlHeld(Control.Left))
        {
            runningLeft = true;
        }
        else
        {
            runningLeft = false;
        }
        if (GetControlHeld(Control.Right))
        {
            runningRight = true;
        }
        else
        {
            runningRight = false;
        }

        if (GetControlDown(Control.Jump))
        {
            jumping = true;
        }
        else
        {
            jumping = false;
        }

        stoppedJumping = GetControlUp(Control.Jump);

        UpdateAugust();
    }

    bool GetControlHeld(Control action)
    {
        if (playerNumber == 0)
        {
            if (action == Control.Jump)
            {
                return Input.GetKey(player1Controls[((int)Control.Jump)]) || Input.GetKey(player1Controls[((int)Control.Jump2)]);
            }
            else
            {
                return Input.GetKey(player1Controls[((int)action)]);
            }
        }
        else if (playerNumber == 1)
        {
            if (action == Control.Jump)
            {
                return Input.GetKey(player2Controls[((int)Control.Jump)]) || Input.GetKey(player2Controls[((int)Control.Jump2)]);
            }
            else
            {
                return Input.GetKey(player2Controls[((int)action)]);
            }
        }
        return false;
    }

    bool GetControlDown(Control action)
    {
        if (playerNumber == 0)
        {
            if (action == Control.Jump)
            {
                return Input.GetKeyDown(player1Controls[((int)Control.Jump)]) || Input.GetKeyDown(player1Controls[((int)Control.Jump2)]);
            }
            else
            {
                return Input.GetKeyDown(player1Controls[((int)action)]);
            }
        }
        else if (playerNumber == 1)
        {
            if (action == Control.Jump)
            {
                return Input.GetKeyDown(player2Controls[((int)Control.Jump)]) || Input.GetKeyDown(player2Controls[((int)Control.Jump2)]);
            }
            else
            {
                return Input.GetKeyDown(player2Controls[((int)action)]);
            }
        }
        return false;
    }

    bool GetControlUp(Control action)
    {
        if (playerNumber == 0)
        {
            if (action == Control.Jump)
            {
                return Input.GetKeyUp(player1Controls[((int)Control.Jump)]) || Input.GetKeyUp(player1Controls[((int)Control.Jump2)]);
            }
            else
            {
                return Input.GetKeyUp(player1Controls[((int)action)]);
            }
        }
        else if (playerNumber == 1)
        {
            if (action == Control.Jump)
            {
                return Input.GetKeyUp(player2Controls[((int)Control.Jump)]) || Input.GetKeyUp(player2Controls[((int)Control.Jump2)]);
            }
            else
            {
                return Input.GetKeyUp(player2Controls[((int)action)]);
            }
        }
        return false;
    }
}
