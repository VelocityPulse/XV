using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

using AnimAction = System.Predicate<AnimationInfo>;

public class ActionBehaviour : PlayableBehaviour
{
	public AnimAction AttachedAction { get; set; }
	public ActionTrack Track { get; set; }

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		Track.QueueAction(AttachedAction);
	}
}
