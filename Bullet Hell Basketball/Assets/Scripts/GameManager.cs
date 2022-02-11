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

    //Spawning
    public Transform playerSpawnLocation;
    public Transform leftBasketLocation;
    public Transform rightBasketLocation;

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
    public BhbPlayerController player1Script;
    [HideInInspector]
    public BhbPlayerController player2Script;
    [HideInInspector]

    public BhbBallPhysics ballPhysicsScript;
    [HideInInspector]

    public Ball ballControlScript;

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
    public bool Paused { get; set; } = true;

    // Start is called before the first frame update
    private void Start()
    {
        //TO ADD: Initialization for both basket, bullet spawners
        player1Score = 0;
        player2Score = 0;

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

        BeginRound();
    }

    private void BeginRound(){
        ResetPlayersAndBall();
    }

    private void Update()
    {
        if (Paused)
            return;


        //TO ADD: This is where the Pause menu will appear.
        //if (Input.GetKeyDown(KeyCode.Escape))
            //menu.Pause();

        //if(player1 or 2 reaches the score limit)
        //{
            // End the game
        //}
    }

    private void EndGame()
    {
        //if (winConditionMet)
            //TO ADD: A WAY TO END THIS NEVER ENDING RIDE
        //else
            //TO ADD: Player 2 win thingy
    }

    public void ResetPlayersAndBall(){
        player1.transform.position = player1SpawnPosition;
        player2.transform.position = player2SpawnPosition;
        ball.transform.position = ballSpawnPosition;

        ballPhysicsScript.simulatePhysics = true;
    }
}
