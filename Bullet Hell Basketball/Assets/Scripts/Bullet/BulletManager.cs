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
    down
}

public class BulletManager : MonoBehaviour
{
    public GameObject bullet;
    public float timer; //Controls how long between bullets this will fire
    private float maxTime;

    private float rotationAmountDegrees;
    public float rotationSpeed = 10;

    public float radius;

    public float angularSpeed;

    private Vector3 fixedPoint;
    private float currentAngle;

    public int ownerNumber = 0; //Is this owned by player 1?

    //Materials
    private MeshRenderer meshRenderer;
    public Material player1Mat;
    public Material player2Mat;

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

    //Add some sort of level up system, and different directions bullets shoot
    //Make more variables so arc movement can work
    private bool otherSide = false;

    public bool destroyOnReload; //This is for any extra spawners that need to be deleted when the game is reset

    // Start is called before the first frame update
    void Start()
    {
        //Sets a default value if the given one isn't good
        if (timer <= 0)
        {
            timer = 10.0f;
        }

        maxTime = timer;

        fixedPoint = transform.position;

        meshRenderer = GetComponent<MeshRenderer>();

        gameManager = FindObjectOfType<GameManager>();

        //Changes material based on the spawner owner
        // if(ownerNumber == 0)
         //{
        //     meshRenderer.material = player1Mat;
         //}

         //else
         //{
         //    meshRenderer.material = player2Mat;
         //}

        ogPosition = transform.position;

        if (distanceTravelled == 0)
        {
            distanceTravelled = 1;
        }

        if (startsRight)
        {
            isRight = true;
        }

        else
        {
            isRight = false;
        }
    }

    public void Reset()
    {
        timer = maxTime;
        rotationAmountDegrees = 0;
        currentAngle = 0;
        bulletPattern = BulletPatterns.front;

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
                        OneDirectionPattern(0);

                    else OneDirectionPattern(180);
                    
                    break;

                case BulletPatterns.down:
                    OneDirectionPattern(270);
                    break;
            }
            
            if(ownerNumber == 0){
                timer = Mathf.Max(maxTime - Mathf.Max((gameManager.player2Score - gameManager.player1Score)/20 , 0), 0.5f);  
                
            }

            else
            {
                timer = Mathf.Max(maxTime - Mathf.Max((gameManager.player1Score - gameManager.player2Score)/20, 0), 0.5f);
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
            Debug.Log("Player 1 Angle: " + currentAngle);
            
            if (currentAngle < 3 && !otherSide)
            {
                currentAngle -= angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle >= 3)
            {
                otherSide = true;
            }

            if (otherSide)
            {
                currentAngle += angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle <= 0 && otherSide)
            {
                otherSide = false;
            }
        }

        else
        {
            Debug.Log("Player 2 Angle: " + currentAngle);

            if (currentAngle > - 3 && !otherSide)
            {
                currentAngle -= angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle <= -3)
            {
                otherSide = true;
            }

            if (otherSide)
            {
                currentAngle += angularSpeed * Time.deltaTime;
                Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
                transform.position = fixedPoint + offset;
            }

            if (currentAngle >= 0 && otherSide)
            {
                otherSide = false;
            }
        }
    }

    
    //Method of all the shared methods that set up bullets
    private GameObject BulletSetup()
    {
        GameObject newBullet = Instantiate(bullet);
        newBullet.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0.0f);
        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.ownerNumber = ownerNumber;
        bulletScript.gameManager = gameManager;
        MeshRenderer bulletMesh = newBullet.GetComponentInChildren<MeshRenderer>();

        if (bulletScript.ownerNumber == 0)
        {
            bulletMesh.material = player1Mat;
        }

        else
        {
            bulletMesh.material = player2Mat;
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

    private void OneDirectionPattern(float angle)
    {
        GameObject newBullet = BulletSetup();
        Bullet bulletScript = newBullet.GetComponent<Bullet>();
        bulletScript.direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (angle)), Mathf.Sin(Mathf.Deg2Rad * (angle)));
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

        maxTime -= 0.5f;
        
        
        //In the last 30 seconds, random bullshit go
        /*if(gameManager.bulletLevel == 4)
        {
            bulletPattern = BulletPatterns.omni;
        }*/
    }

    /// <summary>
    /// Spawns another bullet spawner
    /// </summary>
    private void SpawnAnother()
    {
        GameObject newBulletManger = Instantiate(this.gameObject);
        BulletManager newScript = newBulletManger.GetComponent<BulletManager>();
        newScript.destroyOnReload = true;
        newScript.bulletPattern = BulletPatterns.down;

        //Its location will be determined by whether its controlled by player1 or 2
    }
}
