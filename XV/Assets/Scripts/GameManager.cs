using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
	public const string ItemBankPath = "Prefabs/ItemBank/";
	public const string ExternItemBankPath = "SavedData/Models/";

	private static GameManager mInstance;
	private static bool mLockInstance;

	private readonly DataScene mDataScene = new DataScene();

	private ObjectEntity mSelectedEntity;

	public ObjectEntity SelectedEntity
	{
		get
		{
			return mSelectedEntity;
		}
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

	static public GameManager Instance
	{
		get
		{
			if (mInstance == null) {
				mLockInstance = true;
				mInstance = new GameObject("GameManager").AddComponent<GameManager>();
			}
			return mInstance;
		}
	}

	void Start()
	{
		if (mInstance == null) {
			mInstance = this;
		} else if (!mLockInstance) {
			Destroy(this);
			throw new Exception("An instance of this singleton already exists.");
		}
		mLockInstance = false;
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit lHit;

			Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(lRay, out lHit)) {

				if (lHit.transform == null) {
					Debug.Log("Collider hit is null");
					SelectedEntity = null;
				}
				else if (lHit.transform.tag != ObjectEntity.TAG) {
					Debug.Log(lHit.transform.tag);
					SelectedEntity = null;
				}
			} else {
				Debug.Log("Raycast hasnt hit nothing");
				SelectedEntity = null;
			}
		}
	}

	public GameObject BuildObject(ObjectDataScene iODS)
	{
		GameObject oGameObject = null;

		if (iODS.Type == ObjectDataSceneType.BUILT_IN) {
			oGameObject = Resources.Load<GameObject>(ItemBankPath + iODS.Name);
			if (oGameObject == null) {
				Debug.LogError("Load prefab " + iODS.Name + " failed.");
				return oGameObject;
			}
		} else {
			oGameObject = Resources.Load<GameObject>("SavedData/Models/" + iODS.Name);
			if (oGameObject == null) {
				Debug.LogError("Load model " + iODS.Name + " failed.");
				return oGameObject;
			}
		}

		oGameObject = Instantiate(oGameObject);

		// Getting size and center
		MeshFilter[] lElementMeshs = oGameObject.GetComponentsInChildren<MeshFilter>();
		// https://forum.unity.com/threads/getting-the-bounds-of-the-group-of-objects.70979/
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

		GameObject lUIBubbleInfo;
		if ((lUIBubbleInfo = Resources.Load<GameObject>("Prefabs/UI/UIBubbleInfo")) != null) {
			lUIBubbleInfo = Instantiate(lUIBubbleInfo, oGameObject.transform);
			lUIBubbleInfo.GetComponent<RectTransform>().position = new Vector3(lBounds.center.x, lBounds.size.y + 1, lBounds.center.z);
		}

		// Setting positions
		oGameObject.name = iODS.Name;
		oGameObject.transform.position = iODS.Position;
		oGameObject.transform.eulerAngles = iODS.Rotation;
		oGameObject.transform.localScale = iODS.Scale;

		// Setting GameEntity
		oGameObject.AddComponent<ObjectEntity>()
				   .InitDataScene(mDataScene)
				   .SetObjectDataScene(iODS)
				   .SetUIBubbleInfo(lUIBubbleInfo.GetComponent<UIBubbleInfo>())
				   .SaveEntity()
				   .SetSize(lBounds.size)
				   .SetCenter(lBounds.center);

		return oGameObject;
	}
}
