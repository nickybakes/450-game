using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using UnityEngine.EventSystems;// Required when using Event data.

public class HologramButton : MonoBehaviour, ISelectHandler, IDeselectHandler// required interface when using the OnSelect method.
{
	//Do this when the selectable UI object is selected.
	
	public void OnSelect(BaseEventData eventData)
	{
		transform.GetChild(0).gameObject.SetActive(true);
		transform.GetChild(1).gameObject.SetActive(false);

		if (transform.GetChild(4).gameObject != null)
		{
			transform.GetChild(3).gameObject.SetActive(true);
			transform.GetChild(4).gameObject.SetActive(false);
		}
	}

	public void OnDeselect(BaseEventData eventData)
	{
		transform.GetChild(1).gameObject.SetActive(true);
		transform.GetChild(0).gameObject.SetActive(false);

		if (transform.GetChild(4).gameObject != null)
		{
			transform.GetChild(4).gameObject.SetActive(true);
			transform.GetChild(3).gameObject.SetActive(false);
		}
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