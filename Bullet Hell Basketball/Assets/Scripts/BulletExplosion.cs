using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// For bullets that explode. Handles playing explosion particles and camera shake.
/// </summary>
public class BulletExplosion : MonoBehaviour
{
    private ParticleSystem ps;
    private CameraShake cameraShake;

    // Start is called before the first frame update
    void Start()
    {
        ps = GetComponent<ParticleSystem>();
        ps.Stop();
        cameraShake = FindObjectOfType<Camera>().GetComponent<CameraShake>();
    }


    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ps.Play();
            StartCoroutine(cameraShake.Shake(.2f, .5f));
        }
        //if (!ps.isEmitting) //if no longer emitting, destroy prefab.
        //{
        //    Destroy(this);
        //}
    }
}
