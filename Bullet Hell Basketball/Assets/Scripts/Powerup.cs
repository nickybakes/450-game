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

    public ParticleSystem ps;

    public GameObject bulletPrefab;
    private AudioManager audioManager;

    public Material explosiveBulletMatTeam0;
    public Material explosiveBulletMatTeam1;

    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
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

    public void ActivatePowerup(BhbPlayerController player)
    {
        if (type == PowerupType.HomingBullet)
        {
            gameManager.SpawnHomingBullet();
        }
        else if (type == PowerupType.Airstrike)
        {
            gameManager.SpawnAirStrike(player.teamNumber);
        }
        else if (type == PowerupType.BulletShield)
        {
            Vector2[] directions = new Vector2[] { new Vector2(.5f, .5f), new Vector2(0, 0), new Vector2(.5f, -.5f) };
            Vector2[] positions = new Vector2[] { new Vector2(3, 4), new Vector2(3.5f, 0), new Vector2(3, -4) };

            audioManager.Play("Shield");

            for (int i = 0; i < directions.Length; i++)
            {
                GameObject newBullet = Instantiate(bulletPrefab, player.transform);
                newBullet.transform.position = new Vector2(player.transform.position.x, player.transform.position.y + (player.height/2.0f));
                newBullet.transform.Translate(positions[i], Space.Self);
                Bullet bulletScript = newBullet.GetComponent<Bullet>();
                bulletScript.ownerNumber = player.teamNumber;
                bulletScript.gameManager = gameManager;

                bulletScript.speed = 0;
                bulletScript.timer = 99999;
                bulletScript.direction = directions[i];
                bulletScript.explosive = true;

                MeshRenderer bulletMesh = newBullet.GetComponentInChildren<MeshRenderer>();
                ParticleSystemRenderer ps = newBullet.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
                if (player.teamNumber == 0)
                {
                    bulletMesh.material = explosiveBulletMatTeam0;
                }
                else
                {
                    bulletMesh.material = explosiveBulletMatTeam1;
                }
            }
        }

        ps.transform.parent = null;
        ps.Play();

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
