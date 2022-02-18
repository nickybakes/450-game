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
    private KeyCode[] player2Controls = { KeyCode.P, KeyCode.Semicolon, KeyCode.L, KeyCode.Quote, KeyCode.RightControl, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.Escape };

    public int playerNumber;
    public float pickupRadius;
    public Vector3 playerHandPos;

    private GameObject ball;
    private Ball ballScript;
    private BhbBallPhysics ballPhysics;
    private float timer;

    public Material player2Sprite;

    public void Init(int playerNumber){
        this.playerNumber = playerNumber;

        if(playerNumber == 1)
        {
            gameObject.GetComponent<MeshRenderer>().material = player2Sprite;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pickupRadius = 5;
        playerHandPos = new Vector3(1.8f, 1.1f, 0.0f);

        ball = GameObject.FindGameObjectWithTag("Ball");
        ballScript = ball.GetComponent<Ball>();
        ballPhysics = ball.GetComponent<BhbBallPhysics>();
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

        if (GetControlDown(Control.Action))
        {
            //If outside the range to pickup the ball, apply cooldown.
            //(Implement dive/tackle/steal in future.)
            if (Vector2.Distance(ball.transform.position, gameObject.transform.position) > pickupRadius)
            {
                //apply action cooldown
                //give visual feedback that it's on cooldown (grey out jerma)
                return;
            }

            //if holding the ball...
            if (ball.transform.parent)
            {
                ballScript.ShootBall(playerNumber);
            }
            else
            {
                ballPhysics.simulatePhysics = false;

                //reverses the x-coord for second player.
                if(playerNumber == 1)
                {
                    ball.transform.position = (gameObject.transform.position + new Vector3(playerHandPos.x * -1, playerHandPos.y, playerHandPos.z));
                }
                else
                {
                    ball.transform.position = (gameObject.transform.position + playerHandPos);
                }
                ball.transform.parent = gameObject.transform;
            }
        }

        //pickup on radius < 5.0f
        if (Vector2.Distance(ball.transform.position, transform.position) < 5.0f && timer > 1.5f)
        {
            timer = 0;
            ballPhysics.simulatePhysics = false;

            //reverses the x-coord for second player.
            if (playerNumber == 1)
            {
                ball.transform.position = (gameObject.transform.position + new Vector3(playerHandPos.x * -1, playerHandPos.y, playerHandPos.z));
            }
            else
            {
                ball.transform.position = (gameObject.transform.position + playerHandPos);
            }
            ball.transform.parent = gameObject.transform;
        }
        if (timer < 2)
        {
            timer += Time.deltaTime;
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
