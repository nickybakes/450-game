using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class HologramButton : MonoBehaviour, ISelectHandler, IDeselectHandler// required interface when using the OnSelect method.
{
    //Do this when the selectable UI object is selected.

    private MainMenuManager menuManager;
    private Button buttonComponent;

    private Image border;

    private Text text;

    private int defaultFontSize;
    private int selectedFontSize;

    private Shadow textShadow;
    private UnityEngine.UI.Outline textOutline;

    public bool hideBorderWhileSelected;

    public bool isSlider;

    public Slider sliderComponent;

    private PanelManager parentPanel;


    void Awake()
    {
        menuManager = FindObjectOfType<MainMenuManager>();
        buttonComponent = GetComponent<Button>();

        if (!isSlider)
            border = transform.GetChild(1).GetComponent<Image>();

        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger)
            Destroy(trigger);

        trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        EventTrigger.Entry exit = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        exit.eventID = EventTriggerType.PointerExit;
#if UNITY_EDITOR
        trigger.runInEditMode = true;
#endif
        entry.callback.AddListener((eventData) => { this.OnMouseEnter(); });
        exit.callback.AddListener((eventData) => { this.OnMouseExit(); });
        trigger.triggers.Add(entry);
        trigger.triggers.Add(exit);

        if (!isSlider)
        {
            EventTrigger.Entry click = new EventTrigger.Entry();
            click.eventID = EventTriggerType.PointerClick;
            click.callback.AddListener((eventData) => { this.OnMouseClick(); });
            trigger.triggers.Add(click);
        }

        text = GetComponentInChildren<Text>();
        if (text != null)
        {
            defaultFontSize = text.fontSize;
            selectedFontSize = (int)(defaultFontSize * 1.07f);
            textShadow = text.GetComponent<Shadow>();
            if (textShadow != null && !(textShadow is UnityEngine.UI.Outline))
                Destroy(textShadow);
            textShadow = text.gameObject.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0, 0, 0, 1);
            textShadow.effectDistance = new Vector2(0, -4 * (Mathf.Max(2.5f, 1.0f / text.transform.localScale.y)));
            textShadow.enabled = false;
        }

        GameObject currentPanelSearchObject = gameObject;
        for (int i = 0; i < 50; i++)
        {
            parentPanel = currentPanelSearchObject.GetComponentInParent<PanelManager>();
            if (parentPanel == null)
            {
                if (currentPanelSearchObject.transform.parent == null)
                    break;
                currentPanelSearchObject = currentPanelSearchObject.transform.parent.gameObject;
            }
            else
            {
                break;
            }
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        SelectVisual();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DeselectVisual();
    }

    public void SelectVisual()
    {
        transform.GetChild(0).gameObject.SetActive(true);

        if (isSlider)
            return;
        //border.material = menuManager.borderFlashMaterial;
        if (text != null)
        {
            text.fontSize = selectedFontSize;
            textShadow.enabled = true;
        }
    }

    public void DeselectVisual()
    {
        transform.GetChild(0).gameObject.SetActive(false);

        if (isSlider)
            return;

        border.material = null;
        if (text != null)
        {
            text.fontSize = defaultFontSize;
            textShadow.enabled = false;
        }
    }

    public void OnMouseClick()
    {
        if (parentPanel != null && parentPanel.IsInTransition())
        {
            return;
        }

        if (isSlider)
            return;

        if (menuManager != null)
            menuManager.masterController = "M";
    }

    public void OnMouseEnter()
    {
        if (parentPanel != null && parentPanel.IsInTransition())
        {
            return;
        }

        if (isSlider)
        {
            SelectVisual();
        }

        buttonComponent.Select();
    }

    public void OnMouseExit()
    {
        // if (EventSystem.current.currentSelectedGameObject != gameObject)
        //     DeselectVisual();
    }

    public void ForceDeselectVisual()
    {
        DeselectVisual();
    }
}