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
    private AudioManager audioManager;
    private float soundTimer;


    public GameObject swipeVisual;

    public float autoCatchCooldownTimerMax = 1;

    public float swipeTimeMax = .23f;

    private float swipeTimeCurrent = 0;

    public float swipeCooldownTimeMax = .3f;

    private float swipeCooldownTimeCurrent = 0;

    public float flashTimeMax = .07f;

    private float flashTimeCurrent = 0;

    public float stunTimeMax = .1f;

    private float stunTimeCurrent = 0;

    public float invinsibilityTimeMax = .15f;

    private float invinsibilityTimeCurrent = 0;

    public Material player2Sprite;
    public Material player2HurtSprite;
    public Material player2FlashSprite;

    private Vector2 prevControlAxis = Vector2.zero;

    private const float axisDeadZone = .3f;

    public bool IsSwiping
    {
        get { return swipeTimeCurrent < swipeTimeMax; }
        set
        {
            if (value)
            {
                swipeTimeCurrent = 0;
                velocity = Vector2.zero;
                transform.GetChild(0).gameObject.SetActive(true);
            }
            else
            {
                swipeCooldownTimeCurrent = 0;
                swipeTimeCurrent = swipeTimeMax;
                transform.GetChild(0).gameObject.SetActive(false);
            }
        }
    }

    public bool IsStunned
    {
        get { return stunTimeCurrent < stunTimeMax; }
        set
        {
            if (value)
            {
                flashTimeCurrent = 0;
                IsSwiping = false;
                stunTimeCurrent = 0;
                transform.GetChild(1).gameObject.SetActive(false);
                transform.GetChild(2).gameObject.SetActive(true);
                // gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                invinsibilityTimeCurrent = 0;
                stunTimeCurrent = stunTimeMax;
                transform.GetChild(1).gameObject.SetActive(false);
                // gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void Init(int playerNumber)
    {
        this.playerNumber = playerNumber;

        if (playerNumber == 1)
        {
            // gameObject.GetComponent<MeshRenderer>().material = player2Sprite;
            gameObject.transform.GetChild(1).GetComponent<MeshRenderer>().material = player2HurtSprite;
            gameObject.transform.GetChild(2).GetComponent<MeshRenderer>().material = player2FlashSprite;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pickupRadius = 5;
        playerHandPos = new Vector3(1.8f, 5.2f, 0.0f);
        swipeTimeCurrent = swipeTimeMax;
        flashTimeCurrent = flashTimeMax;
        soundTimer = 0;

        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        ball = GameObject.FindGameObjectWithTag("Ball");
        ballScript = ball.GetComponent<Ball>();
        ballPhysics = ball.GetComponent<BhbBallPhysics>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;

        if (ball.transform.parent != transform)
        {
            if (ball.transform.position.x < transform.position.x)
            {
                Quaternion q = Quaternion.Euler(0, 180, 0);
                transform.SetPositionAndRotation(transform.position, q);
            }
            if (ball.transform.position.x >= transform.position.x)
            {
                Quaternion q = Quaternion.Euler(0, 0, 0);
                transform.SetPositionAndRotation(transform.position, q);
            }
        }
        else
        {
            if (playerNumber == 0)
            {
                Quaternion q = Quaternion.Euler(0, 0, 0);
                transform.SetPositionAndRotation(transform.position, q);
            }
            else if (playerNumber == 1)
            {
                Quaternion q = Quaternion.Euler(0, 180, 0);
                transform.SetPositionAndRotation(transform.position, q);
            }
        }

        if (GetControlHeld(Control.Left) && !IsStunned)
        {
            //If changed direction and timer is long enough, play audio.
            if (!runningLeft && soundTimer > 1 && grounded)
            {
                soundTimer = 0;
                //random chance to play.
                if (Random.Range(0.1f, 0.8f) > 0.5)
                    audioManager.Play("Squeak", 0.8f, 1.2f);
            }
            soundTimer += Time.deltaTime;
            runningLeft = true;
        }
        else
        {
            runningLeft = false;
        }
        if (GetControlHeld(Control.Right) && !IsStunned)
        {
            //If changed direction and timer is long enough, play audio.
            if (!runningRight && soundTimer > 1 && grounded)
            {
                soundTimer = 0;
                //random chance to play.
                if (Random.Range(0.1f, 0.8f) > 0.5)
                    audioManager.Play("Squeak", 0.8f, 1.2f);
            }
            soundTimer += Time.deltaTime;
            runningRight = true;
        }
        else
        {
            runningRight = false;
        }

        if (GetControlDown(Control.Jump) && !IsStunned)
        {
            jumping = true;
        }
        else
        {
            jumping = false;
        }

        if (!ballScript.IsResetting)
        {
            if (GetControlDown(Control.Action) && !IsStunned)
            {
                //if holding the ball...
                if (ball.transform.parent == transform)
                {
                    autoCatchCooldownTimer = 0;
                    IsSwiping = false;
                    ballScript.ShootBall(playerNumber, false);
                }
                else if (!IsSwiping && swipeCooldownTimeCurrent >= swipeCooldownTimeMax && !IsStunned)
                {
                    IsSwiping = true;

                    if (playerNumber == 0)
                    {
                        if (Vector2.Distance(transform.GetChild(0).transform.position, gameManager.player2.transform.position) < 5.2)
                        {
                            if (ball.transform.parent != null && ball.transform.parent != transform)
                                GrabBall();
                            if (gameManager.player2.transform.position.x < transform.position.x)
                            {
                                gameManager.player2Script.GetsHit(new Vector2(-50, 20));
                            }
                            else if (gameManager.player2.transform.position.x >= transform.position.x)
                            {
                                gameManager.player2Script.GetsHit(new Vector2(50, 20));
                            }
                        }
                    }
                    else if (playerNumber == 1)
                    {
                        if (Vector2.Distance(transform.GetChild(0).transform.position, gameManager.player1.transform.position) < 6.5)
                        {
                            if (ball.transform.parent != null && ball.transform.parent != transform)
                                GrabBall();
                            if (gameManager.player1.transform.position.x < transform.position.x)
                            {
                                gameManager.player1Script.GetsHit(new Vector2(-50, 20));
                            }
                            else if (gameManager.player1.transform.position.x >= transform.position.x)
                            {
                                gameManager.player1Script.GetsHit(new Vector2(50, 20));
                            }
                        }
                    }

                }
                if (IsSwiping && swipeTimeCurrent <= .5)
                {
                    if (ball.transform.parent == null)
                    {
                        Vector2 midpoint = transform.position + (transform.GetChild(0).transform.position - transform.position) / 2.0f;
                        if (Vector2.Distance(midpoint, (Vector2)ball.transform.position + (ballPhysics.velocity * Time.deltaTime)) < 6.5)
                        {
                            ball.transform.parent = transform;
                            ballPhysics.simulatePhysics = false;
                            ballScript.ShootBall(playerNumber, true);

                            if (playerNumber == 1)
                            {
                                gameManager.yellowShevrons.SetActive(false);
                                gameManager.blueShevrons.SetActive(true);
                            }
                            else
                            {
                                gameManager.yellowShevrons.SetActive(true);
                                gameManager.blueShevrons.SetActive(false);
                            }
                        }
                    }
                }
                // else if (Vector2.Distance(ball.transform.position, transform.position) < 5.0f && autoCatchCooldownTimer > autoCatchCooldownTimerMax && ball.transform.parent != transform && ball.transform.parent != null)
                // {
                //     GrabBall();
                // }
            }
            else if (!ballScript.IsBullet && swipeCooldownTimeCurrent >= swipeCooldownTimeMax && !IsStunned && !IsSwiping && Vector2.Distance(ball.transform.position, transform.position) < 5.0f && autoCatchCooldownTimer > autoCatchCooldownTimerMax && ball.transform.parent == null)
            {
                GrabBall();
            }
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

        if (invinsibilityTimeCurrent < invinsibilityTimeMax)
        {
            invinsibilityTimeCurrent += Time.deltaTime;
        }

        if (IsStunned)
        {
            stunTimeCurrent += Time.deltaTime;
            if (stunTimeCurrent >= stunTimeMax)
                IsStunned = false;
        }

        if (flashTimeCurrent < flashTimeMax)
        {
            flashTimeCurrent += Time.deltaTime;
            if (flashTimeCurrent >= flashTimeMax)
            {
                transform.GetChild(1).gameObject.SetActive(true);
                transform.GetChild(2).gameObject.SetActive(false);
            }
        }

        if (!IsSwiping)
        {
            if (swipeCooldownTimeCurrent < swipeCooldownTimeMax)
            {
                swipeCooldownTimeCurrent += Time.deltaTime;
            }
            UpdateAugust();
        }
        else if (IsSwiping)
        {
            swipeTimeCurrent += Time.deltaTime;
            if (swipeTimeCurrent >= swipeTimeMax)
                IsSwiping = false;
        }
    }

    public void GrabBall()
    {


        autoCatchCooldownTimer = 0;
        ballPhysics.simulatePhysics = false;

        ball.transform.SetPositionAndRotation(ball.transform.position, Quaternion.Euler(Vector3.zero));

        //reverses the x-coord for second player.
        if (playerNumber == 1)
        {
            Quaternion q = Quaternion.Euler(0, 180, 0);
            transform.SetPositionAndRotation(transform.position, q);

            ball.transform.position = (gameObject.transform.position + new Vector3(playerHandPos.x * -1, playerHandPos.y, playerHandPos.z));

            gameManager.yellowShevrons.SetActive(false);
            gameManager.blueShevrons.SetActive(true);
        }
        else
        {
            Quaternion q = Quaternion.Euler(0, 0, 0);
            transform.SetPositionAndRotation(transform.position, q);

            ball.transform.position = (gameObject.transform.position + playerHandPos);

            gameManager.yellowShevrons.SetActive(true);
            gameManager.blueShevrons.SetActive(false);
        }
        ball.transform.parent = gameObject.transform;

    }


    public void GetsHit(Vector2 knockback)
    {
        if (!IsStunned && invinsibilityTimeCurrent >= invinsibilityTimeMax)
        {
            IsStunned = true;
            grounded = false;
            ballScript.lineRenderer.enabled = false;
            velocity = knockback;
            if (ball.transform.parent == transform)
            {
                ball.transform.parent = null;
                ballPhysics.simulatePhysics = true;
                ballPhysics.velocity = new Vector2(0, 44);
            }

            //sound if any player gets hit. (including bullets & player swipes)
            audioManager.Play("Hit", 0.9f, 1.1f);
        }
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
