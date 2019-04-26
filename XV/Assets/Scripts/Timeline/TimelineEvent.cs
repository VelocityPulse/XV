﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineEvent
{
	public enum Source { FROM_WORLD, FROM_UI }
	public class Data
	{
		public int TrackID { get; private set; }
		public Source Source { get; private set; }
		public int ClipIndex { get; set; }
		public double ClipStart { get; set; }
		public double ClipLength { get; set; }

		public Data(int iTrackID, Source iSource)
		{
			TrackID = iTrackID;
			Source = iSource;
		}
	}

	public delegate void TimelineAction(Data iData);
	public static event TimelineAction AddTrackEvent;
	public static event TimelineAction DeleteTrackEvent;
	public static event TimelineAction AddClipEvent;
	public static event TimelineAction DeleteClipEvent;
	public static event TimelineAction ResizeClipEvent;

	public static void OnAddTrack(Data iData)
	{
		if (AddTrackEvent != null) {
			AddTrackEvent(iData);
		}
	}

	public static void OnAddClip(Data iData)
	{
		if (AddClipEvent != null) {
			AddClipEvent(iData);
		}
	}

	public static void OnResizeClip(Data iData)
	{
		if (ResizeClipEvent != null) {
			ResizeClipEvent(iData);
		}
	}
}
