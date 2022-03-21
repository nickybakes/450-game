using UnityEngine;
using TMPro;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    [SerializeField] private Text title;

    private Button[] buttons;

    [SerializeField] private Button playButton;
    [SerializeField] private Button optionsButton;
    [SerializeField] private Button quitButton;

    [SerializeField] private Button backButton;

    [SerializeField] private bool startInPlayMode = false;

    private void Start()
    {
        buttons = new Button[]
        {
            playButton,
            optionsButton,
            quitButton,
            backButton
        };
        foreach (Button b in buttons)
        {
            b.interactable = false;
        }
        if(startInPlayMode)
        {
            foreach (Button b in buttons)
                b.interactable = false;
            
            GameManager.Instance.Paused = false;
        }
    }

    private void FadeInButtons()
    {
        foreach (Button b in buttons)
        {
            b.interactable = true;
        }
    }

    private void FadeOutButtons()
    {
        foreach(Button b in buttons)
        {
            b.interactable = false;
        }
    }

    public void Show()
    {
        foreach (Button b in buttons)
        {
            b.interactable = true;
        }

        GameManager.Instance.Paused = true;
    }

    public void Appeared()
    {
        foreach (Button b in buttons)
        {
            b.interactable = true;
        }
    }

    public void ButtonPlay()
    {
        foreach (Button b in buttons)
        {
            b.interactable = false;
        }

        GameManager.Instance.Paused = false;
    }

    public void ButtonOptions()
    {
        foreach (Button b in buttons)
        {
            b.interactable = false;
        }
    }

    public void ButtonQuit()
    {
        Application.Quit();
    }

    public void ButtonBack()
    {
        foreach (Button b in buttons)
        {
            b.interactable = false;
        }
    }
}
