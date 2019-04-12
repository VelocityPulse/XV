﻿using System.Collections;
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

	private DataScene mDataScene;
	private ObjectDataScene mODS;
	private bool mSelected;

	private UIBubbleInfo mUIBubbleInfo;
	private Vector3 mCenter;
	private Vector3 mSize;

	public bool Selected
	{
		get
		{
			return mSelected;
		}
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
		if (sAllEntites == null)
			sAllEntites = new List<ObjectEntity>();
		sAllEntites.Add(this);

		Debug.Log("Start ObjectEntity");
		gameObject.tag = TAG;
		Transform[] lTransforms = GetComponentsInChildren<Transform>();

		foreach (Transform childObject in lTransforms) {
			MeshFilter meshFilter = childObject.gameObject.GetComponent<MeshFilter>();

			if (meshFilter != null) {

				ColliderMouseHandler lCMH = childObject.gameObject.AddComponent<ColliderMouseHandler>();

				lCMH.OnMouseUpAction = () => { };
				lCMH.OnMouseExitAction = () => { };
				lCMH.OnMouseEnterAction = () => { };
				lCMH.OnMouseDownAction = () => { OnMouseDown(); };
			}
		}
	}

	void Update()
	{

	}

	public void OnDestroy()
	{
		Debug.Log("Destroying : " + gameObject.name);
		sAllEntites.Remove(this);
	}

	private void OnMouseDown()
	{
		if (!Selected) {
			GameManager.Instance.SelectedEntity = this;
			Debug.Log("ObjectEntity : " + mODS.Name + " has been selected");
			mUIBubbleInfo.Display();
		}
	}

	public ObjectEntity InitDataScene(DataScene iDataScene)
	{
		mDataScene = iDataScene;
		return this;
	}

	public ObjectEntity SetObjectDataScene(ObjectDataScene iODS)
	{
		mODS = iODS;
		if (!mDataScene.DataObjects.Contains(mODS))
			mDataScene.DataObjects.Add(mODS);
		return this;
	}

	public ObjectEntity SetUIBubbleInfo(UIBubbleInfo iBubbleInfo)
	{
		mUIBubbleInfo = iBubbleInfo;
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
}