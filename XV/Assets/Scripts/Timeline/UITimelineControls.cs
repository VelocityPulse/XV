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

	private bool mIsPlaying;
	private bool mLooping;
	private int mCurrentTimeScale;
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
		double lDuration = TimelineManager.Instance.Duration;
		int lIntDuration = (int)Mathf.Ceil((float)lDuration);
		SetTimeScale(lIntDuration.ToString());
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
		int lIntValue = int.Parse(iStringValue);
		if (mCurrentTimeScale != lIntValue) {
			foreach (Transform lChild in timeScale) {
				Destroy(lChild.gameObject);
			}
			for (int lCount = 0; lCount < lIntValue; lCount++) {
				Instantiate(mTimeScaleLinePrefab, timeScale);
			}
			slider.maxValue = lIntValue;
		}
		TimelineManager.Instance.Rebuild();
	}
}
