using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;

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

	public void CreateClip(int iTrackID, AnimationClip iClip)
	{
		AnimationTrack lTrack = (AnimationTrack)GetTrack(iTrackID, TimelineData.TrackType.ANIMATION);
		TimelineClip lTimelineClip = lTrack.CreateClip(iClip);

		TimelineEvent.Data lEventData = new TimelineEvent.Data(iTrackID);
		lEventData.ClipStart = lTimelineClip.start;
		lEventData.ClipLength = iClip.length;
		TimelineEvent.OnAddClip(lEventData);
	}

	public void DestroyClip(int iTrackID, AnimationClip iClip)
	{
		TrackAsset lTrack = GetTrack(iTrackID, TimelineData.TrackType.ANIMATION);
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		
		int lIndexToDelete = lClips.FindIndex(lClip => lClip.animationClip == iClip);
		mTimeline.DeleteClip(lClips[lIndexToDelete]);

		TimelineEvent.Data lEventData = new TimelineEvent.Data(iTrackID);
		lEventData.ClipIndex = lIndexToDelete;
		TimelineEvent.OnDeleteClip(lEventData);
		CheckEmptyTrack(iTrackID);
	}

	public GroupTrack CreateTrack(GameObject iObject)
	{
		int lID = iObject.GetInstanceID();
		GroupTrack lGroup = (GroupTrack)mTimeline.CreateTrack(typeof(GroupTrack), null, lID.ToString());
		mTimeline.CreateTrack(typeof(AnimationTrack), lGroup, TrackType.ANIMATION.ToString());
		mTimeline.CreateTrack(typeof(ActionTrack), lGroup, TrackType.TRANSLATION.ToString());
		mTimeline.CreateTrack(typeof(ActionTrack), lGroup, TrackType.ROTATION.ToString());
		mBindings.Add(lID, lGroup);
		mDirector.SetGenericBinding(lGroup, iObject);
		TimelineEvent.OnAddTrack(new TimelineEvent.Data(lID));
		return lGroup;
	}

	private void DestroyTrack(int iID)
	{
		GroupTrack lGroup = GetGroupTrack(iID);
		mTimeline.DeleteTrack(lGroup);
		mBindings.Remove(iID);
		TimelineEvent.Data lEventData = new TimelineEvent.Data(iID);
		TimelineEvent.OnDeleteTrack(lEventData);
	}

	public TrackAsset GetTrack(int iID, TrackType iType)
	{
		GroupTrack lGroup = GetGroupTrack(iID);
		return GetTrackFromGroup(lGroup, iType);
	}

	public TrackAsset GetTrackFromGroup(GroupTrack iGroup, TrackType iType)
	{
		TrackAsset lTrack = null;
		if (iGroup != null) {
			List<TrackAsset> lTracks = iGroup.GetChildTracks().ToList();
			lTrack = lTracks.Find(iTrack => iTrack.name == iType.ToString());
		}
		return lTrack;
	}

	public GroupTrack GetGroupTrack(int iID)
	{
		GroupTrack lGroup = null;
		mBindings.TryGetValue(iID, out lGroup);
		return lGroup;
	}

	public Dictionary<int, TrackAsset> GetAllTracksOfType(TrackType iType)
	{
		Dictionary<int, TrackAsset> lTracks = new Dictionary<int, TrackAsset>();
		foreach (KeyValuePair<int, GroupTrack> iBinding in mBindings) {
			TrackAsset lTrack = GetTrackFromGroup(iBinding.Value, iType);
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
}
