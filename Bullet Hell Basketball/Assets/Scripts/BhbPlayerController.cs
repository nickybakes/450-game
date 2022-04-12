using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/*
// Player Header
// has health count on there out of 4
bullets take 1
big bullets take 2
explosions take 3

on 0/4 hp
KO'd text appears on their player
they become transparent hologram material
after 1 second, KO'd text switches to 3 second timer counting down
While happening, switch player to fall anim, and move them linearly to their spawn position (get their positon on player list and make them not overlap)
once 3 seconds is up, refill health to 4, give invincibility frames, dead = false (enable controls), switch back to non transparent material

Resetting players between rounds should make them all Not Dead (only if they were dead, do reset hp unless its 0)
	Unless its the resetting the whole game, in that case yeah


*/

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

public enum AnimationState
{
    Run_Forward_No_Ball,
    Run_Forward_With_Ball,
    Run_Backward_No_Ball,
    Run_Backward_With_Ball,
    Idle_No_Ball,
    Idle_With_Ball,
    Fall_No_Ball,
    Fall_With_Ball,
    Jump_No_Ball,
    Jump_With_Ball,
    Swipe_Grounded,
    Damage,
    Shoot_Grounded,
    Shoot_Air,
    Swipe_Shot_Grounded

}

public class BhbPlayerController : NeonHeightsCharacterController
{
    private KeyCode[] player1Controls = { KeyCode.W, KeyCode.S, KeyCode.A, KeyCode.D, KeyCode.Space, KeyCode.B, KeyCode.N, KeyCode.Escape };
    private KeyCode[] player2Controls = { KeyCode.P, KeyCode.Semicolon, KeyCode.L, KeyCode.Quote, KeyCode.LeftArrow, KeyCode.LeftArrow, KeyCode.DownArrow, KeyCode.Escape };
    private string[] gamepadControls = { "Vertical", "DVertical", "Horizontal", "DHorizontal", "A", "B", "X", "Y", "Start" };


    public int controllerNumber = -1;
    public int playerNumber;
    public int teamNumber;
    public int playerControlNumber;
    public float pickupRadius;

    public int health;

    public Renderer swipeRenderer;

    public Collider2D playerCollider;

    private GameObject ball;
    private Ball ballScript;
    private BhbBallPhysics ballPhysics;
    public float autoCatchCooldownTimer;
    private GameManager gameManager;
    private AudioManager audioManager;
    private float soundTimer;

    public Animator animator;
    public Animator ballAnimator;
    public GameObject ballHolder;

    public AnimationState currentAnimationState;

    public float dribbleSoundTimerCurrent = 0;
    public float dribbleSoundTimerMax = .24f;



    public bool facingRight = true;

    public GameObject swipeVisual;

    public float autoCatchCooldownTimerMax = 1;

    public float throwCoolDownTimerCurrent = 0;
    public float throwCoolDownTimerMax = .2f;

    public float swipeTimeMax = .23f;

    private float swipeTimeCurrent = 0;

    public float shootTimeMax = .24f;

    private float shootTimeCurrent = 0;

    public float swipeShotTimeMax = .4f;

    private float swipeShotTimeCurrent = 0;

    public float swipeCooldownTimeMax = .3f;

    private float swipeCooldownTimeCurrent = 0;

    public float flashTimeMax = .07f;

    private float flashTimeCurrent = 0;

    public float stunTimeMax = .1f;

    public float stunTimeCurrent = 0;

    public float invinsibilityTimeMax = .15f;

    private float invinsibilityTimeCurrent = 0;

    public GameObject player1Mesh;
    public GameObject player2Mesh;
    public Animator player2Animator;

    private Vector2 prevControlAxis = Vector2.zero;

    private const float axisDeadZone = .3f;

    public bool isBot;

    public float botShootRange;

    public bool botAlwaysDoubleJump;

    public bool isDummy;

