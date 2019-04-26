﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineUtility : MonoBehaviour
{
	public static double ClipSizeToDuration(float iClipSize, float iTrackSize)
	{
		double lTotalDuration = TimelineManager.Instance.Duration;
		double lDuration = lTotalDuration * iClipSize / iTrackSize;
		return lDuration;
	}

	public static float ClipDurationToSize(double iClipLength, float iTrackSize)
	{
		double lTotalDuration = TimelineManager.Instance.Duration;
		float lSize = ((float)iClipLength * iTrackSize) / (float)lTotalDuration;
		return lSize;
	}

	public static float ClipStartToPosition(double iClipStart, Vector2 iTrackLimits)
	{
		double lTotalDuration = TimelineManager.Instance.Duration;
		float lPosition = (float)iClipStart * (iTrackLimits.y - iTrackLimits.x) / ((float)lTotalDuration) + iTrackLimits.x;
		return lPosition;
	}

	public static double ClipPositionToStart(float iClipPosition, Vector2 iTrackLimits)
	{
		double lTotalDuration = TimelineManager.Instance.Duration;
		double lStart = (iClipPosition - iTrackLimits.x) / (iTrackLimits.y - iTrackLimits.x) * lTotalDuration;
		return lStart;
	}
}
