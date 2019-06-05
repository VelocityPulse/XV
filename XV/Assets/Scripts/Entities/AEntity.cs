﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class AEntity : MonoBehaviour
{

	public abstract bool Selected { get; set; }

	public abstract string Name { get; set; }

	public List<Action> PostPoppingAction { get; private set; }

	public virtual Vector3 Size { get { return mSize; } }

	public static List<AEntity> sAllEntites;

	public static AEntity[] AllEntities
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

	protected UIBubbleInfo mUIBubbleInfo;

	protected EntityParameters mEntityParameters;

	protected DataScene mDataScene;

	protected Vector3 mSize;

	protected bool mBusy;

	private AObjectDataScene mODS;

	public abstract void Dispose();

	protected virtual void Awake()
	{
		PostPoppingAction = new List<Action>();
		mEntityParameters = GetComponent<EntityParameters>();

		if (mEntityParameters != null && GetComponent<AInteraction>() == null)
			gameObject.AddComponent<GenericInteraction>();
	}

	public AEntity InitDataScene(DataScene iDataScene)
	{
		mDataScene = iDataScene;
		return this;
	}

	public virtual void SetObjectDataScene(AObjectDataScene iODS)
	{
		mODS = iODS;
	}

	protected virtual void Start()
	{
		// Adding this to all ObjectEntities
		if (sAllEntites == null)
			sAllEntites = new List<AEntity>();
		sAllEntites.Add(this);
	}

	protected virtual void OnDestroy()
	{
		if (sAllEntites != null)
			sAllEntites.Remove(this);
	}

	public AEntity SetUIBubbleInfo(UIBubbleInfo iBubbleInfo)
	{
		mUIBubbleInfo = iBubbleInfo;
		mUIBubbleInfo.Parent = this;
		if (mODS != null)
			mUIBubbleInfo.SetUIName(mODS.Name);
		else
			Debug.LogError("[AENTITY] mODS is null when setting UIBubbleInfo");
		mUIBubbleInfo.RefreshCanvas();
		return this;
	}

	public Button CreateBubbleInfoButton(UIBubbleInfoButton iButtonInfo)
	{
		if (mUIBubbleInfo == null) {
			Debug.LogError("[AENTITY] mUIBubbleInfo is null when create button");
			return null;
		}
		return mUIBubbleInfo.CreateButton(iButtonInfo);
	}

	public void DestroyBubbleInfoButton(UIBubbleInfoButton iButtonInfo)
	{
		if (mUIBubbleInfo != null)
			mUIBubbleInfo.DestroyButton(iButtonInfo.Tag);
	}

	public void DestroyBubbleInfoButton(string iTag)
	{
		if (mUIBubbleInfo != null)
			mUIBubbleInfo.DestroyButton(iTag);
	}

	public static void ForEachEntities(Action<AEntity> iAction)
	{
		if (iAction == null)
			return;

		AEntity[] lEntities = AllEntities;
		foreach (AEntity lEntity in lEntities)
			iAction(lEntity);
	}

	public static void HideNoInteractable(EntityParameters.EntityType[] iTypes,
		AEntity iIgnored = null)
	{
		ForEachEntities((iEntity) => {
			if (iEntity == iIgnored)
				return;
			if (iEntity.mEntityParameters != null) {
				foreach (EntityParameters.EntityType lType in iTypes) {
					if (iEntity.mEntityParameters.Type == lType) {
						iEntity.gameObject.SetActive(true);
						return;
					}
				}
			}
			iEntity.gameObject.SetActive(false);
		});
	}

	public static void DisableHideNoInteractable()
	{
		ForEachEntities((iEntity) => {
			iEntity.gameObject.SetActive(true);
		});
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
			Renderer lR = iObject.GetComponent<Renderer>();
			if (lR != null) {
				Material[] lO = new Material[lR.materials.Length];
				for (int lIndex = 0; lIndex < lO.Length; lIndex++) {
					lO.SetValue(lGhostMaterial, lIndex);
				}
				lR.materials = lO;
			}

			MonoBehaviour[] lMBs = iObject.GetComponents<MonoBehaviour>();
			foreach (MonoBehaviour lMB in lMBs)
				lMB.enabled = false;
		});

		if (oGhostObject == null)
			Debug.LogError("[AENTITY] oGhostObject is null");

		return oGhostObject;
	}

	public virtual void ResetWorldState()
	{
		transform.position = mODS.Position;
		transform.transform.eulerAngles = mODS.Rotation;
	}

}