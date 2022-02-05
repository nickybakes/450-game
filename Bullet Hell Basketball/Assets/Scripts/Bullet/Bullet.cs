using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int speed; //Multiplies this by time.deltaTime to increase the speed
    
    // Start is called before the first frame update
    void Start()
    {
        if(speed <= 0)
        {
            speed = 1;
        }
    }

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.right * Time.deltaTime * speed, Space.World);
    }
}
