using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimelineControls : MonoBehaviour
{
	[SerializeField]
	private Slider slider;

	[SerializeField]
	private Transform timeScale;

	[SerializeField]
	private Text maxDurationText;

	private bool mIsPlaying;
	private bool mLooping;
	private GameObject mTimeScaleLinePrefab;

	private void Start()
	{
		mIsPlaying = false;
		mLooping = false;
		slider.value = slider.minValue;
		mTimeScaleLinePrefab = Resources.Load<GameObject>(GameManager.UI_TEMPLATE_PATH + "UITimeScaleLine");
	}

	private void Update()
	{
		if (mIsPlaying) {
			slider.value += Time.deltaTime;
		}
		if (slider.value == slider.maxValue && mLooping) {
			slider.value = slider.minValue;
		}
		float lDuration = (float)TimelineManager.Instance.Duration;
		if (!Mathf.Approximately(lDuration, slider.maxValue)) {
			SetTimeScale(lDuration.ToString());
		}
	}

	public void SetIsPlaying(bool iIsPlaying)
	{
		mIsPlaying = iIsPlaying;
	}

	public void SetLooping(bool iLooping)
	{
		mLooping = iLooping;
	}

	public void Reset()
	{
		slider.value = slider.minValue;
		mIsPlaying = false;
	}

	public void SetTimeScale(string iStringValue)
	{
		float lValue = float.Parse(iStringValue);
		if (iStringValue != null) {
			maxDurationText.text = iStringValue.Substring(0, Mathf.Min(iStringValue.Length, 4)) + "s";
		}
		foreach (Transform lChild in timeScale) {
			Destroy(lChild.gameObject);
		}
		for (int lCount = 0; lCount < lValue; lCount++) {
			Instantiate(mTimeScaleLinePrefab, timeScale);
		}
		slider.maxValue = lValue;
		TimelineManager.Instance.Rebuild();
	}
}
