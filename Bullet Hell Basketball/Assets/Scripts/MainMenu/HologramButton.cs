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
    private Outline textOutline;

    public bool hideBorderWhileSelected;

    void Awake()
    {
        menuManager = FindObjectOfType<MainMenuManager>();
        buttonComponent = GetComponent<Button>();
        border = transform.GetChild(1).GetComponent<Image>();

        EventTrigger trigger = GetComponent<EventTrigger>();
        if (trigger)
            Destroy(trigger);

        trigger = gameObject.AddComponent<EventTrigger>();
        EventTrigger.Entry entry = new EventTrigger.Entry();
        EventTrigger.Entry exit = new EventTrigger.Entry();
        entry.eventID = EventTriggerType.PointerEnter;
        exit.eventID = EventTriggerType.PointerExit;
        trigger.runInEditMode = true;
        entry.callback.AddListener((eventData) => { this.OnMouseEnter(); });
        exit.callback.AddListener((eventData) => { this.OnMouseExit(); });
        trigger.triggers.Add(entry);
        trigger.triggers.Add(exit);

        text = GetComponentInChildren<Text>();
        if (text != null)
        {
            defaultFontSize = text.fontSize;
            selectedFontSize = (int)(defaultFontSize * 1.07f);
            textShadow = text.GetComponent<Shadow>();
            if (textShadow != null && !(textShadow is Outline))
                Destroy(textShadow);
            textShadow = text.gameObject.AddComponent<Shadow>();
            textShadow.effectColor = new Color(0, 0, 0, 1);
            textShadow.effectDistance = new Vector2(0, -4 * (Mathf.Max(2.5f, 1.0f/text.transform.localScale.y)));
            textShadow.enabled = false;
        }

    }

    public void OnSelect(BaseEventData eventData)
    {
        SelectVisual();
        // transform.GetChild(0).gameObject.SetActive(true);
        // transform.GetChild(1).gameObject.SetActive(false);

        // if (transform.GetChild(4).gameObject != null)
        // {
        // 	transform.GetChild(3).gameObject.SetActive(true);
        // 	transform.GetChild(4).gameObject.SetActive(false);
        // }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        DeselectVisual();

        // transform.GetChild(1).gameObject.SetActive(true);
        // transform.GetChild(0).gameObject.SetActive(false);

        // if (transform.GetChild(4).gameObject != null)
        // {
        // 	transform.GetChild(4).gameObject.SetActive(true);
        // 	transform.GetChild(3).gameObject.SetActive(false);
        // }
    }


    public void OnModeSelect()
    {
        if (transform.GetChild(4).gameObject != null)
        {
            transform.GetChild(3).gameObject.SetActive(true);
            transform.GetChild(4).gameObject.SetActive(false);
        }
    }


    public void OnModeDeselect()
    {
        if (transform.GetChild(4).gameObject != null)
        {
            transform.GetChild(4).gameObject.SetActive(true);
            transform.GetChild(3).gameObject.SetActive(false);
        }
    }


    public void SelectVisual()
    {
        transform.GetChild(0).gameObject.SetActive(true);
        border.material = menuManager.borderFlashMaterial;
        if (text != null)
        {
            text.fontSize = selectedFontSize;
            textShadow.enabled = true;
        }
    }

    public void DeselectVisual()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        border.material = null;
        if (text != null)
        {
            text.fontSize = defaultFontSize;
            textShadow.enabled = false;
        }
    }

    public void OnMouseEnter()
    {
        buttonComponent.Select();
    }

    public void OnMouseExit()
    {
        // if (EventSystem.current.currentSelectedGameObject != gameObject)
        //     DeselectVisual();
    }

    public void ForceDeselectVisual(){
        DeselectVisual();
    }
}