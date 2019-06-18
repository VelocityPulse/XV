using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public sealed class ObjectDataScene : AObjectDataScene
{
	// This variable shouldn't be edited manually!
	public string GUID { get; set; }

	public Vector3 Scale { get; set; }

	public ObjectDataScene()
	{
		if (GUID == null)
			GUID = Guid.NewGuid().ToString();
	}
}
