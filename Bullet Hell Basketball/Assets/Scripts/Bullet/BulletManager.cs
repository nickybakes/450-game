using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Movement
{
    circle,
    sideToSide,
    arc,
    upDown,
    none
}

public enum BulletPatterns
{
    omni,
    front,
    down,
    angular
}

public class BulletManager : MonoBehaviour
{
    public GameObject bullet;
    private float timer; //Controls how long between bullets this will fire
    public float maxTime;

    private float originalMaxTime;

    private float rotationAmountDegrees;
    public float rotationSpeed = 10;

    public float radius;

    public float angularSpeed;

    private Vector3 fixedPoint;
    private float currentAngle;

    public float bulletSeperationAngle = 5f;


    public int ownerNumber = 0; //Is this owned by player 1?

    //Materials
    public Material player1Mat;
    public Material player2Mat;
    public Material trailMatrialTeam1;

    public GameManager gameManager;

    //Movement
    public Movement movement;
    public BulletPatterns bulletPattern;
    private Vector3 ogPosition;
    public float distanceTravelled;
    private Vector3 newPosition;
    public bool startsRight;
    private bool isRight;
    private bool reachedOppositeSide = false;

    private int numBullets; //How many bullets will be spawned
    public float minAngle = 0;
    public float maxAngle = 45;
    public int initialNumberOfBullets = 1; //Eventually input from bullet spawner data
    private BulletMovement bulletMovement;

    private float sinWaveLength;
    private float sinWaveFrequency;


    //Add some sort of level up system, and different directions bullets shoot
    //Make more variables so arc movement can work
    private bool otherSide = false;

    private GameObject mesh;

    public bool destroyOnReload; //This is for any extra spawners that need to be deleted when the game is reset

    // Start is called before the first frame update
    void Start()
    {
        if (distanceTravelled == 0)
        {
            distanceTravelled = 1;
        }
    }

    public void Init(int playerNumber, Vector2 pos, BulletLauncherData data, GameManager manager)
    {
        gameManager = manager;
        this.movement = data.movement;
        this.bulletPattern = data.bulletPattern;
        this.radius = data.arcMovementRadius;
        this.rotationSpeed = data.rotationSpeed;
        this.angularSpeed = data.moveFullCircleSpeed;
        this.bulletSeperationAngle = data.bulletSeperationAngle;
        this.minAngle = data.arcMinAngle;
        this.maxAngle = data.arcMaxAngle;
        this.bulletMovement = data.bulletMovement;
        this.sinWaveLength = data.sinWaveLength;
        this.sinWaveFrequency = data.sinWaveFrequency;


        timer = data.initialTimeBetweenBullets;
        maxTime = data.initialTimeBetweenBullets;
        originalMaxTime = data.initialTimeBetweenBullets;

        numBullets = data.initialNumberOfBullets;

        this.ownerNumber = playerNumber;

        //flip everything around for the other team
        if (ownerNumber == 1)
        {
            isRight = true;
            this.rotationSpeed = -this.rotationSpeed;
            this.angularSpeed = -this.angularSpeed;
            this.minAngle = -this.minAngle;
            pos.x = -pos.x;
        }

        transform.position = pos;
        fixedPoint = pos;
        ogPosition = pos;

        transform.GetChild(1 - playerNumber).gameObject.SetActive(false);
        mesh = transform.GetChild(playerNumber).gameObject;
    }

    public void Reset()
    {
        maxTime = originalMaxTime;
        timer = maxTime;
        rotationAmountDegrees = 0;
        currentAngle = 0;
        bulletPattern = BulletPatterns.front;
        numBullets = initialNumberOfBullets;

        //Add new patterns
        transform.position = fixedPoint;

    }

