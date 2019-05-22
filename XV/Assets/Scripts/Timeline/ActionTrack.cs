using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Timeline;

[TrackClipType(typeof(ActionAsset))]
[TrackBindingType(typeof(GameObject))]
public class ActionTrack : TrackAsset
{
	private Queue<Predicate<float>> mActions;
	private static bool sPaused;
	private static bool sStopped;

	public static float PAUSE = -1F;
	public static float STOP = -2F;

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
		mActions = new Queue<Predicate<float>>();
		TimelineManager.Instance.StartCoroutine(ActionQueueCallAsync());
		sPaused = false;
		sStopped = false;
	}

	public void QueueAction(Predicate<float> iAction)
	{
		mActions.Enqueue(iAction);
	}

	private IEnumerator ActionQueueCallAsync()
	{
		while (true) {
			if (mActions.Count > 0) {
				Predicate<float> lAction = mActions.Dequeue();
				yield return new WaitUntil(() => lAction(GetState()));
			}
			else {
				yield return new WaitForSeconds(ACTIONS_LOOP_TIME);
			}
		}
	}

	private float GetState()
	{
		if (sStopped) {
			return STOP;
		}
		if (sPaused) {
			return PAUSE;
		}
		// This is supposed to be the speed
		// Should be improved
		return 1F;
	}
	
	private static void Reset()
	{
		sPaused = false;
		sStopped = false;
	}

	private static void Play(TimelineEvent.Data iData)
	{
		Reset();
	}

	private static void Pause(TimelineEvent.Data iData)
	{
		sPaused = true;
	}

	private static void Stop(TimelineEvent.Data iData)
	{
		sStopped = true;
		TimelineManager.Instance.StartCoroutine(Utils.WaitForAsync(ACTIONS_LOOP_TIME, Reset));
	}
}
