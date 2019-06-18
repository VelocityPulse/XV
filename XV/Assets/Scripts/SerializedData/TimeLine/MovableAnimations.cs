using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[SerializeField]
public class MovableAnimations
{
	public string EntityGUID { get; set; }

	public bool IsMoveAnim { get; set; }

	public bool IsRotateAnim { get; set; }

	public Vector3 TargetPosition { get; set; }

	public Vector3 TargetRotation { get; set; }

	public double Time { get; set; }
}