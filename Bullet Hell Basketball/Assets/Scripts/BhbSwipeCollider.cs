using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BhbSwipeCollider : MonoBehaviour
{
    private BhbPlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = gameObject.GetComponentInParent<BhbPlayerController>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
