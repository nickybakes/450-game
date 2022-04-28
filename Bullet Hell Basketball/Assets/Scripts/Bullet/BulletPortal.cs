using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPortal : MonoBehaviour
{

    public GameObject bulletPrefab;

    public float scaleAnimationTimeMax = .5f;
    public float scaleAnimationTimeCurrent;

    private bool scalingUp = true;

    public float intervalBetweenBulletsMax = .13f;
    private float intervalBetweenBulletsCurrent;

    public float totalTimeAliveMax = 2.5f;
    private float totalTimeAliveCurrent;

    public int bulletSpeed = 30;
    public float bulletTime = 2f;

    public float frequency = 3;

    public float sinLength = 3;

    public int teamNumber;

    public Material player1Mat;
    public Material player2Mat;
    public Material trailMatrialTeam1;

    public GameManager gameManager;

    void Awake()
    {
        transform.localScale = new Vector3(1, 0, 1);
    }

    public void Init(int teamNumber, GameManager gameManager)
    {
        this.teamNumber = teamNumber;
        this.gameManager = gameManager;

        transform.GetChild(teamNumber).gameObject.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        if (scalingUp)
        {
            if (scaleAnimationTimeCurrent <= scaleAnimationTimeMax)
            {
                scaleAnimationTimeCurrent += Time.deltaTime;
                transform.localScale = new Vector3(1, Mathf.Min(1, scaleAnimationTimeCurrent / scaleAnimationTimeMax), 1);
                return;
            }
        }
        else
        {
            if (scaleAnimationTimeCurrent > 0)
            {
                scaleAnimationTimeCurrent -= Time.deltaTime;
                transform.localScale = new Vector3(1, Mathf.Max(0, scaleAnimationTimeCurrent / scaleAnimationTimeMax), 1);
                return;
            }
            else
            {
                Destroy(gameObject);
            }
        }

        intervalBetweenBulletsCurrent += Time.deltaTime;
        totalTimeAliveCurrent += Time.deltaTime;

        if (intervalBetweenBulletsCurrent >= intervalBetweenBulletsMax)
        {
            BulletSetup(this.sinLength);
            BulletSetup(-this.sinLength);
            intervalBetweenBulletsCurrent = 0;
        }

        if (totalTimeAliveCurrent >= totalTimeAliveMax)
            ForceDestroy();
    }

    public void ForceDestroy()
    {
        scalingUp = false;
    }

    private Bullet BulletSetup(float sinLength)
    {
        GameObject newBullet = Instantiate(bulletPrefab);
        newBullet.transform.position = transform.position;
        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.ownerNumber = teamNumber;
        bulletScript.gameManager = gameManager;

        bulletScript.timer = bulletTime;
        bulletScript.speed = bulletSpeed;
        bulletScript.frequency = frequency;
        bulletScript.sinLength = sinLength;

        if (teamNumber == 0)
            bulletScript.direction = Vector2.right;
        else
            bulletScript.direction = Vector2.left;

        bulletScript.movement = BulletMovement.sine;

        MeshRenderer bulletMesh = newBullet.GetComponentInChildren<MeshRenderer>();
        ParticleSystemRenderer ps = newBullet.transform.GetChild(1).GetComponent<ParticleSystemRenderer>();

        if (bulletScript.ownerNumber == 0)
        {
            bulletMesh.material = player1Mat;
        }
        else
        {
            bulletMesh.material = player2Mat;
            ps.trailMaterial = trailMatrialTeam1;
        }

        return bulletScript;
    }
}
