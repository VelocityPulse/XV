﻿using System; //
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
		lNewTrack.Name = iData.TrackID.ToString();
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
					lTrack.AddAnimationClip(iData.ClipStart, iData.ClipLength);
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
			lTrack.ResizeClip(iData.ClipIndex, iData.ClipStart, iData.ClipLength);
		}
	}

	public void ToggleVisibility()
	{
		mAnimator.SetBool("IsVisible", !mAnimator.GetBool("IsVisible"));
	}

	// These functions are for testing only
	public void NewTimelineBinding()
	{
		GameObject lObject = new GameObject("TimelineBoundObject");
		lObject.AddComponent<Animator>();
		TimelineManager.Instance.AddAnimation(lObject, new AnimationClip());
		TimelineManager.Instance.AddTranslation(lObject, new Action(() => {
			Debug.Log("Action Translation has been called");
		}));
		TimelineManager.Instance.AddRotation(lObject, new Action(() => {
			Debug.Log("Action Rotation has been called");
		}));
	}

}
