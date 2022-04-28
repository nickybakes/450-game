using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System.Collections;

public enum Gamemode
{
    Exhibition,
    Tutorial,
    Rally
}

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The instance of the GameManager (it's a singleton).
    /// </summary>
    public static GameManager Instance;
    public Camera cam;
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

    public GameObject superBulletPrefab;
    public GameObject bulletPortalPrefab;

    public GameObject powerUpPrefab;

    public float powerUpTimeSpawnMin = 12;
    public float powerUpTimeSpawnMax = 27;
    public float powerUpTimeSpawn;
    public float powerUpTimeSpawnCurrent;
    public int powerUpsSpawnInARow;
    public int powerUpsSpawnInARowMax;

    private int maxAlivePowerups = 16;

    private PowerupType previousPowerupType = PowerupType.SuperBullet;

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
    private float tipOffTimer;
    public bool hasTippedOff;

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
    public GameObject scoresUI;
    private GameObject scoresUITeam0;
    private GameObject scoresUITeam1;
    public GameObject playerHeadersPanel;
    public GameObject tipOffUI;
    private Text tipOffUIText;

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
    public Text indicatorText;

    public bool gameOver;
    public bool paused;

    public Button currentSelection;
    public Button defaultSelection;
    private bool[] canMoveSelection = new bool[9];
    private string[] controllers = { "1", "2", "3", "4", "5", "6", "7", "8", "K" };

    public bool overTime;


    //Score Tracker
    public int team0Score = 0;
    public int team1Score = 0;

    public int previousScorer = -1;

    public Gamemode gamemode;

    public TutorialManager tutorialManager;

    public BulletSpawnage bulletSpawnage;
    public float bigBulletScale;
    public CameraShake cameraShake;

    public bool cameraShakeEnabled;
    public bool powerUpsEnabled;


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
        cameraShake = FindObjectOfType<Camera>().GetComponent<CameraShake>();
        cameraShake.gameManager = this;

        allAlivePowerups = new List<Powerup>();

        bulletLevelUI = panelUI.transform.GetChild(6).GetComponent<Text>();
        bulletIncreaseUI = panelUI.transform.GetChild(5).GetComponent<Text>();
        dunkBonusUI = panelUI.transform.GetChild(7).GetComponent<Text>();
        scoresUITeam0 = scoresUI.transform.GetChild(0).gameObject;
        scoresUITeam1 = scoresUI.transform.GetChild(1).gameObject;

        audioManager = FindObjectOfType<AudioManager>();
        music = audioManager.Find("Music");
        pauseMusic = audioManager.Find("MusicPause");
        midair = audioManager.Find("Midair");

        audioManager.Play("Music");
        audioManager.Play("MusicPause");
        pauseMusic.source.volume = 0.0f;

        //player 1.
        panelUI.transform.GetChild(0).GetComponent<Text>().text = "0";
        //player 2.
        panelUI.transform.GetChild(1).GetComponent<Text>().text = "0";
        tipOffUIText = tipOffUI.GetComponentInChildren<Text>();

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
            data.playerControlsTeam0 = new List<int>();
            data.playerNumbersTeam0 = new List<int>();
            data.playerControlsTeam1 = new List<int>();
            data.playerNumbersTeam1 = new List<int>();
            data.isSwipeShotRally = false;

            //uncommented this code to have 2 players on KB spawn in instead of Bots

            data.playerControlsTeam0 = new List<int>() { 0 };
            data.playerNumbersTeam0 = new List<int>() { 0 };
            data.playerControlsTeam1 = new List<int>() { 1 };
            data.playerNumbersTeam1 = new List<int>() { 1 };
        }

        if (gamemode == Gamemode.Tutorial)
        {
            if (loadedData == null)
            {
                GameObject gameDataObjectStandin = new GameObject("Game Data Object Standin");
                data = gameDataObjectStandin.AddComponent<GameData>();
            }
            data.playerControlsTeam0 = new List<int>();
            data.playerNumbersTeam0 = new List<int>();
            data.playerControlsTeam1 = new List<int>();
            data.playerNumbersTeam1 = new List<int>();
            data.isSwipeShotRally = false;

            //uncommented this code to have 2 players on KB spawn in instead of Bots

            data.playerControlsTeam0 = new List<int>() { 0 };
            data.playerNumbersTeam0 = new List<int>() { 0 };
            data.playerControlsTeam1 = new List<int>() { 8 };
            data.playerNumbersTeam1 = new List<int>() { 8 };
        }

        playersTeam0 = new GameObject[data.playerNumbersTeam0.Count];
        playerScriptsTeam0 = new BhbPlayerController[data.playerNumbersTeam0.Count];
        for (int i = 0; i < data.playerNumbersTeam0.Count; i++)
        {
            SpawnPlayer(playersTeam0, playerScriptsTeam0, data.playerNumbersTeam0, data.playerControlsTeam0, i, 0);
        }

        playersTeam1 = new GameObject[data.playerNumbersTeam1.Count];
        playerScriptsTeam1 = new BhbPlayerController[data.playerNumbersTeam1.Count];
        for (int i = 0; i < data.playerNumbersTeam1.Count; i++)
        {
            SpawnPlayer(playersTeam1, playerScriptsTeam1, data.playerNumbersTeam1, data.playerControlsTeam1, i, 1);
        }

        matchTimeMax = data.matchLength;

        bulletLevelUpInterval = matchTimeMax / (numOfBulletLevelUps + 1);

        powerUpsEnabled = data.powerUps;
        cameraShakeEnabled = data.cameraShake;
        bulletSpawnage = data.bulletSpawnage;

        int[] powerUpSpawnMins = new int[] { 7, 12, 17, 24, -1 };
        int[] powerUpSpawnMaxs = new int[] { 18, 25, 31, 36, -1 };
        int[] powerUpSpawnRowMaxs = new int[] { 4, 3, 2, 1, -1 };

        powerUpTimeSpawnMin = powerUpSpawnMins[(int)data.powerUpSpawnage];
        powerUpTimeSpawnMax = powerUpSpawnMaxs[(int)data.powerUpSpawnage];
        powerUpsSpawnInARow = powerUpSpawnRowMaxs[(int)data.powerUpSpawnage];

        if (data.powerUpSpawnage == PowerUpSpawnage.None)
            powerUpsEnabled = false;

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
        if (bulletSpawnage != BulletSpawnage.None)
            SpawnBulletSpawnersFromData();

        if (gamemode == Gamemode.Tutorial)
        {
            tutorialManager.gameManager = this;
            Destroy(pausedMenuUI);
            Destroy(tipOffUI);
            Destroy(dunkBonusUI);
            paused = false;
            matchTimeText.text = "";
            bulletLevelUI.text = "";
            data.numOfBulletLevelUps = 3;

            playerScriptsTeam1[0].isDummy = true;
            tutorialManager.ChangeControlType(ControlType.Keyboard1);

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
            playerScript.Init(8, team, -1);
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
        playerHeader.GetComponent<PlayerHeader>().Init(playerScript, this);
    }

    private void BeginMatch()
    {
        matchTimeCurrent = matchTimeMax;

        team0Score = 0;
        team1Score = 0;
        panelUI.transform.GetChild(1).GetComponent<Text>().text = team1Score.ToString();
        panelUI.transform.GetChild(0).GetComponent<Text>().text = team0Score.ToString();
        panelUI.transform.GetChild(2).GetComponent<Text>().text = TimeSpan.FromSeconds(Mathf.Max(matchTimeCurrent, 0)).ToString("m\\:ss");

        //sets bullet level and dunk value back to default.
        bulletLevelUI.text = "Bullets Level: " + bulletLevel;
        dunkBonusUI.text = "Dunk Bonus: +" + dunkBonusValue;

        panelUI.SetActive(true);
        playerOneWins.SetActive(false);
        playerTwoWins.SetActive(false);
        pausedMenuUI.SetActive(false);
        previousScorer = -1;
        gameOver = false;
        overTime = false;

        hasTippedOff = false;
        tipOffTimer = 3;
        tipOffUI.SetActive(true);

        if (gamemode != Gamemode.Tutorial)
            paused = true;

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

        BulletPortal[] bulletPortals = FindObjectsOfType<BulletPortal>();
        foreach (BulletPortal portal in bulletPortals)
        {
            Destroy(portal.gameObject);
        }

        allAlivePowerups.Clear();
        powerUpTimeSpawn = UnityEngine.Random.Range(powerUpTimeSpawnMin, powerUpTimeSpawnMax);


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

    /// <summary>
    /// Starts right after BeginMatch(). Counts down tip off.
    /// </summary>
    private void StartTipOff()
    {
        //After 3 seconds.
        if (!hasTippedOff)
        {
            if (tipOffTimer > 0)
            {
                tipOffTimer -= Time.deltaTime;
                tipOffUIText.text = ((int)tipOffTimer + 1).ToString();
            }
            else
            {
                if (gamemode == Gamemode.Exhibition)
                    tipOffUIText.text = "Tip Off!";
                else if (gamemode == Gamemode.Rally)
                    tipOffUIText.text = "Rally!";

                audioManager.Play("TipOffBuzzer");

                paused = false;
                tipOffTimer = 0;
                hasTippedOff = true;
            }
        }
        else
        {
            //turns off "tip off" text after short time
            if ((int)matchTimeCurrent % 30 == 27)
                tipOffUI.SetActive(false);
        }
    }

    public void SpawnSpecificPowerup(PowerupType powerupType, Vector2 position)
    {
        GameObject p = Instantiate(powerUpPrefab, position, Quaternion.identity);

        Powerup pScript = p.GetComponent<Powerup>();
        pScript.originalPosition = position;
        pScript.gameManager = this;
        pScript.Init(powerupType);
        allAlivePowerups.Add(pScript);
    }

    public void SpawnRandomPowerUp()
    {
        if (allAlivePowerups.Count >= maxAlivePowerups)
            return;

        PowerupType type = (PowerupType)UnityEngine.Random.Range(0, 5);
        while (type == previousPowerupType)
            type = (PowerupType)UnityEngine.Random.Range(0, 5);

        previousPowerupType = type;

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

    public void SpawnSuperBullet(int teamNumber)
    {
        GameObject superBullet = Instantiate(superBulletPrefab);
        SuperBullet superBulletScript = superBullet.GetComponent<SuperBullet>();
        superBulletScript.Init(teamNumber, this);
        StartCoroutine(cameraShake.Shake(.2f, .5f));
    }

    public void SpawnBulletPortal(int teamNumber, Vector2 location)
    {
        GameObject bulletPortal = Instantiate(bulletPortalPrefab);
        bulletPortal.transform.position = location;
        BulletPortal bulletPortalScript = bulletPortal.GetComponent<BulletPortal>();
        bulletPortalScript.Init(teamNumber, this);
    }

    void Update()
    {
        if (gamemode != Gamemode.Tutorial)
            StartTipOff();

        if (ball.transform.parent == null)
            currentBallOwner = null;
        //If the ball is above the screen height (will also happen when held).
        if (ball.transform.position.y > 33)
            ShowBallChevron(true);
        else
            ShowBallChevron(false);

        if (gamemode == Gamemode.Tutorial)
        {
            if (playerScriptsTeam0[0].playerControlNumber == 0)
            {
                int controller = TutorialCheckAllGamepadInputs();
                if (controller != -1)
                {
                    playerScriptsTeam0[0].playerControlNumber = controller + 1;
                    tutorialManager.ChangeControlType(ControlType.Gamepad);
                }
                if (TutorialCheckKBInputs1())
                {
                    playerScriptsTeam0[0].playerControlNumber = 1;
                    tutorialManager.ChangeControlType(ControlType.Keyboard2);
                }
            }
            else if (playerScriptsTeam0[0].playerControlNumber == 1)
            {
                int controller = TutorialCheckAllGamepadInputs();
                if (controller != -1)
                {
                    playerScriptsTeam0[0].playerControlNumber = controller + 1;
                    tutorialManager.ChangeControlType(ControlType.Gamepad);
                }
                if (TutorialCheckKBInputs0())
                {
                    playerScriptsTeam0[0].playerControlNumber = 0;
                    tutorialManager.ChangeControlType(ControlType.Keyboard1);
                }
            }
            else if (playerScriptsTeam0[0].playerControlNumber > 1)
            {
                if (TutorialCheckKBInputs0())
                {
                    playerScriptsTeam0[0].playerControlNumber = 0;
                    tutorialManager.ChangeControlType(ControlType.Keyboard1);
                }
                if (TutorialCheckKBInputs1())
                {
                    playerScriptsTeam0[0].playerControlNumber = 1;
                    tutorialManager.ChangeControlType(ControlType.Keyboard2);
                }
            }

            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            {
                tutorialManager.DisplayNextMessage();
                if (playerScriptsTeam0[0].playerControlNumber > 1)
                {
                    playerScriptsTeam0[0].playerControlNumber = 0;
                    tutorialManager.ChangeControlType(ControlType.Keyboard1);
                }
            }

            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetButtonDown("J" + i + "Start"))
                {
                    tutorialManager.DisplayNextMessage();
                    if (playerScriptsTeam0[0].playerControlNumber <= 1)
                    {
                        playerScriptsTeam0[0].playerControlNumber = i + 1;
                        tutorialManager.ChangeControlType(ControlType.Gamepad);
                    }
                    break;
                }
            }

            if (tutorialManager.controlChangeAlertTimeCurrent < tutorialManager.controlChangeAlertTimeMax)
            {
                tutorialManager.controlChangeAlertTimeCurrent += Time.deltaTime;
                tutorialManager.controlChangeAlert.gameObject.SetActive(true);
            }
            else
            {
                tutorialManager.controlChangeAlert.gameObject.SetActive(false);
            }

            if (playerScriptsTeam0[0].playerControlNumber == 0)
            {

            }
        }
        else if (gamemode == Gamemode.Rally)
        {
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            {
                TogglePauseMenu();
            }

            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetButtonDown("J" + i + "Start"))
                {
                    TogglePauseMenu();
                    break;
                }
            }

            if (paused && hasTippedOff)
            {
                PauseMenuUpdate();
            }

            if (paused)
                return;
        }
        else
        {
            if (Input.GetKeyDown(KeyCode.Backspace) || Input.GetKeyDown(KeyCode.Escape))
            {
                if (gameOver)
                {
                    BeginMatch();
                }
                else
                    TogglePauseMenu();

                panelUI.SetActive(true);
            }

            for (int i = 1; i <= 8; i++)
            {
                if (Input.GetButtonDown("J" + i + "Start"))
                {
                    if (gameOver)
                    {
                        BeginMatch();
                    }
                    else
                        TogglePauseMenu();
                    break;
                }
            }

            if (paused && hasTippedOff)
            {
                PauseMenuUpdate();
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
                        StartCoroutine(EndGame());
                    }
                }
            }

            if (powerUpsEnabled)
            {
                powerUpTimeSpawnCurrent += Time.deltaTime;
                if (powerUpTimeSpawnCurrent >= powerUpTimeSpawn)
                {
                    SpawnRandomPowerUp();
                    powerUpTimeSpawnCurrent = 0;
                    powerUpTimeSpawn = UnityEngine.Random.Range(powerUpTimeSpawnMin, powerUpTimeSpawnMax);
                    if (powerUpsSpawnInARow == 0)
                    {
                        powerUpsSpawnInARow = UnityEngine.Random.Range(0, 4);
                    }
                    else
                    {
                        powerUpsSpawnInARow--;
                        powerUpTimeSpawn = .6f;
                    }
                }
            }

            //Shows bullets increased UI element on regular interval.
            ShowBulletIncreaseUI();
        }
    }


    private void PauseMenuUpdate()
    {
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            currentSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        }
        else if (currentSelection is Button)
        {
            currentSelection.Select();
        }
        else
        {
            currentSelection = null;
        }

        for (int i = 0; currentSelection != null && i < 9; i++)
        {
            //checks if controllers or keyboard have pressed "A" or space, if so, "click" the current button
            if (Input.GetButtonDown("J" + controllers[i] + "A"))
            {
                HologramButton hologramButton = currentSelection.gameObject.GetComponent<HologramButton>();
                ExecuteEvents.Execute(currentSelection.gameObject,
                    new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                if (hologramButton != null)
                {
                    hologramButton.DeselectVisual();
                }
                break;
            }
            //get the current direction the player is pressing in
            float horizontalInput = 0f;
            float verticalInput = 0f;

            //hover sound for buttons. Does not play on title screen.

            if (((Input.GetAxis("J" + controllers[i] + "Vertical") + (Input.GetAxis("J" + controllers[i] + "Horizontal")) == 0)))
            {
                horizontalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "DHorizontal"));
                verticalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "DVertical"));
            }
            else
            {
                horizontalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "Horizontal"));
                verticalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "Vertical"));
            }



            //this prevents the user from pressing same direction each frame (for controls sticks, dpad, and KB)
            if (horizontalInput == 0 && verticalInput == 0)
            {
                canMoveSelection[i] = true;
            }

            //this find the direction (right, left, up, down) the user pressed, and tries to select the button
            //that is next to the current selection in that direction
            if (canMoveSelection[i])
            {
                if (horizontalInput > 0)
                {
                    if (currentSelection.navigation.selectOnRight != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnRight);
                        currentSelection.navigation.selectOnRight.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
                else if (horizontalInput < 0)
                {
                    if (currentSelection.navigation.selectOnLeft != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnLeft);
                        currentSelection.navigation.selectOnLeft.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
                else if (verticalInput > 0)
                {
                    if (currentSelection.navigation.selectOnUp != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnUp);
                        currentSelection.navigation.selectOnUp.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
                else if (verticalInput < 0)
                {
                    if (currentSelection.navigation.selectOnDown != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnDown);
                        currentSelection.navigation.selectOnDown.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
            }
        }
    }
    private int TutorialCheckAllGamepadInputs()
    {
        for (int i = 1; i <= 8; i++)
        {
            for (int j = 0; j < playerScriptsTeam0[0].GetControlsArrayGamepad().Length - 1; j++)
            {
                if (j < 4)
                {
                    if (Mathf.Abs(Input.GetAxisRaw("J" + i + playerScriptsTeam0[0].GetControlsArrayGamepad()[j])) > BhbPlayerController.axisDeadZone)
                    {
                        return i;
                    }
                }
                if (Input.GetButton("J" + i + playerScriptsTeam0[0].GetControlsArrayGamepad()[j]))
                {
                    return i;
                }
            }
        }
        return -1;
    }

    private bool TutorialCheckKBInputs0()
    {
        for (int i = 0; i < playerScriptsTeam0[0].GetControlsArrayKb0().Length - 1; i++)
        {
            if (Input.GetKey(playerScriptsTeam0[0].GetControlsArrayKb0()[i]))
            {
                return true;
            }
        }
        return false;
    }

    private bool TutorialCheckKBInputs1()
    {
        for (int i = 0; i < playerScriptsTeam0[0].GetControlsArrayKb1().Length - 1; i++)
        {
            if (Input.GetKey(playerScriptsTeam0[0].GetControlsArrayKb1()[i]))
            {
                return true;
            }
        }
        return false;
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

    public void TogglePauseMenu()
    {
        if (hasTippedOff)
        {
            pausedMenuUI.SetActive(!pausedMenuUI.activeSelf);
            paused = !paused;

            HologramButton hb = currentSelection.GetComponent<HologramButton>();
            if(hb != null)
                hb.DeselectVisual();

            defaultSelection.Select();
            defaultSelection.GetComponent<HologramButton>().SelectVisual();

            //toggles audio.
            if (paused)
            {
                audioManager.Play("MusicPauseStart");
                pauseMusic.source.volume = 0.1f;
                music.source.volume = 0;

                midair.source.volume = 0;
            }
            else
            {
                pauseMusic.source.volume = 0;
                music.source.volume = 0.1f;
            }

            //if rally, makes sure ball spawns in front of a player.
            if (gamemode == Gamemode.Rally)
                FindObjectOfType<SwipeShotManager>().Reset();
        }
    }

    public IEnumerator EndGame()
    {
        //Explodes ball on end, sends ball offscreen, sets to is resetting, sets to not be a bullet.
        SpawnExplosion(2, ball.transform.position);
        ballControlScript.IsResetting = true;
        ball.transform.position = new Vector3(0, -1000, 0);
        ballControlScript.IsBullet = false;

        //Removes UI, Adds scoresUI.
        panelUI.SetActive(false);
        scoresUI.SetActive(true);

        scoresUITeam0.transform.GetChild(2).GetComponent<Text>().text = team0Score.ToString();
        scoresUITeam1.transform.GetChild(2).GetComponent<Text>().text = team1Score.ToString();

        //Light up the winning score.
        if (team0Score > team1Score)
        {
            for (int i = 0; i < 2; i++)
            {
                scoresUITeam0.transform.GetChild(i).GetComponent<Image>().enabled = true;
                scoresUITeam1.transform.GetChild(i).GetComponent<Image>().enabled = false;
            }
        }
        else
        {
            for (int i = 0; i < 2; i++)
            {
                scoresUITeam0.transform.GetChild(i).GetComponent<Image>().enabled = false;
                scoresUITeam1.transform.GetChild(i).GetComponent<Image>().enabled = true;
            }
        }

        audioManager.Play("Buzzer");
        midair.source.volume = 0;

        audioManager.Stop("Overtime");
        audioManager.Play("Music");
        audioManager.Play("MusicPause");

        gameOver = true;
        overTime = false;
        bulletLevel = 1;

        bulletIncreaseUI.gameObject.SetActive(false);
        bulletTimerUI = 0;

        HomingBullet[] homingBullets = FindObjectsOfType<HomingBullet>();
        foreach (HomingBullet hb in homingBullets)
        {
            hb.Explode();
        }

        SuperBullet[] superBullets = FindObjectsOfType<SuperBullet>();
        foreach (SuperBullet sb in superBullets)
        {
            sb.ForceDestroy();
        }

        yield return new WaitForSecondsRealtime(2);

        if (team0Score > team1Score)
            playerOneWins.SetActive(!playerOneWins.activeSelf);
        else
            playerTwoWins.SetActive(!playerTwoWins.activeSelf);

        audioManager.Play("2points"); //change to applause?
        team0Score = 0;
        team1Score = 0;
        paused = true;

        scoresUI.SetActive(false);
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
        {
            indicatorShevron.transform.position = cam.WorldToScreenPoint(new Vector3(ball.transform.position.x, 33, 0));

            float sizeChange = (ball.transform.position.y - 33);
            float scaleChange = 1.0f + (sizeChange / 20.0f);

            indicatorText.text = sizeChange.ToString("F0") + "m";
            indicatorText.fontSize = 48 + (int)sizeChange;
            indicatorShevron.transform.localScale = new Vector3(scaleChange, scaleChange, 1.0f);
        }
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

    public bool IsBallOwnerOppositeTeam(BhbPlayerController source)
    {

        if (currentBallOwner == null)
            return false;

        return currentBallOwner.teamNumber != source.teamNumber;
    }

    public void ResumeGame()
    {
        TogglePauseMenu();
    }

    public void RestartGame()
    {
        TogglePauseMenu();
        //This EndGame call should NOT use the coroutine.
        EndGame();
        BeginMatch();
    }

    public void BackToMenu()
    {
        Destroy(FindObjectOfType<AudioManager>().gameObject);
        SceneManager.LoadScene(0);
    }
}
