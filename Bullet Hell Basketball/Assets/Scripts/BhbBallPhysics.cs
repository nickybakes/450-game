using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BhbBallPhysics : NeonHeightsPhysicsObject
{
    // Start is called before the first frame update
    void Start()
    {

    }

    private void SimulatePhysics()
    {
        DrawBoundingRect();

        CheckCollisionsBottom();
        CheckCollisionsTop();
        GroundCheck();

        ApplyGravity();

        ApplyVerticalCollisions();


        if(bottomCollision != null){
            this.velocity = bottomCollision.segment.normalNormalized * 5000;
        }


        CheckCollisionsLeft();
        CheckCollisionsRight();

        ApplyHorizontalCollisions();


        UpdateCollisionRect();
    }

    // Update is called once per frame
    void Update()
    {
        SimulatePhysics();
    }
}
