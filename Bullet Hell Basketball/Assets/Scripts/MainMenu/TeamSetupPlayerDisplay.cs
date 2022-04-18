using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TeamSetupPlayerDisplay : MonoBehaviour
{

    public Color32 color;

    public Image shevron;
    public Text playerNumberText;

    public int playerNumber;

    public int inputId;

    public Text inputMethod;

    public GameObject shineFull;

    public Image shineColored;
    public Image shineWhite;

    public float shineAnimationTimeCurrent;
    public float shineAnimationTimeMax = .6f;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (shineAnimationTimeCurrent < shineAnimationTimeMax)
        {
            shineFull.gameObject.SetActive(true);
            shineAnimationTimeCurrent += Time.deltaTime;
            float scale = Mathf.Lerp(.4f, 2f, shineAnimationTimeCurrent / shineAnimationTimeMax);
            float alpha = Mathf.Clamp((shineAnimationTimeMax - shineAnimationTimeCurrent) / shineAnimationTimeMax, 0, 1);
            shineFull.transform.localScale = new Vector3(scale, scale, scale);
            shineColored.color = new Color(shineColored.color.r, shineColored.color.g, shineColored.color.b, alpha);
            shineWhite.color = new Color(1, 1, 1, alpha);
        }
        else
        {
            shineFull.gameObject.SetActive(false);
        }

    }

    public void PlayShineAnimation()
    {
        shineAnimationTimeCurrent = 0;
        shineFull.gameObject.SetActive(true);
    }

    public void Init(int playerNumber, int inputId)
    {
        this.playerNumber = playerNumber;
        this.inputId = inputId;

        inputMethod.text = "";

        if (inputId == 0)
        {
            inputMethod.text = "Keyboard 1";
        }
        else if (inputId == 1)
        {
            inputMethod.text = "Keyboard 2";
        }

        this.color = PlayerHeader.colors[this.playerNumber];

        if (this.playerNumber != 8)
            playerNumberText.text = "P" + (this.playerNumber + 1);
        else
            playerNumberText.text = "BOT";
        shevron.color = color;

        shineColored.color = color;

        PlayShineAnimation();
    }
}
