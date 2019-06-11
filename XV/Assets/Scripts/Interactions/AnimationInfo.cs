using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInfo
{
	public enum State { PLAY, PAUSE, STOP };

	/// <summary>
	/// Global state of the all animations.
	/// </summary>
	public static State sGlobalState;

	/// <summary>
	/// The parameters use by the Implementation of the Animation.
	/// </summary>
	public AnimationParameters Parameters
	{
		get { return (AnimationParameters)tag; }
	}

	public object tag;
}