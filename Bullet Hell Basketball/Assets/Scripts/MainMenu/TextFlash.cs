using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFlash : MonoBehaviour
{

    private Text text;

    private Image image;

    public float flashSpeed = 2;

    public bool flashImageInstead;

    private float originalAlpha;

    void Awake()
    {

        if (flashImageInstead)
        {
            image = gameObject.GetComponent<Image>();
            originalAlpha = image.color.a;
        }
        else
        {
            text = gameObject.GetComponent<Text>();
            originalAlpha = text.color.a;
        }
    }

    void Update()
    {
        if (flashImageInstead)
        {
            image.color = new Color(image.color.r, image.color.g, image.color.b, originalAlpha * Mathf.Abs(Mathf.Sin(Time.time * flashSpeed)));
        }
        else
        {
            text.color = new Color(text.color.r, text.color.g, text.color.b, originalAlpha * Mathf.Abs(Mathf.Sin(Time.time * flashSpeed)));
        }
    }
}
