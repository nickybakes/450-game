using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The instance of the GameManager (it's a singleton).
    /// </summary>
    public static GameManager Instance;
    private AudioManager audioManager;
    private Sound music;
    private Sound pauseMusic;
    private Sound midair;

    //TO ADD: MENU
    //[SerializeField] private MainMenu menu;

    public GameObject playerPrefab;

    public GameObject playerHeaderPrefab;
    public GameObject ballPrefab;
    public GameObject leftBasketPrefab;
    public GameObject rightBasketPrefab;

    public GameObject bulletManagerPrefab;

    public GameObject explosionPrefab;

    public GameObject homingBulletPrefab;
    public GameObject airStrikePrefab;

    public GameObject powerUpPrefab;

    public float powerUpTimeSpawnMin = 16;
    public float powerUpTimeSpawnMax = 32;
    public float powerUpTimeSpawn;
    public float powerUpTimeSpawnCurrent;

    public List<Powerup> allAlivePowerups;

    //Spawning
    public Transform playerSpawnLocation;
    public Transform basketLocation;
    public Transform ballSpawnHeight;

    public BulletLauncherData bulletLauncherData;

    private Vector2 team0SpawnPosition;
    private Vector2 team1SpawnPosition;
    private Vector2 ballSpawnPosition;

    //Players and Ball
    [HideInInspector]
    public GameObject ball;

    public float matchTimeMax = 180;
    public float matchTimeCurrent;

    public Text matchTimeText;

    public bool friendlyFireSwipe;
    public bool friendlyFireBullets;

    public GameObject[] playersTeam0;
    public GameObject[] playersTeam1;
    public BhbPlayerController[] playerScriptsTeam0;
    public BhbPlayerController[] playerScriptsTeam1;

    public BhbPlayerController currentBallOwner;

    [HideInInspector]
    public GameObject leftBasket;

    [HideInInspector]
    public GameObject rightBasket;

    [HideInInspector]

    public BhbBallPhysics ballPhysicsScript;
    [HideInInspector]

    public Ball ballControlScript;

    public float horizontalEdge = 40;

    public GameObject pausedMenuUI;
    public GameObject panelUI;
    public GameObject playerHeadersPanel;

    private BulletManager[] bulletManagers;
    public int bulletLevel;
    private bool increaseLevelOnce;
    private Text bulletLevelUI;
    private Text bulletIncreaseUI;
    private float bulletTimerUI;
    private Text dunkBonusUI;
    private int dunkBonusValue;

    public int numOfBulletLevelUps = 3;

    public float bulletLevelUpInterval;
    public float bulletLevelUpCurrentTime;

    public GameObject playerOneWins;
    public GameObject playerTwoWins;

    public GameObject yellowShevrons;
    public GameObject blueShevrons;
    public GameObject indicatorShevron;

    public bool gameOver;
    public bool paused;

    public bool overTime;


    //Score Tracker
    public int team0Score = 0;
    public int team1Score = 0;

    public int previousScorer = -1;

    public bool isTutorial;

    public TutorialManager tutorialManager;


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
        paused = true;


        allAlivePowerups = new List<Powerup>();

        bulletLevelUI = panelUI.transform.GetChild(6).GetComponent<Text>();
        bulletIncreaseUI = panelUI.transform.GetChild(5).GetComponent<Text>();
        dunkBonusUI = panelUI.transform.GetChild(7).GetComponent<Text>();

        audioManager = FindObjectOfType<AudioManager>();
        music = audioManager.Find("Music");
        pauseMusic = audioManager.Find("MusicPause");
        midair = audioManager.Find("Midair");

        panelUI.SetActive(true);
        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = "0";
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = "0";

        matchTimeText = panelUI.transform.GetChild(2).GetComponent<Text>();
        matchTimeText.text = TimeSpan.FromSeconds(Mathf.Max(matchTimeCurrent, 0)).ToString("m\\:ss");

        team0SpawnPosition = new Vector2(playerSpawnLocation.position.x, playerSpawnLocation.position.y);
        team1SpawnPosition = new Vector2(-playerSpawnLocation.position.x, playerSpawnLocation.position.y);
        ballSpawnPosition = new Vector2(0, ballSpawnHeight.position.y);

        GameData loadedData = FindObjectOfType<GameData>();
        GameData data = loadedData;


        if (loadedData == null)
        {
            GameObject gameDataObjectStandin = new GameObject("Game Data Object Standin");
            data = gameDataObjectStandin.AddComponent<GameData>();
            int numOfBotsTeam0 = 4;
            int numOfBotsTeam1 = 4;
            data.playerControlsTeam0 = new List<int>();
            data.playerNumbersTeam0 = new List<int>();
            data.playerControlsTeam1 = new List<int>();
            data.playerNumbersTeam1 = new List<int>();

            for (int i = 0; i < numOfBotsTeam0; i++)
            {
                data.playerControlsTeam0.Add(8);
                data.playerNumbersTeam0.Add(8);
            }
            for (int i = 0; i < numOfBotsTeam1; i++)
            {
                data.playerControlsTeam1.Add(8);
                data.playerNumbersTeam1.Add(8);
            }

            //uncommented this code to have 2 players on KB spawn in instead of Bots

            data.playerControlsTeam0 = new List<int>() { 0 };
            data.playerNumbersTeam0 = new List<int>() { 1 };
            data.playerControlsTeam1 = new List<int>() { 1 };
            data.playerNumbersTeam1 = new List<int>() { 2 };

            data.numOfBulletLevelUps = 3;
        }

        playersTeam0 = new GameObject[data.playerNumbersTeam0.Count];
        playerScriptsTeam0 = new BhbPlayerController[data.playerNumbersTeam0.Count];
        for (int i = 0; i < data.playerNumbersTeam0.Count; i++)
        {
            SpawnPlayer(playersTeam0, playerScriptsTeam0, data.playerControlsTeam0, data.playerControlsTeam0, i, 0);
        }

        playersTeam1 = new GameObject[data.playerNumbersTeam1.Count];
        playerScriptsTeam1 = new BhbPlayerController[data.playerNumbersTeam1.Count];
        for (int i = 0; i < data.playerNumbersTeam1.Count; i++)
        {
            SpawnPlayer(playersTeam1, playerScriptsTeam1, data.playerControlsTeam1, data.playerControlsTeam1, i, 1);
        }

        matchTimeMax = data.matchLength;

        bulletLevelUpInterval = matchTimeMax / (numOfBulletLevelUps + 1);

        ball = Instantiate(ballPrefab);
        ballControlScript = ball.GetComponent<Ball>();
        ballPhysicsScript = ball.GetComponent<BhbBallPhysics>();
        ballControlScript.gameManager = this;

        leftBasket = Instantiate(leftBasketPrefab);
        leftBasket.transform.position = new Vector2(basketLocation.position.x, basketLocation.position.y);
        ballControlScript.leftBasket = leftBasket;

        rightBasket = Instantiate(rightBasketPrefab);
        rightBasket.transform.position = new Vector2(-basketLocation.position.x, basketLocation.position.y);
        ballControlScript.rightBasket = rightBasket;

        //spawn the bullet launchers
        SpawnBulletSpawnersFromData();

        if (isTutorial)
        {
            tutorialManager.gameManager = this;
            Destroy(pausedMenuUI);
            paused = false;
            matchTimeText.text = "";
            bulletLevelUI.text = "";
            //dunkBonusUI.text = "";

            panelUI.transform.GetChild(0).gameObject.SetActive(false);
            panelUI.transform.GetChild(1).gameObject.SetActive(false);

            bulletManagers = FindObjectsOfType<BulletManager>();

            for (int i = 0; i < bulletManagers.Length; i++)
            {
                bulletManagers[i].Reset();
                bulletManagers[i].MoveToInitialPosition();
                bulletManagers[i].gameObject.SetActive(false);
            }
            tutorialManager.bulletManagers = bulletManagers;

            audioManager.Stop("Music");
            audioManager.Play("MusicTutorial");
        }
        else
        {
            Destroy(tutorialManager);
        }

        //set crosshair colors
        //leftBasket.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(0, 146, 255, 255);
        //rightBasket.transform.GetChild(0).GetComponent<SpriteRenderer>().color = new Color32(255, 255, 0, 255);

        BeginMatch();
    }

    private void SpawnPlayer(GameObject[] playerObjects, BhbPlayerController[] playerScripts, List<int> playerNumbers, List<int> controlNumbers, int index, int team)
    {
        GameObject player = Instantiate(playerPrefab);
        GameObject playerHeader = Instantiate(playerHeaderPrefab, playerHeadersPanel.transform);
        BhbPlayerController playerScript = player.GetComponent<BhbPlayerController>();
        if (playerNumbers[index] == 8)
        {
            //create bot
            playerScript.Init(playerNumbers[index], team, 0);
            playerScript.isBot = true;

            //spawn bot Header on Canvas
        }
        else
        {
            //create regular player
            playerScript.Init(playerNumbers[index], team, controlNumbers[index]);

            //spawn player Header on Canvas

        }
        playerObjects[index] = player;
        playerScripts[index] = playerScript;
        playerHeader.GetComponent<PlayerHeader>().Init(playerScript);
    }

    private void BeginMatch()
    {
        matchTimeCurrent = matchTimeMax;

        team0Score = 0;
        team1Score = 0;
        panelUI.transform.GetChild(1).GetComponent<Text>().text = team1Score.ToString();
        panelUI.transform.GetChild(0).GetComponent<Text>().text = team0Score.ToString();

        //sets bullet level and dunk value back to default.
        bulletLevelUI.text = "Bullets Level: " + bulletLevel;
        dunkBonusUI.text = "Dunk Bonus: +" + dunkBonusValue;

        if (isTutorial)
        {
            bulletLevelUI.text = "";
        }

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

        for (int i = 0; i < allAlivePowerups.Count; i++)
        {
            Destroy(allAlivePowerups[i].gameObject);
        }

        allAlivePowerups.Clear();

        bulletManagers = FindObjectsOfType<BulletManager>();

        for (int i = 0; i < bulletManagers.Length; i++)
        {
            bulletManagers[i].Reset();
        }

        bulletLevelUpCurrentTime = 0;
        bulletLevel = 1;
        increaseLevelOnce = true;

        //bullet level & Dunk value.
        bulletLevelUI.text = "Bullets Level: " + bulletLevel;
        dunkBonusValue = (bulletLevel * 2) - 2;
        dunkBonusUI.text = "Dunk Bonus: +" + dunkBonusValue;

        ballControlScript.IsResetting = false;
    }

    public void SpawnRandomPowerUp()
    {
        PowerupType type = (PowerupType)UnityEngine.Random.Range(0, 3);

        bool closeToAnotherPowerup = false;
        float yPos, xPos;
        int closeChecks = 0;
        do
        {
            yPos = UnityEngine.Random.Range(4f, 9f);
            if (UnityEngine.Random.Range(0, 2) == 0)
            {
                yPos = UnityEngine.Random.Range(16f, 32f);
            }
            xPos = UnityEngine.Random.Range(-10f, 10f);

            closeToAnotherPowerup = false;
            foreach (Powerup powerup in allAlivePowerups)
            {
                if (Vector2.Distance(new Vector2(xPos, yPos), powerup.transform.position) < 5)
                {
                    closeToAnotherPowerup = true;
                    break;
                }
            }
            closeChecks++;
        } while (closeToAnotherPowerup && closeChecks < 50);

        if (closeChecks == 50)
            return;

        GameObject p = Instantiate(powerUpPrefab, new Vector2(xPos, yPos), Quaternion.identity);

        Powerup pScript = p.GetComponent<Powerup>();
        pScript.originalPosition = new Vector2(xPos, yPos);
        pScript.gameManager = this;
        pScript.Init(type);
        allAlivePowerups.Add(pScript);
    }

    public void SpawnBulletSpawnersFromData()
    {
        if (bulletLauncherData != null)
        {
            GameObject launcher1 = Instantiate(bulletManagerPrefab);
            BulletManager launcherScript1 = launcher1.GetComponent<BulletManager>();

            launcherScript1.Init(0, bulletLauncherData.transform.position, bulletLauncherData, this);

            GameObject launcher2 = Instantiate(bulletManagerPrefab);
            BulletManager launcherScript2 = launcher2.GetComponent<BulletManager>();

            launcherScript2.Init(1, bulletLauncherData.transform.position, bulletLauncherData, this);
        }
    }


    public void SpawnExplosion(int teamNumber, Vector2 location)
    {

        GameObject explosion = Instantiate(explosionPrefab);
        explosion.transform.position = location;
        Explosion explosionScript = explosion.GetComponent<Explosion>();
        explosionScript.gameManager = this;
        explosionScript.Init(teamNumber);
    }

    public void SpawnHomingBullet()
    {
        Vector2[] startPositions = new Vector2[] { new Vector2(0, 40), new Vector2(40, 20), new Vector2(-40, 20) };
        Vector2[] directions = new Vector2[] { Vector2.down, Vector2.right, Vector2.left };

        for (int i = 0; i < startPositions.Length; i++)
        {
            GameObject bullet = Instantiate(homingBulletPrefab);
            bullet.transform.position = startPositions[i];
            HomingBullet bulletScript = bullet.GetComponent<HomingBullet>();
            bulletScript.ball = ballControlScript;
            bulletScript.direction = directions[i];
            bulletScript.gameManager = this;
        }
    }

    public void SpawnAirStrike(int teamNumber)
    {
        GameObject airStrike = Instantiate(airStrikePrefab);
        Airstrike airStrikeScript = airStrike.GetComponent<Airstrike>();
        airStrikeScript.teamNumber = teamNumber;
        airStrikeScript.gameManager = this;
    }

    void Update()
    {
        if (ball.transform.parent == null)
            currentBallOwner = null;
        //If the ball is above the screen height (will also happen when held).
        if (ball.transform.position.y > 33)
            ShowBallChevron(true);
        else
            ShowBallChevron(false);

        if (isTutorial)
        {
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                tutorialManager.DisplayNextMessage();
            }

            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetButtonDown("J" + i + "Start"))
                {
                    tutorialManager.DisplayNextMessage();
                    break;
                }
            }

            // if (player1Script.controllerNumber == -1)
            // {
            //     for (int i = 1; i <= 8; i++)
            //     {
            //         if ((Input.GetButton("J" + i + "A") || Input.GetButton("J" + i + "B") || Input.GetButton("J" + i + "X") || Input.GetButton("J" + i + "Y") || Input.GetButton("J" + i + "Start") || Mathf.Abs(Input.GetAxis("J" + i + "Horizontal")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "Vertical")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "DHorizontal")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "DVertical")) > .5f) && player2Script.controllerNumber != i)
            //             player1Script.controllerNumber = i;
            //     }
            // }
        }
        else
        {
            if (paused && Input.GetKeyDown(KeyCode.T))
            {
                Destroy(FindObjectOfType<AudioManager>().gameObject);
                SceneManager.LoadScene(1);
                return;
            }

            // if (paused && Input.GetKeyDown(KeyCode.Alpha2))
            // {
            //     player1IsBot = true;
            //     player2IsBot = true;
            //     player1Script.isBot = true;
            //     player2Script.isBot = true;
            // }
            // else if (paused && Input.GetKeyDown(KeyCode.Alpha1))
            // {
            //     player1IsBot = false;
            //     player2IsBot = true;
            //     player1Script.isBot = false;
            //     player2Script.isBot = true;
            // }
            // else if (paused && Input.GetKeyDown(KeyCode.Alpha0))
            // {
            //     player1IsBot = false;
            //     player2IsBot = false;
            //     player1Script.isBot = false;
            //     player2Script.isBot = false;
            // }


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
                bulletLevelUpCurrentTime += Time.deltaTime;
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
                    if (team0Score == team1Score)
                    {
                        overTime = true;
                        matchTimeText.text = "Overtime";
                        //play overtime music.
                        audioManager.Stop("Music");
                        audioManager.Stop("MusicPause");
                        audioManager.Play("Overtime");
                    }
                    else
                    {
                        EndGame();
                    }
                }
            }

            //Shows bullets increased UI element on regular interval.
            ShowBulletIncreaseUI();
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            SpawnExplosion(-1, new Vector2(0, 20));
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SpawnExplosion(0, new Vector2(0, 20));
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SpawnExplosion(1, new Vector2(0, 20));
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SpawnHomingBullet();
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SpawnRandomPowerUp();
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SpawnAirStrike(0);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SpawnAirStrike(1);
        }

        // if (player1Script.controllerNumber == -1)
        // {
        //     for (int i = 1; i <= 8; i++)
        //     {
        //         if ((Input.GetButton("J" + i + "A") || Input.GetButton("J" + i + "B") || Input.GetButton("J" + i + "X") || Input.GetButton("J" + i + "Y") || Input.GetButton("J" + i + "Start") || Mathf.Abs(Input.GetAxis("J" + i + "Horizontal")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "Vertical")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "DHorizontal")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "DVertical")) > .5f) && player2Script.controllerNumber != i)
        //             player1Script.controllerNumber = i;
        //     }
        // }
        // else if (player2Script.controllerNumber == -1)
        // {
        //     for (int i = 1; i <= 8; i++)
        //     {
        //         if ((Input.GetButton("J" + i + "A") || Input.GetButton("J" + i + "B") || Input.GetButton("J" + i + "X") || Input.GetButton("J" + i + "Y") || Input.GetButton("J" + i + "Start") || Mathf.Abs(Input.GetAxis("J" + i + "Horizontal")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "Vertical")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "DHorizontal")) > .5f || Mathf.Abs(Input.GetAxis("J" + i + "DVertical")) > .5f) && player1Script.controllerNumber != i)
        //             player2Script.controllerNumber = i;
        //     }
        // }
    }

    /// <summary>
    /// Used in update every [30]seconds to show "Bullets++" to the screen.
    /// Will most likely be changed to only work after some amt of time + on a basket.
    /// </summary>
    private void ShowBulletIncreaseUI()
    {
        //If timer is at 30 second interval, show bullet increased UI element for [3] seconds.
        if (bulletLevelUpCurrentTime >= bulletLevelUpInterval && matchTimeCurrent > 5)
        {
            if (increaseLevelOnce)
            {
                bulletLevel++;
                increaseLevelOnce = false;
                for (int i = 0; i < bulletManagers.Length; i++)
                {
                    bulletManagers[i].LevelUp();
                }
            }
            bulletLevelUpCurrentTime = 0;
            bulletIncreaseUI.gameObject.SetActive(true);
            bulletLevelUI.text = "Bullets Level: " + bulletLevel;
            dunkBonusValue = (bulletLevel * 2) - 2;
            dunkBonusUI.text = "Dunk Bonus: +" + dunkBonusValue;

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
        pausedMenuUI.SetActive(!pausedMenuUI.activeSelf);
        paused = !paused;

        //toggles audio.
        if (paused)
        {
            pauseMusic.source.volume = 0.1f;
            music.source.volume = 0;

            midair.source.volume = 0;
        }
        else
        {
            pauseMusic.source.volume = 0;
            music.source.volume = 0.1f;
        }
    }

    public void EndGame()
    {
        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = team0Score.ToString();
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = team1Score.ToString();

        if (team0Score > team1Score)
            playerOneWins.SetActive(!playerOneWins.activeSelf);
        else
            playerTwoWins.SetActive(!playerTwoWins.activeSelf);

        audioManager.Play("Buzzer");
        midair.source.volume = 0;

        audioManager.Stop("Overtime");
        audioManager.Play("Music");
        audioManager.Play("MusicPause");

        paused = true;
        gameOver = true;
        overTime = false;
        team0Score = 0;
        team1Score = 0;
        bulletLevel = 1;

        bulletIncreaseUI.gameObject.SetActive(false);
        bulletTimerUI = 0;
    }

    public void ResetPlayersAndBall()
    {
        for (int i = 0; i < playerScriptsTeam0.Length; i++)
        {
            if (playerScriptsTeam0[i].isBot)
                playerScriptsTeam0[i].BotRandomizeBehavior();
        }

        for (int i = 0; i < playerScriptsTeam1.Length; i++)
        {
            if (playerScriptsTeam1[i].isBot)
                playerScriptsTeam1[i].BotRandomizeBehavior();
        }

        float playerSpawnSeparationAmount = 2;
        float startingPosition0X = team0SpawnPosition.x - ((playerScriptsTeam0.Length / 2f) - .5f) * playerSpawnSeparationAmount;
        float startingPosition1X = team1SpawnPosition.x + ((playerScriptsTeam1.Length / 2f) - .5f) * playerSpawnSeparationAmount;

        for (int i = 0; i < playerScriptsTeam0.Length; i++)
        {
            playerScriptsTeam0[i].velocity = Vector2.zero;
            playerScriptsTeam0[i].autoCatchCooldownTimer = playerScriptsTeam0[i].autoCatchCooldownTimerMax;
            float posX = startingPosition0X + i * playerSpawnSeparationAmount;
            playersTeam0[i].transform.position = new Vector2(posX, team0SpawnPosition.y);

        }

        for (int i = 0; i < playerScriptsTeam1.Length; i++)
        {
            playerScriptsTeam1[i].velocity = Vector2.zero;
            playerScriptsTeam1[i].autoCatchCooldownTimer = playerScriptsTeam1[i].autoCatchCooldownTimerMax;
            float posX = startingPosition1X - i * playerSpawnSeparationAmount;
            playersTeam1[i].transform.position = new Vector2(posX, team1SpawnPosition.y);
        }

        if (previousScorer == -1)
        {
            ball.transform.position = ballSpawnPosition;
        }
        else if (previousScorer == 0)
        {
            ball.transform.position = new Vector2(team1SpawnPosition.x - 5, team1SpawnPosition.y + 10);
        }
        else if (previousScorer == 1)
        {
            ball.transform.position = new Vector2(team0SpawnPosition.x + 5, team0SpawnPosition.y + 10);
        }

        ballControlScript.lineRenderer.enabled = false;
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

    public Powerup SwipePowerupCheck(BhbPlayerController source)
    {
        for (int i = 0; i < allAlivePowerups.Count; i++)
        {
            if (source.swipeRenderer.bounds.Intersects(allAlivePowerups[i].powerupCollider.bounds) || source.playerCollider.bounds.Intersects(allAlivePowerups[i].powerupCollider.bounds))
            {
                return allAlivePowerups[i];
            }
        }
        return null;
    }


    public List<BhbPlayerController> SwipeBoundsIntersectCheck(BhbPlayerController source)
    {
        List<BhbPlayerController> victims = new List<BhbPlayerController>();
        if (source.teamNumber == 0 && friendlyFireSwipe || source.teamNumber == 1)
        {
            for (int i = 0; i < playerScriptsTeam0.Length; i++)
            {
                if (source.swipeRenderer.bounds.Intersects(playerScriptsTeam0[i].playerCollider.bounds))
                {
                    victims.Add(playerScriptsTeam0[i]);
                }
            }
        }
        if (source.teamNumber == 1 && friendlyFireSwipe || source.teamNumber == 0)
        {
            for (int i = 0; i < playerScriptsTeam1.Length; i++)
            {
                if (source.swipeRenderer.bounds.Intersects(playerScriptsTeam1[i].playerCollider.bounds))
                {
                    victims.Add(playerScriptsTeam1[i]);
                }
            }
        }
        return victims;
    }

    public bool isBallOwnerOppositeTeam(BhbPlayerController source)
    {

        if (currentBallOwner == null)
            return false;

        return currentBallOwner.teamNumber != source.teamNumber;
    }


}
