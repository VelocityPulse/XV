using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectEntity : MonoBehaviour
{
	public static string TAG = "ObjectEntity";
	public static List<ObjectEntity> sAllEntites;

	public static ObjectEntity[] AllEntities
	{
		get
		{
			if (sAllEntites != null)
				return sAllEntites.ToArray();
			else
				return new ObjectEntity[0];
		}
	}

	public static int InstantiatedEntity { get { return sAllEntites.Count; } }

	private DataScene mDataScene;
	private ObjectDataScene mODS;
	private bool mBusy;
	private bool mSelected;
	private bool mControlPushed;
	private bool mMouseDown;

	private bool mMouseOverObjectEntity;
	private bool mMouseDownOnObjectEntity;
	private bool mMouseDragObjectEntity;

	private Vector3 mMouseOriginClick;

	private UIBubbleInfo mUIBubbleInfo;
	private Vector3 mCenter;
	private Vector3 mSize;
	private GameObject mCenteredParent;

	public bool Selected
	{
		get { return mSelected; }

		set
		{
			if (!value) {
				Debug.Log("ObjectEntity : " + mODS.Name + "Has been unselected");
				mUIBubbleInfo.Hide();
			}
			mSelected = value;
		}
	}

	void Start()
	{
        // Adding this to all ObjectEntities
		if (sAllEntites == null)
			sAllEntites = new List<ObjectEntity>();
		sAllEntites.Add(this);

        // Set tag
		gameObject.tag = TAG;

        // --------------------- CENTERING OBJECT ENTITY
        // Creating a new empty GameObject
        mCenteredParent = new GameObject();

        // Setting the future parent position to the center bound of the ObjectEntity
        mCenteredParent.transform.position = transform.position + mCenter;
        // Put the ObjectEntity as a child of the new parent GameObject
		transform.parent = mCenteredParent.transform;

        // Setting the offset placement of the ObjectEntity regarding the new parent.
        transform.position = Vector3.zero;
        transform.localPosition = -mCenter;

        // Setting new names
        mCenteredParent.name = name;
        name = name + "_mesh";
        // ---------------------- CENTERING OBJECT ENTITY


        mUIBubbleInfo.GetComponent<RectTransform>().localPosition = new Vector3(mCenter.x, mSize.y + 1, mCenter.z);

        StartCoroutine(PostPoppingAsync());
	}

	void Update()
	{
		if (!mSelected)
			return;

		if (Input.GetKeyDown(KeyCode.Mouse0)) {
			mMouseDown = true;
			mMouseOriginClick = Input.mousePosition;
		} else if (Input.GetKeyUp(KeyCode.Mouse0))
			mMouseDown = false;

		if (Input.GetKeyDown(KeyCode.LeftControl)) {
			GameManager.Instance.SetCursorRotation();
			mControlPushed = true;
			mUIBubbleInfo.SetInteractable(false);
		} else if (Input.GetKeyUp(KeyCode.LeftControl)) {
			if (mMouseOverObjectEntity)
				GameManager.Instance.SetCursorHandOver();
			else
				GameManager.Instance.SetCursorStandard();
			mUIBubbleInfo.SetInteractable(true);
			mControlPushed = false;

		}

        // Rotation section
		if (mControlPushed && mMouseDown) {
			mCenteredParent.transform.rotation = Quaternion.Euler(
				mCenteredParent.transform.rotation.eulerAngles.x,
				mCenteredParent.transform.rotation.eulerAngles.y + (Input.mousePosition.x - mMouseOriginClick.x),
				mCenteredParent.transform.rotation.eulerAngles.z);

			mMouseOriginClick = Input.mousePosition;
		}

        // Moving section
		if (mMouseDragObjectEntity && Input.mousePosition != mMouseOriginClick) {
			mMouseOriginClick = Input.mousePosition;

			RaycastHit lHit;
			Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(lRay, out lHit, 1000, LayerMask.GetMask("dropable"))) {
				Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);

                lHit.point = new Vector3(lHit.point.x, mCenter.y, lHit.point.z);
				mCenteredParent.transform.position = lHit.point;
			}
		}
	}

    // Place all the code you want to execute only after all the mesh enable animations
	private IEnumerator PostPoppingAsync()
	{
		// Waiting the end of the GameManager initialization of this class
		yield return new WaitForEndOfFrame();

		while (mBusy)
			yield return null;

		Transform[] lTransforms = GetComponentsInChildren<Transform>();

		foreach (Transform childObject in lTransforms) {
			MeshFilter meshFilter = childObject.gameObject.GetComponent<MeshFilter>();

			if (meshFilter != null) {
				MouseHandler lCMH = childObject.gameObject.AddComponent<MouseHandler>();
				lCMH.OnMouseDownAction = OnMouseDown;
				lCMH.OnMouseOverAction = OnMouseOver;
				lCMH.OnMouseExitAction = OnMouseExit;
				lCMH.OnMouseDragAction = OnMouseDrag;
				lCMH.OnMouseUpAction = OnMouseUp;
			}
		}

		mUIBubbleInfo.CreateButton(new UIBubbleInfoButton {
			Text = "Destroy",
			ClickAction = (iObjectEntity) => {
				Dispose();
				RemoveEntity();
			}
		});

		mUIBubbleInfo.CreateButton(new UIBubbleInfoButton {
			Text = "bubble test",
			ClickAction = (iObjectEntity) => {
				Debug.LogWarning("Test button of " + iObjectEntity.name + " has been clicked");
			}
		});

		mUIBubbleInfo.CreateButton(new UIBubbleInfoButton {
			Text = "bubble test",
			ClickAction = (iObjectEntity) => {
				Debug.LogWarning("Test button of " + iObjectEntity.name + " has been clicked");
			}
		});
	}

	// Called by unity only !
	public void OnDestroy()
	{
		sAllEntites.Remove(this);
	}

	// Called by XV
	public void Dispose()
	{
		if (!mBusy)
			StartCoroutine(DestroyObjectsTimedAsync());
	}

	private IEnumerator DestroyObjectsTimedAsync()
	{
		mBusy = true;

		Transform[] lTransforms = gameObject.GetComponentsInChildren<Transform>();
		Array.Reverse(lTransforms);

		if (lTransforms.Length > 0) {
			float lWaiting = 0.05F / lTransforms.Length;

			foreach (Transform lTransform in lTransforms) {
				if (lTransform.gameObject.tag == TAG) {
					Destroy(lTransform.gameObject);
					yield return new WaitForSeconds(lWaiting);
				}
			}
		}
		mBusy = false;
	}

	public ObjectEntity InitDataScene(DataScene iDataScene)
	{
		mDataScene = iDataScene;
		return this;
	}

	public ObjectEntity StartAnimation(bool iAnimatedPopping)
	{
		if (iAnimatedPopping && !mBusy) {

			Transform[] lTransforms = gameObject.GetComponentsInChildren<Transform>();
			Array.Reverse(lTransforms);

			if (lTransforms.Length > 0) {
				float lWaiting = 0.05F / lTransforms.Length;

				foreach (Transform lTransform in lTransforms) {
					if (lTransform.gameObject.tag == TAG) {
						lTransform.gameObject.SetActive(false);
					}
				}
			}

			gameObject.SetActive(true);
			StartCoroutine(PoppingObject(lTransforms));
		}
		return this;
	}

	private IEnumerator PoppingObject(Transform[] iTransforms)
	{
		mBusy = true;

		if (iTransforms.Length > 0) {
			float lWaiting = 0.05F / iTransforms.Length;

			foreach (Transform lTransform in iTransforms) {
				if (lTransform.gameObject.tag == TAG) {
					lTransform.gameObject.SetActive(true);
					yield return new WaitForSeconds(lWaiting);
				}
			}
		}
		mBusy = false;
	}

	public ObjectEntity SetObjectDataScene(ObjectDataScene iODS)
	{
		mODS = iODS;
		if (!mDataScene.IsDataObjectsContains(mODS)) {
			mDataScene.AddODS(mODS);
			mDataScene.Serialize();
		}
		return this;
	}

	public ObjectEntity SetUIBubbleInfo(UIBubbleInfo iBubbleInfo)
	{
		mUIBubbleInfo = iBubbleInfo;
		mUIBubbleInfo.Parent = this;
		return this;
	}

	public ObjectEntity SetCenter(Vector3 iVector)
	{
		mCenter = iVector;
		return this;
	}

	public ObjectEntity SetSize(Vector3 iVector)
	{
		mSize = iVector;
		return this;
	}

	public ObjectEntity SaveEntity()
	{
		if (mODS != null) {
			mODS.Position = transform.position;
			mODS.Rotation = transform.rotation.eulerAngles;
			mODS.Scale = transform.localScale;

			mDataScene.Serialize();
		}
		return this;
	}

	public ObjectEntity RemoveEntity()
	{
		if (mODS != null) {
			if (mDataScene.IsDataObjectsContains(mODS)) {
				Debug.Log("Removing ODS : " + mDataScene.RemoveODS(mODS));
				mDataScene.Serialize();
			} else {
				Debug.Log("ODS not contained in DO");
			}
		}
		return this;
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
		mMouseDownOnObjectEntity = false;

		if (mMouseDragObjectEntity) {
			mMouseDragObjectEntity = false;
			if (mMouseOverObjectEntity)
				GameManager.Instance.SetCursorHandOver();
			else {
				GameManager.Instance.SetCursorStandard();
			}
			Utils.SetLayerRecursively(this.gameObject, LayerMask.NameToLayer("dropable"));
		}

		SaveEntity();
	}

	private void OnMouseDown()
	{
		// If the click is on a GUI : 
		if (EventSystem.current.IsPointerOverGameObject())
			return;

		if (!mSelected || mBusy) {
			GameManager.Instance.SelectedEntity = this;
			mUIBubbleInfo.Display();
		} else {
			mMouseDownOnObjectEntity = true;
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

	// ------------------- MOUSE EVENTS
}