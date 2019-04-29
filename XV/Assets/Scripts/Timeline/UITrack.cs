using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class UITrack : MonoBehaviour /*, IPointerClickHandler*/
{
	private List<UIClip> mClips;

	private AnimationTrack mTrack;
	private RectTransform mRectTransform;
	private UIClip UIClipPrefab;
	private int mCounter;

	[SerializeField]
	private Text nameText;

	public int ID { get; set; }

	public string Name
	{
		get { return nameText.text; }
		set
		{
			if (value != string.Empty) {
				nameText.text = value;
			}
		}
	}

	public float Size
	{
		get { return mRectTransform.rect.size.x; }
	}

	private void Awake()
	{
		mRectTransform = transform.Find("Track") as RectTransform;
		mClips = new List<UIClip>();
		UIClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "UIClip");
	}

	public void AddClip(double iStart, double iLength)
	{
		UIClip lClip = Instantiate(UIClipPrefab, mRectTransform);

		string lClipName = "Clip " + mCounter.ToString();
		lClip.name = lClipName;
		lClip.Name = lClipName;
		mCounter++;

		float lClipX = BuildClip(lClip, iStart, iLength);
		int lPrevIndex = GetPreviousAtPosition(lClipX);

		// Insert clip at the end of the list
		if (lPrevIndex == mClips.Count - 1) {
			mClips.Add(lClip);
		}
		// Insert clip in the list at specified index
		else {
			mClips.Insert(lPrevIndex + 1, lClip);
		}
	}

	public void DeleteClip(UIClip iClip)
	{
		TimelineEvent.Data lEventData = new TimelineEvent.Data(ID);
		lEventData.ClipIndex = GetIndex(iClip);
		if (mClips.Remove(iClip)) {
			TimelineEvent.OnUIDeleteClip(lEventData);
			Destroy(iClip.gameObject);
		}
	}

	public void ResizeClip(int iClipIndex, double iClipStart, double iClipLength)
	{
		if (mClips.Count > iClipIndex) {
			BuildClip(mClips[iClipIndex], iClipStart, iClipLength);
		}
	}

	private float BuildClip(UIClip iClip, double iClipStart, double iClipLength)
	{
		float lClipSize = TimelineUtility.ClipDurationToSize(iClipLength, mRectTransform.rect.size.x);
		float lClipX = TimelineUtility.ClipStartToPosition(iClipStart, GetLimits()) + lClipSize / 2F;
		iClip.Build(lClipSize, lClipX);
		return lClipX;
	}

	public int GetPreviousAtPosition(float iClipX)
	{
		if (mClips.Count == 0) {
			return -1;
		}

		int i;
		for (i = 0; i < mClips.Count; i++) {
			if (mClips[i].transform.localPosition.x >= iClipX) {
				return i - 1;
			}
		}
		return i - 1;
	}

	public int GetNextAtPosition(float iClipX)
	{
		if (mClips.Count == 0) {
			return -1;
		}

		int i;
		for (i = 0; i < mClips.Count; i++) {
			if (mClips[i].transform.localPosition.x > iClipX) {
				return i;
			}
		}
		return -1;
	}

	public UIClip GetClip(int iIndex)
	{
		if (iIndex >= 0 && iIndex < mClips.Count) {
			return mClips[iIndex];
		}
		return null;
	}

	public int GetIndex(UIClip iClip)
	{
		return mClips.IndexOf(iClip);
	}

	public Vector2 GetLimits()
	{
		float lHalfSize = mRectTransform.rect.size.x / 2F;
		return new Vector2(-lHalfSize, lHalfSize);
	}

// Removed possibility to add a clip by clicking on the track
/*
	public void OnPointerClick(PointerEventData iData)
	{
		Vector2 lLocalPointerPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, iData.position, iData.pressEventCamera, out lLocalPointerPosition);
		if (iData.button == PointerEventData.InputButton.Left) {
			GameObject lObject = TimelineManager.Instance.GetObjectFromID(ID);
			TimelineManager.Instance.AddClip(lObject, new AnimationClip());
		}
	}
 */
}
