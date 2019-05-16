using UnityEditor.Media;
using UnityEngine;
using System.IO;

#if UNITY_EDITOR_OSX
    using UnityEngine.Collections;
#endif

using System.Collections;

public class Recorder : MonoBehaviour
{

	//https://docs.unity3d.com/ScriptReference/Camera.Render.html
	//https://docs.unity3d.com/ScriptReference/Camera.CopyFrom.html
	//https://docs.unity3d.com/ScriptReference/Media.MediaEncoder.html
	//https://answers.unity.com/questions/22954/how-to-save-a-picture-take-screenshot-from-a-camer.html

	public const string RES_PATH = "/Resources/RecordedVideo/";

	private int mWidth;
	private int mHeight;

	private bool mEncode;

	private MediaEncoder mEncoder;

	private VideoTrackAttributes mVideoAttr;
	private Camera mCamera;

	private void Start()
	{
		mWidth = Camera.main.pixelWidth;
		mHeight = Camera.main.pixelHeight;
		mCamera = Camera.main;
	}

	public void StartRecord(string iPath = null)
	{
		Debug.Log("Start record");
		if (string.IsNullOrEmpty(iPath))
			iPath = Application.dataPath + RES_PATH + System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") + ".mp4";
		
		Utils.CreateFolder(iPath);
		Debug.Log("Recording to : " + iPath);
		Init(iPath);
		mEncode = true;
	}

	public void ReleaseRecord()
	{
		if (mEncode) {
			Debug.Log("Stop record");
			mEncode = false;
			mEncoder.Dispose();
		}
	}

	private void Init(string iPath)
	{
		VideoTrackAttributes videoAttr = new VideoTrackAttributes {
			frameRate = new MediaRational((int)((1 / Time.smoothDeltaTime)) / 2),
			width = (uint)mWidth,
			height = (uint)mHeight,
			includeAlpha = false
		};

		Debug.Log((int)((1 / Time.smoothDeltaTime)) / 2);

		mEncoder = new MediaEncoder(iPath, videoAttr, null);
	}

	private void Update()
	{
		if (!mEncode)
			return;

		//Debug.Log("Add frame");
		Texture2D lTex = RTImage();
		mEncoder.AddFrame(lTex);

		Object.Destroy(lTex);
		lTex = null;
	}


	private Texture2D RTImage()
	{
		Rect rect = new Rect(0, 0, mWidth, mHeight);
		RenderTexture renderTexture = new RenderTexture(mWidth, mHeight, 24);
		Texture2D screenShot = new Texture2D(mWidth, mHeight, TextureFormat.RGBA32, false);


		mCamera.targetTexture = renderTexture;
		mCamera.Render();

		RenderTexture.active = renderTexture;
		screenShot.ReadPixels(rect, 0, 0);

		mCamera.targetTexture = null;
		RenderTexture.active = null;

		Destroy(renderTexture);
		renderTexture = null;
		return screenShot;
	}
}