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

    public GameObject tempHud;
    public GameObject panelUI;

    public GameObject playerOneWins;
    public GameObject playerTwoWins;

    public GameObject yellowShevrons;
    public GameObject blueShevrons;

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


        BeginRound();
    }

    private void BeginRound()
    {
        player1Score = 0;
        player2Score = 0;

        ResetPlayersAndBall();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            if (gameOver)
            {
                playerOneWins.SetActive(false);
                playerTwoWins.SetActive(false);
                gameOver = false;
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
                ToggleHowToPlay();
                break;
            }
        }

        if (paused)
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

        if (paused)
            paused = false;
        else
            paused = true;
    }

    public void EndGame()
    {
        if (player1Score >= 10)
            playerOneWins.SetActive(!playerOneWins.activeSelf);
        if (player2Score >= 10)
            playerTwoWins.SetActive(!playerTwoWins.activeSelf);

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
}
