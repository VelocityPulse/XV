using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

using AnimAction = System.Predicate<AnimationInfo>;

public class ActionAsset : PlayableAsset
{
	public AnimAction AttachedAction { get; set; }
	public ActionTrack Track { get; set; }

	public override Playable CreatePlayable(PlayableGraph iGraph, GameObject iOwner)
	{
		ScriptPlayable<ActionBehaviour> lPlayable = ScriptPlayable<ActionBehaviour>.Create(iGraph);
		ActionBehaviour lBehaviour = lPlayable.GetBehaviour();
		lBehaviour.AttachedAction = AttachedAction;
		lBehaviour.Track = Track;
		return lPlayable;
	}

}
