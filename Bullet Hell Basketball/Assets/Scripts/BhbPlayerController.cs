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
    private string[] gamepadControls = { "Vertical", "DVertical", "Horizontal", "DHorizontal", "A", "B", "X", "Y", "Start" };


    public int controllerNumber = -1;
    public int playerNumber;
    public float pickupRadius;
    public Vector3 playerHandPos;

    private GameObject ball;
    private Ball ballScript;
    private BhbBallPhysics ballPhysics;
    private float autoCatchCooldownTimer;
    private GameManager gameManager;

    public float autoCatchCooldownTimerMax = 1;

    public Material player2Sprite;

    private Vector2 prevControlAxis = Vector2.zero;

    private const float axisDeadZone = .3f;

    public void Init(int playerNumber)
    {
        this.playerNumber = playerNumber;

        if (playerNumber == 1)
        {
            gameObject.GetComponent<MeshRenderer>().material = player2Sprite;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pickupRadius = 5;
        playerHandPos = new Vector3(1.8f, 1.1f, 0.0f);

        gameManager = FindObjectOfType<GameManager>();
        ball = GameObject.FindGameObjectWithTag("Ball");
        ballScript = ball.GetComponent<Ball>();
        ballPhysics = ball.GetComponent<BhbBallPhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;

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
            //if holding the ball...
            if (ball.transform.parent == transform)
            {
                autoCatchCooldownTimer = 0;
                ballScript.ShootBall(playerNumber);
            }
            else if (Vector2.Distance(ball.transform.position, transform.position) < 5.0f && autoCatchCooldownTimer > autoCatchCooldownTimerMax && ball.transform.parent != transform && ball.transform.parent != null)
            {
                GrabBall();
            }
        }
        else if (Vector2.Distance(ball.transform.position, transform.position) < 5.0f && autoCatchCooldownTimer > autoCatchCooldownTimerMax && ball.transform.parent == null)
        {
            GrabBall();
        }
        if (autoCatchCooldownTimer < autoCatchCooldownTimerMax + .5f)
        {
            autoCatchCooldownTimer += Time.deltaTime;
        }

        stoppedJumping = GetControlUp(Control.Jump);

        if (controllerNumber != -1)
        {
            string gamepadIdentifier = "J" + controllerNumber;
            //control sticks
            float controlStickX = Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]);
            float controlStickY = Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]);
            //dpads
            float dPadX = Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]);
            float dPadY = Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]);
            //in order to get the previous axis, we gotta take the max absolute value of either the dpad or control sticks
            prevControlAxis = new Vector2((Mathf.Abs(controlStickX) > Mathf.Abs(dPadX) ? controlStickX : dPadX), (Mathf.Abs(controlStickY) > Mathf.Abs(dPadY) ? controlStickY : dPadY));
        }   
        UpdateAugust();
    }

    public void GrabBall()
    {
        autoCatchCooldownTimer = 0;
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

    bool GetControlHeld(Control action)
    {

        if (GetGamepadControlHeld(action))
        {
            return true;
        }

        //if(Input.GetButton())
        if (playerNumber == 0)
        {
            if (action == Control.Jump)
            {
                return Input.GetKey(player1Controls[((int)Control.Jump)]) || Input.GetKey(player1Controls[((int)Control.Jump2)]) || Input.GetKey(player1Controls[((int)Control.Up)]);
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
                return Input.GetKey(player2Controls[((int)Control.Jump)]) || Input.GetKey(player2Controls[((int)Control.Jump2)]) || Input.GetKey(player2Controls[((int)Control.Up)]);
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
        if (GetGamepadControlDown(action))
        {
            return true;
        }

        if (playerNumber == 0)
        {
            if (action == Control.Jump)
            {
                return Input.GetKeyDown(player1Controls[((int)Control.Jump)]) || Input.GetKeyDown(player1Controls[((int)Control.Jump2)]) || Input.GetKeyDown(player1Controls[((int)Control.Up)]);
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
                return Input.GetKeyDown(player2Controls[((int)Control.Jump)]) || Input.GetKeyDown(player2Controls[((int)Control.Jump2)]) || Input.GetKeyDown(player2Controls[((int)Control.Up)]);
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
        if (GetGamepadControlUp(action))
        {
            return true;
        }

        if (playerNumber == 0)
        {
            if (action == Control.Jump)
            {
                return Input.GetKeyUp(player1Controls[((int)Control.Jump)]) || Input.GetKeyUp(player1Controls[((int)Control.Jump2)]) || Input.GetKeyUp(player1Controls[((int)Control.Up)]);
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
                return Input.GetKeyUp(player2Controls[((int)Control.Jump)]) || Input.GetKeyUp(player2Controls[((int)Control.Jump2)]) || Input.GetKeyUp(player2Controls[((int)Control.Up)]);
            }
            else
            {
                return Input.GetKeyUp(player2Controls[((int)action)]);
            }
        }
        return false;
    }


    // Up,
    // Down,
    // Left,
    // Right,
    // Jump,
    // Jump2,
    // Action,
    // Pause
    bool GetGamepadControlHeld(Control action)
    {
        if (controllerNumber == -1)
            return false;


        string gamepadIdentifier = "J" + controllerNumber;

        if (action == Control.Up)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]) > axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]) > 0;
        }
        else if (action == Control.Down)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]) < -axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]) < 0;
        }

        if (action == Control.Right)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]) > axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]) > 0;
        }
        else if (action == Control.Left)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]) < -axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]) < 0;
        }


        if (action == Control.Jump || action == Control.Jump2)
        {
            return Input.GetButton(gamepadIdentifier + gamepadControls[4]) || Input.GetButton(gamepadIdentifier + gamepadControls[5]);
        }
        else if (action == Control.Action)
        {
            return Input.GetButton(gamepadIdentifier + gamepadControls[6]) || Input.GetButton(gamepadIdentifier + gamepadControls[7]);
        }
        else if (action == Control.Pause)
        {
            return Input.GetButton(gamepadIdentifier + gamepadControls[8]);
        }

        return false;
    }


    bool GetGamepadControlDown(Control action)
    {
        if (controllerNumber == -1)
            return false;

        string gamepadIdentifier = "J" + controllerNumber;

        if (action == Control.Up && prevControlAxis.y <= axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]) > axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]) > 0;
        }
        else if (action == Control.Down && prevControlAxis.y >= -axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]) < -axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]) < 0;
        }

        if (action == Control.Right && prevControlAxis.x <= axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]) > axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]) > 0;
        }
        else if (action == Control.Left && prevControlAxis.y >= -axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]) < -axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]) < 0;
        }


        if (action == Control.Jump || action == Control.Jump2)
        {
            return Input.GetButtonDown(gamepadIdentifier + gamepadControls[4]) || Input.GetButtonDown(gamepadIdentifier + gamepadControls[5]);
        }
        else if (action == Control.Action)
        {
            return Input.GetButtonDown(gamepadIdentifier + gamepadControls[6]) || Input.GetButtonDown(gamepadIdentifier + gamepadControls[7]);
        }
        else if (action == Control.Pause)
        {
            return Input.GetButtonDown(gamepadIdentifier + gamepadControls[8]);
        }

        return false;
    }

    bool GetGamepadControlUp(Control action)
    {
        if (controllerNumber == -1)
            return false;

        string gamepadIdentifier = "J" + controllerNumber;

        if (action == Control.Up && prevControlAxis.y > axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]) <= axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]) < 0;
        }
        else if (action == Control.Down && prevControlAxis.y < -axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[0]) >= -axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[1]) > 0;
        }

        if (action == Control.Right && prevControlAxis.x > axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]) <= axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]) < 0;
        }
        else if (action == Control.Left && prevControlAxis.y < -axisDeadZone)
        {
            return Input.GetAxisRaw(gamepadIdentifier + gamepadControls[2]) >= -axisDeadZone || Input.GetAxisRaw(gamepadIdentifier + gamepadControls[3]) > 0;
        }


        if (action == Control.Jump || action == Control.Jump2)
        {
            return Input.GetButtonUp(gamepadIdentifier + gamepadControls[4]) || Input.GetButtonUp(gamepadIdentifier + gamepadControls[5]);
        }
        else if (action == Control.Action)
        {
            return Input.GetButtonUp(gamepadIdentifier + gamepadControls[6]) || Input.GetButtonUp(gamepadIdentifier + gamepadControls[7]);
        }
        else if (action == Control.Pause)
        {
            return Input.GetButtonUp(gamepadIdentifier + gamepadControls[8]);
        }

        return false;
    }
}
