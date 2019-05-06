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

	public bool Looping
	{
		set { mDirector.extrapolationMode = value ? DirectorWrapMode.Loop : DirectorWrapMode.None; }
	}

	public double Time
	{
		get { return mDirector.time; }
	}

#if UNITY_EDITOR
	private EditorWindow mTimelineWindow;
#endif

	private PlayableDirector mDirector;
	private TimelineAsset mTimeline;
	private DirectorWrapMode mWrapMode;
	private TimelineData mData;

	private void OnEnable()
	{
		TimelineEvent.UIResizeClipEvent += UIResizeClip;
		TimelineEvent.UIDeleteClipEvent += UIDeleteClip;
	}

	private void OnDisable()
	{
		TimelineEvent.UIResizeClipEvent -= UIResizeClip;
		TimelineEvent.UIDeleteClipEvent -= UIDeleteClip;
	}

	private void Start()
	{
		if (Instance == null) {
			Instance = this;
		}
		mDirector = GetComponent<PlayableDirector>();
		mTimeline = (TimelineAsset)mDirector.playableAsset;
		mData = new TimelineData(mTimeline, mDirector);
		ClearTimeline();
	}

	public void AddClip(GameObject iObject, AnimationClip iClip)
	{
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			if (!mData.TrackExists(lID)) {
				mData.CreateTrack(iObject);
			}
			mData.CreateClip(lID, iClip);
		}
	}

	public void DeleteClip(GameObject iObject, AnimationClip iClip)
	{
#if UNITY_EDITOR
		// Removing a track from the timeline at runtime causes errors in the timeline EditorWindow.
		// This closes the timeline EditorWindow before removing the track to avoid these errors.
		CloseTimelineWindow();
#endif
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			if (mData.TrackExists(lID)) {
				mData.DestroyClip(lID, iClip);
			}
		}
	}

	private void UIResizeClip(TimelineEvent.Data iData)
	{
		TrackAsset lTrack = mData.GetTrack(iData.TrackID, TimelineData.TrackType.ANIMATION);
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		if (lClips.Count > iData.ClipIndex) {
			TimelineClip lClip = lClips[iData.ClipIndex];
			lClip.start = iData.ClipStart;
			lClip.duration = iData.ClipLength;
		}
	}

	private void UIDeleteClip(TimelineEvent.Data iData)
	{
		TrackAsset lTrack = mData.GetTrack(iData.TrackID, TimelineData.TrackType.ANIMATION);
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		if (lClips.Count > iData.ClipIndex) {
			mTimeline.DeleteClip(lClips[iData.ClipIndex]);
		}
		mData.CheckEmptyTrack(iData.TrackID);
	}

	public void Rebuild()
	{
		Dictionary<int, TrackAsset> lTracks = mData.GetAllTracksOfType(TimelineData.TrackType.ANIMATION);
		foreach (KeyValuePair<int, TrackAsset> lTrack in lTracks) {
			List<TimelineClip> lClips = lTrack.Value.GetClips().ToList();
			for (int lIndex = 0; lIndex < lClips.Count; lIndex++) {
				TimelineEvent.Data lEventData = new TimelineEvent.Data(lTrack.Key);
				lEventData.ClipIndex = lIndex;
				lEventData.ClipStart = lClips[lIndex].start;
				lEventData.ClipLength = lClips[lIndex].duration;
				TimelineEvent.OnResizeClip(lEventData);
			}
		}
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

	public void Play()
	{
		mDirector.Play();
	}

	public void Pause()
	{
		mDirector.Pause();
	}

	public void Stop()
	{
		mDirector.Stop();
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