using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;

[RequireComponent(typeof(PlayableDirector))]
public class TimelineManager : MonoBehaviour {

	public static TimelineManager Instance { get; private set; }

	private PlayableDirector mDirector;
	private TimelineAsset mTimeline;
	private List<GameObject> mBoundObjects;

#if UNITY_EDITOR
	private EditorWindow mTimelineWindow;
#endif

	private void Start()
	{
		if (Instance == null) {
			Instance = this;
		}
		mDirector = GetComponent<PlayableDirector>();
		mTimeline = (TimelineAsset)mDirector.playableAsset;
		ClearTimeline();
	}

	public void AddObject(GameObject iObject)
	{
		if (iObject != null) {
			AnimationTrack lTrack = (AnimationTrack)mTimeline.CreateTrack(typeof(AnimationTrack), null, iObject.name);
			mDirector.SetGenericBinding(lTrack, iObject);
		}
	}

	public void RemoveObject(GameObject iObject)
	{
#if UNITY_EDITOR
		// Removing a track from the timeline at runtime causes errors in the timeline EditorWindow.
		// This closes the timeline EditorWindow before removing the track to avoid these errors.
		CloseTimelineWindow();
#endif
		if (iObject != null) {
			TrackAsset lTrackToDelete = GetTrackWithName(iObject.name);
			if (lTrackToDelete != null) {
				mTimeline.DeleteTrack(lTrackToDelete);
			}
		}
	}

	private TrackAsset GetTrackWithName(string iTrackName) {
		foreach (TrackAsset lTrack in mTimeline.GetRootTracks()) {
			if (lTrack.name == iTrackName) {
				return lTrack;
			}
		}
		return null;
	}

	private void ClearTimeline()
	{
		List<TrackAsset> lToDelete = new List<TrackAsset>();
		foreach (TrackAsset lRootTrack in mTimeline.GetRootTracks()) {
			lToDelete.Add(lRootTrack);
		}
		foreach (TrackAsset lRootTrack in lToDelete) {
			mTimeline.DeleteTrack(lRootTrack);
		}
	}

#if UNITY_EDITOR
	private void CloseTimelineWindow()
	{
		if (mTimelineWindow == null) {
			mTimelineWindow = GetTimelineWindow();
		}
		if (mTimelineWindow != null) {
			mTimelineWindow.Close();
		}
	}

	private EditorWindow GetTimelineWindow()
	{
		EditorWindow[] lAllWindows = Resources.FindObjectsOfTypeAll<EditorWindow>();
		foreach (EditorWindow lWin in lAllWindows) {
			if (lWin.GetType().Name == "TimelineWindow") {
				return lWin;
			}
		}
		return null;
	}
#endif
}