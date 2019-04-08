using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public enum ObjectDataSceneType {

	BUILT_IN,

	EXTERN

}

[Serializable]
public sealed class ObjectDataScene {

	public string Name { get; set; }

	public ObjectDataSceneType Type { get; set; }

	public Vector3 Position { get; set; }

	public Vector3 Rotation { get; set; }

	public Vector3 Scale { get; set; }

}
