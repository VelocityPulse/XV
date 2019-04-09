using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	private readonly DataScene mDataScene;

	public ObjectEntity SelectedEntity { get; set; }

	GameManager()
	{
		mDataScene = new DataScene();
	}

	void Start()
	{
	}

	void Update()
	{
		if (Input.GetMouseButtonDown(0)) {
			RaycastHit hit;
			Ray ray = Camera.current.ScreenPointToRay(Input.mousePosition);

			if (Physics.Raycast(ray, out hit)) {

				if (hit.collider == null)
					Debug.Log("collider hit is null");
				else
					Debug.Log("hit : " + hit.collider.gameObject.name);
			} else
				Debug.Log("Raycast hasnt hit anything");
			//SelectedEntity.HideUi();
			//SelectedEntity = null;

		}
	}

	//TODO : 
	// rendre gamemanager singleton et faire un pull request vers antho
	// S'occuper du selectedEntity. 
	// sur un mouse click dans object entity, le signaler a gamemanager

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

	/// <summary>
	/// ////////////////////////////// DEBUG 
	/// </summary>
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

	//////////////////////////////////// DEBUG

}
