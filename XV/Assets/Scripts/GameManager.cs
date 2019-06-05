﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(Recorder))]
public class GameManager : MonoBehaviour
{
	public const string UI_MATERIAL = "Materials/UI/";
	public const string MODELS_MATERIAL = "Materials/Models/";
	public const string ITEM_BANK_PATH = "Prefabs/ItemBank/";
	public const string HUMAN_ITEM_PATH = "Prefabs/Character";
	public const string EXTERN_ITEM_BANK_PATH = "SavedData/Models/";
	public const string UI_TEMPLATE_PATH = "Prefabs/UI/";
	public const string UI_ICON_PATH = "Sprites/UI/Icons/";
	public const string UI_MODEL_SPRITE_PATH = "Sprites/UI/ModelsSprites/";

	private static GameManager sInstance;


	private DataScene mCurrentDataScene;

	private AEntity mSelectedEntity;

	public AEntity SelectedEntity
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

	public DataScene CurrentDataScene { get { return mCurrentDataScene; } }

	public Texture2D OverTexturCursor { get; private set; }

	public Texture2D CatchedTexturCursor { get; private set; }

	public Texture2D RotationTexturCursor { get; private set; }

	public Recorder Recorder { get; private set; }

	public bool KeyboardDeplacementActive { get; set; }

	static public GameManager Instance
	{
		get
		{
			if (sInstance == null) {

				GameObject lGameObject = null;
				if ((lGameObject = GameObject.Find("GameManager"))) {
					if ((sInstance = lGameObject.GetComponent<GameManager>()))
						return sInstance;
				}
				sInstance = new GameObject("GameManager").AddComponent<GameManager>();
			}
			return sInstance;
		}
	}

