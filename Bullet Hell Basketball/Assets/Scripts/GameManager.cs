using System;
using UnityEngine;
using UnityEngine.UI;

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

    public float matchTimeMax = 180;
    public float matchTimeCurrent;

    public Text matchTimeText;


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
    public GameObject tutorialSelectionScreen;
    public GameObject panelUI;

    private BulletManager[] bulletManagers;
    public int bulletLevel;
    private bool increaseLevelOnce;
    private Text bulletLevelUI;
    private Text bulletIncreaseUI;
    private float bulletTimerUI;

    public int bulletLevelUpInterval;

    public GameObject playerOneWins;
    public GameObject playerTwoWins;

    public GameObject yellowShevrons;
    public GameObject blueShevrons;
    public GameObject indicatorShevron;

    public bool gameOver;
    public bool paused;

    public bool overTime;


    //Score Tracker
    public int player1Score = 0;
    public int player2Score = 0;

    public int previousScorer = -1;

    public bool player1IsBot;
    public bool player2IsBot;

    public bool isTutorial;

    public bool IsTutorial
    {
        get { return isTutorial; }
        set
        {
            isTutorial = value;
            if (value)
            {
                matchTimeText.text = "Tutorial";
                panelUI.transform.GetChild(1).gameObject.SetActive(false);
                panelUI.transform.GetChild(0).gameObject.SetActive(false);
                paused = false;

            }
            else
            {

            }
        }
    }


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
        matchTimeCurrent = matchTimeMax;
        player1Score = 0;
        player2Score = 0;
        bulletLevel = 1;
        bulletTimerUI = 0;
        increaseLevelOnce = true;

        //lowest interval is 5 seconds.
        if (bulletLevelUpInterval <= 4)
            bulletLevelUpInterval = 30;

        bulletLevelUI = panelUI.transform.GetChild(6).GetComponent<Text>();
        bulletIncreaseUI = panelUI.transform.GetChild(5).GetComponent<Text>();

        audioManager = FindObjectOfType<AudioManager>();

        panelUI.SetActive(true);
        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = "0";
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = "0";

        //bullet level
        bulletLevelUI.text = "Bullets Level: " + bulletLevel;

        matchTimeText = panelUI.transform.GetChild(2).GetComponent<Text>();
        matchTimeText.text = TimeSpan.FromSeconds(Mathf.Max(matchTimeCurrent, 0)).ToString("m\\:ss");


        paused = true;
        gameOver = false;
        overTime = false;

        player1SpawnPosition = new Vector2(playerSpawnLocation.position.x, playerSpawnLocation.position.y);
        player2SpawnPosition = new Vector2(-playerSpawnLocation.position.x, playerSpawnLocation.position.y);
        ballSpawnPosition = new Vector2(0, ballSpawnHeight.position.y);

        player1 = Instantiate(playerPrefab);
        player1Script = player1.GetComponent<BhbPlayerController>();
        player1Script.Init(0);
        player1Script.isBot = player1IsBot;

        player2 = Instantiate(playerPrefab);
        player2Script = player2.GetComponent<BhbPlayerController>();
        player2Script.Init(1);
        player2Script.isBot = player2IsBot;

        ball = Instantiate(ballPrefab);
        ballControlScript = ball.GetComponent<Ball>();
        ballPhysicsScript = ball.GetComponent<BhbBallPhysics>();

        leftBasket = Instantiate(leftBasketPrefab);
        leftBasket.transform.position = new Vector2(basketLocation.position.x, basketLocation.position.y);
        ballControlScript.leftBasket = leftBasket;

        rightBasket = Instantiate(rightBasketPrefab);
        rightBasket.transform.position = new Vector2(-basketLocation.position.x, basketLocation.position.y);
        ballControlScript.rightBasket = rightBasket;

        //set crosshair colors
        leftBasket.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 146, 255, 255);
        rightBasket.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 0, 255);


        if (isTutorial)
            IsTutorial = true;
        BeginMatch();
    }

    private void BeginMatch()
    {
        matchTimeCurrent = matchTimeMax;

        player1Score = 0;
        player2Score = 0;
        panelUI.transform.GetChild(1).GetComponent<Text>().text = player2Score.ToString();
        panelUI.transform.GetChild(0).GetComponent<Text>().text = player1Score.ToString();

        //sets bullet level back to 1.
        bulletLevelUI.text = "Bullets Level: " + bulletLevel;

        playerOneWins.SetActive(false);
        playerTwoWins.SetActive(false);
        previousScorer = -1;
        gameOver = false;
        overTime = false;

        Bullet[] bullets = FindObjectsOfType<Bullet>();

        for (int i = 0; i < bullets.Length; i++)
        {
            if (!bullets[i].GetComponent<Bullet>().dontUpdate)
            {
                Destroy(bullets[i].gameObject);
            }
        }

        bulletManagers = FindObjectsOfType<BulletManager>();

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

        if (isTutorial)
        {
            TutorialUpdate();
            return;
        }

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

        if (!ballControlScript.IsResetting && !overTime)
        {
            matchTimeCurrent -= Time.deltaTime;
            if (matchTimeCurrent <= 10)
            {
                matchTimeText.text = Mathf.Max(matchTimeCurrent, 0).ToString("0.000");

                //changes text color to pulsing red, increases font size.
                float changingColor = Mathf.Cos(matchTimeCurrent % 2);

                matchTimeText.color = new Color(255, changingColor, changingColor);
                matchTimeText.fontSize = 100;
            }
            else
            {
                matchTimeText.text = TimeSpan.FromSeconds(Mathf.Max(matchTimeCurrent, 0)).ToString("m\\:ss");
            }
            if (matchTimeCurrent <= 0)
            {
                if (player1Score == player2Score)
                {
                    overTime = true;
                    matchTimeText.text = "Overtime";
                }
                else
                {
                    EndGame();
                }
            }
        }

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
        //TO ADD: This is where the Pause menu will appear.
        //if (Input.GetKeyDown(KeyCode.Escape))
        //menu.Pause();

        //if(player1 or 2 reaches the score limit)
        //{
        // End the game
        //}


        //Shows bullets increased UI element on regular interval.
        ShowBulletIncreaseUI();
    }

    /// <summary>
    /// Used in update every [30]seconds to show "Bullets++" to the screen.
    /// Will most likely be changed to only work after some amt of time + on a basket.
    /// </summary>
    private void ShowBulletIncreaseUI()
    {
        //If timer is at 30 second interval, show bullet increased UI element for [3] seconds.
        if ((int)matchTimeCurrent % bulletLevelUpInterval == 0 && matchTimeCurrent > 5)
        {
            if (increaseLevelOnce)
            {
                bulletLevel++;
                increaseLevelOnce = false;
            }
            bulletIncreaseUI.gameObject.SetActive(true);
            bulletLevelUI.text = "Bullets Level: " + bulletLevel;

            bulletTimerUI += Time.deltaTime;
        }
        else if (matchTimeCurrent <= 0 || bulletTimerUI >= 3)
        {
            bulletIncreaseUI.gameObject.SetActive(false);
            increaseLevelOnce = true;
            bulletTimerUI = 0;
        }

        //starts timer
        if (!increaseLevelOnce)
        {
            bulletTimerUI += Time.deltaTime;
        }
    }

    public void ToggleHowToPlay()
    {
        if (IsTutorial)
        {
            tempHud.SetActive(!tempHud.activeSelf);
            paused = !paused;
        }
        else
        {
            tempHud.SetActive(!tempHud.activeSelf);
            paused = !paused;
        }
    }

    public void EndGame()
    {
        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = player1Score.ToString();
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = player2Score.ToString();

        if (player1Score > player2Score)
            playerOneWins.SetActive(!playerOneWins.activeSelf);
        else
            playerTwoWins.SetActive(!playerTwoWins.activeSelf);

        audioManager.Play("Buzzer");



        paused = true;
        gameOver = true;
        overTime = false;
        player1Score = 0;
        player2Score = 0;
        bulletLevel = 1;
    }

    public void ResetPlayersAndBall()
    {
        if (player1Script.isBot)
            player1Script.BotRandomizeBehavior();

        if (player2Script.isBot)
            player2Script.BotRandomizeBehavior();

        player1.transform.position = player1SpawnPosition;
        player2.transform.position = player2SpawnPosition;

        player1Script.velocity = Vector2.zero;
        player2Script.velocity = Vector2.zero;

        if (previousScorer == -1)
        {
            ball.transform.position = ballSpawnPosition;
        }
        else if (previousScorer == 0)
        {
            ball.transform.position = new Vector2(player2SpawnPosition.x - 5, player2SpawnPosition.y + 10);
        }
        else if (previousScorer == 1)
        {
            ball.transform.position = new Vector2(player1SpawnPosition.x + 5, player1SpawnPosition.y + 10);
        }

        ball.transform.parent = null;
        ballPhysicsScript.velocity = Vector2.zero;

        ballPhysicsScript.simulatePhysics = true;
        ballControlScript.currentTarget = null;

        leftBasket.transform.GetChild(0).gameObject.SetActive(false);
        rightBasket.transform.GetChild(0).gameObject.SetActive(false);

        yellowShevrons.SetActive(false);
        blueShevrons.SetActive(false);

        panelUI.SetActive(true);
        matchTimeText.fontSize = 75;
        matchTimeText.color = new Color(255, 255, 255);

        bulletLevel = 1;
        bulletTimerUI = 0;
        bulletIncreaseUI.gameObject.SetActive(false);
    }

    /// <summary>
    /// Helper method, shows ball chevron if above a certain height, changes its color based on x.
    /// </summary>
    private void ShowBallChevron(bool isAboveScreen)
    {
        if (isAboveScreen)
            indicatorShevron.transform.position = new Vector3(ball.transform.position.x, 33, 0);

        indicatorShevron.SetActive(isAboveScreen);
    }

    void TutorialUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            //increment the tutorial progress
        }

        for (int i = 1; i <= 8; i++)
        {
            if (Input.GetButtonDown("J" + i + "Start"))
            {
                //increment the tutorial progress
            }
        }
    }

}
