using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class TimelineEvent
{
	public class Data
	{
		public int TrackID { get; private set; }
		public TimelineData.EventType Type { get; set; }
		public int ClipIndex { get; set; }
		public double ClipStart { get; set; }
		public double ClipLength { get; set; }
		public string ClipName { get; set; }

		public Data(int iTrackID)
		{
			TrackID = iTrackID;
		}
	}

	public delegate void TimelineAction(Data iData);

	// Manager -> UI events
	public static event TimelineAction AddTrackEvent;
	public static event TimelineAction DeleteTrackEvent;
	public static event TimelineAction AddClipEvent;
	public static event TimelineAction DeleteClipEvent;
	public static event TimelineAction ResizeClipEvent;

	// UI -> Manager events
	public static event TimelineAction UIDeleteClipEvent;
	public static event TimelineAction UIResizeClipEvent;

	// Other events
	public static event TimelineAction PauseEvent;
	public static event TimelineAction StopEvent;
	public static event TimelineAction PlayEvent;
	public static event TimelineAction GetDescriptionEvent;
	public static event TimelineAction SetDescriptionEvent;

	public static void OnAddTrack(Data iData)
	{
		if (AddTrackEvent != null) {
			AddTrackEvent(iData);
		}
	}

	public static void OnDeleteTrack(Data iData)
	{
		if (DeleteTrackEvent != null) {
			DeleteTrackEvent(iData);
		}
	}

	public static void OnAddClip(Data iData)
	{
		if (AddClipEvent != null) {
			AddClipEvent(iData);
		}
	}

	public static void OnDeleteClip(Data iData)
	{
		if (DeleteClipEvent != null) {
			DeleteClipEvent(iData);
		}
	}

	public static void OnResizeClip(Data iData)
	{
		if (ResizeClipEvent != null) {
			ResizeClipEvent(iData);
		}
	}

	public static void OnUIResizeClip(Data iData)
	{
		if (UIResizeClipEvent != null) {
			UIResizeClipEvent(iData);
		}
	}

	public static void OnUIDeleteClip(Data iData)
	{
		if (UIDeleteClipEvent != null) {
			UIDeleteClipEvent(iData);
		}
	}

	public static void OnPause(Data iData)
	{
		if (PauseEvent != null) {
			PauseEvent(iData);
		}
	}

	public static void OnStop(Data iData)
	{
		if (StopEvent != null) {
			StopEvent(iData);
		}
	}

	public static void OnPlay(Data iData)
	{
		if (PlayEvent != null) {
			PlayEvent(iData);
		}
	}

	public static void OnGetDescription(Data iData)
	{
		if (GetDescriptionEvent != null) {
			GetDescriptionEvent(iData);
		}
	}

	public static void OnSetDescription(Data iData)
	{
		if (SetDescriptionEvent != null) {
			SetDescriptionEvent(iData);
		}
	}
}
