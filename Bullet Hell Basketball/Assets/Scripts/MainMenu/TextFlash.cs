using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextFlash : MonoBehaviour
{

    private Text text;

    public float flashSpeed = 2;

    private float originalAlpha;

    void Awake()
    {
        text = gameObject.GetComponent<Text>();
        originalAlpha = text.color.a;
    }

    void Update()
    {
        text.color = new Color(text.color.r, text.color.g, text.color.b, originalAlpha * Mathf.Abs(Mathf.Sin(Time.time * flashSpeed)));
    }
}
