using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeline : MonoBehaviour {

	private UITrack mUITrackPrefab;
	private Stack<UITrack> mTracks;
	private Animator mAnimator;

	[SerializeField]
	private Transform contentPanel;

	private void Start()
	{
		mUITrackPrefab = Resources.Load<UITrack>(GameManager.UI_TEMPLATE_PATH + "UITrack");
		mTracks = new Stack<UITrack>();
		mAnimator = GetComponent<Animator>();
	}

	public void NewTrack()
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		UITrack lNewTrack = Instantiate(mUITrackPrefab, contentPanel);
		mTracks.Push(lNewTrack);
	}

	public void DeleteTrack()
	{
		if (!mAnimator.GetBool("IsVisible")) {
			ToggleVisibility();
		}
		GameObject lTrack = mTracks.Pop().gameObject;
		if (lTrack != null) {
			Destroy(lTrack);
		}
	}

	public void ToggleVisibility()
	{
		mAnimator.SetBool("IsVisible", !mAnimator.GetBool("IsVisible"));
	}

}
