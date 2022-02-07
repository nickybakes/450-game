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
    private bool boolThrow = true;
    //private Vector3 midPoint;
    private Vector3 startPoint;
    private float timer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && transform.parent != null)
        {
            GameObject target = GameObject.FindWithTag("Target");

            //midPoint = CalculateMidPoint(target, ballHeight);
            startPoint = transform.position;

            //reset timer
            timer = 0;
            //resets bool
            boolThrow = true;
            //unparents ball from player.
            transform.parent = null;
        }

        if (transform.parent == null)
        {
            GameObject target = GameObject.FindWithTag("Target");

            //If the ball is too far away from the basket (maxThrowDist), uses old arc to MISS the basket.
            if (Mathf.Abs(transform.position.x - target.transform.position.x) > maxThrowDist && !boolThrow)
            {
                boolThrow = false;
                //transform.position += FakeArc()
            }

            timer += Time.deltaTime * speed; //completes the parabola trip in one second
            transform.position = SampleParabola(startPoint, target.transform.position, ballHeight, timer);
        }
    }

    /// Use later for shots that are too far to make the shot.
    //private Vector3 FakeArc(float midPoint.x)
    //{
    //    //adding a decimal value slows down the ball.
    //    float y = (speed * (midPoint.x - transform.position.x));

    //    return new Vector3(speed * Time.deltaTime, y * Time.deltaTime, 0);
    //}

    /// <summary>
    /// Find the midpoint of the parabola.
    /// </summary>
    /// <param name="target">Target the ball is aiming towards.</param>
    /// <returns>Vector3 of the midpoint position.</returns>
    //private Vector3 CalculateMidPoint(GameObject target, float ballHeight)
    //{
    //    Vector3 midPoint = new Vector3(0.0f,0.0f,0.0f);

    //    midPoint.x = (transform.position.x + target.transform.position.x) / 2;

    //    if (transform.position.y > target.transform.position.y)
    //        midPoint.y = (transform.position.y + ballHeight) + Random.Range(-3.0f, 3.0f);
    //    else
    //        midPoint.y = (target.transform.position.y + ballHeight) + Random.Range(-3.0f, 3.0f);

    //    return midPoint;
    //}

    ///Calculates a parabola at an angle based on the height difference between the player and target.
    ///Attained from this forum:
    ///https://forum.unity.com/threads/generating-dynamic-parabola.211681/#post-1426169
    Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
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