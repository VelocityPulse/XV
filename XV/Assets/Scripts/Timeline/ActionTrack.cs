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

	private void Awake()
	{
		mActions = new Queue<Predicate<float>>();
		TimelineManager.Instance.StartCoroutine(ActionQueueCallAsync());
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
				yield return new WaitUntil(() => { return lAction(1F); });
			}
			else {
				yield return new WaitForSeconds(0.2F);
			}
		}
	}
}
