using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ObjectEntity : MonoBehaviour
{

	private DataScene mDataScene;
	private ObjectDataScene mODS;
	private bool mSelected;

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
			if (!value)
				Debug.Log("ObjectEntity : " + mODS.Name + "Has been unselected");
			mSelected = value;
		}
	}

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
		if (!Selected) {
			GameManager.Instance.SelectedEntity = this;
			Debug.Log("ObjectEntity : " + mODS.Name + " has been selected");
		}
	}

	public ObjectEntity InitDataScene(DataScene iDataScene)
	{
		mDataScene = iDataScene;
		return this;
	}

	public ObjectEntity SetObjectDataScene(ObjectDataScene iOBS)
	{
		mODS = iOBS;
        if (!mDataScene.DataObjects.Contains(mODS))
            mDataScene.DataObjects.Add(mODS);
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

    public ObjectEntity SetCenter(Vector3 iVector) {
        mCenter = iVector;
        return this;
    }

    public ObjectEntity SetSize(Vector3 iVector) {
        mSize = iVector;
        return this;
    }
}