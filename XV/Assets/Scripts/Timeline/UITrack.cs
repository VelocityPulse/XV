using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class UITrack : MonoBehaviour, IPointerClickHandler {

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

	private void Awake()
	{
		mRectTransform = transform.Find("Track") as RectTransform;
		mClips = new List<UIClip>();
		UIClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "UIClip");
	}

	private float GetClipRealSize(double iClipLength)
	{
		double lTotalDuration = TimelineManager.Instance.Duration;
		float lTotalSize = mRectTransform.rect.size.x;
		float lWantedSize = ((float)iClipLength * lTotalSize) / (float)lTotalDuration;
		return lWantedSize;
	}

	private float GetClipRealStartPosition(double iClipStart)
	{
		Vector2 lLimits = GetLimits();
		double lTotalDuration = TimelineManager.Instance.Duration;
		float lClipStartPosition = (float)iClipStart * (lLimits.y - lLimits.x) / ((float)lTotalDuration) + lLimits.x;
		return lClipStartPosition;
	}

	public void AddClip()
	{
		AddClip(0D, 1D);
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
		if (mClips.Remove(iClip)) {
			Destroy(iClip.gameObject);
		}
	}

	public void ResizeClip(int iClipIndex, double iClipStart, double iClipLength)
	{
		for (int lIndex = 0; lIndex < mClips.Count; lIndex++) {
			if (lIndex == iClipIndex) {
				BuildClip(mClips[lIndex], iClipStart, iClipLength);
			}
		}
	}

	private float BuildClip(UIClip iClip, double iClipStart, double iClipLength)
	{
		float lClipSize = GetClipRealSize(iClipLength);
		float lClipX = GetClipRealStartPosition(iClipStart) + lClipSize / 2F;
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

	public Vector2 GetLimits()
	{
		float lHalfSize = mRectTransform.rect.size.x / 2F;
		return new Vector2(-lHalfSize, lHalfSize);
	}

	public void OnPointerClick(PointerEventData iData)
	{
		Vector2 lLocalPointerPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, iData.position, iData.pressEventCamera, out lLocalPointerPosition);
		if (iData.button == PointerEventData.InputButton.Left) {
			//AddClip(lLocalPointerPosition.x);
		}
	}
}
