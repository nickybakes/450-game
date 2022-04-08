using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    public float timeAlive = 0;

    public int ownerNumber;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        timeAlive += Time.deltaTime;

        if (timeAlive > 2)
        {
            Destroy(gameObject);
        }
    }


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (timeAlive < .17)
        {
            if (other.gameObject.tag == "Player")
            {
                BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();

                if (ownerNumber != playerScript.teamNumber)
                {
                    if (other.gameObject.transform.position.x < transform.position.x)
                    {
                        playerScript.GetsHit(new Vector2(-80, 50), false);
                    }
                    else if (other.gameObject.transform.position.x >= transform.position.x)
                    {
                        playerScript.GetsHit(new Vector2(80, 50), false);
                    }
                }
            }

            if (other.gameObject.tag == "Ball" && other.transform.parent == null)
            {
                Ball ballScript = other.gameObject.GetComponent<Ball>();

                if (other.gameObject.transform.position.x < transform.position.x)
                {
                    ballScript.physics.velocity = new Vector2(-80, 60);
                }
                else if (other.gameObject.transform.position.x >= transform.position.x)
                {
                    ballScript.physics.velocity = new Vector2(80, 60);
                }
            }
        }
    }
}
