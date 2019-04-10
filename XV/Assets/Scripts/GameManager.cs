using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{

	private static GameManager mInstance;

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
			Debug.Log("singleton");
			if (mInstance == null)
				mInstance = new GameObject("GameManager").AddComponent<GameManager>();
			return mInstance;
		}
	}

	void Start()
	{
		if (mInstance == null) {
			mInstance = this;
		} else {
			Destroy(this);
			throw new System.Exception("An instance of this singleton already exists.");
		}
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;

			Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {

				if (hit.collider == null) {
					Debug.Log("Collider hit is null");
					SelectedEntity = null;
				} else
					Debug.Log("Hit : " + hit.collider.gameObject.name);
			} else {
				Debug.Log("Raycast hasnt hit nothing");
				SelectedEntity = null;
			}
		}
	}

	GameObject BuildObject(ObjectDataScene iODS)
	{
		GameObject lGameObject = null;

		if (iODS.Type == ObjectDataSceneType.BUILT_IN) {
			lGameObject = Resources.Load<GameObject>("Prefabs/" + iODS.Name);
			if (lGameObject == null) {
				Debug.LogError("Load prefab " + iODS.Name + " failed.");
				return lGameObject;
			}
		} else {
			lGameObject = Resources.Load<GameObject>("SavedData/Models/" + iODS.Name);
			if (lGameObject == null) {
				Debug.LogError("Load model " + iODS.Name + " failed.");
				return lGameObject;
			}
		}

		lGameObject = Instantiate(lGameObject);
		lGameObject.name = iODS.Name + mInvokedBox;
		lGameObject.transform.position = iODS.Position;
		lGameObject.transform.eulerAngles = iODS.Rotation;
		lGameObject.transform.localScale = iODS.Scale;

		lGameObject.AddComponent<ObjectEntity>()
				   .InitDataScene(mDataScene)
				   .SetObjectDataScene(iODS)
				   .SaveEntity();

		return lGameObject;
	}








	private int mInvokedBox = 0;
	private int mInvokedModel = 0;
	public void InvokeBox()
	{
		ObjectDataScene lObject = new ObjectDataScene {
			Name = "ChariotPlateForme",
			Type = ObjectDataSceneType.BUILT_IN,
			Position = new Vector3(3 + mInvokedBox * 1.5F, 0, 0),
			Rotation = Vector3.zero,
			Scale = Vector3.one
		};

		GameObject lRet = BuildObject(lObject);

		if (lRet != null)
			mInvokedBox++;
	}

	public void InvokeBoxModel()
	{

		ObjectDataScene lObject = new ObjectDataScene {
			Name = "TableEmballage",
			Type = ObjectDataSceneType.EXTERN,
			Position = new Vector3(3 + mInvokedModel * 1.5F, 0, 0),
			Rotation = Vector3.zero,
			Scale = Vector3.one
		};

		GameObject lRet = BuildObject(lObject);

		if (lRet != null)
			mInvokedModel++;
	}
}
