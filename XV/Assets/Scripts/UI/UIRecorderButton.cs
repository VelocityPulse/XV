using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
[RequireComponent(typeof(Image))]
public class UIRecorderButton : MonoBehaviour
{
	private bool mRecording;
	private Image mImage;
	private Text mText;

	private Recorder Recorder { get { return GameManager.Instance.Recorder; } }

	private void Awake()
	{
		mRecording = false;
		mImage = GetComponent<Image>();
		mText = GetComponentInChildren<Text>();
	}

	private void StartRecord()
	{
		Recorder.StartRecord();
		mRecording = true;
		mText.text = "Stop Recording";
		mText.color = Color.white;
		mImage.color = new Color32(188, 0, 0, 100);
	}

	private void StopRecord()
	{
		Recorder.ReleaseRecord();
		mRecording = false;
		mText.text = "Start Recording";
		mText.color = Color.black;
		mImage.color = Color.white;
	}

	public void ToggleRecord()
	{
		if (mRecording) {
			StopRecord();
		}
		else {
			StartRecord();
		}
	}

}
