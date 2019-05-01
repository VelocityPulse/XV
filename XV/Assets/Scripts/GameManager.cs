using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public const string ITEM_BANK_PATH = "Prefabs/ItemBank/";
	public const string EXTERN_ITEM_BANK_PATH = "SavedData/Models/";
	public const string UI_TEMPLATE_PATH = "Prefabs/UI/";
	public const string UI_ICON_PATH = "Sprites/UI/Icons/";
	public const string UI_MODEL_SPRITE_PATH = "Sprites/UI/ModelsSprites/";

	private static GameManager sInstance;
	private static bool sLockInstance;

	private readonly DataScene mDataScene = new DataScene();

	private ObjectEntity mSelectedEntity;

	public ObjectEntity SelectedEntity
	{
		get { return mSelectedEntity; }

		set
		{
			if (value == null && mSelectedEntity == null)
				return;

			if (value == null && mSelectedEntity != null) {
				mSelectedEntity.Selected = false;
				mSelectedEntity = null;
				return;
			}

			if (mSelectedEntity != null)
				mSelectedEntity.Selected = false;
			mSelectedEntity = value;
			mSelectedEntity.Selected = true;
		}
	}

	public Texture2D OverTexturCursor { get; private set; }

	public Texture2D CatchedTexturCursor { get; private set; }

	public Texture2D RotationTexturCursor { get; private set; }

	public bool KeyboardDeplacementActive { get; set; }

	static public GameManager Instance
	{
		get
		{
			if (sInstance == null) {
				sLockInstance = true;
				sInstance = new GameObject("GameManager").AddComponent<GameManager>();
			}
			return sInstance;
		}
	}

	void Start()
	{
		if (sInstance == null)
			sInstance = this;
		else if (!sLockInstance) {
			Destroy(this);
			throw new Exception("An instance of this singleton already exists.");
		}
		sLockInstance = false;

		OverTexturCursor = Resources.Load<Texture2D>("Sprites/UI/Icons/Cursor/cursor_hand");
		CatchedTexturCursor = Resources.Load<Texture2D>("Sprites/UI/Icons/Cursor/cursor_catch");
		RotationTexturCursor = Resources.Load<Texture2D>("Sprites/UI/Icons/Cursor/cursor_rotate");
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0) && !Input.GetKey(KeyCode.LeftControl)) {
			RaycastHit lHit;

			// If the click is on a GUI : 
			if (!EventSystem.current.IsPointerOverGameObject(-1)) {

				// If the click is on anything else
				Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);

				if (Physics.Raycast(lRay, out lHit)) {
					if (lHit.transform == null)
						SelectedEntity = null;
					else if (lHit.transform.tag != ObjectEntity.TAG && lHit.transform.tag != UIBubbleInfo.TAG) {
						Debug.Log(lHit.transform.tag);
						SelectedEntity = null;
					}
				} else
					SelectedEntity = null;
			}
		}
	}

	public GameObject BuildObject(ObjectDataScene iODS, bool iAnimatedPopping = false)
	{
		GameObject oGameObject = null;

		if (iODS.Type == ObjectDataSceneType.BUILT_IN) {
			oGameObject = ModelLoader.Instance.GetModelGameObject(iODS.Name);
			if (oGameObject == null) {
				Debug.LogError("Load prefab " + iODS.Name + " failed.");
				return oGameObject;
			}
		} else {
			oGameObject = ModelLoader.Instance.GetModelGameObject(iODS.Name);
			if (oGameObject == null) {
				Debug.LogError("Load model " + iODS.Name + " failed.");
				return oGameObject;
			}
		}

		// Instantiation
		oGameObject = Instantiate(oGameObject);
		if (iAnimatedPopping)
			oGameObject.SetActive(false);

		// Getting size and center
		MeshFilter[] lElementMeshs = oGameObject.GetComponentsInChildren<MeshFilter>();
		Bounds lBounds = new Bounds(Vector3.zero, Vector3.zero);
		foreach (MeshFilter lMesh in lElementMeshs) {

			// Set tag on all mesh GameObject
			lMesh.gameObject.tag = ObjectEntity.TAG;

			// Bound mesh
			lBounds.Encapsulate(lMesh.sharedMesh.bounds);

			// Add mesh collider
			if (lMesh.gameObject.GetComponent<MeshCollider>() == null)
				lMesh.gameObject.AddComponent<MeshCollider>().sharedMesh = lMesh.sharedMesh;
		}

		// Add UI Bubble 
		GameObject lUIBubbleInfo;
		if ((lUIBubbleInfo = Resources.Load<GameObject>("Prefabs/UI/UIBubbleInfo")) != null) {
			lUIBubbleInfo = Instantiate(lUIBubbleInfo, oGameObject.transform);
			// The set position is make in ObjectEntity after the hierachy rework Start();
		}

		// Setting positions
		oGameObject.name = iODS.Name;
		oGameObject.transform.position = iODS.Position;
		oGameObject.transform.eulerAngles = iODS.Rotation;
		oGameObject.transform.localScale = iODS.Scale;

		// Setting GameEntity
		oGameObject.AddComponent<ObjectEntity>()
				   .InitDataScene(mDataScene)
				   .StartAnimation(iAnimatedPopping)
				   .SetObjectDataScene(iODS)
				   .SetUIBubbleInfo(lUIBubbleInfo.GetComponent<UIBubbleInfo>())
				   .SaveEntity()
				   .SetSize(lBounds.size)
				   .SetCenter(lBounds.center);

		return oGameObject;
	}

	public void LoadSceneDebug()
	{
		LoadScene(DataScene.Unserialize());
	}

	public void LoadScene(DataScene iDataScene)
	{
		StartCoroutine(LoadSceneAsync(iDataScene));
	}

	private IEnumerator LoadSceneAsync(DataScene iDataScene)
	{
		ObjectEntity[] lObjectEntities = ObjectEntity.AllEntities;

		foreach (ObjectEntity lObjectEntity in lObjectEntities) {
			lObjectEntity.Dispose();
		}

		while (ObjectEntity.InstantiatedEntity != 0) {
			yield return new WaitForSeconds(0.1F);
		}

		foreach (ObjectDataScene lODS in iDataScene.DataObjects) {
			BuildObject(lODS, true);
		}
	}

	public void SetCursorHandOver()
	{
		Cursor.SetCursor(OverTexturCursor, Vector2.zero, CursorMode.Auto);
	}

	public void SetCursorStandard()
	{
		Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}

	public void SetCursorCatchedHand()
	{
		Cursor.SetCursor(CatchedTexturCursor, Vector2.zero, CursorMode.Auto);
	}

	public void SetCursorRotation()
	{
		Cursor.SetCursor(RotationTexturCursor, Vector2.zero, CursorMode.Auto);
	}
}
