using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PowerupType
{
    HomingBullet,
    Airstrike,

    BulletShield
}

public class Powerup : MonoBehaviour
{

    public PowerupType type;

    public GameManager gameManager;

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();

            if (type == PowerupType.HomingBullet)
            {
                gameManager.SpawnHomingBullet();
            }

            Destroy(gameObject);

            // if (other.gameObject.transform.position.x < transform.position.x)
            // {
            //     playerScript.GetsHit(new Vector2(-80, 50), false);
            // }
            // else if (other.gameObject.transform.position.x >= transform.position.x)
            // {
            //     playerScript.GetsHit(new Vector2(80, 50), false);
            // }
        }
    }
}
