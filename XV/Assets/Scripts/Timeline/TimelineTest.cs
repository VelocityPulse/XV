using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimelineTest : MonoBehaviour {

	private int mNum = 0;

	private List<GameObject> mObjects;

	private void Start()
	{
		mObjects = new List<GameObject>();
	}

	public void AddToTimeline() {
		GameObject lNewObject = new GameObject("Object" + mNum.ToString());
		lNewObject.AddComponent<Animator>();
		TimelineManager.Instance.AddObject(lNewObject);
		mObjects.Add(lNewObject);
		mNum++;
	}

	public void DeleteFromTimeline() {
		if (mObjects.Count > 0) {
			GameObject lLast = mObjects[mObjects.Count - 1];
			TimelineManager.Instance.RemoveObject(lLast);
			mObjects.RemoveAt(mObjects.Count - 1);
			GameObject.Destroy(lLast);
		}
	}
}
