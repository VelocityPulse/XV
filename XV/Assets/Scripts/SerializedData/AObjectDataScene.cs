using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ObjectDataSceneType
{
	BUILT_IN,

	EXTERN,
}

[Serializable]
public class AObjectDataScene {

	public string Name { get; set; }

	public ObjectDataSceneType Type { get; set; }

	public string PrefabName { get; set; }

	public Vector3 Position { get; set; }

	public Vector3 Rotation { get; set; }

}
