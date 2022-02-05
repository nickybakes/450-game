using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    public GameObject bullet;
    public float timer; //Controls how long between bullets this will fire
    private float maxTime;
    public Transform bulletSpawn;
    
    
    // Start is called before the first frame update
    void Start()
    {
        //Sets a default value if the given one isn't good
        if(timer <= 0)
        {
            timer = 10.0f;
        }
        
        maxTime = timer;
    }

    private void FixedUpdate()
    {
        timer -= Time.deltaTime;

        if(timer <= 0)
        {
            Instantiate(bullet, bulletSpawn);
            timer = maxTime;
        }
    }
}