    public bool IsSwiping
    {
        get { return swipeTimeCurrent < swipeTimeMax; }
        set
        {
            if (value)
            {
                SetAnimationState(AnimationState.Swipe_Grounded);
                audioManager.Play("SwipeShot", 0.1f, 1.3f, 1.5f);

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

    public bool IsShooting
    {
        get { return shootTimeCurrent < shootTimeMax; }
        set
        {
            if (value)
            {
                if (grounded)
                    SetAnimationState(AnimationState.Shoot_Grounded);
                else
                    SetAnimationState(AnimationState.Shoot_Air);

                shootTimeCurrent = 0;
                velocity = Vector2.zero;
            }
            else
            {
                shootTimeCurrent = shootTimeMax;
                autoCatchCooldownTimer = 0;
            }
        }
    }

    public bool IsSwipeShooting
    {
        get { return swipeShotTimeCurrent < swipeShotTimeMax; }
        set
        {
            if (value)
            {
                IsSwiping = false;
                GrabBall();
                SetAnimationState(AnimationState.Swipe_Shot_Grounded);
                swipeShotTimeCurrent = 0;
                velocity = Vector2.zero;
            }
            else
            {
                swipeShotTimeCurrent = swipeShotTimeMax;
                autoCatchCooldownTimer = 0;
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
                SetAnimationState(AnimationState.Damage);
                flashTimeCurrent = 0;
                IsSwiping = false;
                IsShooting = false;
                IsSwipeShooting = false;
                stunTimeCurrent = 0;
                // transform.GetChild(2).gameObject.SetActive(false);
                // transform.GetChild(3).gameObject.SetActive(true);
                // gameObject.GetComponent<MeshRenderer>().enabled = false;
            }
            else
            {
                invinsibilityTimeCurrent = 0;
                stunTimeCurrent = stunTimeMax;
                // transform.GetChild(2).gameObject.SetActive(false);
                // gameObject.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }

    public void Init(int playerNumber, int teamNumber, int playerControlNumber)
    {
        this.playerNumber = playerNumber;
        this.teamNumber = teamNumber;
        this.playerControlNumber = playerControlNumber;

        if (teamNumber == 0)
        {
            Destroy(player2Mesh);
        }
        else if (teamNumber == 1)
        {
            Destroy(player1Mesh);
            animator = player2Animator;
            player2Mesh.SetActive(true);
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        pickupRadius = 5;
        swipeShotTimeCurrent = swipeShotTimeMax;
        shootTimeCurrent = shootTimeMax;
        swipeTimeCurrent = swipeTimeMax;
        flashTimeCurrent = flashTimeMax;
        soundTimer = 0;

        gameManager = FindObjectOfType<GameManager>();
        audioManager = FindObjectOfType<AudioManager>();
        ball = GameObject.FindGameObjectWithTag("Ball");
        ballScript = ball.GetComponent<Ball>();
        ballPhysics = ball.GetComponent<BhbBallPhysics>();
        swipeRenderer = transform.GetChild(0).GetComponent<Renderer>();
        playerCollider = gameObject.GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;

        //face towards the ball if not holding it
        if (ball.transform.parent != transform)
        {
            if (ball.transform.position.x < transform.position.x)
            {
                Quaternion q = Quaternion.Euler(0, 180, 0);
                transform.SetPositionAndRotation(transform.position, q);
                facingRight = false;
            }
            if (ball.transform.position.x >= transform.position.x)
            {
                Quaternion q = Quaternion.Euler(0, 0, 0);
                transform.SetPositionAndRotation(transform.position, q);
                facingRight = true;
            }
        }
        else
        {
            //if holding it, face toward enemy hoop
            if (teamNumber == 0)
            {
                Quaternion q = Quaternion.Euler(0, 0, 0);
                Quaternion q2 = Quaternion.Euler(-90, 90, 0);
                transform.SetPositionAndRotation(transform.position, q);
                ballAnimator.transform.rotation = q2;
                facingRight = true;
            }
            else if (teamNumber == 1)
            {
                Quaternion q = Quaternion.Euler(0, 180, 0);
                Quaternion q2 = Quaternion.Euler(-90, -90, 0);
                transform.SetPositionAndRotation(transform.position, q);
                ballAnimator.transform.rotation = q2;
                facingRight = false;
            }

            ball.transform.position = ballHolder.transform.position;
            ball.transform.localScale = ballHolder.transform.localScale;
            ball.transform.rotation = ballHolder.transform.rotation;

            if (throwCoolDownTimerCurrent <= throwCoolDownTimerMax)
                throwCoolDownTimerCurrent += Time.deltaTime;
        }

        //If you just landed, plays landing sound.
        if (!prevGrounded && grounded)
        {
            audioManager.Play("JumpEnd", 0.8f, 1.0f);
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

        if (grounded && (runningRight || runningLeft))
        {
            if (facingRight && runningRight || !facingRight && runningLeft)
                SetAnimationState(AnimationState.Run_Forward_No_Ball);
            else if (facingRight && runningLeft || !facingRight && runningRight)
                SetAnimationState(AnimationState.Run_Backward_No_Ball);

        }
        else if (grounded)
        {
            SetAnimationState(AnimationState.Idle_No_Ball);

        }

        if (currentAnimationState == AnimationState.Idle_With_Ball)
        {
            dribbleSoundTimerCurrent += Time.deltaTime;
            if (dribbleSoundTimerCurrent >= dribbleSoundTimerMax)
            {
                dribbleSoundTimerCurrent = 0;
                DribbleSound();
            }
        }

        if (currentAnimationState == AnimationState.Run_Forward_With_Ball || currentAnimationState == AnimationState.Run_Backward_With_Ball)
        {
            dribbleSoundTimerCurrent += Time.deltaTime;
            if (dribbleSoundTimerCurrent >= dribbleSoundTimerMax)
            {
                dribbleSoundTimerCurrent = 0;
                DribbleSound(.8f);
            }
        }

        if (GetControlDown(Control.Jump) && !IsStunned && !jumping && jumpsInAir < jumpsInAirMax)
        {
            //Plays jump start sound.
            audioManager.Play("Jump");

            jumping = true;
            SetAnimationStateAlways(AnimationState.Jump_No_Ball);
        }
        else
        {
            jumping = false;
        }

        if (!grounded && velocity.y < 0)
        {
            SetAnimationState(AnimationState.Fall_No_Ball);
        }

        if (!ballScript.IsResetting)
        {
            if (GetControlDown(Control.Action) && !IsStunned && !IsShooting && !IsSwipeShooting)
            {
                //if holding the ball...
                if (ball.transform.parent == transform)
                {
                    if (throwCoolDownTimerCurrent > throwCoolDownTimerMax)
                    {
                        IsSwiping = false;
                        IsShooting = true;
                    }
                }
                else if (!IsSwiping && swipeCooldownTimeCurrent >= swipeCooldownTimeMax && !IsStunned)
                {
                    IsSwiping = true;

                    List<BhbPlayerController> swipeVictims = gameManager.SwipeBoundsIntersectCheck(this);
                    for (int i = 0; i < swipeVictims.Count; i++)
                    {
                        if (ball.transform.parent == swipeVictims[i].transform)
                            GrabBall();
                        if (swipeVictims[i].transform.position.x < transform.position.x)
                        {
                            swipeVictims[i].GetsHit(new Vector2(-50, 20), true, false);
                        }
                        else if (swipeVictims[i].transform.position.x >= transform.position.x)
                        {
                            swipeVictims[i].GetsHit(new Vector2(50, 20), true, false);
                        }
                    }

                    Powerup powerup = gameManager.SwipePowerupCheck(this);
                    if (powerup != null)
                    {
                        powerup.ActivatePowerup(teamNumber);
                    }
                }
                if (IsSwiping && swipeTimeCurrent <= .5)
                {
                    if (ball.transform.parent == null)
                    {
                        if (swipeRenderer.bounds.Intersects(ballScript.ballRenderer.bounds))
                        {
                            IsSwipeShooting = true;
                        }
                    }
                }
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
                // transform.GetChild(2).gameObject.SetActive(true);
                // transform.GetChild(3).gameObject.SetActive(false);
            }
        }

        if (IsSwiping)
        {
            swipeTimeCurrent += Time.deltaTime;
            if (swipeTimeCurrent >= swipeTimeMax)
                IsSwiping = false;
        }
        else if (IsShooting)
        {
            shootTimeCurrent += Time.deltaTime;
            if (shootTimeCurrent >= shootTimeMax)
            {
                IsShooting = false;
                ballScript.ShootBall(teamNumber, false);
                grounded = false;
                stunTimeCurrent = stunTimeMax * .2f;
                if (teamNumber == 0)
                {
                    velocity = new Vector2(-40, 20);
                }
                else
                {
                    velocity = new Vector2(40, 20);
                }
            }
        }
        else if (IsSwipeShooting)
        {
            swipeShotTimeCurrent += Time.deltaTime;
            if (swipeShotTimeCurrent >= swipeShotTimeMax)
            {
                IsSwipeShooting = false;
                ballScript.ShootBall(teamNumber, true);
                grounded = false;
                stunTimeCurrent = stunTimeMax * .2f;
                if (teamNumber == 0)
                {
                    velocity = new Vector2(-60, 20);
                }
                else
                {
                    velocity = new Vector2(60, 20);
                }
            }
        }
        else
        {
            if (swipeCooldownTimeCurrent < swipeCooldownTimeMax)
            {
                swipeCooldownTimeCurrent += Time.deltaTime;
            }
            UpdateAugust();
        }
    }

    public void BotRandomizeBehavior()
    {
        botShootRange = Random.Range(10, 40);
        if (Random.Range(0, 10) > 8)
            botAlwaysDoubleJump = true;
        else
            botAlwaysDoubleJump = false;
    }

    public void GrabBall()
    {
        if (isBot)
        {
            BotRandomizeBehavior();
        }

        gameManager.currentBallOwner = this;

        throwCoolDownTimerCurrent = 0;
        autoCatchCooldownTimer = 0;
        ballPhysics.simulatePhysics = false;

        ball.transform.SetPositionAndRotation(ball.transform.position, Quaternion.Euler(Vector3.zero));
        ballScript.threePointShot = false;

        //reverses the x-coord for second player.
        if (teamNumber == 1)
        {
            Quaternion q = Quaternion.Euler(0, 180, 0);
            Quaternion q2 = Quaternion.Euler(-90, 90, 0);
            transform.SetPositionAndRotation(transform.position, q);
            ballAnimator.transform.rotation = q2;
            facingRight = false;

            // ball.transform.position = (gameObject.transform.position + new Vector3(playerHandPos.x * -1, playerHandPos.y, playerHandPos.z));

            gameManager.yellowShevrons.SetActive(false);
            gameManager.blueShevrons.SetActive(true);
        }
        else
        {
            Quaternion q = Quaternion.Euler(0, 0, 0);
            Quaternion q2 = Quaternion.Euler(-90, -90, 0);
            transform.SetPositionAndRotation(transform.position, q);
            ballAnimator.transform.rotation = q2;
            facingRight = true;

            // ball.transform.position = (gameObject.transform.position + playerHandPos);

            gameManager.yellowShevrons.SetActive(true);
            gameManager.blueShevrons.SetActive(false);
        }
        ball.transform.parent = gameObject.transform;


        if (currentAnimationState == AnimationState.Jump_No_Ball)
            ballAnimator.SetTrigger(AnimationState.Fall_With_Ball.ToString());
    }


    public void GetsHit(Vector2 knockback, bool isByPlayer, bool ignoreAlreadyStunned)
    {
        if ((!IsStunned && invinsibilityTimeCurrent >= invinsibilityTimeMax) || ignoreAlreadyStunned)
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
            if (isByPlayer)
                audioManager.Play("Hit", 0.9f, 1.1f);
            else
                audioManager.Play("HitBullet", 0.9f, 1.1f);
        }
    }

    public void SetAnimationState(AnimationState state)
    {
        if (ball.transform.parent == transform && state.ToString().Contains("No_Ball"))
        {
            state++;
        }

        if ((IsSwiping || IsShooting || IsSwipeShooting) && state != AnimationState.Damage || IsStunned)
            return;

        if (currentAnimationState == state)
            return;

        SetAnimationStateAlways(state);
    }

    public void SetAnimationStateAlways(AnimationState state)
    {
        if (ball.transform.parent == transform)
        {
            if (state == AnimationState.Jump_No_Ball)
                state = AnimationState.Jump_With_Ball;

            if (state == AnimationState.Damage)
            {
                ball.transform.localScale = Vector3.one;
                return;
            }

            ballAnimator.SetTrigger(state.ToString());
        }

        if (state == AnimationState.Idle_With_Ball)
        {
            dribbleSoundTimerMax = .96f;
            dribbleSoundTimerCurrent = 0;
        }

        if (state == AnimationState.Run_Forward_With_Ball)
        {
            dribbleSoundTimerMax = .48f;
            dribbleSoundTimerCurrent = .36f;
        }

        if (state == AnimationState.Run_Backward_With_Ball)
        {
            dribbleSoundTimerMax = .48f;
            dribbleSoundTimerCurrent = .2f;
        }

        currentAnimationState = state;

        animator.SetTrigger(state.ToString());
    }

    public void DribbleSound()
    {
        audioManager.Play("Bounce", .5f);
    }

    public void DribbleSound(float volume)
    {
        audioManager.Play("Bounce", volume);
    }

    bool GetControlHeld(Control action)
    {
        if (isDummy)
            return false;

        if (isBot)
        {
            if (ballScript.IsResetting)
                return false;
            if (action == Control.Left)
            {
                if (ball.transform.parent != transform)
                {
                    if (teamNumber == 1 && ball.transform.parent != null && gameManager.isBallOwnerOppositeTeam(this) && gameManager.currentBallOwner.IsSwipeShooting && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return false;
                    }
                    if (teamNumber == 1 && ballScript.isSwipeShot == true && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return false;
                    }
                    if (teamNumber == 0 && ball.transform.parent != null && gameManager.isBallOwnerOppositeTeam(this) && gameManager.currentBallOwner.IsSwipeShooting && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return true;
                    }
                    if (teamNumber == 0 && ballScript.isSwipeShot == true && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return true;
                    }
                    if (ball.transform.position.x > -14 && ball.transform.position.x < 14 && ball.transform.position.y < 6.5f)
                    {
                        if (transform.position.x > -18 && transform.position.x < 18 && transform.position.y > 8)
                        {
                            if (teamNumber == 0)
                                return true;
                            else
                                return false;
                        }
                    }

                    if (Mathf.Abs(ball.transform.position.x - transform.position.x) < 1)
                        return false;

                    if (ball.transform.position.x < transform.position.x)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (teamNumber == 0 && transform.position.x > gameManager.rightBasket.transform.position.x - botShootRange)
                    {
                        return true;
                    }
                    else if (teamNumber == 1 && transform.position.x > gameManager.leftBasket.transform.position.x + botShootRange)
                    {
                        return true;
                    }
                    return false;
                }
            }
            if (action == Control.Right)
            {
                if (ball.transform.parent != transform)
                {
                    if (teamNumber == 0 && ball.transform.parent != null && gameManager.isBallOwnerOppositeTeam(this) && gameManager.currentBallOwner.IsSwipeShooting && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return false;
                    }
                    if (teamNumber == 0 && ballScript.isSwipeShot == true && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return false;
                    }
                    if (teamNumber == 1 && ball.transform.parent != null && gameManager.isBallOwnerOppositeTeam(this) && gameManager.currentBallOwner.IsSwipeShooting && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return true;
                    }
                    if (teamNumber == 1 && ballScript.isSwipeShot == true && Mathf.Abs(transform.position.x - ball.transform.position.x) > 10)
                    {
                        return true;
                    }
                    if (ball.transform.position.x > -14 && ball.transform.position.x < 14 && ball.transform.position.y < 6.5f)
                    {
                        if (transform.position.x > -18 && transform.position.x < 18 && transform.position.y > 8)
                        {
                            if (teamNumber == 1)
                                return true;
                            else
                                return false;
                        }
                    }

                    if (Mathf.Abs(ball.transform.position.x - transform.position.x) < 1)
                        return false;

                    if (ball.transform.position.x > transform.position.x)
                        return true;
                    else
                        return false;
                }
                else
                {
                    if (teamNumber == 0 && transform.position.x < gameManager.rightBasket.transform.position.x - botShootRange)
                    {
                        return true;
                    }
                    else if (teamNumber == 1 && transform.position.x < gameManager.leftBasket.transform.position.x + botShootRange)
                    {
                        return true;
                    }
                    return false;
                }
            }

            return false;
        }


        if (playerControlNumber > 1 && GetGamepadControlHeld(action))
        {
            return true;
        }
        else if (playerControlNumber == 0)
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
        else if (playerControlNumber == 1)
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
        if (isDummy)
            return false;

        if (isBot)
        {
            if (ballScript.IsResetting)
                return false;

            if (action == Control.Jump)
            {

                if (botAlwaysDoubleJump)
                {
                    if (grounded || velocity.y <= 0)
                        return true;

                    return false;
                }
                else
                {
                    if (ballScript.isSwipeShot == true)
                    {
                        if ((grounded || velocity.y <= 0) && ball.transform.position.y - transform.position.y > Mathf.Abs(ball.transform.position.x - transform.position.x) * .8f && ball.transform.parent != transform)
                        {
                            return true;
                        }
                        return false;
                    }
                    else
                    {
                        if ((grounded || velocity.y <= 0) && ball.transform.position.y - transform.position.y > Mathf.Abs(ball.transform.position.x - transform.position.x) * 1.5f && ball.transform.parent != transform)
                        {
                            return true;
                        }
                        return false;
                    }
                }
            }

            if (action == Control.Action)
            {
                if (ball.transform.parent != null && ball.transform.parent != transform)
                {
                    if (gameManager.isBallOwnerOppositeTeam(this) && Vector2.Distance(transform.GetChild(0).transform.position, gameManager.currentBallOwner.transform.position) < 3.4f + Mathf.PerlinNoise(transform.position.x, transform.position.y) * 2.0f)
                    {
                        if (Mathf.PerlinNoise(transform.position.y, transform.position.x) > .3)
                            return true;
                    }
                    else if (gameManager.isBallOwnerOppositeTeam(this) && Vector2.Distance(transform.GetChild(0).transform.position, gameManager.currentBallOwner.transform.position) < 3.4f + Mathf.PerlinNoise(transform.position.x, transform.position.y) * 2.0f)
                    {
                        if (Mathf.PerlinNoise(transform.position.y, transform.position.x) > .3)
                            return true;
                    }
                    return false;
                }
                else if (ball.transform.parent == transform)
                {
                    if (botShootRange < 10)
                    {
                        if (teamNumber == 0 && Mathf.Abs(transform.position.x - gameManager.rightBasket.transform.position.x) <= botShootRange && Mathf.Abs(prevPosition.x - gameManager.rightBasket.transform.position.x) > botShootRange)
                        {
                            if (Random.Range(0, 2) == 0)
                            {
                                return true;
                            }
                            BotRandomizeBehavior();
                            return false;
                        }
                        else if (teamNumber == 1 && Mathf.Abs(transform.position.x - gameManager.leftBasket.transform.position.x) <= botShootRange && Mathf.Abs(prevPosition.x - gameManager.leftBasket.transform.position.x) > botShootRange)
                        {
                            if (Random.Range(0, 2) == 0)
                            {
                                return true;
                            }
                            BotRandomizeBehavior();
                            return false;
                        }
                    }
                    else
                    {
                        if (teamNumber == 0 && Mathf.Abs(transform.position.x - gameManager.rightBasket.transform.position.x) <= botShootRange)
                        {
                            if (Mathf.PerlinNoise(transform.position.x, transform.position.y) > .4)
                            {
                                return true;
                            }
                            BotRandomizeBehavior();
                            return false;
                        }
                        else if (teamNumber == 1 && Mathf.Abs(transform.position.x - gameManager.leftBasket.transform.position.x) <= botShootRange)
                        {
                            if (Mathf.PerlinNoise(transform.position.x, transform.position.y) > .4)
                            {
                                return true;
                            }
                            BotRandomizeBehavior();
                            return false;
                        }
                        // if (transform.position.x)
                    }
                }
                else if (ball.transform.parent == null)
                {
                    if (Vector2.Distance(transform.GetChild(0).transform.position, (Vector2)ball.transform.position + (ballPhysics.velocity * Time.deltaTime)) < 3)
                    {
                        if (Mathf.PerlinNoise(-transform.position.y * Random.Range(-4, 4), -transform.position.x * Random.Range(-4, 4)) > .8)
                        {
                            return true;
                        }
                        return false;
                    }
                }

                return false;
            }
            return false;

        }


        if (playerControlNumber > 1 && GetGamepadControlDown(action))
        {
            return true;
        }
        else if (playerControlNumber == 0)
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
        else if (playerControlNumber == 1)
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
        if (isDummy)
            return false;

        if (isBot)
        {

            if (action == Control.Jump)
            {
                if (ballScript.isSwipeShot)
                {
                    if (ball.transform.position.y - transform.position.y < Mathf.Abs(ball.transform.position.x - transform.position.x) * .7f)
                    {
                        return true;
                    }
                    return false;
                }
                else
                {
                    if (ball.transform.position.y - transform.position.y < Mathf.Abs(ball.transform.position.x - transform.position.x) * 1.5f)
                    {
                        return true;
                    }
                    return false;
                }

            }
            return false;

        }


        if (playerControlNumber > 1 && GetGamepadControlUp(action))
        {
            return true;
        }
        else if (playerControlNumber == 0)
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
        else if (playerControlNumber == 1)
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
        string gamepadIdentifier = "J" + (playerControlNumber - 1);

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
        string gamepadIdentifier = "J" + (playerControlNumber - 1);


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
        string gamepadIdentifier = "J" + (playerControlNumber - 1);

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
