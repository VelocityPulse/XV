using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour {

	private readonly DataScene mDataScene;

	public GameObject mTestGameObject;

	GameManager()
	{
		mDataScene = new DataScene();
	}

	// Use this for initialization
	void Start()
	{
		ObjectDataScene lMyObject = new ObjectDataScene();

		lMyObject.Name = mTestGameObject.name;
		lMyObject.Position = mTestGameObject.transform.position;
		lMyObject.Rotation = mTestGameObject.transform.eulerAngles;
		lMyObject.Scale = mTestGameObject.transform.localScale;

		mDataScene.DataObjects.Add(lMyObject);
		DataScene.Serialize(mDataScene);
	}

	// Update is called once per frame
	void Update()
	{

	}

	//TODO : 

	//Coder la fonction import qui prend un path.
	//ObjectEntity
	//Ajouter le addComponent de ObjectEntity dans BuildObject
	//Ajouter le mDataScene.DataObjects.Add() dans le init de ObjectEntity

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

		// Todo : ajouter le script ObjectEntity
		//lGameObject.AddComponent("ObjectEntity");

		//lGameObject.AddComponent<DataScene>();

		//lGameObject.AddComponent<Collider>();

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
