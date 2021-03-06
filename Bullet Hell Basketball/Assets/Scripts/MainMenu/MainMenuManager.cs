using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;

public enum Menu
{
    Title,
    Main,
    GamemodeSelect,
    ExhibitionTeamSetup,
    Customize,
    Options,

    SwipeShotSetup,

    CustomizeCourt
}

public static class Controller
{
    public const KeyCode j10 = KeyCode.Joystick1Button0;
    public const KeyCode w = KeyCode.W;
}


public class MainMenuManager : MonoBehaviour
{
    private string[] controllers = { "1", "2", "3", "4", "5", "6", "7", "8", "K" };
    public string masterController;
    private bool[] canMoveSelection = new bool[9];

    public Material borderFlashMaterial;

    //public PanelManager TitlePanel;
    //public PanelManager MainPanel;
    //public PanelManager ModePanel;
    //public PanelManager PlayerPanel;
    //public PanelManager StagePanel;
    //public PanelManager OptionsPanel;

    public PanelManager[] panels;

    public Selectable currentSelection;

    private AudioManager audioManager;
    private Selectable prevSelection;
    public Menu currentPanelId;

    private GameData data;

    public GridLayoutGroup gridTeam0;
    public GridLayoutGroup gridTeam1;

    public GridLayoutGroup gridDuo0;
    public GridLayoutGroup gridDuo1;

    public GameObject teamSetupPlayerDisplayPrefab;

    public Text maxPlayersReachedWarning;
    public Text maxPlayersReachedWarningSwipeShot;

    public VolumeProfile menuProfile;

    public VolumeProfile gameProfile;

    public Volume postProcessingVolume;

    public Image backgroundOverlay;

    public Material daySkybox;
    public Material nightSkybox;

    public GameObject middlePlatform;

    public Text spawnText, bulletText, lengthText, dunkText, shoeSqueakText, cameraShakeText, timeOfDayText, midPlatformText;

