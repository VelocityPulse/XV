using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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

	public static int InstantiatedEntity { get { return AllEntities.Length; } }

	private DataScene mDataScene;
	private ObjectDataScene mODS;
	private bool mBusy;
	private bool mSelected;
	private bool mControlPushed;
	private bool mMouseDown;

	private bool mMouseOverObjectEntity;
	private bool mMouseDragObjectEntity;

	private Vector3 mMouseOriginClick;

	private UIBubbleInfo mUIBubbleInfo;
	private Vector3 mCenter;
	private Vector3 mSize;
	private GameObject mCenteredParent;
	private GameObject mOffsetRotationParent;

	public bool IsBusy { get { return mBusy; } }

	public Vector3 Size
	{
		get { return mSize; }
	}

	public Vector3 Center
	{
		get { return mCenter; }
	}

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

	public string Name
	{
		get { return mCenteredParent.name; }

		set
		{
			if (string.IsNullOrEmpty(value))
				return;

			mCenteredParent.name = value;
			name = value + "_mesh";
			mODS.Name = value;
			SaveEntity();
		}
	}

	private List<Action> mPostPoppingAction = new List<Action>();

	public List<Action> PostPoppingAction { get { return mPostPoppingAction; } }

	void Start()
	{
		// Adding this to all ObjectEntities
		if (sAllEntites == null)
			sAllEntites = new List<ObjectEntity>();
		sAllEntites.Add(this);

		// Set tag
		gameObject.tag = TAG;

		mCenteredParent = transform.parent.gameObject.transform.parent.gameObject;

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

		// Add a Nav Mesh obstacle on each object
		NavMeshObstacle lObstacle;
		if ((lObstacle = transform.gameObject.AddComponent<NavMeshObstacle>()) != null) {
			lObstacle.center = mCenter;
			lObstacle.size = mSize;
			lObstacle.carving = true;
			float limit = 0F;
			if (lObstacle.size.y < 0.2F) {
				lObstacle.size = new Vector3(lObstacle.size.x, lObstacle.size.y + limit, lObstacle.size.z);
				lObstacle.center = new Vector3(lObstacle.center.x, lObstacle.center.y + (limit / 2), lObstacle.center.z);
			}
		}

		mUIBubbleInfo.SetInteractable(false);

		foreach (Action lAction in PostPoppingAction) {
			if (lAction != null)
				lAction();
		}
	}

	// This function Instantiate associated Model & make it child of OffsetRotation
	// Then all material are replace by GhostMaterial
	public GameObject CreateGhostObject()
	{
		GameObject lGameObject = null;
		GameObject oGhostObject = null;
		Material lGhostMaterial = null;

		if ((lGhostMaterial = Resources.Load<Material>(GameManager.UI_MATERIAL + "Ghost")) == null) {
			Debug.LogError("Load material : 'Ghost' failed.");
			return null;
		}

		if (mODS.Type == ObjectDataSceneType.BUILT_IN) {
			lGameObject = ModelLoader.Instance.GetModelGameObject(mODS.PrefabName);
			if (lGameObject == null) {
				Debug.LogError("Load prefab " + mODS.PrefabName + " failed.");
				return null;
			}
		} else {
			lGameObject = ModelLoader.Instance.GetModelGameObject(mODS.PrefabName);
			if (lGameObject == null) {
				Debug.LogError("Load model " + mODS.PrefabName + " failed.");
				return null;
			}
		}

		oGhostObject = Instantiate(lGameObject, transform.position, transform.rotation, transform.parent);

		Utils.BrowseChildRecursively(oGhostObject, (iObject) => {

			// Perform this action on each child of iObject, which is oGhostObject
			Renderer r = iObject.GetComponent<Renderer>();
			if (r != null) {
				Material[] o = new Material[r.materials.Length];
				for (int i = 0; i < o.Length; i++) {
					o.SetValue(lGhostMaterial, i);
				}
				r.materials = o;
			}
		});

		return oGhostObject;
	}

	public Button CreateBubleInfoButton(UIBubbleInfoButton iButtonInfo)
	{
		if (iButtonInfo == null)
			return null;
		return mUIBubbleInfo.CreateButton(iButtonInfo);
	}

	// Called by unity only !
	public void OnDestroy()
	{
		if (sAllEntites != null)
			sAllEntites.Remove(this);
		Destroy(mCenteredParent);
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

		// Detach from parent & delete parent
		//if (transform.parent != null) {
		//    transform.parent = null;
		//    Destroy(mCenteredParent);
		//}

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
		if (mUIBubbleInfo != null)
			mUIBubbleInfo.RefreshCanvas();
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
		mUIBubbleInfo.SetUIName(mODS.Name);
		mUIBubbleInfo.RefreshCanvas();
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

	public ObjectEntity SetParent(GameObject iTopParent, GameObject iOffsetRotationParent)
	{
		mCenteredParent = iTopParent;
		mCenteredParent.tag = TAG;

		mOffsetRotationParent = iOffsetRotationParent;
		mOffsetRotationParent.tag = TAG;
		return this;
	}

	public ObjectEntity SaveEntity()
	{
		if (mODS != null && mCenteredParent != null) {
			Vector3 lPosition = new Vector3(
				mCenteredParent.transform.position.x - mCenter.x,
				transform.position.y,
				mCenteredParent.transform.position.z - mCenter.z
			);
			mODS.Position = lPosition;
			mODS.Rotation = mCenteredParent.transform.rotation.eulerAngles;
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
				Debug.LogWarning("ODS not contained in DO");
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
		// If the click is on a GUI : 
		if (EventSystem.current.IsPointerOverGameObject() || mControlPushed)
			return;

		if (!mSelected || mBusy) {
			GameManager.Instance.SelectedEntity = this;
			mUIBubbleInfo.Display();
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

	// ------------------- MOUSE EVENTS
}