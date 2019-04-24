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

	private void Start()
	{
		mUITrackPrefab = Resources.Load<UITrack>(GameManager.UI_TEMPLATE_PATH + "UITrack");
		mTracks = new List<UITrack>();
		mAnimator = GetComponent<Animator>();
	}

	public void NewTrack(string iTrackName)
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		UITrack lNewTrack = Instantiate(mUITrackPrefab, contentPanel);
		lNewTrack.Name = iTrackName;
		mTracks.Add(lNewTrack);
	}

	public void DeleteTrack()
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		GameObject lTrack = mTracks[0].gameObject;
		if (lTrack != null) {
			Destroy(lTrack);
		}
	}
	
	public void AddClipToTrack(string iTrackName, string iClipName)
	{
		foreach (UITrack lTrack in mTracks) {
			if (lTrack.Name == iTrackName) {
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
