using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletLauncherData : MonoBehaviour
{

    public Movement movement;

    public BulletPatterns bulletPattern;

    public BulletMovement bulletMovement;
    
    public float initialTimeBetweenBullets;

    public float arcMovementRadius;


    public float rotationSpeed = 10;

    public float moveFullCircleSpeed;

    public float bulletSeperationAngle = 5f;

    public float arcMinAngle = 0;
    public float arcMaxAngle = 45;

    public int initialNumberOfBullets = 2;

    public float sinWaveLength;

    public float sinWaveFrequency;
}
