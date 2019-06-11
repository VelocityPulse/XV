using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

using AnimAction = System.Predicate<AnimationInfo>;

public class ActionBehaviour : PlayableBehaviour
{
	public List<AnimAction> Actions { get; set; }
	public List<object> Parameters { get; set; }
	public ActionTrack Track { get; set; }

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		Track.QueueActions(Actions, Parameters);
	}
}
