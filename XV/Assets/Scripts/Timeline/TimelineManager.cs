using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayableDirector))]
public class TimelineManager : MonoBehaviour {

	public static TimelineManager Instance { get; private set; }

#if UNITY_EDITOR
	private EditorWindow mTimelineWindow;
#endif

	private PlayableDirector mDirector;
	private TimelineAsset mTimeline;
	private Dictionary<int, AnimationTrack> mBindings;

	[SerializeField]
	private UITimeline UI;

	private void Start()
	{
		if (Instance == null) {
			Instance = this;
		}
		mDirector = GetComponent<PlayableDirector>();
		mTimeline = (TimelineAsset)mDirector.playableAsset;
		mBindings = new Dictionary<int, AnimationTrack>();
		ClearTimeline();
	}

	public void AddAnimation(GameObject iObject, AnimationClip iClip)
	{
		if (iObject != null) {
			AnimationTrack lTrack = GetTrackFromObject(iObject);
			lTrack.CreateClip(iClip);
			UI.AddClipToTrack(iObject.GetInstanceID().ToString(), "New Clip"); // Temporary
		}
	}

	public void RemoveAnimation(GameObject iObject, AnimationClip iClip)
	{
#if UNITY_EDITOR
		// Removing a track from the timeline at runtime causes errors in the timeline EditorWindow.
		// This closes the timeline EditorWindow before removing the track to avoid these errors.
		CloseTimelineWindow();
#endif
		if (iObject != null) {
			/*
			TrackAsset lTrackToDelete = GetTrackWithName(iObject.GetInstanceID().ToString());
			if (lTrackToDelete != null) {
				mTimeline.DeleteTrack(lTrackToDelete);
			}
			*/
		}
	}

	private AnimationTrack GetTrackFromObject(GameObject iObject)
	{
		AnimationTrack lTrack = null;
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			// If the object is already bound to a track
			if (mBindings.ContainsKey(lID)) {
				mBindings.TryGetValue(lID, out lTrack);
			}
			// Otherwise create a new binding
			else {
				lTrack = (AnimationTrack)mTimeline.CreateTrack(typeof(AnimationTrack), null, lID.ToString());
				mBindings.Add(lID, lTrack);
				mDirector.SetGenericBinding(lTrack, iObject);
				UI.NewTrack(lID.ToString());
			}
		}
		return lTrack;
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