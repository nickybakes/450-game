using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float speed = 30;
    [Range(0.0f, 100.0f)]
    public float ballHeight = 10;
    [Range(0.0f, 100.0f)]
    public float maxThrowDist = 10;

    private bool boolWillHit = true;
    private bool calculateOnce = true;

    private Vector3 startPoint;
    private Vector3 prevPos;
    private float timer = 0.0f;

    public BhbBallPhysics physics;

    [HideInInspector]
    public GameObject leftBasket;
    [HideInInspector]
    public GameObject rightBasket;

    public GameObject currentTarget;
    private GameManager gameManager;

    private void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (transform.parent == null && physics.simulatePhysics == false)
        {

            if (currentTarget == null)
            {
                physics.simulatePhysics = true;
                return;
            }

            //If the ball is too far away from the basket (maxThrowDist), uses old arc to MISS the basket.
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
            }

            if (boolWillHit)
            {
                timer += Time.deltaTime * speed; //completes the parabola trip in one second
                transform.position = CalculateParabola(startPoint, currentTarget.transform.position, ballHeight, timer);
            }
            else
            {
                PhysicsArc();
            }
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
        //resets previous position (used to calculate velocity).
        prevPos = transform.parent.position;
        //unparents ball from player.
        transform.parent = null;

        //turns off physics
        physics.simulatePhysics = false;
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
        }
        else
        {
            //deals with physics collisions.
            //calculates current velocity at time of impact.
            physics.velocity = (transform.position - prevPos) / Time.deltaTime;

            //check everything.
            if (physics.bottomCollision != null ||
                physics.topCollision != null ||
                //physics.groundCollision != null ||
                physics.rightCollision != null ||
                physics.leftCollision != null)
            {
                physics.simulatePhysics = true;
                physics.SimulatePhysics();
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
}