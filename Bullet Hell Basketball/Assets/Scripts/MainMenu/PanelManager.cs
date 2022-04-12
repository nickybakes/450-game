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

    public bool enabled;

    // Start is called before the first frame update
    void Start()
    {
        if (mainMenuManager == null)
            mainMenuManager = FindObjectOfType<MainMenuManager>();

        enabled = true;
    }



    public void EnableMenu()
    {
        enabled = true;
        gameObject.SetActive(true);
        defaultButton.Select();
    }

    public void DisableMenu()
    {
        gameObject.SetActive(false);
        enabled = false;
    }
}
