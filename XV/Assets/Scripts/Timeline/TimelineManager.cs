﻿using System;
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

using AnimAction = System.Predicate<object>;

[RequireComponent(typeof(PlayableDirector))]
public sealed class TimelineManager : MonoBehaviour
{
	public static TimelineManager Instance { get; private set; }
	public enum State { PLAY, PAUSE, STOP };
	public static State sGlobalState;

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

	public void AddAnimation(GameObject iObject, AnimAction iAction, object iParams)
	{
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			if (!mData.TrackExists(lID)) {
				mData.CreateTrack(iObject);
			}
			mData.CreateEventClip(lID, iAction, TimelineData.EventType.ANIMATION, iParams);
		}
	}

	public void AddInteraction(GameObject iObject, List<InteractionStep> iSteps)
	{
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			if (!mData.TrackExists(lID)) {
				mData.CreateTrack(iObject);
			}
			mData.CreateInteractionEventClip(lID, iSteps);
		}
	}

	public void AddTranslation(GameObject iObject, AnimAction iAction)
	{
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			if (!mData.TrackExists(lID)) {
				mData.CreateTrack(iObject);
			}
			mData.CreateEventClip(lID, iAction, TimelineData.EventType.TRANSLATION);
		}
	}

	public void AddRotation(GameObject iObject, AnimAction iAction)
	{
		if (iObject != null) {
			int lID = iObject.GetInstanceID();
			if (!mData.TrackExists(lID)) {
				mData.CreateTrack(iObject);
			}
			mData.CreateEventClip(lID, iAction, TimelineData.EventType.ROTATION);
		}
	}

	public void DeleteTrack(int iID)
	{
#if UNITY_EDITOR
		// Removing a track from the timeline at runtime causes errors in the timeline EditorWindow.
		// This closes the timeline EditorWindow before removing the track to avoid these errors.
		CloseTimelineWindow();
#endif
		if (mData.TrackExists(iID)) {
			mData.DestroyTrack(iID);
		}
	}

	public void Rebuild()
	{
		mData.RebuildTracksOfType(TimelineData.EventType.ANIMATION);
		mData.RebuildTracksOfType(TimelineData.EventType.TRANSLATION);
		mData.RebuildTracksOfType(TimelineData.EventType.ROTATION);
		mData.RebuildTracksOfType(TimelineData.EventType.INTERACTION);
	}

	public GameObject GetObjectFromID(int iID)
	{
		return mData.GetBinding(iID);
	}

	public string GetClipDescription(TimelineEventData iData)
	{
		return mData.GetClipDescription(iData);
	}

	public void SetClipDescription(TimelineEventData iData, string iDescription)
	{
		mData.SetClipDescription(iData, iDescription);
	}

	private void UIResizeClip(TimelineEventData iData)
	{
		TrackAsset lTrack = mData.GetTrack(iData.TrackID, iData.Type);
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		if (lClips.Count > iData.ClipIndex) {
			TimelineClip lClip = lClips[iData.ClipIndex];
			lClip.start = iData.ClipStart;
			lClip.duration = iData.ClipLength;
		}
	}

	private void UIDeleteClip(TimelineEventData iData)
	{
		TrackAsset lTrack = mData.GetTrack(iData.TrackID, iData.Type);
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		if (lClips.Count > iData.ClipIndex) {
			mTimeline.DeleteClip(lClips[iData.ClipIndex]);
		}
		mData.CheckEmptyTrack(iData.TrackID);
	}

	public void Play()
	{
		mDirector.Play();
		TimelineEvent.OnPlay(null);
	}

	public void Pause()
	{
		mDirector.Pause();
		TimelineEvent.OnPause(null);
	}

	public void Stop()
	{
		mDirector.Stop();
		TimelineEvent.OnStop(null);
		AEntity.ForEachEntities(iEntity => iEntity.ResetWorldState());
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
		// Creation of a dummy track to set a minimum duration of 5 seconds
		ActivationTrack lTrack = (ActivationTrack)mTimeline.CreateTrack(typeof(ActivationTrack), null, "Duration Track");
		lTrack.CreateDefaultClip();
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