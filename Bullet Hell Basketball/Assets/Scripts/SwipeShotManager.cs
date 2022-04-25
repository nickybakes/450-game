using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SwipeShotManager : MonoBehaviour
{
    //needs:
    //access to players, UI, gameManager,
    public GameManager gameManager;


    // Start is called before the first frame update
    void Start()
    {
        //No powerups, dunk bonus, etc.
        gameManager.powerUpsEnabled = false;

        //Sets the ball either in front of team 0 or team 1.
        //would only work on first time, needs to run on reset.
        int randomBallStartPos = (Random.Range(0, 1) * 50) - 25;
        gameManager.ballSpawnPosition = new Vector3(randomBallStartPos, 25, 0);
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

        //Make it so somehow the player's can't grab the ball.
        //gameManager.ballPhysicsScript.removeCollision();

        //Limit to 1/2 players, if 1 player, bot. Bot usually* returns ball.
        //*discuss w/ team, turn into its own game?
    }
}
