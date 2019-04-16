using System;
using UnityEngine;

public class ColliderMouseHandler : MonoBehaviour {

	public Action OnMouseEnterAction { get; set; }

	public Action OnMouseExitAction { get; set; }

	public Action OnMouseUpAction { get; set; }

	public Action OnMouseDownAction { get; set; }

	public Action OnMouseDragAction { get; set; }

	public Action OnMouseOverAction { get; set; }

	private void OnMouseEnter()
	{
		if (OnMouseEnterAction != null)
			OnMouseEnterAction();
	}

	private void OnMouseExit()
	{
		if (OnMouseExitAction != null)
			OnMouseExitAction();
	}

	private void OnMouseUp()
	{
		if (OnMouseUpAction != null)
			OnMouseUpAction();
	}

	private void OnMouseDown()
	{
		if (OnMouseDownAction != null)
			OnMouseDownAction();
	}

	private void OnMouseDrag()
	{
		if (OnMouseDragAction != null)
			OnMouseDragAction();
	}

	private void OnMouseOver()
	{
		if (OnMouseOverAction != null)
			OnMouseOverAction();
	}

}
