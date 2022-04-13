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

    void Awake()
    {
        menuManager = FindObjectOfType<MainMenuManager>();
        buttonComponent = GetComponent<Button>();
        border = transform.GetChild(1).GetComponent<Image>();
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
    }

    public void DeselectVisual()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        border.material = null;
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
}