using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Timeline;

public class UITrack : MonoBehaviour, IPointerClickHandler {

	private List<UIClip> mClips;

	private AnimationTrack mTrack;
	private RectTransform mRectTransform;
	private UIClip UIClipPrefab;
	private int mCounter;

	private void Start()
	{
		mRectTransform = transform as RectTransform;
		mClips = new List<UIClip>();
		UIClipPrefab = Resources.Load<UIClip>(GameManager.UITemplatePath + "UIClip");
	}

	public void AddClip(float iClipX)
	{
		UIClip lClip = Instantiate(UIClipPrefab, transform);

		lClip.name = "CLIP" + mCounter.ToString();
		mCounter++;

		lClip.transform.localPosition = new Vector3(iClipX, 0F, 0F);

		int lPrevIndex = GetPreviousAtPosition(iClipX);

		// Insert clip at the end of the list
		if (lPrevIndex == mClips.Count - 1) {
			mClips.Add(lClip);
		}
		// Insert clip in the list at specified index
		else {
			mClips.Insert(lPrevIndex + 1, lClip);
		}
		// Debug
		/*
		for (int i = 0; i < mClips.Count; i++) {
			Debug.Log(i + " -> " + mClips[i].name);
		}
		Debug.Log("--------------");
		*/
	}

	public void DeleteClip(UIClip iClip)
	{
		if (mClips.Remove(iClip)) {
			Destroy(iClip.gameObject);
		}
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
        Vector3[] lTrackCorners = new Vector3[4];

		mRectTransform.GetLocalCorners(lTrackCorners);
		return new Vector2(lTrackCorners[0].x, lTrackCorners[2].x);
	}

	public void OnPointerClick(PointerEventData iData)
	{
		Vector2 lLocalPointerPosition;
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, iData.position, iData.pressEventCamera, out lLocalPointerPosition);
		if (iData.button == PointerEventData.InputButton.Left) {
			AddClip(lLocalPointerPosition.x);
		}
	}
}
