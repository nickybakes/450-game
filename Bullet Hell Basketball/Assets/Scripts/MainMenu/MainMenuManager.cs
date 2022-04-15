using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum Menu
{
    Title,
    Main,
    GamemodeSelect,
    ExhibitionTeamSetup
}

public static class Controller
{
    public const KeyCode j10 = KeyCode.Joystick1Button0;
    public const KeyCode w = KeyCode.W;
}


public class MainMenuManager : MonoBehaviour
{
    private string[] controllers = { "1", "2", "3", "4", "5", "6", "7", "8", "K" };
    private string masterController;
    private bool[] canMoveSelection = new bool[9];

    public Material borderFlashMaterial;

    //public PanelManager TitlePanel;
    //public PanelManager MainPanel;
    //public PanelManager ModePanel;
    //public PanelManager PlayerPanel;
    //public PanelManager StagePanel;
    //public PanelManager OptionsPanel;

    public PanelManager[] panels;

    public Button currentSelection;

    private AudioManager audioManager;
    private Button prevSelection;
    private int currentPanel;


    // Start is called before the first frame update
    void Start()
    {
        audioManager = FindObjectOfType<AudioManager>();
        audioManager.Play("MusicMenu");

        foreach (PanelManager panel in panels)
        {
            panel.gameObject.SetActive(false);
        }

        panels[0].gameObject.SetActive(true);

        EventSystem.current.firstSelectedGameObject = panels[0].defaultButton.gameObject;

    }

    // Update is called once per frame
    void Update()
    {
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

        for (int i = 1; currentSelection != null && i < 9; i++)
        {
            //checks if controllers or keyboard have pressed "A" or space, if so, "click" the current button
            if (Input.GetButtonDown("J" + controllers[i] + "A"))
            {
                masterController = controllers[i];
                
                HologramButton hologramButton = currentSelection.gameObject.GetComponent<HologramButton>();
                if (hologramButton != null)
                {
                    hologramButton.DeselectVisual();
                }
                ExecuteEvents.Execute(currentSelection.gameObject,
                    new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                break;
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
        //if(GameObject.Find("Squeak").GetComponentInChildren<Text>().text == "Semi-Frequent")
        //{
            //BhbPlayerController.shoeSqueakRate = 0.5f;
        //}
        //if(GameObject.Find("Timer").GetComponentInChildren<Text>().text == "3:00")
        //{
            //GameManager.matchTimeMax = 180;
        //}
        SceneManager.LoadScene(1);
    }

    public void PlayTutorial()
    {
        Destroy(FindObjectOfType<AudioManager>().gameObject);
        SceneManager.LoadScene(2);
    }

    public void SqueakyOnClick()
    {
        if(GameObject.Find("Squeak").GetComponentInChildren<Text>().text == "Semi-Frequent")
        {
            GameObject.Find("Squeak").GetComponentInChildren<Text>().text = "Frequent";
            BhbPlayerController.shoeSqueakRate = 0.01f;
        }
        else if (GameObject.Find("Squeak").GetComponentInChildren<Text>().text == "Frequent")
        {
            GameObject.Find("Squeak").GetComponentInChildren<Text>().text = "Never";
            BhbPlayerController.shoeSqueakRate = 10000.0f;
        }
        else if (GameObject.Find("Squeak").GetComponentInChildren<Text>().text == "Never")
        {
            GameObject.Find("Squeak").GetComponentInChildren<Text>().text = "Semi-Frequent";
            BhbPlayerController.shoeSqueakRate = 0.5f;
        }
    }

    public void CameraShakeOnClick()
    {
        if(GameObject.Find("CameraShake").GetComponentInChildren<Text>().text == "Enabled")
        {
            GameObject.Find("CameraShake").GetComponentInChildren<Text>().text = "Disabled";
        }
        else if (GameObject.Find("CameraShake").GetComponentInChildren<Text>().text == "Disabled")
        {
            GameObject.Find("CameraShake").GetComponentInChildren<Text>().text = "Enabled";
        }
    }

    public void PowerOnClick()
    {
        if(GameObject.Find("Power").GetComponentInChildren<Text>().text == "Enabled")
        {
            GameObject.Find("Power").GetComponentInChildren<Text>().text = "Disabled";
        }
        else if (GameObject.Find("Power").GetComponentInChildren<Text>().text == "Disabled")
        {
            GameObject.Find("Power").GetComponentInChildren<Text>().text = "Enabled";
        }
    }

    public void TimerOnClick()
    {
        if(GameObject.Find("Timer").GetComponentInChildren<Text>().text == "2:00")
        {
            GameObject.Find("Timer").GetComponentInChildren<Text>().text = "3:00";
            GameData.matchLength = 180;
        }
        else if (GameObject.Find("Timer").GetComponentInChildren<Text>().text == "3:00")
        {
            GameObject.Find("Timer").GetComponentInChildren<Text>().text = "4:00";
            GameData.matchLength = 240;
        }
        else if (GameObject.Find("Timer").GetComponentInChildren<Text>().text == "4:00")
        {
            GameObject.Find("Timer").GetComponentInChildren<Text>().text = "5:00";
            GameData.matchLength = 300;
        }
        else if (GameObject.Find("Timer").GetComponentInChildren<Text>().text == "5:00")
        {
            GameObject.Find("Timer").GetComponentInChildren<Text>().text = "2:00";
            GameData.matchLength = 120;
        }
    }

    public void BulletSizeOnClick()
    {
        if(GameObject.Find("Bullets").GetComponentInChildren<Text>().text == "Small Only")
        {
            GameObject.Find("Bullets").GetComponentInChildren<Text>().text = "Big Only";
        }
        else if (GameObject.Find("Bullets").GetComponentInChildren<Text>().text == "Big Only")
        {
            GameObject.Find("Bullets").GetComponentInChildren<Text>().text = "Big & Small";
        }
        else if (GameObject.Find("Bullets").GetComponentInChildren<Text>().text == "Big & Small")
        {
            GameObject.Find("Bullets").GetComponentInChildren<Text>().text = "No Bullets";
        }
        else if (GameObject.Find("Bullets").GetComponentInChildren<Text>().text == "No Bullets")
        {
            GameObject.Find("Bullets").GetComponentInChildren<Text>().text = "Small Only";
        }
    }

    public void DunkBonusOnClick()
    {
        if(GameObject.Find("DunkBonus").GetComponentInChildren<Text>().text == "Enabled")
        {
            GameObject.Find("DunkBonus").GetComponentInChildren<Text>().text = "Disabled";
        }
        else if (GameObject.Find("DunkBonus").GetComponentInChildren<Text>().text == "Disabled")
        {
            GameObject.Find("DunkBonus").GetComponentInChildren<Text>().text = "Enabled";
        }
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
