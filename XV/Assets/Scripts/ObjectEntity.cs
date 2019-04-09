﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectEntity : MonoBehaviour {

	private DataScene mDataScene;
	private ObjectDataScene mODS;

	public bool Selected { get; set; }

	void Start()
	{
		Debug.Log("Start ObjectEntity");


		Transform[] lTransforms = GetComponentsInChildren<Transform>();

		foreach (Transform childObject in lTransforms) {
			MeshFilter meshFilter = childObject.gameObject.GetComponent<MeshFilter>();

			if (meshFilter != null) {

				ColliderMouseHandler lCMH = childObject.gameObject.AddComponent<ColliderMouseHandler>();

				lCMH.OnMouseUpAction = () => { Debug.Log("up"); };
				lCMH.OnMouseExitAction = () => { Debug.Log("exit"); };
				lCMH.OnMouseEnterAction = () => { Debug.Log("enter"); };
				lCMH.OnMouseDownAction = () => { OnMouseDown(); };
			}
		}
	}

	void Update()
	{

	}

	private void OnMouseDown()
	{
		//GameManager.Instance.SelectedEntity = this;
		//DisplayUi();
	}

	public ObjectEntity InitDataScene(DataScene iDataScene)
	{
		mDataScene = iDataScene;
		return this;
	}

	public ObjectEntity SetObjectDataScene(ObjectDataScene iOBS)
	{
		mODS = iOBS;
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



}
