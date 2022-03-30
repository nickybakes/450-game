using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class Ball : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float speed = 1;
    [Range(0.0f, 1.0f)]
    public float ballHeight = 10;
    [Range(2, 100)]
    public int previewArcSmoothness;
    [Range(0.0f, 1.0f)]
    public float spinAmt = 1.0f;

    private bool boolWillHit = true;
    private bool calculateOnce = true;

    private Vector3 startPoint;
    private float timer = 0.0f;

    public BhbBallPhysics physics;

    [HideInInspector]
    public GameObject leftBasket;
    [HideInInspector]
    public GameObject rightBasket;

    private AudioManager audioManager;
    private bool midairIsPlaying = false;

    public GameObject currentTarget;
    private GameManager gameManager;
    public LineRenderer lineRenderer;

    //true if the the ball was shot via a swipe and not a normal throw
    public bool isSwipeShot;

    public float bulletTimeMax = .23f;
    public float bulletTimeCurrent;

    public float resetTimeMax = 2f;
    public float resetTimeCurrent;

    public bool threePointShot = false;

    public bool IsBullet
    {
        get { return bulletTimeCurrent < bulletTimeMax; }
        set
        {
            if (value)
            {
                bulletTimeCurrent = 0;
            }
            else
            {
                bulletTimeCurrent = bulletTimeMax;
                transform.GetChild(2).gameObject.SetActive(false);
                transform.GetChild(3).gameObject.SetActive(false);
            }
        }
    }

    public bool IsResetting
    {
        get { return resetTimeCurrent < resetTimeMax; }
        set
        {
            if (value)
            {
                transform.parent = null;
                resetTimeCurrent = 0;
            }
            else
            {
                gameManager.panelUI.transform.GetChild(3).gameObject.SetActive(false);
                gameManager.panelUI.transform.GetChild(4).gameObject.SetActive(false);
                gameManager.ResetPlayersAndBall();
                resetTimeCurrent = resetTimeMax;
            }
        }
    }

    private float distMod;
    private float heightMod = 0;

    private void Start()
    {
        IsBullet = false;
        gameManager = FindObjectOfType<GameManager>();
        lineRenderer.positionCount = previewArcSmoothness;
        audioManager = FindObjectOfType<AudioManager>();
        IsResetting = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;

        if (transform.parent == null)
            transform.localScale = Vector3.one;
            
        if (physics.simulatePhysics || transform.parent != null)
            isSwipeShot = false;

        if (transform.position.x > gameManager.horizontalEdge && transform.parent == null)
        {
            physics.simulatePhysics = true;
            transform.position = new Vector3(gameManager.horizontalEdge - .5f, transform.position.y, transform.position.z);
            physics.velocity.x = -Mathf.Abs(physics.velocity.x);
        }

        if (transform.position.x < -gameManager.horizontalEdge && transform.parent == null)
        {
            physics.simulatePhysics = true;
            transform.position = new Vector3(-gameManager.horizontalEdge + .5f, transform.position.y, transform.position.z);
            physics.velocity.x = Mathf.Abs(physics.velocity.x);
        }

        SetBasketCrosshair(rightBasket, false);
        SetBasketCrosshair(leftBasket, false);

        if (IsBullet)
        {
            bulletTimeCurrent += Time.deltaTime;
            if (bulletTimeCurrent >= bulletTimeMax)
                IsBullet = false;
        }

        if (IsResetting)
        {
            resetTimeCurrent += Time.deltaTime;
            if (resetTimeCurrent >= resetTimeMax)
                IsResetting = false;
        }

        PlayMidairSound();

        if (transform.parent == null && physics.simulatePhysics == false)
        {

            //If the ball is too far away from the basket, boolWillHit = false.
            if (calculateOnce)
            {
                //check whether the ball was thrown from the side with the target basket
                if (isSwipeShot)
                {
                    //plays swipe shot audio.
                    audioManager.Play("SwipeShot", 0.9f, 1.1f);

                    boolWillHit = true;
                    calculateOnce = false;
                    if (currentTarget == leftBasket)
                    {
                        threePointShot = transform.position.x > 0;
                    }
                    else if (currentTarget == rightBasket)
                    {
                        threePointShot = transform.position.x < 0;
                    }
                }
                else
                {
                    if (currentTarget == leftBasket)
                    {
                        threePointShot = transform.position.x > 0;
                        boolWillHit = transform.position.x < 0;
                    }
                    else if (currentTarget == rightBasket)
                    {
                        threePointShot = transform.position.x < 0;
                        boolWillHit = transform.position.x > 0;
                    }
                    calculateOnce = false;
                }
                
                //so it's not calculated at runtime
                heightMod = HeightModifier();

                //Calculates a speed modifier based on the starting distance, closer = faster. 0 if outside 3 point line.
                float currentDist = Vector2.Distance(transform.position, currentTarget.transform.position);
                if (currentDist > 30)
                    distMod = 0;
                else
                    distMod = Mathf.Pow(5 / (currentDist + 0.01f), 1.1f);
            }

            if (boolWillHit)
            {
                float speedAddition = 0;
                if (isSwipeShot)
                {
                    //extra swipeshot speed based on how fast the ball was moving + how close to the basket you are.
                    speedAddition = (physics.velocity.magnitude / 2000) + (distMod * 2);
                    if (speedAddition > 0.5f)
                        speedAddition = 0.5f;
                }
                //completes the parabola trip in one second (* by speed), changing speed based on height and dist from basket.
                float speedMod = speed + ((300 - heightMod) / 200) + distMod + speedAddition /*+ (2 / (Vector2.Distance(transform.position, currentTarget.transform.position) + 1))*/;
                timer += Time.deltaTime * speedMod;
                Vector2 newPosition = CalculateParabola(startPoint, currentTarget.transform.GetChild(0).transform.position, ballHeight * heightMod, timer, false);
                if (physics.simulatePhysics)
                    return;
                transform.position = newPosition;

                //This is like a failsafe for if physics doesnt find the collision of the ball going into the basket on a fast
                //swipe shot
                //thought from testing, it seems this works before collision detection even can
                if (physics.velocity.y < 0 && transform.position.y < currentTarget.transform.position.y && Vector2.Distance(transform.position, currentTarget.transform.position) < 3)
                {
                    if (IsResetting)
                        return;
                    if (currentTarget == leftBasket)
                        ScoreLeftBasket();
                    else if (currentTarget == rightBasket)
                        ScoreRightBasket();

                    AfterScore();
                }
            }
            else
            {
                PhysicsArc();
            }

        }
        else if (transform.parent != null && !physics.simulatePhysics)
        {

            int playerNumber = transform.parent.GetComponent<BhbPlayerController>().playerNumber;
            lineRenderer.enabled = false;

            if (playerNumber == 0)
            {
                currentTarget = rightBasket;
            }
            else
            {
                currentTarget = leftBasket;
            }

            SetBasketCrosshair(currentTarget, false);

            //the ball is currently being held, now checks if the ball is targeting a basket and in the right zone, if so, shows previewParabola.
            //if aimed at left basket on left side, sets left crosshair.
            //Keeping PreviewParabola() & SetPositions() in the ifs for efficiency.
            if (currentTarget == leftBasket && transform.position.x < 0)
            {
                SetBasketCrosshair(leftBasket, true);

                Vector3[] pointsArray = PreviewParabola(transform.position, currentTarget.transform.GetChild(0).transform.position, ballHeight * HeightModifier(), previewArcSmoothness);
                lineRenderer.SetPositions(pointsArray);
                lineRenderer.enabled = true;
            }
            if (currentTarget == rightBasket && transform.position.x > 0)
            {

                SetBasketCrosshair(rightBasket, true);

                Vector3[] pointsArray = PreviewParabola(transform.position, currentTarget.transform.GetChild(0).transform.position, ballHeight * HeightModifier(), previewArcSmoothness);
                lineRenderer.SetPositions(pointsArray);
                lineRenderer.enabled = true;
            }
        }
        if (transform.parent == null)
        {
            transform.Rotate(0, 0, spinAmt * -physics.velocity.x, Space.World);
        }
    }

    public void ShootBall(int playerNumber, bool swipeShot)
    {
        IsBullet = true;

        if (playerNumber == 0)
        {
            currentTarget = rightBasket;
            transform.GetChild(2).gameObject.SetActive(true);
        }
        else
        {
            currentTarget = leftBasket;
            transform.GetChild(3).gameObject.SetActive(true);
        }

        isSwipeShot = swipeShot;

        //shoots the ball
        startPoint = transform.position;

        //reset timer
        timer = 0;
        //resets bool
        calculateOnce = true;
        //unparents ball from player.
        transform.parent = null;
    }

    /// <summary>
    /// Used for all ball collisions. Dunking, bouncing, players, bullets.
    /// </summary>
    /// <param name="collision">The thing hitting the ball.</param>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (IsResetting)
            return;

        //if the ball is touching the basket...
        if (collision.collider.CompareTag("Target"))
        {
            //stop own goaling when dunking
            if (transform.parent != null)
            {
                threePointShot = false;
                BhbPlayerController playerController = transform.parent.gameObject.GetComponent<BhbPlayerController>();
                if (playerController.playerNumber == 0 && collision.collider.gameObject == gameManager.leftBasket)
                    return;
                else if (playerController.playerNumber == 1 && collision.collider.gameObject == gameManager.rightBasket)
                    return;
            }

            //only if the ball goes in from top or from dunk
            if ((physics.velocity.y < 0 && transform.parent == null) || transform.parent != null)
            {
                if (collision.collider.gameObject == gameManager.rightBasket)
                {
                    ScoreRightBasket();
                }
                else if (collision.collider.gameObject == gameManager.leftBasket)
                {
                    ScoreLeftBasket();
                }

                AfterScore();
            }
        }
    }

    /// Used for shots that are too far to make the shot.
    private void PhysicsArc()
    {
        physics.simulatePhysics = true;
        if (currentTarget == rightBasket)
        {
            physics.velocity = new Vector2(30, 50);
        }
        else if (currentTarget == leftBasket)
        {
            physics.velocity = new Vector2(-30, 50);
        }
    }

    ///Calculates a parabola at an angle based on the height difference between the player and target.
    ///Attained from this forum:
    ///https://forum.unity.com/threads/generating-dynamic-parabola.211681/#post-1426169
    Vector3 CalculateParabola(Vector3 start, Vector3 end, float height, float t, bool preview)
    {
        float parabolicT = t * 2 - 1;

        //start and end are roughly level, pretend they are - simpler solution with less steps
        Vector3 travelDirection = end - start;
        Vector3 result = start + t * travelDirection;
        result.y += (-parabolicT * parabolicT + 1) * height;

        if (!preview)
        {
            physics.velocity = (result - gameObject.transform.position) * (1.0f / Time.deltaTime);

            if (transform.parent == null && !physics.simulatePhysics)
            {
                physics.CheckCollisionsBottom();
                physics.CheckCollisionsTop();
                physics.CheckCollisionsRight();
                physics.CheckCollisionsLeft();

                if ((physics.bottomCollision != null && !physics.bottomCollision.segment.semiSolidPlatform) ||
                (physics.topCollision != null && !physics.topCollision.segment.semiSolidPlatform) ||
                (physics.rightCollision != null && !physics.rightCollision.segment.semiSolidPlatform) ||
                (physics.leftCollision != null && !physics.leftCollision.segment.semiSolidPlatform))
                {
                    physics.simulatePhysics = true;
                    physics.UpdateCollisionRect();
                    physics.ApplyVerticalCollisions();
                    physics.ApplyHorizontalCollisions();
                    physics.velocity = new Vector2(-physics.velocity.x * .7f, physics.velocity.y);
                    physics.SimulatePhysics();
                }
            }
        }

        return result;

        /*
        //start and end are not level, gets more complicated
        Vector3 travelDirection = end - start;
        Vector3 levelDirection = end - new Vector3(start.x, end.y, start.z);
        Vector3 right = Vector3.Cross(travelDirection, levelDirection);
        Vector3 up = Vector3.Cross(right, travelDirection);
        if (end.y > start.y) up = -up;
        Vector3 result = start + t * travelDirection;
        result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
        physics.velocity = (result - gameObject.transform.position) * (1.0f / Time.deltaTime);
        
        return result;
        */
    }

    /// <summary>
    /// Draws a preview of the parabola when a player has the ball and is inside the appropriate shot line.
    /// </summary>
    /// <param name="start">Starting point of the parabola. (changes every update)</param>
    /// <param name="end">Ending point of the parabola. (changes based on which basket is targeted)</param>
    /// <param name="height">Changes based on how long the player holds the action button down for.</param>
    /// <param name="arraySize">Changes how smooth the arc looks.</param>
    /// <returns>Returns an array of Vector3s on the parabola.</returns>
    private Vector3[] PreviewParabola(Vector3 start, Vector3 end, float height, int arraySize)
    {
        Vector3[] drawnParabola = new Vector3[arraySize];

        //keeps preview line on z=0.
        start.z = 0;

        //t is a value from 0 to 1 for time, convert arraySize (equally spaced points) into decimal values between this.
        for (int i = 0; i < arraySize; i++)
        {
            drawnParabola[i] = CalculateParabola(start, end, height, (float)i / (arraySize - 1), true);
        }

        return drawnParabola;
    }

    /// <summary>
    /// Does some math to make the ball fly straighter when near the basket
    /// </summary>
    /// <returns>A value to modify the height variable on Update.</returns>
    private float HeightModifier()
    {
        //ball height changes on distance to basket. Capped height modifier.
        float heightCeiling = 300;
        float heightFloor = 30;

        //higher if player is lower, lower if player is higher
        float playerHeightMod = (currentTarget.transform.position.y - transform.position.y) * 50;
        float ballHeightMod = Mathf.Pow(Vector3.Distance(currentTarget.transform.position, transform.position), 2);
        ballHeightMod += playerHeightMod;

        //max/min height to arc
        if (ballHeightMod > heightCeiling)
            ballHeightMod = heightCeiling;

        if (ballHeightMod < heightFloor)
            ballHeightMod = heightFloor;

        return ballHeightMod;
    }

    /// <summary>
    /// Helper method for setting crosshairs.
    /// </summary>
    /// <param name="basket">Which basket to target.</param>
    /// <param name="a">Bool if set active.</param>
    private void SetBasketCrosshair(GameObject basket, bool a)
    {
        basket.transform.GetChild(0).gameObject.SetActive(a);
    }

    /// <summary>
    /// Helper method for when the shot has made it to the Right basket.
    /// </summary>
    private void ScoreRightBasket()
    {
        //Plays net sound.
        audioManager.Play("Net", 0.8f, 1.2f);

        //changes position of ball so it goes 'through' the basket.
        transform.position = new Vector3(gameManager.rightBasket.transform.GetChild(0).transform.position.x, transform.position.y, transform.position.z);

        if (threePointShot)
        {
            gameManager.player1Score += 3;
            gameManager.panelUI.transform.GetChild(3).GetComponent<Text>().text = "+3";
            audioManager.Play("3points");
        }
        else
        {
            gameManager.player1Score += 2;
            gameManager.panelUI.transform.GetChild(3).GetComponent<Text>().text = "+2";

            if (transform.parent == null)
                audioManager.Play("2points");
            else
                audioManager.Play("Dunk");
        }
        gameManager.previousScorer = 0;
        if (!gameManager.overTime)
            gameManager.panelUI.transform.GetChild(3).gameObject.SetActive(true);
        gameManager.panelUI.transform.GetChild(0).GetComponent<Text>().text = gameManager.player1Score.ToString();
    }

    /// <summary>
    /// Helper method for when the shot has made it to the Left basket.
    /// </summary>
    private void ScoreLeftBasket()
    {
        //Plays net sound.
        audioManager.Play("Net", 0.8f, 1.2f);

        //changes position of ball so it goes 'through' the basket.
        transform.position = new Vector3(gameManager.leftBasket.transform.GetChild(0).transform.position.x, transform.position.y, transform.position.z);

        if (threePointShot)
        {
            gameManager.player2Score += 3;
            gameManager.panelUI.transform.GetChild(4).GetComponent<Text>().text = "+3";
            audioManager.Play("3points");
        }
        else
        {
            gameManager.player2Score += 2;
            gameManager.panelUI.transform.GetChild(4).GetComponent<Text>().text = "+2";

            if (transform.parent == null)
                audioManager.Play("2points");
            else
                audioManager.Play("Dunk");
        }
        gameManager.previousScorer = 1;
        if (!gameManager.overTime)
            gameManager.panelUI.transform.GetChild(4).gameObject.SetActive(true);
        gameManager.panelUI.transform.GetChild(1).GetComponent<Text>().text = gameManager.player2Score.ToString();

    }

    private void AfterScore()
    {

        physics.simulatePhysics = true;
        lineRenderer.enabled = false;

        if (gameManager.overTime)
            gameManager.EndGame();
        else
            IsResetting = true;
    }

    /// <summary>
    /// Plays midair sounds when ball is above certain velocity magnitude
    /// </summary>
    private void PlayMidairSound()
    {
        //turns on midair sound if velocity is above threshhold
        if (physics.velocity.magnitude > 20 && !midairIsPlaying)
        {
            audioManager.Play("Midair");
            midairIsPlaying = true;
        }
        else
        {
            audioManager.Stop("Midair");
            midairIsPlaying = false;
        }
    }
}