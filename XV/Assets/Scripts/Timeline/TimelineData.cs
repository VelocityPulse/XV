using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

using AnimAction = System.Predicate<AnimationInfo>;

public sealed class TimelineData
{
	public enum TrackType { ANIMATION, TRANSLATION, ROTATION };
	private TimelineAsset mTimeline;
	private PlayableDirector mDirector;
	private Dictionary<int, GroupTrack> mBindings;

	public TimelineData(TimelineAsset iTimeline, PlayableDirector iDirector)
	{
		mTimeline = iTimeline;
		mDirector = iDirector;
		mBindings = new Dictionary<int, GroupTrack>();
	}

	public void CreateEventClip(int iTrackID, AnimAction iAction, TrackType iType)
	{
		ActionTrack lTrack = (ActionTrack)GetTrack(iTrackID, iType);
		TimelineClip lTimelineClip = lTrack.CreateClip<ActionAsset>();
		ActionAsset lActionAsset = lTimelineClip.asset as ActionAsset;
		lActionAsset.AttachedAction = iAction;
		lActionAsset.Track = lTrack;
		lTimelineClip.duration = 0.1D;

		TimelineEvent.Data lEventData = new TimelineEvent.Data(iTrackID);
		lEventData.ClipStart = lTimelineClip.start;
		lEventData.Type = iType;
		TimelineEvent.OnAddClip(lEventData);
	}

	public GroupTrack CreateTrack(GameObject iObject)
	{
		int lID = iObject.GetInstanceID();
		GroupTrack lGroup = (GroupTrack)mTimeline.CreateTrack(typeof(GroupTrack), null, iObject.name);
		TrackAsset lAnim = mTimeline.CreateTrack(typeof(ActionTrack), lGroup, TrackType.ANIMATION.ToString());
		TrackAsset lTrans = mTimeline.CreateTrack(typeof(ActionTrack), lGroup, TrackType.TRANSLATION.ToString());
		TrackAsset lRot = mTimeline.CreateTrack(typeof(ActionTrack), lGroup, TrackType.ROTATION.ToString());
		mBindings.Add(lID, lGroup);
		mDirector.SetGenericBinding(lGroup, iObject);
		mDirector.SetGenericBinding(lAnim, iObject);
		mDirector.SetGenericBinding(lTrans, iObject);
		mDirector.SetGenericBinding(lRot, iObject);
		TimelineEvent.OnAddTrack(new TimelineEvent.Data(lID));
		return lGroup;
	}

	public void DestroyTrack(int iID)
	{
		GroupTrack lGroup = GetGroupTrack(iID);
		mTimeline.DeleteTrack(lGroup);
		mBindings.Remove(iID);
		TimelineEvent.Data lEventData = new TimelineEvent.Data(iID);
		TimelineEvent.OnDeleteTrack(lEventData);
	}

	public void RebuildTracksOfType(TrackType iType)
	{
		Dictionary<int, ActionTrack> lTracks = GetAllTracksOfType(iType);
		foreach (KeyValuePair<int, ActionTrack> lTrack in lTracks) {
			List<TimelineClip> lClips = lTrack.Value.GetClips().ToList();
			for (int lIndex = 0; lIndex < lClips.Count; lIndex++) {
				TimelineEvent.Data lEventData = new TimelineEvent.Data(lTrack.Key);
				lEventData.ClipIndex = lIndex;
				lEventData.ClipStart = lClips[lIndex].start;
				lEventData.Type = iType;
				TimelineEvent.OnResizeClip(lEventData);
			}
		}
	}

	public ActionTrack GetTrack(int iID, TrackType iType)
	{
		GroupTrack lGroup = GetGroupTrack(iID);
		return GetTrackFromGroup(lGroup, iType);
	}

	public ActionTrack GetTrackFromGroup(GroupTrack iGroup, TrackType iType)
	{
		ActionTrack lTrack = null;
		if (iGroup != null) {
			List<TrackAsset> lTracks = iGroup.GetChildTracks().ToList();
			lTrack = lTracks.Find(iTrack => iTrack.name == iType.ToString()) as ActionTrack;
		}
		return lTrack;
	}

	public GroupTrack GetGroupTrack(int iID)
	{
		GroupTrack lGroup = null;
		mBindings.TryGetValue(iID, out lGroup);
		return lGroup;
	}

	public Dictionary<int, ActionTrack> GetAllTracksOfType(TrackType iType)
	{
		Dictionary<int, ActionTrack> lTracks = new Dictionary<int, ActionTrack>();
		foreach (KeyValuePair<int, GroupTrack> iBinding in mBindings) {
			ActionTrack lTrack = GetTrackFromGroup(iBinding.Value, iType);
			lTracks.Add(iBinding.Key, lTrack);
		}
		return lTracks;
	}

	public bool TrackExists(int iID)
	{
		return mBindings.ContainsKey(iID);
	}

	public void CheckEmptyTrack(int iID)
	{
		GroupTrack lGroup = GetGroupTrack(iID);
		if (lGroup != null) {
			int lEmptyTracks = 0;
			foreach (TrackAsset lTrack in lGroup.GetChildTracks()) {
				if (lTrack.isEmpty) {
					lEmptyTracks++;
				}
			}
			if (lEmptyTracks == lGroup.GetChildTracks().Count()) {
				DestroyTrack(iID);
			}
		}
	}

	public GameObject GetBinding(int iID)
	{
		GroupTrack lGroup = GetGroupTrack(iID);
		GameObject lObject = (GameObject)mDirector.GetGenericBinding(lGroup);
		return lObject;
	}
}
