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

    // Update is called once per frame
    void Update()
    {
        //returns ball to players hand (for miss/testing)
        if (Input.GetKeyDown(KeyCode.Mouse1))
        {
            Vector3 positionToHand = new Vector3(1.8f, 1.1f, 0.0f);

            transform.parent = GameObject.FindGameObjectWithTag("Player").transform;
            transform.position = (transform.parent.position + positionToHand);

            //turns off physics
            //physics.simulatePhysics = false;
        }
        //shoots the ball
        if (Input.GetKeyDown(KeyCode.Mouse0) && transform.parent != null)
        {
            GameObject target = GameObject.FindWithTag("Target");
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
            //physics.simulatePhysics = false;
        }

        if (transform.parent == null)
        {
            GameObject target = GameObject.FindWithTag("Target");

            //If the ball is too far away from the basket (maxThrowDist), uses old arc to MISS the basket.
            if (calculateOnce)
            {
                boolWillHit = Mathf.Abs(transform.position.x - target.transform.position.x) < maxThrowDist;
                calculateOnce = false;
            }

            if (boolWillHit)
            {
                timer += Time.deltaTime * speed; //completes the parabola trip in one second
                transform.position = CalculateParabola(startPoint, target.transform.position, ballHeight, timer);
            }
            else
            {
                transform.position += PhysicsArc();
            }
        }
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
            //...and the player is not holding it.
            if (transform.parent == null)
            {
                //turns off physics
                //physics.simulatePhysics = false;

                GameObject player = GameObject.FindGameObjectWithTag("Player");
                Vector3 positionToHand = new Vector3(1.8f, 1.1f, 0.0f);
                
                transform.parent = player.transform;
                transform.position = (player.transform.position + positionToHand);
            }
            //...and the player is holding it.
            else
            {
                Debug.Log("Get Dunked On!");
            }
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

    /// Use later for shots that are too far to make the shot.
    private Vector3 PhysicsArc()
    {
        //use Nick gravity to throw in regular parabola.
        return new Vector3(speed / 10, 0.0f, 0.0f);

        //give it an initial velocity, then use gravity.
        //physics.SimulatePhysics();
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