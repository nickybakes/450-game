using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int speed; //Multiplies this by time.deltaTime to increase the speed
    public Vector2 direction;
    public float timer;
    public int ownerNumber = 0;

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
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(direction * Time.deltaTime * speed, Space.World);
    }

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
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

    private void OnTriggerEnter(Collider other)
    {
        BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();

        if (other.gameObject.tag == "Player")
        {
            if (ownerNumber != playerScript.playerNumber)
            {
                // Debug.Log("Bullet hit!");
                //Insert method for when player is hit
                if (other.gameObject.transform.position.x < transform.position.x)
                {
                    playerScript.GetsHit(new Vector2(-40, 20));
                }
                else if (other.gameObject.transform.position.x >= transform.position.x)
                {
                    playerScript.GetsHit(new Vector2(40, 20));
                }
                Destroy(this.gameObject);
            }
        }
    }

    /*private void OnCollisionEnter2D(Collision2D collision)
    {

    }*/
}