    private void FixedUpdate()
    {

        if (gameManager.paused)
            return;

        timer -= Time.deltaTime;

        rotationAmountDegrees += Time.deltaTime * rotationSpeed;

        if (timer <= 0)
        {


            //Changes the bullet pattern
            switch (bulletPattern)
            {
                case BulletPatterns.omni:
                    OmniPattern();
                    break;

                case BulletPatterns.front:

                    if (ownerNumber == 0)
                        AngularPattern();

                    else AngularPattern();

                    break;

                case BulletPatterns.angular:

                    if (ownerNumber == 0)
                        AngularPattern();

                    else AngularPattern();

                    break;

                case BulletPatterns.down:
                    AngularPattern();
                    break;
            }

            if (ownerNumber == 0)
            {
                timer = Mathf.Max(maxTime - Mathf.Max((gameManager.team1Score - gameManager.team0Score) / 13, 0), 0.25f);
            }
            
            else
            {
                timer = Mathf.Max(maxTime - Mathf.Max((gameManager.team0Score - gameManager.team1Score) / 13, 0), 0.25f);
            }



            //timer = maxTime;
        }

        //IncreaseBulletSpawn();

        //Spawner rotation 
        //Source: https://forum.unity.com/threads/circular-movement.572797/

        //Changes how the spawner moves based on the enum set
        switch (movement)
        {
            case Movement.circle:
                moveAroundPoint();
                break;

            case Movement.sideToSide:
                sideToSide(true);
                break;

            case Movement.upDown:
                sideToSide(false);
                break;

            case Movement.arc:
                ArcMovement();
                break;
        }
    }

    //Spawner movement helper methods
    private void moveAroundPoint()
    {
        currentAngle += angularSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
        transform.position = fixedPoint + offset;
        //Debug.Log(ownerNumber + ": " +currentAngle);
    }

    private void sideToSide(bool goingX)
    {
        if ((startsRight || isRight) && distanceTravelled > -1)
        {
            distanceTravelled *= -1;
        }

        if (goingX) newPosition = new Vector3(ogPosition.x + distanceTravelled, ogPosition.y, ogPosition.z);
        else newPosition = new Vector3(ogPosition.x, ogPosition.y + distanceTravelled, ogPosition.z);

        if ((isRight && !startsRight) || (!isRight && startsRight))
        {
            transform.position = Vector3.MoveTowards(transform.position, ogPosition, 0.1f);
        }

        else
        {
            transform.position = Vector3.MoveTowards(transform.position, newPosition, 0.1f);
        }

        //Changes direction
        if (transform.position == newPosition)
        {
            reachedOppositeSide = true;
            isRight = reverseBool(isRight);
        }

        if (transform.position == ogPosition && reachedOppositeSide)
        {
            reachedOppositeSide = false;
            isRight = reverseBool(isRight);
            distanceTravelled *= -1;
        }


    }

