using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActionBehaviour : PlayableBehaviour
{
	public Action AttachedAction { get; set; }

	public override void OnBehaviourPlay(Playable playable, FrameData info)
	{
		AttachedAction();
	}
}
