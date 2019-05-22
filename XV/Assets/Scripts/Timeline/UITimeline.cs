using System; //
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeline : MonoBehaviour {

	private UITrack mUITrackPrefab;
	private List<UITrack> mTracks;
	private Animator mAnimator;

	[SerializeField]
	private Transform contentPanel;

	[SerializeField]
	private Transform modelManager;

	private void OnEnable()
	{
		TimelineEvent.AddTrackEvent += AddTrack;
		TimelineEvent.DeleteTrackEvent += DeleteTrack;
		TimelineEvent.AddClipEvent += AddClip;
		TimelineEvent.ResizeClipEvent += ResizeClip;
	}

	private void OnDisable()
	{
		TimelineEvent.AddTrackEvent -= AddTrack;
		TimelineEvent.DeleteTrackEvent -= DeleteTrack;
		TimelineEvent.AddClipEvent -= AddClip;
		TimelineEvent.ResizeClipEvent -= ResizeClip;
	}

	private void Start()
	{
		mUITrackPrefab = Resources.Load<UITrack>(GameManager.UI_TEMPLATE_PATH + "Timeline/UITrack");
		mTracks = new List<UITrack>();
		mAnimator = GetComponent<Animator>();
	}

	private void AddTrack(TimelineEvent.Data iData)
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		UITrack lNewTrack = Instantiate(mUITrackPrefab, contentPanel);
		lNewTrack.ID = iData.TrackID;
		GameObject lObject = TimelineManager.Instance.GetObjectFromID(iData.TrackID);
		lNewTrack.Name = (lObject != null) ? lObject.name : "Unbound object";
		mTracks.Add(lNewTrack);
	}

	private void DeleteTrack(TimelineEvent.Data iData)
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		UITrack lTrack = mTracks.Find(iTrack => iTrack.ID == iData.TrackID);
		if (lTrack != null) {
			Destroy(lTrack.gameObject);
		}
	}
	
	private void AddClip(TimelineEvent.Data iData)
	{
		UITrack lTrack = mTracks.Find(iTrack => iTrack.ID == iData.TrackID);
		if (lTrack != null) {
			switch (iData.Type) {
				case TimelineData.TrackType.ANIMATION:
					lTrack.AddAnimationClip(iData.ClipName, iData.ClipStart, iData.ClipLength);
					break;
				case TimelineData.TrackType.TRANSLATION:
					lTrack.AddTranslationClip(iData.ClipStart);
					break;
				case TimelineData.TrackType.ROTATION:
					lTrack.AddRotationClip(iData.ClipStart);
					break;
			}
		}
	}

	private void ResizeClip(TimelineEvent.Data iData)
	{
		UITrack lTrack = mTracks.Find(iTrack => iTrack.ID == iData.TrackID);
		if (lTrack != null) {
			lTrack.ResizeClip(iData);
		}
	}

	public void ToggleVisibility()
	{
		Animator lModelManagerAnimator = modelManager.GetComponent<Animator>();
		mAnimator.SetBool("IsVisible", !mAnimator.GetBool("IsVisible"));
		lModelManagerAnimator.SetBool("IsCropped", !lModelManagerAnimator.GetBool("IsCropped"));
	}

/*
	// These functions are for testing only
	public void NewTimelineBinding()
	{
		GameObject lObject = new GameObject("TimelineBoundObject");
		lObject.AddComponent<Animator>();
		TimelineManager.Instance.AddAnimation(lObject, new AnimationClip());
		TimelineManager.Instance.AddTranslation(lObject, new Predicate<float>(i => {
			Debug.Log("Action Translation has been called");
			return true;
		}));
		TimelineManager.Instance.AddRotation(lObject, new Predicate<float>(i => {
			Debug.Log("Action Rotation has been called");
			return true;
		}));
	}
 */

}
