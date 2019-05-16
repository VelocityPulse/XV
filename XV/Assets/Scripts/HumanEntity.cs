using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CapsuleCollider))]
public class HumanEntity : MonoBehaviour
{

	public static HumanEntity Instance { get; private set; }

	[SerializeField]
	private UIBubbleInfo UIBubbleInfo;

	private CapsuleCollider mCapsuleCollider;

	private DataScene mDataScene;
	private HumanDataScene mHDS;
	private bool mBusy;
	private bool mSelected;
	private bool mControlPushed;
	private bool mMouseDown;

	private bool mMouseOverObjectEntity;
	private bool mMouseDragObjectEntity;

	private Vector3 mCenter;
	//private Vector3 mSize;

	public bool Selected
	{
		get { return mSelected; }

		set
		{
			if (!value) {
				Debug.Log("ObjectEntity : " + mHDS.Name + "Has been unselected");
				UIBubbleInfo.Hide();
			}
			mSelected = value;
		}
	}

	public string Name
	{
		get { return gameObject.name; }

		set
		{
			if (string.IsNullOrEmpty(value))
				return;

			gameObject.name = value;
			name = value + "_mesh";
			mHDS.Name = value;
			SaveEntity();
		}
	}

	private void Start()
	{
		mCapsuleCollider = GetComponent<CapsuleCollider>();

		mCenter = mCapsuleCollider.center;

		BuildHuman();
        Debug.Log("true");
		Instance = this;
	}


	private void Update()
	{


	}

	private void OnDestroy()
	{

	}

    public void Dispose()
    {
        Debug.Log("false");
        Instance = null;
        Destroy(gameObject);
    }

	private void BuildHuman()
	{

	}

	private HumanEntity SaveEntity()
	{
		if (mHDS != null) {
			Vector3 lPosition = new Vector3(
				transform.position.x,
				transform.position.y,
				transform.position.z
			);
			mHDS.Position = lPosition;
			mHDS.Rotation = transform.rotation.eulerAngles;
			mHDS.Scale = transform.localScale;
			mDataScene.Serialize();
		}
		return this;
	}

	public HumanEntity RemoveEntity()
	{
		if (mHDS != null && mDataScene.Human != null) {
			mDataScene.Human = null;
			mDataScene.Serialize();
		}
		return this;
	}

	public HumanEntity InitDataScene(DataScene iDataScene)
	{
		mDataScene = iDataScene;
		return this;
	}

	public HumanEntity SetHumanDataScene(HumanDataScene iHDS)
	{
		mHDS = iHDS;
        if (mDataScene.Human != iHDS) {
            mDataScene.Human = iHDS;
            mDataScene.Serialize();
        }
		return this;
	}

}
