using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BhbBallPhysics : NeonHeightsPhysicsObject
{

    public bool simulatePhysics = true;

    public float currentSpeed;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void SimulatePhysics()
    {
        DrawBoundingRect();

        currentSpeed = velocity.magnitude;

        CheckCollisionsBottom();
        CheckCollisionsTop();
        GroundCheck();

        ApplyGravity();

        ApplyVerticalCollisions();

        if (bottomCollision != null)
        {
            grounded = false;
            this.velocity.y = bottomCollision.segment.normalNormalized.y * currentSpeed * .7f;
        }

        if (topCollision != null)
        {
            this.velocity.y = 0;
        }

        if (Mathf.Abs(velocity.y) < 7 && groundCollision != null && !groundCollision.segment.semiSolidPlatform)
        {
            velocity.y = 0;
            ApplyVerticalCollisions();
        }


        if (!onFlatGround)
        {

            if (velocity.x == 0)
            {
                if (bottomCollision != null)
                {
                    grounded = false;
                    this.velocity.x = bottomCollision.segment.normalNormalized.x * currentSpeed * .7f;
                }
            }
            else if (velocity.x > 0)
            {
                CheckCollisionsRight();
                ApplyHorizontalCollisions();
                if (rightCollision != null && !rightCollision.segment.semiSolidPlatform)
                {
                    ApplyRightCollisionBounce();
                }
                else if (rightCollision != null && rightCollision.segment.semiSolidPlatform && IsOverSegment(rightCollision.segment))
                {
                    ApplyRightCollisionBounce();
                }
            }
            else if (velocity.x < 0)
            {
                CheckCollisionsLeft();
                ApplyHorizontalCollisions();
                if (leftCollision != null && !leftCollision.segment.semiSolidPlatform)
                {
                    ApplyLeftCollisionBounce();
                }
                else if (leftCollision != null && leftCollision.segment.semiSolidPlatform && IsOverSegment(leftCollision.segment))
                {
                    ApplyLeftCollisionBounce();
                }
            }
        }

        if (velocity.x > 0)
        {
            velocity.x = Mathf.Max(0, velocity.x -= 2 * Time.deltaTime);
        }
        else if (velocity.x < 0)
        {
            velocity.x = Mathf.Min(0, velocity.x += 2 * Time.deltaTime);
        }


        ApplyVelocityX();


        UpdateCollisionRect();
    }

    void ApplyRightCollisionBounce()
    {
        if (Mathf.Abs(velocity.x) < 40 && currentSpeed > 30)
        {
            this.velocity.x = rightCollision.segment.normalNormalized.x * Mathf.Abs(velocity.x) * .5f;
        }
        else
        {
            this.velocity.x = rightCollision.segment.normalNormalized.x * currentSpeed * .5f;
        }
        //this.velocity.x = rightCollision.segment.normalNormalized.x * (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) * .5f) * .5f;
    }

    void ApplyLeftCollisionBounce()
    {
        if (Mathf.Abs(velocity.x) < 40 && currentSpeed > 30)
        {
            this.velocity.x = leftCollision.segment.normalNormalized.x * Mathf.Abs(velocity.x) * .5f;
        }
        else
        {
            this.velocity.x = leftCollision.segment.normalNormalized.x * currentSpeed * .5f;
        }
        //this.velocity.x = leftCollision.segment.normalNormalized.x * (Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) * .5f) * .5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (simulatePhysics)
            SimulatePhysics();
    }
}
