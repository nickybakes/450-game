using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int speed; //Multiplies this by time.deltaTime to increase the speed
    public Vector2 direction;
    public float timer;
    public int ownerNumber = 0;

    //used for the ball turning into a bullet
    public bool dontUpdate = false;

    public GameManager gameManager;

    public ParticleSystem ps;

    public bool explosive;

    // Start is called before the first frame update
    void Start()
    {
        if (speed <= 0)
        {
            speed = 1;
        }

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
        else
        {
            transform.rotation = Quaternion.Euler(0, 0, getAngle(Vector2.zero, direction));
        }

        if (dontUpdate)
            return;

        if (gameManager.paused)
            return;

        // if (transform.parent != null)
        // {
        //     transform.localRotation = Quaternion.Inverse(transform.parent.rotation);
        // }



        transform.Translate(direction * Time.deltaTime * speed, Space.World);
    }

    private void FixedUpdate()
    {
        if (dontUpdate)
            return;

        if (gameManager.paused)
            return;

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

    /*private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Player")
        {
            Debug.Log("Bullet hit!");
            Destroy(this.gameObject);
        }
    }*/

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

    /*private void OnCollisionEnter2D(Collision2D collision)
    {

    }*/
}
