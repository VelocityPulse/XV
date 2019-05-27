using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;
using UnityEngine.UI;

public class UITrack : MonoBehaviour
{
	private List<UIClip> mAnimations;
	private List<UIClip> mTranslations;
	private List<UIClip> mRotations;

	private RectTransform mRectTransform;
	private UIClip UIAnimationClipPrefab;
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

	private void OnEnable()
	{
		StartCoroutine(CheckIntegrityAsync());
	}

	private void OnDisable()
	{
		StopAllCoroutines();
	}

	private void Awake()
	{
		mRectTransform = transform.Find("Track") as RectTransform;
		mAnimations = new List<UIClip>();
		mTranslations = new List<UIClip>();
		mRotations = new List<UIClip>();
		UIAnimationClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "Timeline/UIAnimationClip");
		UITranslationClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "Timeline/UITranslationClip");
		UIRotationClipPrefab = Resources.Load<UIClip>(GameManager.UI_TEMPLATE_PATH + "Timeline/UIRotationClip");
	}

	private IEnumerator CheckIntegrityAsync()
	{
		while (true) {
			GameObject lObject = TimelineManager.Instance.GetObjectFromID(ID);
			if (lObject == null) {
				TimelineManager.Instance.DeleteTrack(ID);
			}
			yield return new WaitForSeconds(0.2F);
		}
	}

	public void AddAnimationClip(double iStart)
	{
		UIClip lClip = Instantiate(UIAnimationClipPrefab, mRectTransform);
		lClip.Type = TimelineData.TrackType.ANIMATION;
		float lClipX = TimelineUtility.ClipStartToPosition(iStart, GetLimits()) + UIClip.sSizeMin / 2F;
		mAnimations.Add(lClip);
		lClip.Build(lClip.Size, lClipX);
	}

	public void AddTranslationClip(double iStart)
	{
		UIClip lClip = Instantiate(UITranslationClipPrefab, mRectTransform);
		lClip.Type = TimelineData.TrackType.TRANSLATION;
		mTranslations.Add(lClip);
		BuildClip(lClip, iStart);
	}

	public void AddRotationClip(double iStart)
	{
		UIClip lClip = Instantiate(UIRotationClipPrefab, mRectTransform);
		lClip.Type = TimelineData.TrackType.ROTATION;
		mRotations.Add(lClip);
		BuildClip(lClip, iStart);
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

	public void ResizeClip(TimelineEvent.Data iData)
	{
		List<UIClip> lClips = GetClipList(iData.Type);
		if (lClips.Count > iData.ClipIndex) {
			BuildClip(lClips[iData.ClipIndex], iData.ClipStart);
		}
	}

	private float BuildClip(UIClip iClip, double iClipStart)
	{
		float lClipX = TimelineUtility.ClipStartToPosition(iClipStart, GetLimits());
		lClipX += UIClip.sSizeMin / 2F;
		iClip.Build(iClip.Size, lClipX);
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
				return mAnimations;
			case TimelineData.TrackType.TRANSLATION:
				return mTranslations;
			case TimelineData.TrackType.ROTATION:
				return mRotations;
			default:
				return null;
		}
	}
}
