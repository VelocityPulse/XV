using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UITimelineControls : MonoBehaviour
{
	[SerializeField]
	private Slider slider;
	private bool mIsPlaying;
	private bool mLooping;

	private void Start()
	{
		mIsPlaying = false;
		mLooping = false;
		slider.value = slider.minValue;
	}

	private void Update()
	{
		if (mIsPlaying) {
			slider.value += Time.deltaTime;
		}
		if (slider.value == slider.maxValue && mLooping) {
			slider.value = slider.minValue;
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
}
