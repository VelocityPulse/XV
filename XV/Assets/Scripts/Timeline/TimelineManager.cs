using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityEngine.Animations;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent(typeof(PlayableDirector))]
public sealed class TimelineManager : MonoBehaviour
{
	public static TimelineManager Instance { get; private set; }

	public double Duration
	{
		get { return mTimeline.duration; }
	}

#if UNITY_EDITOR
	private EditorWindow mTimelineWindow;
#endif

	private PlayableDirector mDirector;
	private TimelineAsset mTimeline;
	private Dictionary<int, AnimationTrack> mBindings;

	private void OnEnable()
	{
		TimelineEvent.ResizeClipEvent += ResizeAnimation;
	}

	private void OnDisable()
	{
		TimelineEvent.ResizeClipEvent -= ResizeAnimation;
	}

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
			TimelineClip lTimelineClip = lTrack.CreateClip(iClip);

			TimelineEvent.Data lEventData = new TimelineEvent.Data(iObject.GetInstanceID(), TimelineEvent.Source.FROM_WORLD);
			lEventData.ClipStart = lTimelineClip.start;
			lEventData.ClipLength = iClip.length;
			TimelineEvent.OnAddClip(lEventData);
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

	public void ResizeAnimation(TimelineEvent.Data iData)
	{
		foreach (KeyValuePair<int, AnimationTrack> lBinding in mBindings) {
			int lObjectID = lBinding.Key;
			AnimationTrack lTrack = lBinding.Value;

			if (lObjectID == iData.TrackID) {
				List<TimelineClip> lClips = lTrack.GetClips().ToList();

				for (int lIndex = 0; lIndex < lClips.Count; lIndex++) {
					if (lIndex == iData.ClipIndex) {
						TimelineClip lClip = lClips[lIndex];
						lClip.start = iData.ClipStart;
						lClip.duration = iData.ClipLength;
					}
				}
			}
		}
	}

	public void Rebuild()
	{
		foreach (KeyValuePair<int, AnimationTrack> lBinding in mBindings) {
			int lObjectID = lBinding.Key;
			AnimationTrack lTrack = lBinding.Value;
			List<TimelineClip> lClips = lTrack.GetClips().ToList();

			for (int lIndex = 0; lIndex < lClips.Count; lIndex++) {
				TimelineEvent.Data lEventData = new TimelineEvent.Data(lObjectID, TimelineEvent.Source.FROM_WORLD);
				lEventData.ClipIndex = lIndex;
				lEventData.ClipStart = lClips[lIndex].start;
				lEventData.ClipLength = lClips[lIndex].duration;

				TimelineEvent.OnResizeClip(lEventData);
			}
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
				TimelineEvent.OnAddTrack(new TimelineEvent.Data(lID, TimelineEvent.Source.FROM_WORLD));
			}
		}
		return lTrack;
	}

	public GameObject GetObjectFromID(int iID)
	{
		foreach (KeyValuePair<int, AnimationTrack> lBinding in mBindings) {
			if (lBinding.Key == iID) {
				return (GameObject)mDirector.GetGenericBinding(lBinding.Value);
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