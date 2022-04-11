using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Explosion : MonoBehaviour
{

    public float timeAlive = 0;

    public int ownerNumber = -1;

    private ParticleSystem ps;
    private ParticleSystemRenderer psRenderer;
    private ParticleSystemRenderer psRenderer1;
    private ParticleSystemRenderer psRenderer2;

    private CameraShake cameraShake;

    public Material[] materialsTeam0;
    public Material[] materialsTeam1;

    public GameManager gameManager;
    private AudioSource explosion;

    private void Awake()
    {
        explosion = GetComponent<AudioSource>();
        explosion.pitch = Random.Range(0.9f, 1.1f);
    }

    public void Init(int ownerNumber)
    {
        ps = GetComponent<ParticleSystem>();
        psRenderer = GetComponent<ParticleSystemRenderer>();
        psRenderer1 = transform.GetChild(0).GetComponent<ParticleSystemRenderer>();
        psRenderer2 = transform.GetChild(1).GetComponent<ParticleSystemRenderer>();
        cameraShake = FindObjectOfType<Camera>().GetComponent<CameraShake>();
        StartCoroutine(cameraShake.Shake(.2f, .5f));

        this.ownerNumber = ownerNumber;
        this.timeAlive = 0;
        if (this.ownerNumber == 0)
        {
            psRenderer.material = materialsTeam0[0];
            psRenderer1.material = materialsTeam0[1];
            psRenderer2.material = materialsTeam0[2];
            psRenderer2.trailMaterial = materialsTeam0[3];
        }
        if (this.ownerNumber == 1)
        {
            psRenderer.material = materialsTeam1[0];
            psRenderer1.material = materialsTeam1[1];
            psRenderer2.material = materialsTeam1[2];
            psRenderer2.trailMaterial = materialsTeam1[3];
        }
    }

    // Update is called once per frame
    void Update()
    {

        if (gameManager.paused)
        {
            return;
        }

        this.timeAlive += Time.deltaTime;

        // if (timeAlive > 2)
        // {
        //     Destroy(gameObject);
        // }
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (timeAlive > .06f && timeAlive < .24f)
        {
            if (other.gameObject.tag == "Player")
            {
                BhbPlayerController playerScript = other.gameObject.GetComponent<BhbPlayerController>();

                if (ownerNumber == -1 || ownerNumber != playerScript.teamNumber)
                {
                    if (other.gameObject.transform.position.x < transform.position.x)
                    {
                        playerScript.GetsHit(new Vector2(-80, 85), false);
                    }
                    else if (other.gameObject.transform.position.x >= transform.position.x)
                    {
                        playerScript.GetsHit(new Vector2(80, 85), false);
                    }
                    playerScript.stunTimeCurrent = -playerScript.stunTimeMax;
                }
            }

            if (other.gameObject.tag == "Ball")
            {
                Ball ballScript = other.gameObject.GetComponent<Ball>();
                ballScript.physics.simulatePhysics = true;
                other.transform.parent = null;

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
