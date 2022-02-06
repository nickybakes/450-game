using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetCollision : MonoBehaviour
{
    public Ball ball;
    public GameObject player;

    private void OnCollisionEnter(Collision collision)
    {
        //If the target is colliding with the ball & is not already being held,
        //re-parent the ball to the player and set its position to the player(s hand).
        if (collision.gameObject.tag == "Ball" && ball.transform.parent == null)
        {
            Vector3 positionToHand = new Vector3(1.8f, 1.1f, 0.0f);

            ball.transform.parent = player.transform;
            ball.transform.position = (player.transform.position + positionToHand);
        }
    }
}
