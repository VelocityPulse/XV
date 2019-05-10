using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActionBehaviour : PlayableBehaviour
{
	public Predicate<float> AttachedAction { get; set; }

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		TimelineManager.Instance.StartCoroutine(ActionCallAsync());
		//AttachedAction(1F);
	}

	private IEnumerator ActionCallAsync()
	{
        //yield return new WaitForSeconds(0.2F);
        yield return new WaitUntil(() => { return AttachedAction(1F); });
	}
}
