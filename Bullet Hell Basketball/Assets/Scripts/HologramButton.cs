using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class HologramButton : MonoBehaviour, ISelectHandler// required interface when using the OnSelect method.
{
	//Do this when the selectable UI object is selected.
	
	public void OnSelect(BaseEventData eventData)
	{
		this.gameObject.transform.GetChild(0).SetActive(true);
		this.gameObject.transform.GetChild(1).SetActive(false);
	}

	public void OnDeselect(BaseEventData eventData)
	{
		this.gameObject.transform.GetChild(1).SetActive(true);
		this.gameObject.transform.GetChild(0).SetActive(false);
	}
	

	public void OnMouseOver()
	{
		Debug.Log(" waatrasrsdtsard selected");
	}

	public void OnMouseExit()
	{
		Debug.Log(this.gameObject.name + "rats art art at selected");
	}
}