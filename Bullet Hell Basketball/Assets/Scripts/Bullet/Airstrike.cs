using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Airstrike : MonoBehaviour
{

    public int numberOfTargets = 4;

    public int teamNumber;

    public float spacingBetweenTargets;

    public Material crosshairTeam0;
    public Material crosshairTeam1;

    public GameObject bulletPrefab;

    public GameManager gameManager;

    public int bulletSpeed = 10;
    public float firingInterval = .3f;

    public Material player1Mat;
    public Material player2Mat;
    public Material trailMatrialTeam1;

    public float timer;

    public List<GameObject> crosshairs;

    public int numberOfBulletsFired;

    public float startPosX;

    // Start is called before the first frame update
    void Start()
    {
        crosshairs = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= firingInterval)
        {
            timer = 0;
            if (crosshairs.Count < numberOfTargets)
            {
                GameObject g = GameObject.CreatePrimitive(PrimitiveType.Plane);
                g.transform.parent = transform;

                if (teamNumber == 0)
                {
                    g.GetComponent<MeshRenderer>().material = crosshairTeam0;
                    g.transform.position = new Vector2(startPosX - crosshairs.Count * spacingBetweenTargets, 0);
                }
                else
                {
                    g.GetComponent<MeshRenderer>().material = crosshairTeam1;
                    g.transform.position = new Vector2(-startPosX + crosshairs.Count * spacingBetweenTargets, 0);
                }
                crosshairs.Add(g);
            }
            else if (numberOfBulletsFired < numberOfTargets)
            {
                float heightPercentage = (float)(numberOfTargets - numberOfBulletsFired)/(float)numberOfTargets;
                if (teamNumber == 0)
                {
                    Bullet b = BulletSetup(crosshairs[numberOfBulletsFired].transform.position + (new Vector3(-15, 45) *  heightPercentage));
                }
                else
                {
                    Bullet b = BulletSetup(crosshairs[numberOfBulletsFired].transform.position + (new Vector3(15, 45) * heightPercentage));
                }
                numberOfBulletsFired++;
            }
            else if (numberOfBulletsFired == numberOfTargets)
            {
                for (int i = 0; i < crosshairs.Count; i++)
                {
                    if (crosshairs[i] != null)
                    {
                        Destroy(crosshairs[i]);
                        crosshairs[i] = null;
                        break;
                    }
                }
            }
        }
    }

    private Bullet BulletSetup(Vector3 spawnPosition)
    {
        GameObject newBullet = Instantiate(bulletPrefab, transform);
        newBullet.transform.position = spawnPosition;
        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.ownerNumber = teamNumber;
        bulletScript.gameManager = gameManager;

        bulletScript.timer = Vector2.Distance(crosshairs[numberOfBulletsFired].transform.position, spawnPosition) / bulletSpeed;
        bulletScript.speed = bulletSpeed;

        bulletScript.direction = (crosshairs[numberOfBulletsFired].transform.position - spawnPosition).normalized;
        bulletScript.explosive = true;

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
