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
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Init(int playerNumber, int inputId)
    {
        this.playerNumber = playerNumber;
        this.inputId = inputId;

        this.color = PlayerHeader.colors[this.playerNumber];

        if (this.playerNumber != 8)
            playerNumberText.text = "P" + (this.playerNumber + 1);
        else
            playerNumberText.text = "BOT";
        shevron.color = color;
    }
}
