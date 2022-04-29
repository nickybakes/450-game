using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeShotManager : MonoBehaviour
{
    //needs:
    //access to players, UI, gameManager,
    public GameManager gameManager;
    public bool hasChosenSide;

    public Canvas rallyCanvas;
    private int currentScore = 0;
    private int highScore = 0;
    public Text currentScoreText;
    private Text highScoreText;

    public float tipOffTimer;

    // Start is called before the first frame update
    void Start()
    {
        //No powerups, dunk bonus, etc.
        gameManager.powerUpsEnabled = false;
        hasChosenSide = true;
        tipOffTimer = 3;

        currentScoreText = rallyCanvas.GetComponentsInChildren<Text>()[0];
        highScoreText = rallyCanvas.GetComponentsInChildren<Text>()[1];

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

            data.playerControlsTeam0 = new List<int>() { 0 };
            data.playerNumbersTeam0 = new List<int>() { 0 };
            data.playerControlsTeam1 = new List<int>() { 8 };
            data.playerNumbersTeam1 = new List<int>() { 8 };
        }

        data.gamemode = Gamemode.Rally;

    }

    // Update is called once per frame
    void Update()
    {
        //Vector3 currentPos0 = gameManager.playersTeam0[0].transform.position;
        Vector3 currentPos0 = gameManager.playersTeam0[0].transform.position;
        Vector3 currentPos1 = gameManager.playersTeam1[0].transform.position;

        //restrict player movement, only jumping/swiping
        //gameManager.playersTeam0[0].transform.position = new Vector3(-25, currentPos0.y, currentPos0.z);
        if (gameManager.playerScriptsTeam0[0].playerNumber == 8)
        {
            gameManager.playersTeam0[0].transform.position = new Vector3(-25, currentPos0.y, currentPos0.z);
        }
        else if (gameManager.playerScriptsTeam1[0].playerNumber == 8)
        {
            gameManager.playersTeam1[0].transform.position = new Vector3(25, currentPos1.y, currentPos1.z);
        }

        gameManager.powerUpsEnabled = false;
        gameManager.panelUI.SetActive(false);
        //gameManager.tipOffUI.SetActive(false);

        if (tipOffTimer > 0 && gameManager.hasTippedOff)
        {
            tipOffTimer -= Time.deltaTime;
        }
        else if (gameManager.hasTippedOff)
        {
            gameManager.tipOffUI.SetActive(false);
        }

        //counting for current and high score.
        currentScore = gameManager.ballControlScript.swipeShotPasses;
        if (!gameManager.ballControlScript.IsResetting)
            currentScoreText.text = "" + currentScore;

        if (currentScore > highScore)
        {
            highScore = currentScore;
            highScoreText.text = "High Score: " + highScore;
        }

        //Limit to 1/2 players, if 1 player, bot. Bot usually* returns ball.
    }

    public void Reset()
    {
        tipOffTimer = 3;
        hasChosenSide = true;
    }
}
