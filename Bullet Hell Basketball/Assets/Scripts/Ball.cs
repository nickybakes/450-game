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
    private bool wasBeingHeld = true;

    private Vector3 startPoint;
    private float timer = 0.0f;

    public BhbBallPhysics physics;

    public Renderer ballRenderer;

    [HideInInspector]
    public GameObject leftBasket;
    [HideInInspector]
    public GameObject rightBasket;

    private Text onScoreTextRight;
    private Text onScoreTextLeft;

    private AudioManager audioManager;
    private Sound midAir;

    public GameObject currentTarget;
    public GameManager gameManager;
    public LineRenderer lineRenderer;
    private TrailRenderer trailRenderer;

    //true if the the ball was shot via a swipe and not a normal throw
    public bool isSwipeShot;
    private int swipeShotPasses;

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
        trailRenderer = transform.GetChild(0).GetComponent<TrailRenderer>();

        ballRenderer = transform.GetChild(4).GetComponent<Renderer>();

        onScoreTextRight = gameManager.panelUI.transform.GetChild(3).GetComponentInChildren<Text>();
        onScoreTextLeft = gameManager.panelUI.transform.GetChild(4).GetComponentInChildren<Text>();

        //preset midair sounds.
        midAir = audioManager.Find("Midair");
        midAir.source.volume = 0;
        audioManager.Play("Midair");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;

        if (transform.parent == null)
            transform.localScale = Vector3.one;

        if (physics.simulatePhysics || transform.parent != null)
        {
            isSwipeShot = false;
            timer = 0;
        }

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

        //Resets swipe shot passes.
        if (!isSwipeShot && physics.simulatePhysics)
        {
            swipeShotPasses = -1;
        }
        if (!isSwipeShot)
        {
            //resets color to white.
            trailRenderer.startColor = Color.white;
            trailRenderer.endColor = Color.white;
        }

        if (transform.parent == null && physics.simulatePhysics == false)
        {

            //If the ball is too far away from the basket, boolWillHit = false.
            if (calculateOnce)
            {
                //check whether the ball was thrown from the side with the target basket
                if (isSwipeShot)
                {
                    boolWillHit = true;
                    calculateOnce = false;
                    swipeShotPasses++;

                    //special SFX for back n' forth swipes
                    if (swipeShotPasses > 0)
                    {
                        //caps max passing speed at 4 passes.
                        if (swipeShotPasses > 3)
                            swipeShotPasses = 3;

                        float newSwipePitch = 1.0f + ((swipeShotPasses - 1.0f) / 12.0f);
                        audioManager.Play("SwipeRally", 0.3f, newSwipePitch, newSwipePitch);

                        switch (swipeShotPasses)
                        {
                            case 1:
                                trailRenderer.startColor = new Color32(255, 125, 125, 1);
                                trailRenderer.endColor = new Color32(255, 125, 125, 1);
                                break;
                            case 2:
                                trailRenderer.startColor = new Color32(255, 50, 50, 1);
                                trailRenderer.endColor = new Color32(255, 50, 50, 1);
                                break;
                            case 3:
                                trailRenderer.startColor = new Color32(255, 0, 0, 1);
                                trailRenderer.endColor = new Color32(255, 0, 0, 1);
                                break;
                        }
                    }

                    //Plays swipe shot audio.
                    audioManager.Play("SwipeShot", 0.3f, 0.9f, 1.1f);

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
                    //Plays shot audio.
                    audioManager.Play("Shot", 0.9f, 1.1f);

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

                    //reset passes
                    //so that on first swipe, passes is at 0.
                    swipeShotPasses = -1;
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

                    //extra swipeshot speed based on how many swipes in a row the player's have landed.
                    speedAddition += (swipeShotPasses / 4.0f);
                }
                //completes the parabola trip in one second (* by speed), changing speed based on height and dist from basket.
                float speedMod = speed + ((300 - heightMod) / 200) + distMod + speedAddition /*+ (2 / (Vector2.Distance(transform.position, currentTarget.transform.position) + 1))*/;
                timer += Time.deltaTime * speedMod;
                if (timer >= 1)
                {
                    if (IsResetting)
                        return;
                    if (currentTarget == leftBasket)
                    {
                        ScoreLeftBasket();
                    }
                    else if (currentTarget == rightBasket)
                    {
                        ScoreRightBasket();
                    }
                    AfterScore();
                    return;
                }
                Vector2 newPosition = CalculateParabola(startPoint, currentTarget.transform.GetChild(1).transform.position, ballHeight * heightMod, timer, false);
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

            int teamNumber = gameManager.currentBallOwner.teamNumber;
            lineRenderer.enabled = false;

            if (teamNumber == 0)
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

                Vector3[] pointsArray = PreviewParabola(transform.position, currentTarget.transform.GetChild(1).transform.position, ballHeight * HeightModifier(), previewArcSmoothness);
                lineRenderer.SetPositions(pointsArray);
                lineRenderer.enabled = true;
            }
            if (currentTarget == rightBasket && transform.position.x > 0)
            {

                SetBasketCrosshair(rightBasket, true);

                Vector3[] pointsArray = PreviewParabola(transform.position, currentTarget.transform.GetChild(1).transform.position, ballHeight * HeightModifier(), previewArcSmoothness);
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
                BhbPlayerController playerController = gameManager.currentBallOwner;
                if (playerController.teamNumber == 0 && collision.collider.gameObject == gameManager.leftBasket)
                    return;
                else if (playerController.teamNumber == 1 && collision.collider.gameObject == gameManager.rightBasket)
                    return;
            }

            //only if the ball goes in from top or from dunk
            if ((physics.velocity.y < 0 && transform.parent == null) || transform.parent != null)
            {
                if (IsResetting)
                    return;

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
        transform.position = new Vector3(gameManager.rightBasket.transform.GetChild(1).transform.position.x, transform.position.y, transform.position.z);

        if (threePointShot) //3 point
        {
            gameManager.team0Score += 3;
            onScoreTextRight.text = "+3";
            audioManager.Play("3points");
        }
        else if (transform.parent == null) //2 point
        {
            gameManager.team0Score += 2;
            onScoreTextRight.text = "+2";
            audioManager.Play("2points");
        }
        else //dunk (varied points)
        {
            int dunkValue = gameManager.bulletLevel * 2;

            gameManager.team0Score += dunkValue;
            onScoreTextRight.text = "+" + dunkValue;
            audioManager.Play("Dunk");
        }
        gameManager.previousScorer = 0;
        if (!gameManager.overTime)
            gameManager.panelUI.transform.GetChild(3).gameObject.SetActive(true);
        gameManager.panelUI.transform.GetChild(0).GetComponent<Text>().text = gameManager.team0Score.ToString();
    }

    /// <summary>
    /// Helper method for when the shot has made it to the Left basket.
    /// </summary>
    private void ScoreLeftBasket()
    {
        //Plays net sound.
        audioManager.Play("Net", 0.8f, 1.2f);

        //changes position of ball so it goes 'through' the basket.
        transform.position = new Vector3(gameManager.leftBasket.transform.GetChild(1).transform.position.x, transform.position.y, transform.position.z);

        if (threePointShot)
        {
            gameManager.team1Score += 3;
            onScoreTextLeft.text = "+3";
            audioManager.Play("3points");
        }
        else if (transform.parent == null) //2 point
        {
            gameManager.team1Score += 2;
            onScoreTextLeft.text = "+2";
            audioManager.Play("2points");
        }
        else //dunk (varied points)
        {
            int dunkValue = gameManager.bulletLevel * 2;

            gameManager.team1Score += dunkValue;
            onScoreTextLeft.text = "+" + dunkValue;
            audioManager.Play("Dunk");
        }
        gameManager.previousScorer = 1;
        if (!gameManager.overTime)
            gameManager.panelUI.transform.GetChild(4).gameObject.SetActive(true);
        gameManager.panelUI.transform.GetChild(1).GetComponent<Text>().text = gameManager.team1Score.ToString();

    }

    private void AfterScore()
    {
        physics.simulatePhysics = true;
        lineRenderer.enabled = false;

        HomingBullet[] homingBullets = FindObjectsOfType<HomingBullet>();
        foreach (HomingBullet hb in homingBullets)
        {
            hb.Explode();
        }

        SuperBullet[] superBullets = FindObjectsOfType<SuperBullet>();
        foreach (SuperBullet sb in superBullets)
        {
            sb.ForceDestroy();
        }

        BulletPortal[] bulletPortals = FindObjectsOfType<BulletPortal>();
        foreach (BulletPortal portal in bulletPortals)
        {
            // sb.ForceDestroy();
        }

        if (gameManager.overTime)
            gameManager.EndGame();
        else
            IsResetting = true;
    }

    /// <summary>
    /// Plays midair sounds when ball is above certain velocity magnitude.
    /// Extra if statements to help sound from looping.
    /// </summary>
    private void PlayMidairSound()
    {
        //changes volume based on ball's velocity. Constantly plays.
        //if it's not being held, and the last update it was, play sound, update bool.
        if (transform.parent == null && wasBeingHeld)
        {
            audioManager.Play("Midair");
            wasBeingHeld = false;
        }
        else if (transform.parent != null && !wasBeingHeld) //being held, wasn't before.
        {
            audioManager.Stop("Midair");
            midAir.source.volume = 0;
            wasBeingHeld = true;
        }
        //Always calculates how loud the ball air sound will be.
        midAir.source.volume = Mathf.Pow(physics.velocity.magnitude / 50, 3);
    }
}