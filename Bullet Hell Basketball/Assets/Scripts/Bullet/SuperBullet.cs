using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SuperBullet : MonoBehaviour
{


    public float maxScale;

    public int maxHits;

    public int currentHits;

    public int teamNumber;

    public ParticleSystem ps;

    public float baseSpeed = 20;
    public float topSpeed = 40;

    public GameManager gameManager;

    public float timer;
    public float shrinkInterval = 3;

    public Material bulletMaterialTeam0;
    public Material bulletMaterialTeam1;
    public Material bulletTrailMaterialTeam1;

    // Start is called before the first frame update
    void Start()
    {
        currentHits = 0;
    }

    public void Init(int teamNumber, GameManager gameManager)
    {
        this.gameManager = gameManager;
        this.teamNumber = teamNumber;
        if (teamNumber == 0)
        {
            transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = bulletMaterialTeam0;
            transform.position = new Vector2(-50, 15);
            if (gameManager.wideCourt)
                transform.position = new Vector2(-60, 15);
        }
        else if (teamNumber == 1)
        {
            transform.GetChild(0).gameObject.GetComponent<MeshRenderer>().material = bulletMaterialTeam1;
            ps.GetComponent<ParticleSystemRenderer>().trailMaterial = bulletTrailMaterialTeam1;
            transform.position = new Vector2(50, 15);
            if (gameManager.wideCourt)
                transform.position = new Vector2(60, 15);
            transform.rotation = Quaternion.Euler(0, 0, 180);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gameManager.paused)
            return;


        int direction = 1;

        if (teamNumber == 1)
            direction = -1;

        float scale = Mathf.Lerp(maxScale, 1, (float)currentHits / (float)maxHits);
        transform.localScale = new Vector3(scale, scale, scale);

        transform.Translate(new Vector3(Time.deltaTime * direction * (Mathf.Lerp(baseSpeed, topSpeed, (float)currentHits / (float)maxHits)), 0, 0), Space.World);

        timer += Time.deltaTime;

        if (timer >= shrinkInterval)
        {
            Shrink();
        }
    }

    public void ForceDestroy()
    {
        gameManager.SpawnExplosion(teamNumber, transform.position);
        Destroy(gameObject);
    }

    public void Shrink()
    {
        currentHits++;
        ps.Play();

        if (currentHits == maxHits)
        {
            if (ps != null)
            {
                ps.transform.parent = null;
                ParticleSystem.MainModule module = ps.main;
                module.stopAction = ParticleSystemStopAction.Destroy;
                ps.Play();
            }
            if (timer < shrinkInterval)
                gameManager.SpawnExplosion(teamNumber, transform.position);
            Destroy(this.gameObject);
        }
        timer = 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.tag == "Player")
        {
            BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();

            if (teamNumber != playerScript.teamNumber)
            {
                if (!playerScript.IsStunned)
                    Shrink();

                // Debug.Log("Bullet hit!");
                //Insert method for when player is hit
                if (other.gameObject.transform.position.x < transform.position.x)
                {
                    playerScript.GetsHit(new Vector2(-Mathf.Lerp(70, 40, (float)currentHits / (float)maxHits), Mathf.Lerp(40, 20, (float)currentHits / (float)maxHits)), false, true);
                }
                else if (other.gameObject.transform.position.x >= transform.position.x)
                {
                    playerScript.GetsHit(new Vector2(Mathf.Lerp(70, 40, (float)currentHits / (float)maxHits), Mathf.Lerp(40, 20, (float)currentHits / (float)maxHits)), false, true);
                }
            }
        }
    }
}
