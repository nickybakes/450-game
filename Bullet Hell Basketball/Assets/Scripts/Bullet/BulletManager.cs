using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public GameObject bullet;
    public float timer; //Controls how long between bullets this will fire
    private float maxTime;

    private float rotationAmountDegrees;
    public float rotationSpeed = 10;

    public float radius;

    public float angularSpeed;

    private Vector3 fixedPoint;
    private float currentAngle;

    // Start is called before the first frame update
    void Start()
    {
        //Sets a default value if the given one isn't good
        if (timer <= 0)
        {
            timer = 10.0f;
        }

        maxTime = timer;

        fixedPoint = transform.position;
    }

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;

        rotationAmountDegrees += Time.deltaTime * rotationSpeed;

        if (timer <= 0)
        {
            for (int i = 0; i < 360; i += 90)
            {
                GameObject newBullet = Instantiate(bullet);
                newBullet.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0.0f);
                Bullet bulletScript = newBullet.GetComponent<Bullet>();
                bulletScript.direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (rotationAmountDegrees + i)), Mathf.Sin(Mathf.Deg2Rad * (rotationAmountDegrees + i)));
                timer = maxTime;
            }

        }

        //Spawner rotation 
        //Source: https://forum.unity.com/threads/circular-movement.572797/
        currentAngle += angularSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
        transform.position = fixedPoint + offset;
    }
}
