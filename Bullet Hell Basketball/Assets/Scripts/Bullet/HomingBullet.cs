using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HomingBullet : MonoBehaviour
{

    public Ball ball;

    public Vector2 direction;

    public float speed;

    public float acceleration;

    public float maxSpeed;

    // Start is called before the first frame update
    void Start()
    {
        speed = 5;
        acceleration = 1;
        direction = Vector2.down;
    }

    // Update is called once per frame
    void Update()
    {
        speed = Mathf.Min(speed + acceleration * Time.deltaTime, maxSpeed);

        direction = Vector2.Lerp(direction, ball.transform.position - transform.position, .5f);

        transform.rotation = Quaternion.Euler(0, 0, Bullet.getAngle(Vector2.zero, direction));

        if (Vector2.Distance(transform.position, ball.transform.position) < 1.5f)
        {
            Explode();
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            Explode();            
        }
    }

    public void Explode()
    {
        Destroy(gameObject);
    }
}
