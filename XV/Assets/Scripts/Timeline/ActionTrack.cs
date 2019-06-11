using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using AnimAction = System.Predicate<AnimationInfo>;

[TrackClipType(typeof(ActionAsset))]
[TrackBindingType(typeof(GameObject))]
public class ActionTrack : TrackAsset
{
	private Queue<List<AnimAction>> mActionsSets;
	private Queue<List<object>> mParamsSets;

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
		mActionsSets = new Queue<List<AnimAction>>();
		mParamsSets = new Queue<List<object>>();
		TimelineManager.Instance.StartCoroutine(ActionQueueCallAsync());
	}

	public void QueueActions(List<AnimAction> iAction, List<object> iParams = null)
	{
		mActionsSets.Enqueue(iAction);
		mParamsSets.Enqueue(iParams);
	}

	private IEnumerator ActionQueueCallAsync()
	{
		while (true) {
			if (mActionsSets.Count > 0) {
				List<AnimAction> lActions = mActionsSets.Dequeue();
				for (int lIndex = 0; lIndex < lActions.Count; lIndex++) {
					AnimAction lAction = lActions[lIndex];
					if (lAction != null) {
						AnimationInfo lInfo = GetInfo(lIndex);
						yield return new WaitUntil(() => lAction(lInfo));
					}
					else {
						Debug.LogError("An error occured while trying to execute a Timeline Action");
					}
				}
			}
			else {
				yield return new WaitForSeconds(ACTIONS_LOOP_TIME);
			}
		}
	}

	private AnimationInfo GetInfo(int iIndex)
	{
		AnimationInfo lInfo = new AnimationInfo();
		if (mParamsSets.Count > 0) {
			List<object> lParams = mParamsSets.Dequeue();
			lInfo.tag = lParams[iIndex];
		} else
			lInfo.tag = new AnimationParameters();
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
