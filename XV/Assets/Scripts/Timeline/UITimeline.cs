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
		TimelineEvent.AddTrackEvent += NewTrack;
		TimelineEvent.AddClipEvent += AddClipToTrack;
	}

	private void OnDisable()
	{
		TimelineEvent.AddTrackEvent -= NewTrack;
		TimelineEvent.AddClipEvent -= AddClipToTrack;
	}

	private void Start()
	{
		mUITrackPrefab = Resources.Load<UITrack>(GameManager.UI_TEMPLATE_PATH + "UITrack");
		mTracks = new List<UITrack>();
		mAnimator = GetComponent<Animator>();
	}

	private void NewTrack(TimelineEvent.Data iData)
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		UITrack lNewTrack = Instantiate(mUITrackPrefab, contentPanel);
		lNewTrack.ID = iData.TrackID;
		lNewTrack.Name = iData.TrackID.ToString();
		mTracks.Add(lNewTrack);
	}

	private void DeleteTrack()
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		GameObject lTrack = mTracks[0].gameObject;
		if (lTrack != null) {
			Destroy(lTrack);
		}
	}
	
	private void AddClipToTrack(TimelineEvent.Data iData)
	{
		foreach (UITrack lTrack in mTracks) {
			if (lTrack.ID == iData.TrackID) {
				lTrack.AddClip();
			}
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
	}

}
