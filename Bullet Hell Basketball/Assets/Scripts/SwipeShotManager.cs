using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeShotManager : MonoBehaviour
{
    //needs:
    //access to players, UI, gameManager,
    public GameManager gameManager;
    private bool hasChosenSide;


    // Start is called before the first frame update
    void Start()
    {
        //No powerups, dunk bonus, etc.
        gameManager.powerUpsEnabled = false;
        hasChosenSide = true;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 currentPos0 = gameManager.playersTeam0[0].transform.position;
        Vector3 currentPos1 = gameManager.playersTeam1[0].transform.position;

        //restrict player movement, only jumping/swiping
        gameManager.playersTeam0[0].transform.position = new Vector3(-25, currentPos0.y, currentPos0.z);
        gameManager.playersTeam1[0].transform.position = new Vector3(25, currentPos1.y, currentPos1.z);

        gameManager.powerUpsEnabled = false;

        //Sets the ball either in front of team 0 or team 1.
        if (!gameManager.hasTippedOff && hasChosenSide)
        {
            int randomBallStartPos = (Random.Range(0, 2) * 40) - 20;
            gameManager.ballControlScript.gameObject.transform.position = new Vector3(randomBallStartPos, 5, 0);

            hasChosenSide = false; //Prevents ball from going back and forth.
        }

        //Make it so somehow the player's can't grab the ball.

        //Limit to 1/2 players, if 1 player, bot. Bot usually* returns ball.
        //*discuss w/ team, turn into its own game?
    }
}
