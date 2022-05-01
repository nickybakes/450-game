using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ImageRotate : MonoBehaviour
{
    private RectTransform rect;

    public float rotationSpeed = 2;


    // Start is called before the first frame update
    void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        rect.Rotate(new Vector3(0, 0, Time.deltaTime * rotationSpeed), Space.Self);
    }
}
