using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(0.1f, 10.0f)]
    public float speed = 30;
    public float ballHeight = 10;
    //private float midPointX;
    //private Vector3 midPoint;
    private Vector3 startPoint;
    private float timer = 0.0f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && transform.parent != null)
        {
            GameObject target = GameObject.FindWithTag("Target");
            //midPointX = (transform.position.x + target.transform.position.x) / 2;

            //midPoint = CalculateMidPoint(target, ballHeight);
            startPoint = transform.position;

            //changes midpoint based on height difference between player and target.
            //float ratio = 0.01f;
            //if (transform.position.y > target.transform.position.y)
            //{
            //    midPointX -= Mathf.Sqrt(Mathf.Abs(transform.position.y - target.transform.position.y));
            //}
            //else
            //{
            //    midPointX += Mathf.Sqrt(Mathf.Abs(transform.position.y - target.transform.position.y));
            //}
            timer = 0;
            //unparents ball from player.
            transform.parent = null;
        }

        if (transform.parent == null)
        {
            GameObject target = GameObject.FindWithTag("Target");
            //transform.position += CalculateArc(startPoint, midPoint, target.transform.position);

            //transform.position += FakeArc(midPointX);

            timer += Time.deltaTime * speed; //completes the parabola trip in one second
            transform.position = SampleParabola(startPoint, target.transform.position, ballHeight, timer);
        }
    }

    //private Vector3 FakeArc(float midPointX)
    //{
    //    //adding a decimal value slows down the ball.
    //    float y = (speed * (midPointX - transform.position.x));

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


    ///Is very glitchy, can calculate a parabola, but it can be at an angle, randomly teleports ball instantly to target, or halfway through arc.
    ///Attained from this forum:
    ///https://forum.unity.com/threads/generating-dynamic-parabola.211681/#post-1426169
    Vector3 SampleParabola(Vector3 start, Vector3 end, float height, float t)
    {
        float parabolicT = t * 2 - 1;
        //start and end are not level, gets more complicated
        Vector3 travelDirection = end - start;
        Vector3 levelDirecteion = end - new Vector3(start.x, end.y, start.z);
        Vector3 right = Vector3.Cross(travelDirection, levelDirecteion);
        Vector3 up = Vector3.Cross(right, travelDirection);
        if (end.y > start.y) up = -up;
        Vector3 result = start + t * travelDirection;
        result += ((-parabolicT * parabolicT + 1) * height) * up.normalized;
        return result;
    }
}