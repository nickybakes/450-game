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
    private Vector3 ogPosition;
    public float distanceTravelled;
    private Vector3 newPosition;
    public bool startsRight;
    public bool isRight;
    private bool reachedOppositeSide = false;

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
        // {
        //     meshRenderer.material = player1Mat;
        // }

        // else
        // {
        //     meshRenderer.material = player2Mat;
        // }

        ogPosition = transform.position;

        if(distanceTravelled == 0)
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

    public void Reset(){
        timer = maxTime;
        rotationAmountDegrees = 0;
        currentAngle = 0;
    }

    private void FixedUpdate()
    {
        if (gameManager.paused)
            return;

        timer -= Time.deltaTime;

        rotationAmountDegrees += Time.deltaTime * rotationSpeed;

        if (timer <= 0)
        {
            for (int i = 0; i < 360; i += 90)
            {
                GameObject newBullet = Instantiate(bullet);
                newBullet.transform.position = new Vector3(gameObject.transform.position.x, gameObject.transform.position.y, 0.0f);

                Bullet bulletScript = newBullet.GetComponent<Bullet>();
                bulletScript.ownerNumber = ownerNumber;
                bulletScript.gameManager = gameManager;
                bulletScript.direction = new Vector2(Mathf.Cos(Mathf.Deg2Rad * (rotationAmountDegrees + i)), Mathf.Sin(Mathf.Deg2Rad * (rotationAmountDegrees + i)));

                MeshRenderer bulletMesh = newBullet.GetComponent<MeshRenderer>();

                if (bulletScript.ownerNumber == 0)
                {
                    bulletMesh.material = player1Mat;
                }

                else
                {
                    bulletMesh.material = player2Mat;
                }


                timer = maxTime;
            }

        }

        //Spawner rotation 
        //Source: https://forum.unity.com/threads/circular-movement.572797/
        
        //Changes how the spawner moves based on the enum set
        switch (movement)
        {
            case Movement.circle:
                moveAroundPoint();
                break;

            case Movement.sideToSide:
                sideToSide();  
                break;
        }
        
        
    }

    //Spawner movement helper methods
    private void moveAroundPoint()
    {
        currentAngle += angularSpeed * Time.deltaTime;
        Vector3 offset = new Vector3(Mathf.Sin(currentAngle), Mathf.Cos(currentAngle), fixedPoint.z) * radius;
        transform.position = fixedPoint + offset;
    }

    private void sideToSide()
    {
        if((startsRight || isRight) && distanceTravelled > -1)
        {
            distanceTravelled *= -1;
        }
        
        newPosition = new Vector3(ogPosition.x + distanceTravelled, ogPosition.y, ogPosition.z);

        if((isRight && !startsRight) || (!isRight && startsRight))
        {
            transform.position = Vector3.MoveTowards(transform.position, ogPosition, 0.1f);
        }

        else
        {
            transform.position = Vector3.MoveTowards(transform.position, newPosition, 0.1f);
        }

        //Changes direction
        if(transform.position == newPosition)
        {
            reachedOppositeSide = true;
            isRight = reverseBool(isRight);
        }

        if(transform.position == ogPosition && reachedOppositeSide)
        {
            reachedOppositeSide = false;
            isRight = reverseBool(isRight);
            distanceTravelled *= -1;
        }

        
    }

    /// <summary>
    /// Sets the bool to the opposite value 
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
}
