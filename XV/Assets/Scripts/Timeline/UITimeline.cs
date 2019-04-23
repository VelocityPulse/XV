using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimeline : MonoBehaviour {

	private UITrack mUITrackPrefab;
	private Stack<UITrack> mTracks;

	[SerializeField]
	private Transform contentPanel;

	private void Start()
	{
		mUITrackPrefab = Resources.Load<UITrack>(GameManager.UI_TEMPLATE_PATH + "UITrack");
		mTracks = new Stack<UITrack>();
	}

	public void NewTrack()
	{
		UITrack lNewTrack = Instantiate(mUITrackPrefab, contentPanel);
		mTracks.Push(lNewTrack);
	}

	public void DeleteTrack()
	{
		Destroy(mTracks.Pop().gameObject);
	}
}
