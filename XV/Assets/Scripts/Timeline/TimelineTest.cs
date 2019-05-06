using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineTest : MonoBehaviour 
{
	public AnimationClip clip;

	public void AddClip()
	{
		if (clip != null) {
			GameObject lObject = new GameObject("TimelineTestObject");
			lObject.AddComponent<Animator>();
			TimelineManager.Instance.AddClip(lObject, clip);
		}
	}
}
