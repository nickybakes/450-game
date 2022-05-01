using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public enum PanelAnimationState
{
    Right_To_Center,
    Center_To_Left,
    Left_To_Center,
    Center_To_Right,
}

public class PanelManager : MonoBehaviour
{
    public Button defaultButton;

    public MainMenuManager mainMenuManager;

    public Menu panelId;

    public Animator animator;

    public HologramButton backButton;

    // Start is called before the first frame update
    void Start()
    {
        if (mainMenuManager == null)
            mainMenuManager = FindObjectOfType<MainMenuManager>();

    }

    public void FindAnimator()
    {
        animator = gameObject.GetComponent<Animator>();
    }

    public void AnimateEnableMenu(bool goingForwardInMenu)
    {

        EnableMenu();
        if (goingForwardInMenu)
            SetAnimationState(PanelAnimationState.Right_To_Center);
        else
            SetAnimationState(PanelAnimationState.Left_To_Center);

        if (panelId == Menu.Loading)
        {
            StartCoroutine(StartLoadProcess());
        }
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
        mainMenuManager.currentPanelId = this.panelId;
    }

    public void AnimateDisableMenu(bool goingForwardInMenu)
    {
        if (goingForwardInMenu)
            SetAnimationState(PanelAnimationState.Center_To_Left);
        else
            SetAnimationState(PanelAnimationState.Center_To_Right);

        StartCoroutine(TimedDisableMenu());
    }

    public IEnumerator StartLoadProcess()
    {
        yield return new WaitForSecondsRealtime(2.2f);
        mainMenuManager.LoadGameScene();
        // DisableMenu();
    }

    public IEnumerator TimedDisableMenu()
    {
        yield return new WaitForSecondsRealtime(.2f);
        DisableMenu();
    }

    public void DisableMenu()
    {
        gameObject.SetActive(false);
    }

    public bool IsInTransition()
    {
        if (animator == null)
            return false;
        return animator.IsInTransition(0);
    }


    public void SetAnimationState(PanelAnimationState state)
    {
        if (animator == null)
        {
            return;
        }
        animator.SetTrigger(state.ToString());
    }
}
