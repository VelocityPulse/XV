using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

using AnimAction = System.Predicate<object>;

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
				List<object> lParams = mParamsSets.Count > 0 ? mParamsSets.Dequeue() : null;

				for (int lIndex = 0; lIndex < lActions.Count; lIndex++) {

					AnimAction lAction = lActions[lIndex];
					object lParam = GetParam(lParams, lIndex);

					if (lAction != null) {
						yield return new WaitUntil(() => lAction(lParam));
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

	private object GetParam(List<object> iParams, int iIndex)
	{
		if (iParams != null) {
			if (iIndex < iParams.Capacity) {
				return iParams[iIndex];
			}
		}
		return null;
	}
	
	private static void Reset()
	{
		TimelineManager.sGlobalState = TimelineManager.State.PLAY;
	}

	private static void Play(TimelineEvent.Data iData)
	{
		Reset();
	}

	private static void Pause(TimelineEvent.Data iData)
	{
		TimelineManager.sGlobalState = TimelineManager.State.PAUSE;
	}

	private static void Stop(TimelineEvent.Data iData)
	{
		TimelineManager.sGlobalState = TimelineManager.State.STOP;
		TimelineManager.Instance.StartCoroutine(Utils.WaitForAsync(ACTIONS_LOOP_TIME, Reset));
	}
}
