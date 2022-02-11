using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowBall : MonoBehaviour
{
    [Range(0.0f, 100.0f)]
    public float speed = 1;
    [Range(89.0f, -89.0f)]
    public float angle = 0; //in degrees

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Mouse0))
        {
            transform.parent = null; 
        }

        if (transform.parent == null)
        {
            transform.position += GetVector();
            
        }
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