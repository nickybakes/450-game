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

    private float rotationTimer; //Controls the rotation of the spawner

    // Start is called before the first frame update
    void Start()
    {
        //Sets a default value if the given one isn't good
        if (timer <= 0)
        {
            timer = 10.0f;
        }

        maxTime = timer;

        rotationTimer = 0;
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
                newBullet.transform.position = gameObject.transform.position;
                Bullet bulletScript = newBullet.GetComponent<Bullet>();
                bulletScript.direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (rotationAmountDegrees + i)), Mathf.Sin(Mathf.Deg2Rad * (rotationAmountDegrees + i)));
                timer = maxTime;
            }

        }

        rotationTimer += Time.deltaTime;

        float x = Mathf.Cos(rotationTimer);
        float y = Mathf.Sin(rotationTimer);
        float z = transform.position.z;

        transform.position = new Vector3(x, y, z);
    }
}
