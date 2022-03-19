using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The instance of the GameManager (it's a singleton).
    /// </summary>
    public static GameManager Instance;
    private AudioManager audioManager;

    //TO ADD: MENU
    //[SerializeField] private MainMenu menu;

    public GameObject playerPrefab;
    public GameObject ballPrefab;
    public GameObject leftBasketPrefab;
    public GameObject rightBasketPrefab;

    //Spawning
    public Transform playerSpawnLocation;
    public Transform basketLocation;
    public Transform ballSpawnHeight;


    private Vector2 player1SpawnPosition;
    private Vector2 player2SpawnPosition;
    private Vector2 ballSpawnPosition;

    //Players and Ball
    [HideInInspector]
    public GameObject player1;
    [HideInInspector]
    public GameObject player2;
    [HideInInspector]
    public GameObject ball;


    [HideInInspector]
    public GameObject leftBasket;

    [HideInInspector]
    public GameObject rightBasket;

    [HideInInspector]
    public BhbPlayerController player1Script;
    [HideInInspector]
    public BhbPlayerController player2Script;
    [HideInInspector]

    public BhbBallPhysics ballPhysicsScript;
    [HideInInspector]

    public Ball ballControlScript;

    public float horizontalEdge = 40;

    public GameObject tempHud;
    public GameObject panelUI;

    public GameObject playerOneWins;
    public GameObject playerTwoWins;

    public GameObject yellowShevrons;
    public GameObject blueShevrons;
    public GameObject indicatorShevron;

    public bool gameOver;
    public bool paused;


    //Score Tracker
    public int player1Score = 0;
    public int player2Score = 0;


    [HideInInspector] public bool winConditionMet = false;

    // Set up singleton here
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    /// <summary>
    /// Is the game paused?
    /// </summary>
    public bool Paused { get; set; } = false;

    // Start is called before the first frame update
    private void Start()
    {
        //TO ADD: Initialization for both basket, bullet spawners
        player1Score = 0;
        player2Score = 0;

        audioManager = FindObjectOfType<AudioManager>();

        panelUI.SetActive(true);
        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = "0";
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = "0";

        paused = true;
        gameOver = false;

        player1SpawnPosition = new Vector2(playerSpawnLocation.position.x, playerSpawnLocation.position.y);
        player2SpawnPosition = new Vector2(-playerSpawnLocation.position.x, playerSpawnLocation.position.y);
        ballSpawnPosition = new Vector2(0, ballSpawnHeight.position.y);

        player1 = Instantiate(playerPrefab);
        player1Script = player1.GetComponent<BhbPlayerController>();
        player1Script.Init(0);

        player2 = Instantiate(playerPrefab);
        player2Script = player2.GetComponent<BhbPlayerController>();
        player2Script.Init(1);

        ball = Instantiate(ballPrefab);
        ballControlScript = ball.GetComponent<Ball>();
        ballPhysicsScript = ball.GetComponent<BhbBallPhysics>();

        leftBasket = Instantiate(leftBasketPrefab);
        leftBasket.transform.position = new Vector2(basketLocation.position.x, basketLocation.position.y);
        ballControlScript.leftBasket = leftBasket;

        rightBasket = Instantiate(rightBasketPrefab);
        rightBasket.transform.position = new Vector2(-basketLocation.position.x, basketLocation.position.y);
        ballControlScript.rightBasket = rightBasket;
        rightBasket.gameObject.transform.localScale = new Vector3(-1, 1, 1);

        //set crosshair colors
        leftBasket.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 146, 255, 255);
        rightBasket.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 0, 255);

        BeginMatch();
    }

    private void BeginMatch()
    {
        player1Score = 0;
        player2Score = 0;
        playerOneWins.SetActive(false);
        playerTwoWins.SetActive(false);
        gameOver = false;

        Bullet[] bullets = FindObjectsOfType<Bullet>();

        for (int i = 0; i < bullets.Length; i++)
        {
            if (!bullets[i].GetComponent<Bullet>().dontUpdate)
            {
                Destroy(bullets[i].gameObject);
            }
        }

        BulletManager[] bulletManagers = FindObjectsOfType<BulletManager>();

        for (int i = 0; i < bulletManagers.Length; i++)
        {
            bulletManagers[i].Reset();
        }

        ResetPlayersAndBall();
    }

    void Update()
    {
        //If the ball is above the screen height (will also happen when held).
        if (ball.transform.position.y > 33)
            ShowBallChevron(true);
        else
            ShowBallChevron(false);

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (gameOver)
            {
                BeginMatch();
                paused = false;
            }
            else
                ToggleHowToPlay();

            panelUI.SetActive(true);
        }

        for (int i = 1; i <= 8; i++)
        {
            if (Input.GetButtonDown("J" + i + "Start"))
            {
                if (gameOver)
                {
                    BeginMatch();
                    paused = false;
                }
                else
                    ToggleHowToPlay();
                break;
            }
        }

        if (paused || gameOver)
            return;

        if (player1Script.controllerNumber == -1)
        {
            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetButton("J" + i + "A") && player2Script.controllerNumber != i)
                    player1Script.controllerNumber = i;
            }
        }

        if (player2Script.controllerNumber == -1)
        {
            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetButton("J" + i + "A") && player1Script.controllerNumber != i)
                    player2Script.controllerNumber = i;
            }
        }


        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = player1Score.ToString();
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = player2Score.ToString();

        //TO ADD: This is where the Pause menu will appear.
        //if (Input.GetKeyDown(KeyCode.Escape))
        //menu.Pause();

        //if(player1 or 2 reaches the score limit)
        //{
        // End the game
        //}
    }

    public void ToggleHowToPlay()
    {
        tempHud.SetActive(!tempHud.activeSelf);
        paused = !paused;
    }

    public void EndGame()
    {
        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = player1Score.ToString();
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = player2Score.ToString();

        if (player1Score >= 10)
            playerOneWins.SetActive(!playerOneWins.activeSelf);
        if (player2Score >= 10)
            playerTwoWins.SetActive(!playerTwoWins.activeSelf);

        audioManager.Play("Buzzer");

        paused = true;
        gameOver = true;
        player1Score = 0;
        player2Score = 0;
    }

    public void ResetPlayersAndBall()
    {
        player1.transform.position = player1SpawnPosition;
        player2.transform.position = player2SpawnPosition;
        ball.transform.position = ballSpawnPosition;

        ball.transform.parent = null;
        ballPhysicsScript.velocity = Vector2.zero;

        ballPhysicsScript.simulatePhysics = true;
        ballControlScript.currentTarget = null;

        leftBasket.transform.GetChild(0).gameObject.SetActive(false);
        rightBasket.transform.GetChild(0).gameObject.SetActive(false);

        yellowShevrons.SetActive(false);
        blueShevrons.SetActive(false);

        panelUI.SetActive(true);
    }

    /// <summary>
    /// Helper method, shows ball chevron if above a certain height, changes its color based on x.
    /// </summary>
    private void ShowBallChevron(bool isAboveScreen)
    {
        if (isAboveScreen)
        {
            indicatorShevron.transform.position = new Vector3(ball.transform.position.x, 33, 0);

            //if (transform.position.x > 0)
            //shevronIndicator.GetComponent<Shader>().SetColor(0, Color.yellow);
            //else
            //shevronIndicator.GetComponent<Shader>().SetColor(0, Color.blue);
        }

        indicatorShevron.SetActive(isAboveScreen);
    }
}
