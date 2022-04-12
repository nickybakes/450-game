using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public static class Controller
{
    public const KeyCode j10 = KeyCode.Joystick1Button0;
    public const KeyCode w = KeyCode.W;
}


public class MainMenuManager : MonoBehaviour
{
    private string[] controllers = { "1", "2", "3", "4", "5", "6", "7", "8", "K" };
    private bool[] canMoveSelection = new bool[9];

    //public PanelManager TitlePanel;
    //public PanelManager MainPanel;
    //public PanelManager ModePanel;
    //public PanelManager PlayerPanel;
    //public PanelManager StagePanel;
    //public PanelManager OptionsPanel;

    public PanelManager[] Panels;

    public Button currentSelection;


    // Start is called before the first frame update
    void Start()
    {

        foreach (PanelManager panel in Panels)
        {
            panel.gameObject.SetActive(false);
        }

        Panels[0].gameObject.SetActive(true);

        EventSystem.current.firstSelectedGameObject = Panels[0].defaultButton.gameObject;

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
                Debug.Log(currentSelection);
                ExecuteEvents.Execute(currentSelection.gameObject,
                    new BaseEventData(EventSystem.current), ExecuteEvents.submitHandler);
                break;
            }

            //get the current direction the player is pressing in
            float horizontalInput = 0f;
            float verticalInput = 0f;

            // if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.P))
            //     verticalInput = 1;
            // if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.Semicolon))
            //     verticalInput = -1;
            // if (!Input.GetKey(KeyCode.W) && !Input.GetKey(KeyCode.S) && !Input.GetKey(KeyCode.Semicolon) && !Input.GetKey(KeyCode.P))
            //     verticalInput = 0;

            // if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.Quote))
            //     horizontalInput = 1;
            // if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.L))
            //     horizontalInput = -1;
            // if (!Input.GetKey(KeyCode.A) && !Input.GetKey(KeyCode.D) && !Input.GetKey(KeyCode.Quote) && !Input.GetKey(KeyCode.L))
            //     horizontalInput = 0;


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
        foreach (PanelManager panel in Panels)
        {
            panel.DisableMenu();
        }

        Panels[i].EnableMenu();
    }

    public void StepUp()
    {
        for (int i = 0; i < Panels.Length; i++)
        {
            if (i != Panels.Length - 1 && Panels[i].enabled)
            {
                Panels[i].DisableMenu();
                Panels[i+1].EnableMenu();
                break;
            }
        }
    }

    public void StepBack()
    {
        for(int i = 0; i < Panels.Length; i++)
        {
            if (i != 0 && Panels[i].enabled)
            {
                Panels[i].DisableMenu();
                Panels[i - 1].EnableMenu();
                break;
            }
        }
    }

    public void PlayGame()
    {
        SceneManager.LoadScene(1);
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
