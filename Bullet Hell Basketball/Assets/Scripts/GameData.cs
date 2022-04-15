using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BulletSpawnage
{
    RegularOnly,
    BigOnly,
    BothRegularAndBig,
    None
}
public class GameData : MonoBehaviour
{

    /// <summary>
    /// The instance of the GameData (it's a singleton).
    /// </summary>
    public static GameData Instance;

    /*

    0 = player 1
    7 = player 8
    8 = bot

    */
    public List<int> playerNumbersTeam0 = new List<int>();
    public List<int> playerNumbersTeam1 = new List<int>();


    /*

    0 = keyboard 1 (WASD Space N)
    1 = keyboard 2 (PL;' < ^)
    2-9 = gamepad slots 1-8
    -1 = bot (nothing)

    */
    public List<int> playerControlsTeam0 = new List<int>();
    public List<int> playerControlsTeam1 = new List<int>();

    public BulletSpawnage bulletSpawnage = BulletSpawnage.RegularOnly;


    public int numOfBulletLevelUps = 3;

    public float matchLength = 120;

    public bool powerUps = true;

    public bool cameraShake = true;


    // Set up singleton here
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
        {
            Destroy(this);
            return;
        }

        DontDestroyOnLoad(this.gameObject);
    }


    // Start is called before the first frame update
    void Start()
    {
        if (matchLength == 0)
        {
            matchLength = 120;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
