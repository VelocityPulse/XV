﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using AnimAction = System.Predicate<AnimationInfo>;

[TrackClipType(typeof(ActionAsset))]
[TrackBindingType(typeof(GameObject))]
public class ActionTrack : TrackAsset
{
	private Queue<AnimAction> mActions;
	private Queue<AnimationParameters> mParams;

	private static float ACTIONS_LOOP_TIME = 0.2F;

	private new void OnEnable()
	{
		TimelineEvent.PauseEvent += Pause;
		TimelineEvent.StopEvent += Stop;
		TimelineEvent.PlayEvent += Play;
	}

	private void OnDisable()
	{
		TimelineEvent.PauseEvent -= Pause;
		TimelineEvent.StopEvent -= Stop;
		TimelineEvent.PlayEvent -= Play;
	}

	private void Awake()
	{
		mActions = new Queue<AnimAction>();
		mParams = new Queue<AnimationParameters>();
		TimelineManager.Instance.StartCoroutine(ActionQueueCallAsync());
	}

	public void QueueAction(AnimAction iAction, AnimationParameters iParams = null)
	{
		mActions.Enqueue(iAction);
		mParams.Enqueue(iParams);
	}

	private IEnumerator ActionQueueCallAsync()
	{
		while (true) {
			if (mActions.Count > 0) {
				AnimAction lAction = mActions.Dequeue();
				yield return new WaitUntil(() => lAction(GetInfo()));
			}
			else {
				yield return new WaitForSeconds(ACTIONS_LOOP_TIME);
			}
		}
	}

	private AnimationInfo GetInfo()
	{
		AnimationInfo lInfo = new AnimationInfo();
		if (mActions.Count > 0) {
			lInfo.Parameters = mParams.Dequeue();
		}
		return lInfo;
	}
	
	private static void Reset()
	{
		AnimationInfo.sGlobalState = AnimationInfo.State.PLAY;
	}

	private static void Play(TimelineEvent.Data iData)
	{
		Reset();
	}

	private static void Pause(TimelineEvent.Data iData)
	{
		AnimationInfo.sGlobalState = AnimationInfo.State.PAUSE;
	}

	private static void Stop(TimelineEvent.Data iData)
	{
		AnimationInfo.sGlobalState = AnimationInfo.State.STOP;
		TimelineManager.Instance.StartCoroutine(Utils.WaitForAsync(ACTIONS_LOOP_TIME, Reset));
	}
}
