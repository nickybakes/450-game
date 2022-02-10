using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    /// <summary>
    /// The instance of the GameManager (it's a singleton).
    /// </summary>
    public static GameManager Instance;

    //TO ADD: MENU
    //[SerializeField] private MainMenu menu;


    [SerializeField] private NeonHeightsCharacterController player1;

    //TO ADD: Player 2
    //[SerializeField] private NeonHeightsCharacterController player2;

    //Score Tracker
    public int player1Score = 0;
    public int player2Score = 0;

    [HideInInspector] public bool winConditionMet = false;

    // Set up singleton here
    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    /// <summary>
    /// Is the game paused?
    /// </summary>
    public bool Paused { get; set; } = true;

    // Start is called before the first frame update
    private void Start()
    {
        //TO ADD: Initialization for both players, the ball, basket, bullet spawners, and walls
        player1Score = 0;
        player2Score = 0;
    }

    private void Update()
    {
        if (Paused)
            return;


        //TO ADD: This is where the Pause menu will appear.
        //if (Input.GetKeyDown(KeyCode.Escape))
            //menu.Pause();

        //if(player1 or 2 reaches the score limit)
        //{
            // End the game
        //}
    }

    private void EndGame()
    {
        //if (winConditionMet)
            //TO ADD: A WAY TO END THIS NEVER ENDING RIDE
        //else
            //TO ADD: Player 2 win thingy
    }
}
