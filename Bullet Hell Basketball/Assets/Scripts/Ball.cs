using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float speed = 1;
    [Range(0.0f, 1.0f)]
    public float ballHeight = 10;
    private float heightMod = 0;
    [Range(0.0f, 100.0f)]
    public float maxThrowDist = 10;
    [Range(2, 100)]
    public int previewArcSmoothness;
    [Range(0.0f, 1.0f)]
    public float spinAmt = 1.0f;

    private bool isSpinning = false;
    private bool boolWillHit = true;
    private bool calculateOnce = true;

    private Vector3 startPoint;
    private float timer = 0.0f;

    public BhbBallPhysics physics;

    [HideInInspector]
    public GameObject leftBasket;
    [HideInInspector]
    public GameObject rightBasket;

    public GameObject currentTarget;
    private GameManager gameManager;
    public LineRenderer lineRenderer;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        lineRenderer.positionCount = previewArcSmoothness;
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null && physics.simulatePhysics == false)
        {
            //If the ball is too far away from the basket, boolWillHit = false.
            if (calculateOnce)
            {
                //check whether the ball was thrown from the side with the target basket (will go in)
                //or not (will miss)
                if (currentTarget == leftBasket)
                {
                    boolWillHit = transform.position.x < 0;
                }
                else if (currentTarget == rightBasket)
                {
                    boolWillHit = transform.position.x > 0;
                }
                calculateOnce = false;

                //so it's not calculated at runtime
                heightMod = HeightModifier();
            }

            if (boolWillHit)
            {
                timer += Time.deltaTime * speed; //completes the parabola trip in one second (* by speed)
                transform.position = CalculateParabola(startPoint, currentTarget.transform.position, ballHeight * heightMod, timer);
            }
            else
            {
                PhysicsArc();
            }

            isSpinning = true;
        }
        else if (transform.parent != null && physics.simulatePhysics == false)
        {
            int playerNumber = transform.parent.GetComponent<BhbPlayerController>().playerNumber;
            lineRenderer.enabled = false;
            isSpinning = false;

            if (playerNumber == 0)
            {
                currentTarget = rightBasket;
            }
            else
            {
                currentTarget = leftBasket;
            }

            //the ball is currently being held, now checks if the ball is targeting a basket, if so, shows previewParabola.
            if (currentTarget == leftBasket)
            {
                if (transform.position.x < 0)
                {
                    Vector3[] pointsArray = PreviewParabola(transform.position, currentTarget.transform.position, ballHeight * HeightModifier(), previewArcSmoothness);
                    lineRenderer.enabled = true;
                    lineRenderer.SetPositions(pointsArray);

                    //Change target color (add marker later)
                    //currentTarget.GetComponent<Material>().SetColor("_UnlitColor", Color.red);
                }
            }
            else if (currentTarget == rightBasket)
            {
                if (transform.position.x > 0)
                {
                    Vector3[] pointsArray = PreviewParabola(transform.position, currentTarget.transform.position, ballHeight * HeightModifier(), previewArcSmoothness);
                    lineRenderer.enabled = true;
                    lineRenderer.SetPositions(pointsArray);

                    //Change target color (add marker later)
                    //currentTarget.GetComponent<Material>().SetColor("_UnlitColor", Color.red);
                }
            }

        }

        if (isSpinning)
        {
            //gives a set spin to the ball for now.
            //transform.rotation = Quaternion.Euler(0,0,RandomSpin(spinAmt, ballHeight, 0));
            transform.Rotate(0,0,spinAmt * Mathf.Abs(physics.velocity.y), Space.Self);
        }
    }

    public void ShootBall(int playerNumber)
    {
        if (playerNumber == 0)
        {
            currentTarget = rightBasket;
        }
        else
        {
            currentTarget = leftBasket;
        }
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
    private void OnCollisionEnter(Collision collision)
    {
        //if the ball is touching the basket...
        if (collision.collider.CompareTag("Target"))
        {
            gameManager.ResetPlayersAndBall();
            lineRenderer.enabled = false;
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
    Vector3 CalculateParabola(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        //start and end are not level, gets more complicated
        Vector3 travelDirection = end - start;
        Vector3 levelDirection = end - new Vector3(start.x, end.y, start.z);
        Vector3 right = Vector3.Cross(travelDirection, levelDirection);
        Vector3 up = Vector3.Cross(right, travelDirection);
        if (end.y > start.y) up = -up;
        Vector3 result = start + t * travelDirection;
        result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
        return result;
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

        //t is a value from 0 to 1 for time, convert arraySize (equally spaced points) into decimal values between this.
        for (int i = 0; i < arraySize; i++)
        {
            drawnParabola[i] = CalculateParabola(start, end, height, (float)i / (arraySize - 1));
        }

        return drawnParabola;
    }

    /// <summary>
    /// Does some math to make the ball fly straighter when near the basket
    /// </summary>
    /// <returns>A value to modify the height variable on Update.</returns>
    private float HeightModifier()
    {
        //right above or below basket, throw in straight line
        if (Mathf.Abs(transform.position.x - currentTarget.transform.position.x) < 10)
            return 0;
        //ball height changes on distance to basket, becoming straight throw close. Capped height modifier.

        //higher if player is lower, lower if player is higher
        float playerHeightMod = (currentTarget.transform.position.y - transform.position.y) * 50;
        float ballHeightMod = Mathf.Pow(Vector3.Distance(currentTarget.transform.position, transform.position), 2);
        ballHeightMod += playerHeightMod;

        //max height to arc
        if (ballHeightMod > 400)
            ballHeightMod = 400;
        //comment out to get wacky upside down parabolas!!!
        if (ballHeightMod < 0)
            ballHeightMod = 0;
        return ballHeightMod;
    }
}