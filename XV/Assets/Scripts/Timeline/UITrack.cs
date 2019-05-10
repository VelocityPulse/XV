using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class UITrack : MonoBehaviour
{
	private List<UIClip> mClips;
	private List<UIClip> mTranslations;
	private List<UIClip> mRotations;

	private RectTransform mRectTransform;
	private UIClip UIClipPrefab;
	private UIClip UITranslationClipPrefab;
	private UIClip UIRotationClipPrefab;

	[SerializeField]
	private Text nameText;

	public int ID { get; set; }

	public string Name
	{
		get { return nameText.text; }
		set
		{
			if (!string.IsNullOrEmpty(value)) {
				nameText.text = value;
			}
		}
	}

	public float Size { get { return mRectTransform.rect.size.x; } }

	private void Awake()
	{
		mRectTransform = transform.Find("Track") as RectTransform;
		mClips = new List<UIClip>();
		mTranslations = new List<UIClip>();
		mRotations = new List<UIClip>();
		UIClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "Timeline/UIClip");
		UITranslationClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "Timeline/UITranslationClip");
		UIRotationClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "Timeline/UIRotationClip");
	}

	public void AddAnimationClip(string iName, double iStart, double iLength)
	{
		UIClip lClip = Instantiate(UIClipPrefab, mRectTransform);

		string lClipName = iName + "[" + TimelineUtility.FormatDuration(iLength) + "]";
		lClip.name = lClipName;
		lClip.Name = lClipName;
		lClip.Type = TimelineData.TrackType.ANIMATION;

		float lClipX = BuildClip(lClip, iStart, iLength);
		int lPrevIndex = GetPreviousAtPosition(lClipX, lClip.Type);

		// Insert clip at the end of the list
		if (lPrevIndex == mClips.Count - 1) {
			mClips.Add(lClip);
		}
		// Insert clip in the list at specified index
		else {
			mClips.Insert(lPrevIndex + 1, lClip);
		}
	}

	public void AddTranslationClip(double iStart)
	{
		UIClip lClip = Instantiate(UITranslationClipPrefab, mRectTransform);
		lClip.Type = TimelineData.TrackType.TRANSLATION;
		float lClipX = TimelineUtility.ClipStartToPosition(iStart, GetLimits()) + UIClip.sSizeMin / 2F;
		mTranslations.Add(lClip);
		lClip.Build(lClip.Size, lClipX);
	}

	public void AddRotationClip(double iStart)
	{
		UIClip lClip = Instantiate(UIRotationClipPrefab, mRectTransform);
		lClip.Type = TimelineData.TrackType.ROTATION;
		float lClipX = TimelineUtility.ClipStartToPosition(iStart, GetLimits()) + UIClip.sSizeMin / 2F;
		mRotations.Add(lClip);
		lClip.Build(lClip.Size, lClipX);
	}

	public void DeleteClip(UIClip iClip)
	{
		List<UIClip> lClips = GetClipList(iClip.Type);
		TimelineEvent.Data lEventData = new TimelineEvent.Data(ID);
		lEventData.ClipIndex = GetIndex(iClip);
		lEventData.Type = iClip.Type;
		if (lClips.Remove(iClip)) {
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

	public int GetPreviousAtPosition(float iClipX, TimelineData.TrackType iType)
	{
		List<UIClip> lClips = GetClipList(iType);
		if (lClips.Count == 0) {
			return -1;
		}

		int i;
		for (i = 0; i < lClips.Count; i++) {
			if (lClips[i].transform.localPosition.x >= iClipX) {
				return i - 1;
			}
		}
		return i - 1;
	}

	public int GetNextAtPosition(float iClipX, TimelineData.TrackType iType)
	{
		List<UIClip> lClips = GetClipList(iType);
		if (lClips.Count == 0) {
			return -1;
		}

		int i;
		for (i = 0; i < lClips.Count; i++) {
			if (lClips[i].transform.localPosition.x > iClipX) {
				return i;
			}
		}
		return -1;
	}

	public UIClip GetClip(int iIndex, TimelineData.TrackType iType)
	{
		List<UIClip> lClips = GetClipList(iType);
		if (iIndex >= 0 && iIndex < lClips.Count) {
			return lClips[iIndex];
		}
		return null;
	}

	public int GetIndex(UIClip iClip)
	{
		return GetClipList(iClip.Type).IndexOf(iClip);
	}

	public Vector2 GetLimits()
	{
		float lHalfSize = mRectTransform.rect.size.x / 2F;
		return new Vector2(-lHalfSize, lHalfSize);
	}

	private List<UIClip> GetClipList(TimelineData.TrackType iType)
	{
		switch (iType) {
			case TimelineData.TrackType.ANIMATION:
				return mClips;
			case TimelineData.TrackType.TRANSLATION:
				return mTranslations;
			case TimelineData.TrackType.ROTATION:
				return mRotations;
			default:
				return null;
		}
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