    private void ArcMovement()
    {

        if (rotationSpeed < 0)
        {
            //Debug.Log("Player 1 Angle: " + currentAngle);

            if (currentAngle < maxAngle * Mathf.Deg2Rad && !otherSide)
            {
                currentAngle -= angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            //Turns it around
            if (currentAngle >= maxAngle * Mathf.Deg2Rad)
            {
                otherSide = true;
            }

            if (otherSide)
            {
                currentAngle += angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle <= minAngle * Mathf.Deg2Rad && otherSide)
            {
                otherSide = false;
            }
        }

        else
        {
            //Debug.Log("Player 2 Angle: " + currentAngle);

            if (currentAngle > -maxAngle * Mathf.Deg2Rad && !otherSide)
            {
                currentAngle -= angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle <= -maxAngle * Mathf.Deg2Rad)
            {
                otherSide = true;
            }

            if (otherSide)
            {
                currentAngle += angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle >= minAngle * Mathf.Deg2Rad && otherSide)
            {
                otherSide = false;
            }
        }

        mesh.transform.rotation = Quaternion.Euler(0, 0, getAngle(transform.position, fixedPoint));
        mesh.transform.GetChild(0).Rotate(new Vector3((200 + (120 * (gameManager.bulletLevel - 1))) * Time.deltaTime, 0, 0), Space.Self);
    }


    /// <summary>
    /// Method of all the shared methods that set up bullets Makes one bullet
    /// </summary>
    /// <returns></returns>
    private GameObject BulletSetup()
    {
        GameObject newBullet = Instantiate(bullet);
        newBullet.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0.0f);
        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.ownerNumber = ownerNumber;
        bulletScript.gameManager = gameManager;
        bulletScript.movement = bulletMovement;

        bulletScript.sinLength = sinWaveLength;
        bulletScript.frequency = sinWaveFrequency;

        if (gameManager.randomBigBullets && gameManager.bulletLevel >= 2)
        {
            //Make the bullet RNG more dynamic
            int randomNumber = Random.Range(1, 6 + gameManager.bulletLevel);

            //Debug.Log(randomNumber); 

            if (randomNumber >= 6)
            {
                bulletScript.isBig = true;
            }
        }

        //Also mess with the speed of the big bullets
        if (ownerNumber == 0)
        {
            bulletScript.timer = 4 + Mathf.Max((gameManager.team1Score - gameManager.team0Score) / 13, 0);
            bulletScript.speed = 10 + Mathf.Max((gameManager.team1Score - gameManager.team0Score) / 30, 0) + gameManager.bulletLevel;
        }
        else
        {
            bulletScript.timer = 4 + Mathf.Max((gameManager.team0Score - gameManager.team1Score) / 13, 0);
            bulletScript.speed = 10 + Mathf.Max((gameManager.team0Score - gameManager.team1Score) / 30, 0) + gameManager.bulletLevel;
        }

        if (gameManager.allBigBullets)
        {
            bulletScript.isBig = true;
        }

        if (bulletScript.isBig)
        {
            newBullet.transform.localScale = new Vector3(gameManager.bigBulletScale, gameManager.bigBulletScale, gameManager.bigBulletScale);
            bulletScript.speed = bulletScript.speed / 5 * 3;
        }


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

        return newBullet;
    }



    //Bullet patters
    private void OmniPattern()
    {
        for (int i = 0; i < 360; i += 90)
        {
            GameObject newBullet = BulletSetup();
            Bullet bulletScript = newBullet.GetComponent<Bullet>();
            bulletScript.direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (rotationAmountDegrees + i)), Mathf.Sin(Mathf.Deg2Rad * (rotationAmountDegrees + i)));
        }
    }

    private void AngularPattern()
    {
        float bulletAngle = getAngle(transform.position, fixedPoint);
        //Debug.Log(bulletAngle);
        bulletAngle += Random.Range(-10, 10);
        float startingAngle = bulletAngle - ((numBullets / 2f) - .5f) * bulletSeperationAngle;

        for (int i = 0; i < numBullets; i++)
        {
            GameObject newBullet = BulletSetup();
            Bullet bulletScript = newBullet.GetComponent<Bullet>();

            float direction = startingAngle + i * bulletSeperationAngle;

            bulletScript.direction = new Vector2(Mathf.Cos(direction * Mathf.Deg2Rad), Mathf.Sin(direction * Mathf.Deg2Rad));
        }
    }



    /// <summary>
    /// Sets the bool to the opposite value Maybe work on this when I have more time
    /// </summary>
    /// <param name="value">The boolean to be changed</param>
    private bool reverseBool(bool value)
    {
        if (value)
        {
            value = false;
        }

        else
        {
            value = true;
        }

        return value;
    }

    public void LevelUp()
    {

        //maxTime -= 0.5f;
        numBullets++;

        //In the last 30 seconds, random bullshit go
        /*if(gameManager.bulletLevel == 4)
        {
            bulletPattern = BulletPatterns.omni;
        }*/
    }

    /// <summary>
    /// Spawns another bullet spawner
    /// </summary>
    /*private void SpawnAnother()
    {
        GameObject newBulletManger = Instantiate(this.gameObject);
        BulletManager newScript = newBulletManger.GetComponent<BulletManager>();
        newScript.destroyOnReload = true;
        newScript.bulletPattern = BulletPatterns.down;

        //Its location will be determined by whether its controlled by player1 or 2


        //Add some more randomness later
    }*/
    
    public float getAngle(Vector2 me, Vector2 target)
    {
        return Mathf.Atan2(target.y - me.y, target.x - me.x) * (180 / Mathf.PI);
    }

    public void MoveToInitialPosition()
    {
        FixedUpdate();
    }
}
