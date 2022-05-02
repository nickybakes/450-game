using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletMovement
{
    straight,
    sine,
    heatSeeking
}


public class Bullet : MonoBehaviour
{
    public float speed; //Multiplies this by time.deltaTime to increase the speed
    public Vector2 direction;
    public float timer = 3;
    private float timeAlive;
    public int ownerNumber = 0;

    //used for the ball turning into a bullet
    public bool dontUpdate = false;

    public GameManager gameManager;

    public ParticleSystem ps;
    public bool isBig = false; //Will this be a big bullet

    public BulletMovement movement;

    public float sinLength;

    public float frequency; //Frequency of the sin wave

    private Vector3 ogPosition;

    private Vector2 normal;
    private Vector2 offset;

    public bool explosive;

    // Start is called before the first frame update
    void Start()
    {
        ogPosition = transform.position;
        timeAlive = 0;

        //Sets a default value if the given one isn't good
        if (timer <= 0)
        {
            timer = 10.0f;
        }

        if (dontUpdate)
            gameManager = FindObjectOfType<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (explosive)
            transform.GetChild(2).gameObject.SetActive(true);

        if (dontUpdate && gameManager.ballControlScript.IsBullet)
        {
            if (transform.parent != null)
            {
                transform.localRotation = Quaternion.Inverse(transform.parent.rotation);
            }
            transform.rotation = Quaternion.Euler(0, 0, getAngle(Vector2.zero, gameManager.ballPhysicsScript.velocity));
        }
        
        else if(movement == BulletMovement.sine)
        {
            transform.rotation = Quaternion.Euler(0, 0, getAngle(Vector2.zero, direction + offset));
        }
        
        else
        {
            if (transform.parent != null)
            {
                transform.rotation = Quaternion.Euler(0, transform.parent.rotation.eulerAngles.y, getAngle(Vector2.zero, direction));
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, 0, getAngle(Vector2.zero, direction));
            }
        }

        if (dontUpdate)
            return;

        if (gameManager.paused)
            return;

        // if (transform.parent != null)
        // {
        //     transform.localRotation = Quaternion.Inverse(transform.parent.rotation);
        // }



        switch (movement)
        {
            case BulletMovement.straight:
                transform.Translate(direction * Time.deltaTime * speed, Space.World);
                break;

            case BulletMovement.sine:
                //Get the normal of direction multiply by length then both by the sin of the time alive
                normal = perpCW(direction);
                offset = (normal * sinLength) * Mathf.Sin(timeAlive * frequency);
                transform.Translate((direction + offset) *  Time.deltaTime * speed, Space.World);
                break;

            /*case BulletMovement.heatSeeking:
                //Find the position of the basketball
                Transform ball = FindObjectOfType<Ball>().gameObject.transform;
                transform.Translate(ball.position * Time.deltaTime * speed, Space.World);
                break;*/
        }
        
    }

    private void FixedUpdate()
    {
        if (dontUpdate)
            return;

        if (gameManager.paused)
            return;

        timeAlive += Time.deltaTime;
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            if (ps != null)
            {
                ps.transform.parent = null;
                ps.Play();
            }
            if (explosive)
                gameManager.SpawnExplosion(ownerNumber, transform.position);
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();

            if (ownerNumber != playerScript.teamNumber)
            {
                // Debug.Log("Bullet hit!");
                //Insert method for when player is hit
                if (!explosive)
                {
                    if (other.gameObject.transform.position.x < transform.position.x)
                    {
                        playerScript.GetsHit(new Vector2(-40, 20), false, false);
                    }
                    else if (other.gameObject.transform.position.x >= transform.position.x)
                    {
                        playerScript.GetsHit(new Vector2(40, 20), false, false);
                    }
                }


                if (!dontUpdate)
                {
                    if (ps != null)
                    {
                        ps.transform.parent = null;
                        ps.Play();
                    }
                    if (explosive)
                        gameManager.SpawnExplosion(ownerNumber, transform.position);
                    Destroy(this.gameObject);
                }
            }
        }
    }

    public static float getAngle(Vector2 me, Vector2 target)
    {
        return Mathf.Atan2(target.y - me.y, target.x - me.x) * (180 / Mathf.PI);
    }


    /// <summary>
    /// gets the perpidicular vector to this one, going around clock wise
    /// </summary>
    /// <param name="vector">The vector we want to modify</param>
    /// <returns></returns>
    private Vector2 perpCW(Vector2 vector)
    {
        return new Vector2(vector.y, -1 * vector.x);
    }

}
