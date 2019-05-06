using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

public class ActionAsset : PlayableAsset
{
   public override Playable CreatePlayable (PlayableGraph iGraph, GameObject iOwner)
   {
	   Playable lPlayable = ScriptPlayable<ActionBehaviour>.Create(iGraph);
	   return lPlayable;
   }

}
