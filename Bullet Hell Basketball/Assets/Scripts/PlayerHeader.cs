using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHeader : MonoBehaviour
{

    public BhbPlayerController player;

    Camera cam;

    public static Color32[] colors = {new Color32(255, 40, 40, 255), new Color32(17, 61, 255, 255), new Color32(255, 246, 9, 255), new Color32(0, 188, 37, 255),
            new Color32(252, 156, 2, 255), new Color32(135, 19, 193, 255), new Color32(255, 0, 238, 255), new Color32(2, 214, 221, 255), new Color32(87,87,87, 255)};

    public Color32 color;

    public Text hpText;

    public Image shevron;
    public Text playerNumberText;


    // Start is called before the first frame update
    void Start()
    {
        cam = FindObjectOfType<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (player == null)
            return;

        transform.position = cam.WorldToScreenPoint(new Vector3(player.transform.position.x, player.transform.position.y + player.height, 0));
        // hpText.text = player.health.ToString();
    }

    public void Init(BhbPlayerController player)
    {
        this.player = player;
        this.color = colors[player.playerNumber];
        if (player.playerNumber != 8)
            playerNumberText.text = "P" + (player.playerNumber + 1);
        else
            playerNumberText.text = "BOT";
        shevron.color = color;

    }
}
