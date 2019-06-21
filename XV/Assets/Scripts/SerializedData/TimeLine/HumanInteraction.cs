using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum HumanInteractionType
{
	MOUNT,

	UNMOUNT,

	TAKE,

	TAKEOFF,

	PUSH,

	STOP_PUSH
}

[Serializable]
public class HumanInteraction
{
	public HumanInteractionType InteractionType { get; set; }

	public string TargetGUID { get; set; }

    public double Time { get; set; }
}
