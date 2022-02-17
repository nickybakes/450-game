using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BhbBallPhysics : NeonHeightsPhysicsObject
{

    public bool simulatePhysics = true;

    public float currentSpeed;

    public float speedDepletionAmount = .7f;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void SimulatePhysics()
    {
        DrawBoundingRect();

        //currentSpeed = velocity.magnitude;
        //Vector2 velocityNormalized = this.velocity / currentSpeed;

        CheckCollisionsBottom();
        CheckCollisionsTop();
        GroundCheck();

        ApplyGravity();

        ApplyVerticalCollisions();

        // if (bottomCollision != null)
        // {
        //     grounded = false;
        //     this.velocity.y = bottomCollision.segment.normalNormalized.y * currentSpeed * .7f;
        // }

        // if (topCollision != null)
        // {
        //     this.velocity.y = 0;
        // }

        if (Mathf.Abs(velocity.y) < 7 && groundCollision != null && !groundCollision.segment.semiSolidPlatform && (groundCollision.segment.normalNormalized.x == 0 || onFlatGround) && !grounded)
        {
            velocity.y = 0;
            ApplyVerticalCollisions();
        }

        CheckCollisionsRight();
        CheckCollisionsLeft();


        if (!onFlatGround)
        {
            if (bottomCollision != null)
            {
                this.velocity = Vector2.Reflect(this.velocity, bottomCollision.segment.normalNormalized) * speedDepletionAmount;
                grounded = false;
            }
            else if (topCollision != null)
            {
                this.velocity = Vector2.Reflect(this.velocity, topCollision.segment.normalNormalized) * speedDepletionAmount;
            }
            else if (rightCollision != null && !rightCollision.segment.semiSolidPlatform)
            {
                this.velocity = Vector2.Reflect(this.velocity, rightCollision.segment.normalNormalized) * speedDepletionAmount;
                ApplyHorizontalCollisions();
            }
            else if (velocity.y == 0 && rightCollision != null && rightCollision.segment.semiSolidPlatform && IsOverSegment(rightCollision.segment, 0))
            {
               this.velocity = Vector2.Reflect(this.velocity, rightCollision.segment.normalNormalized) * speedDepletionAmount;
            }
            else if (leftCollision != null && !leftCollision.segment.semiSolidPlatform)
            {
                this.velocity = Vector2.Reflect(this.velocity, leftCollision.segment.normalNormalized) * speedDepletionAmount;
                ApplyHorizontalCollisions();
            }
            else if (velocity.y == 0 && leftCollision != null && leftCollision.segment.semiSolidPlatform && IsOverSegment(leftCollision.segment, 0))
            {
               this.velocity = Vector2.Reflect(this.velocity, leftCollision.segment.normalNormalized) * speedDepletionAmount;
            }
        }
        else if (onFlatGround && bottomCollision != null)
        {
            this.velocity = Vector2.Reflect(this.velocity, Vector2.up) * speedDepletionAmount;
            grounded = false;
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