	void Start()
	{
		if (sInstance == null)
			sInstance = this;

		KeyboardDeplacementActive = true;

		OverTexturCursor = Resources.Load<Texture2D>("Sprites/UI/Icons/Cursor/cursor_hand");
		CatchedTexturCursor = Resources.Load<Texture2D>("Sprites/UI/Icons/Cursor/cursor_catch");
		RotationTexturCursor = Resources.Load<Texture2D>("Sprites/UI/Icons/Cursor/cursor_rotate");

		Recorder = GetComponent<Recorder>();


		// //Record on wake up
		//StartCoroutine(Utils.WaitForAsync(1F, () => {

		//	Recorder.StartRecord();
		//	StartCoroutine(Utils.WaitForAsync(5F, () => {
		//		Recorder.ReleaseRecord();
		//	}));
		//}));
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
					else if (lHit.transform.tag != ObjectEntity.TAG && lHit.transform.tag != UIBubbleInfo.TAG)
						SelectedEntity = null;
				} else
					SelectedEntity = null;
			}
		}
	}

	public GameObject BuildObject(ObjectDataScene iODS, bool iAnimatedPopping = false)
	{
		GameObject oGameObject = null;

		if (iODS.Type == ObjectDataSceneType.BUILT_IN) {
			oGameObject = ModelLoader.Instance.GetModelGameObject(iODS.PrefabName);
			if (oGameObject == null) {
				Debug.LogError("Load prefab " + iODS.PrefabName + " failed.");
				return oGameObject;
			}
		} else {
			oGameObject = ModelLoader.Instance.GetModelGameObject(iODS.PrefabName);
			if (oGameObject == null) {
				Debug.LogError("Load model " + iODS.PrefabName + " failed.");
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

		// Retrieve parameters for this item
		EntityParameters lParameters;
		lParameters = oGameObject.GetComponent<EntityParameters>();

		// ---- Parent Creation ----

		// Creating a new empty GameObject  - OffsetRotation Parent
		GameObject lOffsetRotation = new GameObject();
		lOffsetRotation.name = "ItemOffsetRotation";
		lOffsetRotation.layer = oGameObject.layer;

		// Creating a new empty GameObject - Highest Parent
		GameObject lTopParent = new GameObject();
		lTopParent.name = iODS.Name;
		lTopParent.layer = oGameObject.layer;

		// Setting positions
		lTopParent.transform.position = iODS.Position + lBounds.center;

		// Put the OffsetRotation as a child of the TopParent GameObject
		lOffsetRotation.transform.parent = lTopParent.transform;

		// Setting positions
		lOffsetRotation.transform.position = Vector3.zero;
		lOffsetRotation.transform.localPosition = Vector3.zero;


		////////////////// DEBUG PART clem
		//if (lParameters != null)
		//	lOffsetRotation.transform.rotation = Quaternion.Euler(lParameters.Orientation);
		//else
			lOffsetRotation.transform.rotation = Quaternion.Euler(Vector3.zero);
		//////////////// DEBUG PART clem


		// Put the ObjectEntity as a child of the OffsetRotation GameObject
		oGameObject.transform.parent = lOffsetRotation.transform;

		oGameObject.transform.position = iODS.Position;
		oGameObject.transform.localEulerAngles = Vector3.zero;

		oGameObject.transform.localPosition = -lBounds.center;

		oGameObject.name = iODS.PrefabName + "_mesh";
		oGameObject.transform.localScale = iODS.Scale;
		lTopParent.transform.eulerAngles = iODS.Rotation;

		// Setting GameEntity
		ObjectEntity lObjectEntity = oGameObject.AddComponent<ObjectEntity>()
				   .StartAnimation(iAnimatedPopping)
				   .SaveEntity()
				   .SetSize(lBounds.size)
				   .SetCenter(lBounds.center)
				   .SetParent(lTopParent, lOffsetRotation);

		lObjectEntity.InitDataScene(mCurrentDataScene);
		lObjectEntity.SetObjectDataScene(iODS);
		lObjectEntity.SetUIBubbleInfo(lUIBubbleInfo.GetComponent<UIBubbleInfo>());

		// If this item can move - Add MovableEntity script
		if (lParameters != null && lParameters.Movable) {
			oGameObject.AddComponent<MovableEntity>()
					   .SetParent(lTopParent, lOffsetRotation)
					   .SetEntity(lObjectEntity);
		}

		Utils.SetLayerRecursively(lTopParent, LayerMask.NameToLayer("dropable"));

        //// Check if this item have Interaction, and init it's field.
        //AInteraction lInteraction = oGameObject.GetComponent<AInteraction>();
        //if (lInteraction != null) {
        //    lInteraction.SetEntityParameters(lParameters)
        //                .SetEntity(lObjectEntity);
        //}

		return oGameObject;
	}

	public void DebugButton()
	{
		Recorder.StartRecord();
		StartCoroutine(Utils.WaitForAsync(5F, () => {
			Recorder.ReleaseRecord();
		}));
	}

	public GameObject BuildHuman(HumanDataScene iHDS)
	{
		GameObject oGameObject;

		oGameObject = ModelLoader.Instance.GetModelGameObject(iHDS.PrefabName);
		if (oGameObject == null) {
			Debug.LogError("Load prefab " + iHDS.PrefabName + " failed.");
			return oGameObject;
		}

		oGameObject = Instantiate(oGameObject);
		oGameObject.transform.position = iHDS.Position;
		oGameObject.transform.eulerAngles = iHDS.Rotation;

		HumanEntity lHumanEntity = oGameObject.GetComponent<HumanEntity>();
		lHumanEntity.enabled = true;

		lHumanEntity.InitDataScene(CurrentDataScene);
		lHumanEntity.SetObjectDataScene(iHDS);

		return oGameObject;
	}

	public IEnumerator UnloadSceneAsync()
	{
		AEntity[] lObjectEntities = AEntity.AllEntities;

		foreach (AEntity lObjectEntity in lObjectEntities) {

			lObjectEntity.Dispose();
		}

		while (AEntity.AllEntities.Length != 0)
			yield return null;
		
		mCurrentDataScene = null;
		XV_UI.Instance.SceneNameText.text = "-";
	}

    public void UnloadScene()
    {

    }

	public void LoadScene(DataScene iDataScene)
	{
		StartCoroutine(LoadSceneAsync(iDataScene));
	}

	private IEnumerator LoadSceneAsync(DataScene iDataScene)
	{
		yield return UnloadSceneAsync();
		mCurrentDataScene = iDataScene;
		XV_UI.Instance.SceneNameText.text = "Scene: " + iDataScene.SceneName.Replace(".xml", "");

		while (ObjectEntity.InstantiatedEntity != 0) {
			yield return new WaitForSeconds(0.1F);
		}

		foreach (ObjectDataScene lODS in iDataScene.DataObjects) {
			BuildObject(lODS, true);
		}
		if (iDataScene.Human != null)
			BuildHuman(iDataScene.Human);
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
