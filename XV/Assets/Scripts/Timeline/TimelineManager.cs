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
		mBindings = new Dictionary<int, AnimationTrack>();
		ClearTimeline();
	}

	public void AddClip(GameObject iObject, AnimationClip iClip)
	{
		if (iObject != null) {
			AnimationTrack lTrack = GetTrackFromObject(iObject);
			TimelineClip lTimelineClip = lTrack.CreateClip(iClip);

			TimelineEvent.Data lEventData = new TimelineEvent.Data(iObject.GetInstanceID());
			lEventData.ClipStart = lTimelineClip.start;
			lEventData.ClipLength = iClip.length;
			TimelineEvent.OnAddClip(lEventData);
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
			AnimationTrack lTrack = GetTrackFromObject(iObject);
			List<TimelineClip> lClips = lTrack.GetClips().ToList();

			int lIndexToDelete = lClips.FindIndex(lClip => lClip.animationClip == iClip);
			mTimeline.DeleteClip(lClips[lIndexToDelete]);

			TimelineEvent.Data lEventData = new TimelineEvent.Data(iObject.GetInstanceID());
			lEventData.ClipIndex = lIndexToDelete;
			TimelineEvent.OnDeleteClip(lEventData);
			CheckEmptyTrack(iObject);
		}
	}

	private void UIResizeClip(TimelineEvent.Data iData)
	{
		KeyValuePair<int, AnimationTrack> lBinding = mBindings.FirstOrDefault(iBinding => iBinding.Key == iData.TrackID);
		AnimationTrack lTrack = lBinding.Value;
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		if (lClips.Count > iData.ClipIndex) {
			TimelineClip lClip = lClips[iData.ClipIndex];
			lClip.start = iData.ClipStart;
			lClip.duration = iData.ClipLength;
		}
	}

	private void UIDeleteClip(TimelineEvent.Data iData)
	{
		GameObject lObject = GetObjectFromID(iData.TrackID);
		AnimationTrack lTrack = GetTrackFromObject(lObject);
		List<TimelineClip> lClips = lTrack.GetClips().ToList();
		if (lClips.Count > iData.ClipIndex) {
			mTimeline.DeleteClip(lClips[iData.ClipIndex]);
		}
		CheckEmptyTrack(lObject);
	}

	private void CheckEmptyTrack(GameObject iObject)
	{
		AnimationTrack lTrack = GetTrackFromObject(iObject);
		if (lTrack != null) {
			if (lTrack.GetClips().Count() == 0) {
				TimelineEvent.Data lEventData = new TimelineEvent.Data(iObject.GetInstanceID());
				TimelineEvent.OnDeleteTrack(lEventData);
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
				TimelineEvent.Data lEventData = new TimelineEvent.Data(lObjectID);
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
				TimelineEvent.OnAddTrack(new TimelineEvent.Data(lID));
			}
		}
		return lTrack;
	}

	public GameObject GetObjectFromID(int iID)
	{
		KeyValuePair<int, AnimationTrack> lBinding = mBindings.FirstOrDefault(iBinding => iBinding.Key == iID);
		if (lBinding.Value != null) {
			return (GameObject)mDirector.GetGenericBinding(lBinding.Value);
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