using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBubbleInfo : MonoBehaviour
{
	public static string TAG = "BubbleInfo";

	private bool mDisplayed;
	private CanvasGroup mCanvasGroup;

	// Use this for initialization
	private void Start()
	{
		mCanvasGroup = GetComponent<CanvasGroup>();
		mCanvasGroup.alpha = 0F;
		enabled = false;
	}

	// Update is called once per frame
	private void Update()
	{
		if (mDisplayed) {
			Quaternion lLookAt = Quaternion.LookRotation(
				transform.position - Camera.main.transform.position);

			transform.rotation = Quaternion.Slerp(
				transform.rotation, lLookAt, Time.deltaTime * 2);
		}
	}

	public void Display()
	{
		mDisplayed = true;
		enabled = true;
		StartCoroutine(FadeTo(1F, 0.4F));
	}

	public void Hide()
	{
		mDisplayed = false;
		StartCoroutine(FadeTo(0F, 0.4F));
	}


	IEnumerator FadeTo(float iValue, float iTime)
	{
		float lAlpha = mCanvasGroup.alpha;

		for (float lTime = 0F; lTime < 1F; lTime += Time.deltaTime / iTime) {
			float newAlpha = Mathf.SmoothStep(lAlpha, iValue, lTime);
			mCanvasGroup.alpha = newAlpha;
			yield return null;
		}
	}
}
