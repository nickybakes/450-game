using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ball : MonoBehaviour
{
    [Range(0.0f, 100.0f)]
    public float speed = 1;
    [Range(89.0f, -89.0f)]
    public float angle = 0; //in degrees
    public float arcHeight = 20.0f;

    private float midPointX;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0) && transform.parent != null)
        {
            GameObject target = GameObject.FindWithTag("Target");
            midPointX = (transform.position.x + target.transform.position.x) / 2;

            //changes midpoint based on height difference between player and target.
            float ratio = 0.001f;
            if (transform.position.y > target.transform.position.y)
            {
                midPointX -= ((transform.position.y + target.transform.position.y) / 2) * ratio;
            }
            else
            {
                midPointX += ((transform.position.y + target.transform.position.y) / 2) * ratio;
            }

            //unparents ball from player.
            transform.parent = null;

            
            CreateArc(this.transform.position, target.transform.position, 10, 3);
            //remove previous nodes
            //create new nodes
            //begin trajectory

        }

        if (transform.parent == null)
        {
            //transform.position += GetVector(); //placeholder, throws in one direction

            //for now, line up to a height, line down to target
            transform.position += FakeArc(midPointX);
        }
    }

    private Vector3 FakeArc(float midPointX)
    {
        //adding a decimal value slows down the ball.
        float y = speed * (midPointX - transform.position.x);

        return new Vector3(speed * Time.deltaTime, y * Time.deltaTime, 0);
    }

    /// <summary>
    /// Calculates a parabola given the two "endpoints", the height, and the resolution (amount of nodes).
    /// Then places the nodes in space.
    /// </summary>
    /// <param name="ballPos">The position of the ball at release.</param>
    /// <param name="targetPos">The position of the target.</param>
    /// <param name="height">The height of the arc.</param>
    /// <param name="resolution">The amount of nodes to create on the arc. (higher = smoother)</param>
    private void CreateArc(Vector3 ballPos, Vector3 targetPos, float height, int resolution)
    {

    }

    /// <summary>
    /// gets angle of throw.
    /// </summary>
    /// <returns>Vector3 for ball.</returns>
    private Vector3 GetVector()
    {
        //x * tan(theta) = y
        //Cannot be thrown backwards since Tan only works in Quadrants I & IV.
        float y = speed * Mathf.Tan(Mathf.Deg2Rad * angle);

        return new Vector3(speed * Time.deltaTime, y * Time.deltaTime, 0);
    }
}