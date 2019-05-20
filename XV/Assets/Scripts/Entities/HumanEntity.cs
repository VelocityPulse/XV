using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(CapsuleCollider))]
[RequireComponent(typeof(MovableEntity))]
public class HumanEntity : AEntity
{
	public static HumanEntity Instance { get; private set; }

	[SerializeField]
	private UIBubbleInfo UIBubbleInfo;

	[SerializeField]
	private MouseHandler HumanBodyMouseHandler;

	private MovableEntity mMovableEntity;

	private HumanDataScene mHDS;
	//private bool mBusy;
	private bool mSelected;
	private bool mControlPushed;
	private bool mMouseDown;

	private bool mMouseOverObjectEntity;
	private bool mMouseDragObjectEntity;

	private Vector3 mMouseOriginClick;

	private Vector3 mCenter;
	//private Vector3 mSize;

	public override bool Selected
	{
		get { return mSelected; }

		set
		{
			if (!value) {
				Utils.PrintStackTrace();
				Debug.Log("ObjectEntity : " + mHDS.Name + "Has been unselected");
				UIBubbleInfo.Hide();
			}
			mSelected = value;
		}
	}

	public override string Name
	{
		get { return gameObject.name; }

		set
		{
			if (string.IsNullOrEmpty(value))
				return;

			gameObject.name = value;
			name = value + "_human";
			mHDS.Name = value;
			SaveEntity();
		}
	}

	private void Start()
	{
		mMovableEntity = GetComponent<MovableEntity>();

		mCenter = Vector3.zero;

		Instance = this;

		HumanBodyMouseHandler.OnMouseDownAction = OnMouseDown;
		HumanBodyMouseHandler.OnMouseOverAction = OnMouseOver;
		HumanBodyMouseHandler.OnMouseExitAction = OnMouseExit;
		HumanBodyMouseHandler.OnMouseDragAction = OnMouseDrag;
		HumanBodyMouseHandler.OnMouseUpAction = OnMouseUp;

		SetUIBubbleInfo(UIBubbleInfo);

		UIBubbleInfo.Parent = this;

		UIBubbleInfo.CreateButton(new UIBubbleInfoButton {
			Text = "Destroy",
			ClickAction = (iObjectEntity) => {
				Dispose();
				RemoveEntity();
			}
		});

		mMovableEntity.SetParent(this.gameObject, this.gameObject);
		mMovableEntity.SetEntity(this);
		//mMovableEntity.OnStartMove = {
			
		//}
		StartCoroutine(PostPoppingAsync());
	}

	private void Update()
	{
		if (!mSelected)
			return;

		// Click mouse section
		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			mMouseDown = true;
			mMouseOriginClick = Input.mousePosition;
		} else if (Input.GetKeyUp(KeyCode.Mouse0))
			mMouseDown = false;

		// Left control and Icons section
		if (Input.GetKeyDown(KeyCode.LeftControl)) {
			GameManager.Instance.SetCursorRotation();
			mControlPushed = true;
			UIBubbleInfo.SetInteractable(false);
		} else if (Input.GetKeyUp(KeyCode.LeftControl)) {
			if (mMouseOverObjectEntity)
				GameManager.Instance.SetCursorHandOver();
			else
				GameManager.Instance.SetCursorStandard();
			UIBubbleInfo.SetInteractable(true);
			mControlPushed = false;
		}

		// Rotation section
		if (mControlPushed && mMouseDown) {
			transform.rotation = Quaternion.Euler(
				transform.rotation.eulerAngles.x,
				transform.rotation.eulerAngles.y + (Input.mousePosition.x - mMouseOriginClick.x),
				transform.rotation.eulerAngles.z);
			mMouseOriginClick = Input.mousePosition;
		}

		// Moving section
		if (mMouseDragObjectEntity && Input.mousePosition != mMouseOriginClick) {
			mMouseOriginClick = Input.mousePosition;

			RaycastHit lHit;
			Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
			if (Physics.Raycast(lRay, out lHit, 1000, LayerMask.GetMask("dropable"))) {
				Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);

				if (lHit.point.y > 2)
					lHit.point = new Vector3(lHit.point.x, mCenter.y, lHit.point.z);

				transform.position = lHit.point;
			}
		}
	}

	private void OnDestroy()
	{

	}

	public void Dispose()
	{
		Debug.Log("false");
		Instance = null;
		Destroy(gameObject);
	}

	private IEnumerator PostPoppingAsync()
	{
		yield return new WaitForEndOfFrame();

		while (mBusy)
			yield return null;

		foreach (Action lAction in PostPoppingAction) {
			if (lAction != null)
				lAction();
		}

		yield return new WaitForEndOfFrame();
		
		mMovableEntity.AngularSpeed = 15000F;
	}

	private HumanEntity SaveEntity()
	{
		if (mHDS != null) {
			Vector3 lPosition = new Vector3(
				transform.position.x,
				transform.position.y,
				transform.position.z
			);
			mHDS.Position = lPosition;
			mHDS.Rotation = transform.rotation.eulerAngles;
			mHDS.Scale = transform.localScale;
			mDataScene.Serialize();
		}
		return this;
	}

	public HumanEntity RemoveEntity()
	{
		if (mHDS != null && mDataScene.Human != null) {
			mDataScene.Human = null;
			mDataScene.Serialize();
		}
		return this;
	}

	public override void SetObjectDataScene(AObjectDataScene iODS)
	{
		base.SetObjectDataScene(iODS);

		mHDS = (HumanDataScene)iODS;
		if (mDataScene.Human != mHDS) {
			mDataScene.Human = mHDS;
			mDataScene.Serialize();
		}
	}

	// ------------------- MOUSE EVENTS

	private void OnMouseOver()
	{
		if (!mSelected || mBusy || mControlPushed)
			return;

		if (!mMouseOverObjectEntity && mSelected && !mMouseDragObjectEntity)
			GameManager.Instance.SetCursorHandOver();
		mMouseOverObjectEntity = true;
	}

	private void OnMouseDrag()
	{
		if (!mSelected || mBusy || mControlPushed)
			return;

		if (!mMouseDragObjectEntity) {
			Utils.SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("Ignore Raycast"));
			mMouseOriginClick = Input.mousePosition;
			GameManager.Instance.SetCursorCatchedHand();
			mMouseDragObjectEntity = true;
		}
	}

	private void OnMouseUp()
	{
		if (mMouseDragObjectEntity) {
			mMouseDragObjectEntity = false;
			if (mMouseOverObjectEntity)
				GameManager.Instance.SetCursorHandOver();
			else
				GameManager.Instance.SetCursorStandard();
			Utils.SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("dropable"));
		}

		SaveEntity();
	}

	private void OnMouseDown()
	{
		Debug.Log(EventSystem.current.IsPointerOverGameObject() + " " + mControlPushed);
		// If the click is on a GUI : 
		if (EventSystem.current.IsPointerOverGameObject() || mControlPushed)
			return;

		Debug.Log(mSelected + " " + mBusy);
		if (!mSelected || mBusy) {
			GameManager.Instance.SelectedEntity = this;
			UIBubbleInfo.Display();
		} else {
			GameManager.Instance.SetCursorCatchedHand();
		}
	}

	private void OnMouseExit()
	{
		if (mSelected && mMouseOverObjectEntity) {
			mMouseOverObjectEntity = false;
			if (!mMouseDragObjectEntity && !mControlPushed)
				GameManager.Instance.SetCursorStandard();
		}
	}

}
