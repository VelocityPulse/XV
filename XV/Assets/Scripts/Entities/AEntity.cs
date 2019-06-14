using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
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

	public EntityParameters EntityParameters { get { return mEntityParameters; } }

	public bool NavMeshObjstacleEnabled
	{
		get
		{
			if (mNavMeshObstacle != null)
				return mNavMeshObstacle.enabled;
			return false;
		}

		set
		{
			if (mNavMeshObstacle != null)
				mNavMeshObstacle.enabled = value;
		}
	}

	protected UIBubbleInfo mUIBubbleInfo;

	protected EntityParameters mEntityParameters;

	protected NavMeshObstacle mNavMeshObstacle;

	protected DataScene mDataScene;

	protected Vector3 mSize;

	protected bool mBusy;

	private AObjectDataScene mODS;

	private Queue<Color> mOriginalColorsMaterial;

	public abstract void Dispose();

	public abstract void RemoveEntity();

	public abstract void SaveEntity();

	protected virtual void Awake()
	{
		PostPoppingAction = new List<Action>();
		mOriginalColorsMaterial = new Queue<Color>();
		mEntityParameters = GetComponent<EntityParameters>();

		if (mEntityParameters != null && GetComponent<AInteraction>() == null)
			gameObject.AddComponent<GenericInteractable>();

		//mAInteraction = GetComponent<AInteraction>();

		PostPoppingAction.Add(() => {
			if (mODS.IsColored) {
				mODS.IsColored = false; // This will be reset to true in the followed SetColored
										// We set it to false because if IsColored is true, it will not save the default texture
				SetColored(mODS.Color);
			}
		});
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
        if (mODS != null) {
            mUIBubbleInfo.SetUIName(mODS.Name);
            mUIBubbleInfo.SetUISpeed(mODS.Speed);
        } else
            Debug.LogError("[AENTITY] mODS is null when setting UIBubbleInfo");
		mUIBubbleInfo.RefreshCanvas();

        // Add code to OnEndEdit SpeedInput callback to serialize the value
        mUIBubbleInfo.OnEndEditSpeedCallback.Add((iNewSpeed) => {
            mODS.Speed = iNewSpeed;
            mDataScene.Serialize();
        });
		return this;
	}

    /// <summary>
    /// The Speed input given by the user.
    /// If the user give an invalid input, or if an error occured a default value is used.
    /// Default Value: 1
    /// </summary>
    public float GetSpeedInput()
    {
        return mUIBubbleInfo.Speed;
    }

	public Button CreateBubbleInfoButton(UIBubbleInfoButton iButtonInfo)
	{
		if (mUIBubbleInfo == null) {
			Debug.LogError("[AENTITY] mUIBubbleInfo is null when create button");
			return null;
		}
		return mUIBubbleInfo.CreateButton(iButtonInfo);
	}

	public bool ContainsBubbleInfoButton(string iTag)
	{
		return mUIBubbleInfo.ContainsButton(iTag);
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

	public void ResetColor()
	{
		if (!mODS.IsColored)
			return;

		Utils.BrowseChildRecursively(gameObject, (iObject) => {

			Renderer lR = iObject.GetComponent<Renderer>();
			if (lR != null) {

				Material[] lMaterials = lR.materials;

				if (mODS.IsColored) {
					foreach (Material lMaterial in lMaterials) {
						lMaterial.color = mOriginalColorsMaterial.Dequeue();
					}
				}

				lR.materials = lMaterials;
			}
		});

		mODS.IsColored = false;
		mODS.OriginalColorsMaterial = new List<Color>(mOriginalColorsMaterial);
		mDataScene.Serialize();
	}

	public void SetColored(Color iColor)
	{
		if (!mODS.IsColored)
			mOriginalColorsMaterial.Clear();

		mODS.Color = iColor;

		Utils.BrowseChildRecursively(gameObject, (iObject) => {

			Renderer lR = iObject.GetComponent<Renderer>();
			if (lR != null) {

				Material[] lMaterials = lR.materials;

				if (mODS.IsColored) {
					foreach (Material lMaterial in lMaterials) {
						lMaterial.color = iColor;
					}
				} else if (!mODS.IsColored) {
					foreach (Material lMaterial in lMaterials) {

						mOriginalColorsMaterial.Enqueue(lMaterial.color);
						lMaterial.color = iColor;
					}

				}

				lR.materials = lMaterials;
			}
		});

		mODS.IsColored = true;
		mODS.OriginalColorsMaterial = new List<Color>(mOriginalColorsMaterial);
		mDataScene.Serialize();
	}

	// This function Instantiate associated Model & make it child of OffsetRotation
	// Then all material are replace by GhostMaterial
	public GameObject CreateGhostObject()
	{
		GameObject lGameObject = null;
		GameObject oGhostObject = null;
		Material lGhostMaterial = null;

		if ((lGhostMaterial = Resources.Load<Material>(GameManager.UI_MATERIAL + "Ghost")) == null) {
			Debug.LogError("[AENTITY] Load material : 'Ghost' failed.");
			return null;
		}

		if (mODS.Type == ObjectDataSceneType.BUILT_IN) {
			lGameObject = ModelLoader.Instance.GetModelGameObject(mODS.PrefabName);
			if (lGameObject == null) {
				Debug.LogError("[AENTITY] Load prefab " + mODS.PrefabName + " failed.");
				return null;
			}
		} else {
			lGameObject = ModelLoader.Instance.GetModelGameObject(mODS.PrefabName);
			if (lGameObject == null) {
				Debug.LogError("[AENTITY] Load model " + mODS.PrefabName + " failed.");
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
		transform.parent = null;
		transform.localPosition = Vector3.zero;
		transform.position = mODS.Position;
		transform.transform.eulerAngles = mODS.Rotation;
	}

}