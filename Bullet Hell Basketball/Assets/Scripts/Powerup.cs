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
    public Vector2 originalPosition;

    public Collider2D powerupCollider;

    // Start is called before the first frame update
    void Start()
    {
        powerupCollider = GetComponent<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector2(originalPosition.x, originalPosition.y + Mathf.Sin(Time.time * 3) * 1.2f);
    }

    public void Init(PowerupType type)
    {
        this.type = type;

        transform.GetChild((int)type).gameObject.SetActive(true);
    }

    public void ActivatePowerup()
    {
        if (type == PowerupType.HomingBullet)
        {
            gameManager.SpawnHomingBullet();
        }

        gameManager.allAlivePowerups.Remove(this);
        Destroy(gameObject);
    }

    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     if (other.gameObject.tag == "Player")
    //     {
    //         BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();



    //         // if (other.gameObject.transform.position.x < transform.position.x)
    //         // {
    //         //     playerScript.GetsHit(new Vector2(-80, 50), false);
    //         // }
    //         // else if (other.gameObject.transform.position.x >= transform.position.x)
    //         // {
    //         //     playerScript.GetsHit(new Vector2(80, 50), false);
    //         // }
    //     }
    // }
}
