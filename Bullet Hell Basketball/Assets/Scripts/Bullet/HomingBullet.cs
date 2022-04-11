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

    public GameObject crosshair;
    public GameManager gameManager;
    private AudioManager audioManager;
    private float beepTimer = 0.0f;
    private bool hasPlayedOnce = false;

    // Start is called before the first frame update
    void Start()
    {
        crosshair.transform.parent = null;
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play("HomingBulletStart");
        audioManager.Play("HomingBulletFlying");
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;

        speed = Mathf.Min(speed + acceleration * Time.deltaTime, maxSpeed);

        direction = Vector2.Lerp(direction,  (ball.transform.position - transform.position).normalized, .5f);

        transform.rotation = Quaternion.Euler(0, 0, Bullet.getAngle(Vector2.zero, direction));

        transform.Translate(direction * speed * Time.deltaTime, Space.World);

        crosshair.transform.position = Vector3.Lerp(crosshair.transform.position, new Vector3(ball.transform.position.x, ball.transform.position.y, 0), .5f);

        float bulletDistance = Vector2.Distance(transform.position, ball.transform.position);

        //Audio for beepTimer
        beepTimer += Time.deltaTime;

        if (beepTimer >= (bulletDistance / 50.0f))
        {
            beepTimer = 0.0f;   
            audioManager.Play("HomingBulletBeep");
        }

        //Audio for when the homing bullet is very close to its target.
        if (hasPlayedOnce && bulletDistance > 20.0f)
        {
            hasPlayedOnce = false;
        }
        if (bulletDistance < 8.0f && !hasPlayedOnce)
        {
            hasPlayedOnce = true;
            audioManager.Play("HomingBulletClose");
        }
        if (bulletDistance < 2.5f)
        {
            Explode();
        }
    }


    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.gameObject.tag == "Player")
    //     {
    //         Explode();
    //     }
    // }

    public void Explode()
    {
        //Removes any sound effects. Explosion dealt with in Explosion.cs
        audioManager.Stop("HomingBulletStart");
        audioManager.Stop("HomingBulletFlying");
        audioManager.Stop("HomingBulletClose");

        // gameManager.SpawnExplosion(-1, new Vector2(transform.position.x, transform.position.y + Random.Range(0, 7)));
        // gameManager.SpawnExplosion(-1, new Vector2(transform.position.x + Random.Range(-7, 7), transform.position.y));
        // gameManager.SpawnExplosion(-1, new Vector2(transform.position.x + Random.Range(-7, 7), transform.position.y));
        gameManager.SpawnExplosion(-1, transform.position);
        Destroy(crosshair);
        Destroy(gameObject);

    }
}
