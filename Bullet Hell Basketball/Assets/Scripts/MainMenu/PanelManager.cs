using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class PanelManager : MonoBehaviour
{
    public Button defaultButton;

    public MainMenuManager mainMenuManager;


    // Start is called before the first frame update
    void Start()
    {
        if (mainMenuManager == null)
            mainMenuManager = FindObjectOfType<MainMenuManager>();
    }



    public void EnableMenu()
    {
        enabled = true;
        gameObject.SetActive(true);
        if (mainMenuManager.currentSelection != null)
        {
            mainMenuManager.currentSelection.gameObject.GetComponent<HologramButton>().ForceDeselectVisual();
        }
        defaultButton.Select();
    }

    public void DisableMenu()
    {
        gameObject.SetActive(false);
    }
}