    public Slider masterVolumeSlider;


    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play("MusicMenu");

        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].gameObject.SetActive(false);
            panels[i].panelId = (Menu)i;
        }

        panels[0].gameObject.SetActive(true);

        EventSystem.current.firstSelectedGameObject = panels[0].defaultButton.gameObject;


        GameData loadedData = FindObjectOfType<GameData>();
        data = loadedData;

        if (loadedData == null)
        {
            GameObject gameDataObject = new GameObject("Game Data Manager");
            data = gameDataObject.AddComponent<GameData>();
        }
        else
        {
            for (int i = 0; i < panels.Length; i++)
            {
                panels[i].gameObject.SetActive(false);
                panels[i].panelId = (Menu)i;
            }
            if (data.gamemode == Gamemode.Exhibition)
            {
                panels[(int)Menu.ExhibitionTeamSetup].EnableMenu();
                for (int i = 0; i < data.playerNumbersTeam0.Count; i++)
                {
                    ReaddPlayerToGameAfterReturningFromGame(data.playerControlsTeam0[i], data.playerNumbersTeam0[i], 0);
                }
                for (int i = 0; i < data.playerNumbersTeam1.Count; i++)
                {
                    ReaddPlayerToGameAfterReturningFromGame(data.playerControlsTeam1[i], data.playerNumbersTeam1[i], 1);
                }
            }
            else if (data.gamemode == Gamemode.Tutorial)
            {
                panels[(int)Menu.GamemodeSelect].EnableMenu();
            }
            else if (data.gamemode == Gamemode.Rally)
            {
                panels[(int)Menu.SwipeShotSetup].EnableMenu();
                for (int i = 0; i < data.playerNumbersTeam0.Count; i++)
                {
                    ReaddPlayerToGameAfterReturningFromGame(data.playerControlsTeam0[i], data.playerNumbersTeam0[i], 0);
                }
                for (int i = 0; i < data.playerNumbersTeam1.Count; i++)
                {
                    ReaddPlayerToGameAfterReturningFromGame(data.playerControlsTeam1[i], data.playerNumbersTeam1[i], 1);
                }
            }
        }

        if (data.bulletSpawnage == BulletSpawnage.RegularOnly)
        {
            bulletText.text = "Small Only";
        }
        else if (data.bulletSpawnage == BulletSpawnage.BigOnly)
        {
            bulletText.text = "Big Only";
        }
        else if (data.bulletSpawnage == BulletSpawnage.BothRegularAndBig)
        {
            bulletText.text = "Big & Small";
        }
        else if (data.bulletSpawnage == BulletSpawnage.None)
        {
            bulletText.text = "No Bullets";
        }

        if (data.powerUpSpawnage == PowerUpSpawnage.Low)
        {
            spawnText.text = "Low";
        }
        else if (data.powerUpSpawnage == PowerUpSpawnage.Medium)
        {
            spawnText.text = "Medium";
        }
        else if (data.powerUpSpawnage == PowerUpSpawnage.High)
        {
            spawnText.text = "High";
        }
        else if (data.powerUpSpawnage == PowerUpSpawnage.Chaotic)
        {
            spawnText.text = "Chaotic";
        }

        if (data.matchLength == 180)
        {
            lengthText.text = "3:00";
        }
        else if (data.matchLength == 120)
        {
            lengthText.text = "2:00";
        }
        else if (data.matchLength == 240)
        {
            lengthText.text = "4:00";
        }
        else if (data.matchLength == 300)
        {
            lengthText.text = "5:00";
        }

        if (data.dunkBonus)
        {
            dunkText.text = "Enabled";
        }
        else
        {
            dunkText.text = "Disabled";
        }

        if (BhbPlayerController.shoeSqueakRate == 0.01f)
        {
            shoeSqueakText.text = "Frequent";
        }
        else if (BhbPlayerController.shoeSqueakRate == 10000.0f)
        {
            shoeSqueakText.text = "Never";
        }
        else if (BhbPlayerController.shoeSqueakRate == 0.5f)
        {
            shoeSqueakText.text = "Semi-Frequent";
        }

        if (data.cameraShake)
        {
            cameraShakeText.text = "Enabled";
        }
        else
        {
            cameraShakeText.text = "Disabled";
        }

        SetTimeVisual(data.nightTime);
        if (data.nightTime)
        {
            timeOfDayText.text = "Midnight";
        }
        else
        {
            timeOfDayText.text = "Golden Hour";
        }

        middlePlatform.SetActive(data.middlePlatform);
        if (data.middlePlatform)
        {
            midPlatformText.text = "Enabled";
        }
        else
        {
            midPlatformText.text = "Disabled";
        }

        masterVolumeSlider.value = AudioListener.volume;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentPanelId == Menu.ExhibitionTeamSetup)
        {
            maxPlayersReachedWarning.gameObject.SetActive(data.playerNumbersTeam0.Count + data.playerNumbersTeam1.Count == 8);
        }
        else if (currentPanelId == Menu.SwipeShotSetup)
        {
            if (data.playerNumbersTeam1.Count > 0 && data.playerNumbersTeam1[0] != 8)
            {
                maxPlayersReachedWarningSwipeShot.gameObject.SetActive(data.playerNumbersTeam0.Count + data.playerNumbersTeam1.Count == 2);
            }
            else
            {
                maxPlayersReachedWarningSwipeShot.gameObject.SetActive(false);
            }
        }

        //checks to make sure we are selecting a valid selection (Button)
        if (EventSystem.current.currentSelectedGameObject != null)
        {
            currentSelection = EventSystem.current.currentSelectedGameObject.GetComponent<Button>();
        }
        else if (currentSelection is Button)
        {
            currentSelection.Select();
        }
        else
        {
            currentSelection = null;
        }

        for (int i = 0; currentSelection != null && i < 9; i++)
        {
            //checks if controllers or keyboard have pressed "A" or space, if so, "click" the current button
            if (Input.GetButtonDown("J" + controllers[i] + "A"))
            {
                if (currentPanelId == Menu.ExhibitionTeamSetup)
                {
                    int inputId = 0;

                    if (i == 8 && Input.GetKeyDown(KeyCode.Return))
                        inputId = 1;
                    else if (i != 8)
                        inputId = i + 2;

                    if (!IsInputIdInGame(inputId))
                    {
                        AddPlayerToGame(inputId);
                        break;
                    }

                }

                if (currentPanelId == Menu.SwipeShotSetup)
                {
                    int inputId = 0;

                    if (i == 8 && Input.GetKeyDown(KeyCode.Return))
                        inputId = 1;
                    else if (i != 8)
                        inputId = i + 2;

                    if (!IsInputIdInGame(inputId))
                    {
                        AddPlayerToGame(inputId);
                        break;
                    }

                }

                masterController = controllers[i];

                HologramButton hologramButton = currentSelection.gameObject.GetComponent<HologramButton>();
                if (hologramButton.isSlider)
                    break;
                Menu previousPanelId = currentPanelId;
                ExecuteEvents.Execute(currentSelection.gameObject,
                    new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                if (hologramButton != null && previousPanelId != currentPanelId)
                {
                    hologramButton.DeselectVisual();
                }
                break;
            }

            if (currentPanelId == Menu.ExhibitionTeamSetup && i == 8)
            {
                if (Input.GetKeyDown(KeyCode.Q) && IsInputIdInGame(0))
                {
                    RemovePlayerFromGame(0);
                    break;
                }
                else if (Input.GetKeyDown(KeyCode.O) && IsInputIdInGame(1))
                {
                    RemovePlayerFromGame(1);
                    break;
                }
                else if (Input.GetKeyDown(KeyCode.N) && IsInputIdInGame(0))
                {
                    SwapTeam(0);
                    break;
                }
                else if (Input.GetKeyDown(KeyCode.DownArrow) && IsInputIdInGame(1))
                {
                    SwapTeam(1);
                    break;
                }
            }



            if (currentPanelId == Menu.ExhibitionTeamSetup && Input.GetButtonDown("J" + controllers[i] + "B") && i != 8)
            {
                if (IsInputIdInGame(i + 2))
                {
                    RemovePlayerFromGame(i + 2);
                }
            }
            if (currentPanelId == Menu.ExhibitionTeamSetup && Input.GetButtonDown("J" + controllers[i] + "X") && i != 8)
            {
                if (IsInputIdInGame(i + 2))
                {
                    SwapTeam(i + 2);
                }
            }

            if (currentPanelId == Menu.SwipeShotSetup && i == 8)
            {
                if (Input.GetKeyDown(KeyCode.Q) && IsInputIdInGame(0) && data.playerControlsTeam0[0] != 0)
                {
                    RemovePlayerFromGame(0);
                    break;
                }
                else if (Input.GetKeyDown(KeyCode.O) && IsInputIdInGame(1) && data.playerControlsTeam0[0] != 1)
                {
                    RemovePlayerFromGame(1);
                    break;
                }
            }

            if (currentPanelId == Menu.SwipeShotSetup && Input.GetButtonDown("J" + controllers[i] + "B") && i != 8)
            {
                if (IsInputIdInGame(i + 2) && data.playerControlsTeam0[0] != i + 2)
                {
                    RemovePlayerFromGame(i + 2);
                }
            }

            //get the current direction the player is pressing in
            float horizontalInput = 0f;
            float verticalInput = 0f;

            //hover sound for buttons. Does not play on title screen.
            if (prevSelection != currentSelection && !panels[0].isActiveAndEnabled)
            {
                audioManager.Play("ButtonHover");
            }
            prevSelection = currentSelection;

            if (((Input.GetAxis("J" + controllers[i] + "Vertical") + (Input.GetAxis("J" + controllers[i] + "Horizontal")) == 0)))
            {
                horizontalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "DHorizontal"));
                verticalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "DVertical"));
            }
            else
            {
                horizontalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "Horizontal"));
                verticalInput += Mathf.Round(Input.GetAxis("J" + controllers[i] + "Vertical"));
            }

            //this prevents the user from pressing same direction each frame (for controls sticks, dpad, and KB)
            if (horizontalInput == 0 && verticalInput == 0)
            {
                canMoveSelection[i] = true;
            }

            //this find the direction (right, left, up, down) the user pressed, and tries to select the button
            //that is next to the current selection in that direction
            if (canMoveSelection[i])
            {
                if (horizontalInput > 0)
                {
                    HologramButton hologramButton = currentSelection.GetComponent<HologramButton>();
                    if (hologramButton != null && hologramButton.isSlider)
                    {
                        Slider s = hologramButton.sliderComponent;
                        s.value = Mathf.Clamp(s.value + (((float)(s.maxValue - s.minValue)) * Time.deltaTime), s.minValue, s.maxValue);
                        break;
                    }
                    if (currentSelection.navigation.selectOnRight != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnRight);
                        currentSelection.navigation.selectOnRight.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
                else if (horizontalInput < 0)
                {
                    HologramButton hologramButton = currentSelection.GetComponent<HologramButton>();
                    if (hologramButton != null && hologramButton.isSlider)
                    {
                        Slider s = hologramButton.sliderComponent;
                        s.value = Mathf.Clamp(s.value - (((float)(s.maxValue - s.minValue)) * Time.deltaTime), s.minValue, s.maxValue);
                        break;
                    }
                    if (currentSelection.navigation.selectOnLeft != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnLeft);
                        currentSelection.navigation.selectOnLeft.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
                else if (verticalInput > 0)
                {
                    if (currentSelection.navigation.selectOnUp != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnUp);
                        currentSelection.navigation.selectOnUp.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
                else if (verticalInput < 0)
                {
                    if (currentSelection.navigation.selectOnDown != null)
                    {
                        //Debug.Log(currentSelection.navigation.selectOnDown);
                        currentSelection.navigation.selectOnDown.Select();
                    }
                    canMoveSelection[i] = false;
                    break;
                }
            }
        }
    }

    //the method to call when the top left button on the titlePanel is submitted
    public void SwitchToPanel(int i)
    {
        foreach (PanelManager panel in panels)
        {
            panel.DisableMenu();
        }

        panels[i].EnableMenu();

        if (i == 1)
            audioManager.Play("ButtonBack");
        else
            audioManager.Play("ButtonSelect");
    }

    public void StepUp()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i != panels.Length - 1 && panels[i].gameObject.activeSelf)
            {
                panels[i].DisableMenu();
                panels[i + 1].EnableMenu();
                audioManager.Play("ButtonSelect");
                break;
            }
        }
    }

    public void OpenExhibitionTeamSetup()
    {
        StepUp();
        data.gamemode = Gamemode.Exhibition;
        data.playerNumbersTeam0 = new List<int>();
        data.playerNumbersTeam1 = new List<int>();
        data.playerControlsTeam0 = new List<int>();
        data.playerControlsTeam1 = new List<int>();

        foreach (Transform child in gridTeam0.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in gridTeam1.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        if (masterController == "K" || masterController == "M")
        {
            AddPlayerToGame(0);
        }
        else
        {
            AddPlayerToGame(int.Parse(masterController) + 1);
        }
    }

    public void OpenSwipeShotSetup()
    {
        SwitchToPanel(6);
        data.gamemode = Gamemode.Rally;
        data.playerNumbersTeam0 = new List<int>();
        data.playerNumbersTeam1 = new List<int>();
        data.playerControlsTeam0 = new List<int>();
        data.playerControlsTeam1 = new List<int>();

        foreach (Transform child in gridDuo0.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        foreach (Transform child in gridDuo1.transform)
        {
            GameObject.Destroy(child.gameObject);
        }

        if (masterController == "K" || masterController == "M")
        {
            AddPlayerToGame(0);
        }
        else
        {
            AddPlayerToGame(int.Parse(masterController) + 1);
        }

        AddBotTeam1();
    }

    public void ReaddPlayerToGameAfterReturningFromGame(int inputId, int playerNumber, int teamNumber)
    {
        if (currentPanelId == Menu.ExhibitionTeamSetup)
        {
            if (teamNumber == 0)
            {
                //add to Yellow team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridTeam0.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
            }
            else
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridTeam1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
            }
        }
        else if (currentPanelId == Menu.SwipeShotSetup)
        {
            if (teamNumber == 0)
            {
                //add to Yellow team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo0.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
            }
            else
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
            }
        }
    }

    public void AddPlayerToGame(int inputId)
    {
        if (currentPanelId == Menu.ExhibitionTeamSetup)
        {
            int playerNumber = GetSmalledAvailablePlayerNumber();
            if (playerNumber == 8 || data.playerNumbersTeam0.Count + data.playerNumbersTeam1.Count >= 8)
            {
                return;
            }
            if (data.playerNumbersTeam0.Count <= data.playerNumbersTeam1.Count)
            {
                //add to Yellow team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridTeam0.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
                data.playerControlsTeam0.Add(inputId);
                data.playerNumbersTeam0.Add(playerNumber);
            }
            else
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridTeam1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
                data.playerControlsTeam1.Add(inputId);
                data.playerNumbersTeam1.Add(playerNumber);
            }
        }

        if (currentPanelId == Menu.SwipeShotSetup)
        {
            int playerNumber = GetSmalledAvailablePlayerNumber();
            if (playerNumber == 8 || data.playerNumbersTeam0.Count + data.playerNumbersTeam1.Count >= 2)
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
                data.playerControlsTeam1.Add(inputId);
                data.playerNumbersTeam1.Add(playerNumber);

                RemoveBotTeam1();
                return;
            }
            if (data.playerNumbersTeam0.Count <= data.playerNumbersTeam1.Count)
            {
                //add to Yellow team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo0.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
                data.playerControlsTeam0.Add(inputId);
                data.playerNumbersTeam0.Add(playerNumber);
            }
            else
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(playerNumber, inputId);
                data.playerControlsTeam1.Add(inputId);
                data.playerNumbersTeam1.Add(playerNumber);
            }
        }

        //audio
        audioManager.Play("ControllerOn");
    }

    public void RemovePlayerFromGame(int inputId)
    {
        int teamNumber = GetWhichTeamPlayerIsIn(inputId);

        if (currentPanelId == Menu.ExhibitionTeamSetup)
        {
            if (teamNumber == 0)
            {
                //remove player from Yellow team
                int i = data.playerControlsTeam0.IndexOf(inputId);
                data.playerNumbersTeam0.RemoveAt(i);
                data.playerControlsTeam0.RemoveAt(i);
                Destroy(gridTeam0.transform.GetChild(i).gameObject);
            }
            else
            {
                //remove player from Blue team
                int i = data.playerControlsTeam1.IndexOf(inputId);
                data.playerNumbersTeam1.RemoveAt(i);
                data.playerControlsTeam1.RemoveAt(i);
                Destroy(gridTeam1.transform.GetChild(i).gameObject);
            }
        }

        if (currentPanelId == Menu.SwipeShotSetup)
        {
            if (teamNumber == 0)
            {
                //remove player from Yellow team
                int i = data.playerControlsTeam0.IndexOf(inputId);
                data.playerNumbersTeam0.RemoveAt(i);
                data.playerControlsTeam0.RemoveAt(i);
                Destroy(gridDuo0.transform.GetChild(i).gameObject);
                AddBotTeam0();
            }
            else
            {
                //remove player from Blue team
                int i = data.playerControlsTeam1.IndexOf(inputId);
                data.playerNumbersTeam1.RemoveAt(i);
                data.playerControlsTeam1.RemoveAt(i);
                Destroy(gridDuo1.transform.GetChild(i).gameObject);
                AddBotTeam1();
            }
        }

        //audio
        audioManager.Play("ControllerOff");
    }

    public void SwapTeam(int inputId)
    {
        int teamNumber = GetWhichTeamPlayerIsIn(inputId);
        if (teamNumber == 0)
        {
            //Swap from yellow team to blue team
            int i = data.playerControlsTeam0.IndexOf(inputId);
            data.playerNumbersTeam0.RemoveAt(i);
            data.playerControlsTeam0.RemoveAt(i);

            TeamSetupPlayerDisplay gScript = gridTeam0.transform.GetChild(i).GetComponent<TeamSetupPlayerDisplay>();
            gridTeam0.transform.GetChild(i).SetParent(gridTeam1.transform);
            data.playerNumbersTeam1.Add(gScript.playerNumber);
            data.playerControlsTeam1.Add(gScript.inputId);
            gScript.PlayShineAnimation();
        }
        else
        {
            //Swap from blue team to yellow team
            int i = data.playerControlsTeam1.IndexOf(inputId);
            data.playerNumbersTeam1.RemoveAt(i);
            data.playerControlsTeam1.RemoveAt(i);

            TeamSetupPlayerDisplay gScript = gridTeam1.transform.GetChild(i).GetComponent<TeamSetupPlayerDisplay>();
            gridTeam1.transform.GetChild(i).SetParent(gridTeam0.transform);
            data.playerNumbersTeam0.Add(gScript.playerNumber);
            data.playerControlsTeam0.Add(gScript.inputId);
            gScript.PlayShineAnimation();
        }
    }

    public void AddBotToTeam(int teamNumber)
    {
        if (currentPanelId == Menu.ExhibitionTeamSetup)
        {
            if (data.playerNumbersTeam0.Count + data.playerNumbersTeam1.Count >= 8)
            {
                return;
            }
            if (teamNumber == 0)
            {
                //add to Yellow team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridTeam0.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(8, -1);
                data.playerControlsTeam0.Add(-1);
                data.playerNumbersTeam0.Add(8);
            }
            else
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridTeam1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(8, -1);
                data.playerControlsTeam1.Add(-1);
                data.playerNumbersTeam1.Add(8);
            }
        }

        if (currentPanelId == Menu.SwipeShotSetup)
        {
            if (data.playerNumbersTeam0.Count + data.playerNumbersTeam1.Count >= 2)
            {
                return;
            }
            if (teamNumber == 0)
            {
                //add to Yellow team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo0.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(8, -1);
                data.playerControlsTeam0.Add(-1);
                data.playerNumbersTeam0.Add(8);
            }
            else
            {
                //add to Blue team
                GameObject g = Instantiate(teamSetupPlayerDisplayPrefab, gridDuo1.transform);
                TeamSetupPlayerDisplay gScript = g.GetComponent<TeamSetupPlayerDisplay>();
                gScript.Init(8, -1);
                data.playerControlsTeam1.Add(-1);
                data.playerNumbersTeam1.Add(8);
            }
        }

        //audio
        audioManager.Play("ControllerOn");
    }

    public void RemoveBotFromTeam(int teamNumber)
    {
        if (currentPanelId == Menu.ExhibitionTeamSetup)
        {
            if (teamNumber == 0)
            {
                //remove most recently placed bot from Yellow team
                for (int i = data.playerNumbersTeam0.Count - 1; i >= 0; i--)
                {
                    if (data.playerNumbersTeam0[i] == 8)
                    {
                        //audio
                        audioManager.Play("ControllerOff");

                        data.playerNumbersTeam0.RemoveAt(i);
                        data.playerControlsTeam0.RemoveAt(i);
                        Destroy(gridTeam0.transform.GetChild(i).gameObject);
                        break;
                    }
                }
            }
            else
            {
                //remove most recently placed bot from Blue team
                for (int i = data.playerNumbersTeam1.Count - 1; i >= 0; i--)
                {
                    if (data.playerNumbersTeam1[i] == 8)
                    {
                        //audio
                        audioManager.Play("ControllerOff");

                        data.playerNumbersTeam1.RemoveAt(i);
                        data.playerControlsTeam1.RemoveAt(i);
                        Destroy(gridTeam1.transform.GetChild(i).gameObject);
                        break;
                    }
                }
            }
        }
        if (currentPanelId == Menu.SwipeShotSetup)
        {
            if (teamNumber == 0)
            {
                //remove most recently placed bot from Yellow team
                for (int i = data.playerNumbersTeam0.Count - 1; i >= 0; i--)
                {
                    if (data.playerNumbersTeam0[i] == 8)
                    {
                        //audio
                        audioManager.Play("ControllerOff");

                        data.playerNumbersTeam0.RemoveAt(i);
                        data.playerControlsTeam0.RemoveAt(i);
                        Destroy(gridDuo0.transform.GetChild(i).gameObject);
                        break;
                    }
                }
            }
            else
            {
                //remove most recently placed bot from Blue team
                for (int i = data.playerNumbersTeam1.Count - 1; i >= 0; i--)
                {
                    if (data.playerNumbersTeam1[i] == 8)
                    {
                        //audio
                        audioManager.Play("ControllerOff");

                        data.playerNumbersTeam1.RemoveAt(i);
                        data.playerControlsTeam1.RemoveAt(i);
                        Destroy(gridDuo1.transform.GetChild(i).gameObject);
                        break;
                    }
                }
            }
        }
    }

    public void AddBotTeam0()
    {
        AddBotToTeam(0);
    }

    public void RemoveBotTeam0()
    {
        RemoveBotFromTeam(0);
    }

    public void AddBotTeam1()
    {
        AddBotToTeam(1);
    }

    public void RemoveBotTeam1()
    {
        RemoveBotFromTeam(1);
    }

    public int GetWhichTeamPlayerIsIn(int inputId)
    {
        if (data.playerControlsTeam0.Contains(inputId))
            return 0;
        else
            return 1;
    }

    public int GetSmalledAvailablePlayerNumber()
    {
        bool[] activePlayerNumbers = new bool[8];
        for (int i = 0; i < data.playerNumbersTeam0.Count; i++)
        {
            if (data.playerNumbersTeam0[i] != 8)
                activePlayerNumbers[data.playerNumbersTeam0[i]] = true;
        }
        for (int i = 0; i < data.playerNumbersTeam1.Count; i++)
        {
            if (data.playerNumbersTeam1[i] != 8)
                activePlayerNumbers[data.playerNumbersTeam1[i]] = true;
        }

        for (int i = 0; i < activePlayerNumbers.Length; i++)
        {
            if (!activePlayerNumbers[i])
                return i;
        }
        return activePlayerNumbers.Length;
    }

    public bool IsInputIdInGame(int inputId)
    {
        for (int i = 0; i < data.playerControlsTeam0.Count; i++)
        {
            if (data.playerControlsTeam0[i] == inputId)
                return true;
        }
        for (int i = 0; i < data.playerControlsTeam1.Count; i++)
        {
            if (data.playerControlsTeam1[i] == inputId)
                return true;
        }
        return false;
    }


    public void OpenSetup()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].DisableMenu();
        }
        panels[3].EnableMenu();

        //Master Controller should indicate whicnh player is set to P1

    }


    public void StepBack()
    {
        for (int i = 0; i < panels.Length; i++)
        {
            if (i != 0 && panels[i].gameObject.activeSelf)
            {
                panels[i].DisableMenu();
                panels[i - 1].EnableMenu();
                audioManager.Play("ButtonBack");
                break;
            }
        }
    }

    public void PlayGame()
    {
        Destroy(FindObjectOfType<AudioManager>().gameObject);
        if (data.gamemode == Gamemode.Exhibition)
        {
            if (data.middlePlatform)
            {
                SceneManager.LoadScene(1);
            }
            else
            {
                SceneManager.LoadScene(2);
            }
        }
        else if (data.gamemode == Gamemode.Rally)
        {
            if (data.middlePlatform)
            {
                SceneManager.LoadScene(4);
            }
            else
            {
                SceneManager.LoadScene(5);
            }
        }

    }

    public void PlayTutorial()
    {
        data.gamemode = Gamemode.Tutorial;
        Destroy(FindObjectOfType<AudioManager>().gameObject);
        SceneManager.LoadScene(3);
    }

    public void SqueakyOnClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Semi-Frequent")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Frequent";
            BhbPlayerController.shoeSqueakRate = 0.01f;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Frequent")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Never";
            BhbPlayerController.shoeSqueakRate = 10000.0f;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Never")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Semi-Frequent";
            BhbPlayerController.shoeSqueakRate = 0.5f;
        }

        PlayClickSound();
    }

    public void CameraShakeOnClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Enabled")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Disabled";
            data.cameraShake = false;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Disabled")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Enabled";
            data.cameraShake = true;
        }

        PlayClickSound();
    }

    public void PowerOnClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Off")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Low";
            data.powerUpSpawnage = PowerUpSpawnage.Low;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Low")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Medium";
            data.powerUpSpawnage = PowerUpSpawnage.Medium;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Medium")
        {
            currentSelection.GetComponentInChildren<Text>().text = "High";
            data.powerUpSpawnage = PowerUpSpawnage.High;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "High")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Chaotic";
            data.powerUpSpawnage = PowerUpSpawnage.Chaotic;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Chaotic")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Off";
            data.powerUpSpawnage = PowerUpSpawnage.None;
        }

        PlayClickSound();
    }

    public void TimerOnClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "2:00")
        {
            currentSelection.GetComponentInChildren<Text>().text = "3:00";
            data.matchLength = 180;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "3:00")
        {
            currentSelection.GetComponentInChildren<Text>().text = "4:00";
            data.matchLength = 240;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "4:00")
        {
            currentSelection.GetComponentInChildren<Text>().text = "5:00";
            data.matchLength = 300;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "5:00")
        {
            currentSelection.GetComponentInChildren<Text>().text = "2:00";
            data.matchLength = 120;
        }

        PlayClickSound();
    }

    public void BulletSizeOnClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Small Only")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Big Only";
            data.bulletSpawnage = BulletSpawnage.BigOnly;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Big Only")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Big & Small";
            data.bulletSpawnage = BulletSpawnage.BothRegularAndBig;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Big & Small")
        {
            currentSelection.GetComponentInChildren<Text>().text = "No Bullets";
            data.bulletSpawnage = BulletSpawnage.None;
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "No Bullets")
        {
            currentSelection.GetComponentInChildren<Text>().text = "Small Only";
            data.bulletSpawnage = BulletSpawnage.RegularOnly;
        }

        PlayClickSound();
    }

    public void DunkBonusOnClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Enabled")
        {
            data.dunkBonus = false;
            currentSelection.GetComponentInChildren<Text>().text = "Disabled";
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Disabled")
        {
            data.dunkBonus = true;
            currentSelection.GetComponentInChildren<Text>().text = "Enabled";
        }

        PlayClickSound();
    }

    public void TimeOfDayClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Golden Hour")
        {
            data.nightTime = true;
            currentSelection.GetComponentInChildren<Text>().text = "Midnight";
            SetTimeVisual(true);
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Midnight")
        {
            data.nightTime = false;
            currentSelection.GetComponentInChildren<Text>().text = "Golden Hour";
            SetTimeVisual(false);
        }

        PlayClickSound();
    }

    public void SetTimeVisual(bool nightTime)
    {
        if (nightTime)
        {
            RenderSettings.skybox = nightSkybox;
            RenderSettings.ambientLight = new Color(67.0f / 255.0f, 67.0f / 255.0f, 77.0f / 255.0f);
            RenderSettings.fogColor = new Color(63.0f / 255.0f, 63.0f / 255.0f, 204.0f / 255.0f);
            RenderSettings.sun.color = new Color(131 / 255.0f, 142 / 255.0f, 173 / 255.0f);
        }
        else
        {
            RenderSettings.skybox = daySkybox;
            RenderSettings.ambientLight = new Color(236.0f / 255.0f, 95.0f / 255.0f, 28.0f / 255.0f);
            RenderSettings.fogColor = new Color(202.0f / 255.0f, 139.0f / 255.0f, 106.0f / 255.0f);
            RenderSettings.sun.color = new Color(255 / 255.0f, 203 / 255.0f, 145 / 255.0f);
        }
    }

    public void MiddlePlatformClick()
    {
        if (currentSelection.GetComponentInChildren<Text>().text == "Enabled")
        {
            data.middlePlatform = false;
            middlePlatform.SetActive(false);
            currentSelection.GetComponentInChildren<Text>().text = "Disabled";
        }
        else if (currentSelection.GetComponentInChildren<Text>().text == "Disabled")
        {
            data.middlePlatform = true;
            middlePlatform.SetActive(true);
            currentSelection.GetComponentInChildren<Text>().text = "Enabled";
        }

        PlayClickSound();
    }

    public void GoToCustomizeCourt()
    {
        backgroundOverlay.gameObject.SetActive(false);
        postProcessingVolume.profile = gameProfile;
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].DisableMenu();
        }
        panels[(int)Menu.CustomizeCourt].EnableMenu();
        audioManager.Play("ButtonSelect");
    }

    public void BackOutOfCustomizeCourt()
    {
        backgroundOverlay.gameObject.SetActive(true);
        postProcessingVolume.profile = menuProfile;
        for (int i = 0; i < panels.Length; i++)
        {
            panels[i].DisableMenu();
        }
        if (data.gamemode == Gamemode.Rally)
        {
            panels[(int)Menu.SwipeShotSetup].EnableMenu();
        }
        else
        {
            panels[(int)Menu.ExhibitionTeamSetup].EnableMenu();
        }
        audioManager.Play("ButtonBack");
    }

    private void PlayClickSound()
    {
        audioManager.Play("Hit", 0.2f, 2.5f, 3.0f);
    }

    //takes you back to the first menu
    /*
    public void OptionsMenuBackButton()
    {
        OptionsPanel.DisableMenu();
        TitlePanel.EnableMenu();
    }
    */

    //closes application window or ends Unity editor playing
    public void QuitGame()
    {
#if UNITY_EDITOR
        // Application.Quit() does not work in the editor so
        // UnityEditor.EditorApplication.isPlaying need to be set to false to end the game
        UnityEditor.EditorApplication.isPlaying = false;
#else
         Application.Quit();
#endif
    }
}
