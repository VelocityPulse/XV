using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public sealed class UISceneBrowser : MonoBehaviour
{
	[SerializeField]
	private UINewSceneTitle newSceneTitle;

	private const float NOTIFIER_DURATION = 1F;

	private string mSavedScenePath;

	private List<string> mFullPathFiles;

	private List<string> mFileNames;

	private bool mDisplayed;

	private CanvasGroup mCanvasGroup;

	private GameObject mFileUITemplate;

	private UIElementGridParam mFileUIParam;

	private UIElementGridParam mLastFileUIParamSelected;

	// Assets

	private Sprite mSpriteFile;

	[Header("Intern Reference")]
	[Header("SceneBrowser/Middle/FileContainer/GridWithElements")]
	[SerializeField]
	private GameObject UIGridElement;

	[Header("SceneBrowser/Bottom/ButtonContainer/NewScene")]
	[SerializeField]
	private Button NewSceneButton;

	[Header("SceneBrowser/Bottom/ButtonContainer/OpenFile")]
	[SerializeField]
	private Button OpenButton;

	// Use this for initialization
	private void Start()
	{
		mSavedScenePath = Application.dataPath + DataScene.RES_PATH;

		// Init some variables
		if ((mCanvasGroup = GetComponent<CanvasGroup>()) != null)
			mCanvasGroup.alpha = 0F;

		gameObject.SetActive(false);
		mDisplayed = false;

		// Load Prefab
		mFileUITemplate = Resources.Load<GameObject>(GameManager.UI_TEMPLATE_PATH + "UIFileElementGrid");
		// Get the Script that give acces to settings
		mFileUIParam = mFileUITemplate.GetComponent<UIElementGridParam>();

		// Load Assets
		mSpriteFile = Resources.Load<Sprite>(GameManager.UI_ICON_PATH + "FileBrowser/File");

		// Button setting
		OpenButton.onClick.AddListener(OnClickOpen);
		NewSceneButton.onClick.AddListener(OnClickNewScene);

		DisplayBrowser();
	}

	// Update the list of element
	private void UpdateFiles()
	{
		mFileNames = new List<string>();
		mFullPathFiles = new List<string>(Directory.GetFileSystemEntries(mSavedScenePath));

		foreach (string lFile in mFullPathFiles) {
			FileAttributes lAttr = File.GetAttributes(lFile);
			if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden)
				continue;
			else if (!lFile.EndsWith(".xml"))
				continue;

			if ((lAttr & FileAttributes.Normal) == FileAttributes.Normal) {
				if (mFileUIParam != null) {
					mFileUIParam.Text.color = Utils.ROYAL_BLUE;
					mFileUIParam.Icon.sprite = mSpriteFile;
				}
			}
			if (mFileUIParam != null) {
				string lFileName = lFile.Replace(mSavedScenePath, "");
				mFileNames.Add(lFileName);
				mFileUIParam.Text.text = lFileName;
			}
			GameObject lElement = Instantiate(mFileUITemplate, UIGridElement.transform);
			lElement.GetComponent<Button>().onClick.AddListener(() => { OnClickElement(lElement); });
		}
	}

	private void ClearFiles()
	{
		foreach (Transform lChild in UIGridElement.transform) {
			Destroy(lChild.gameObject);
		}
	}

	public void OnClickElement(GameObject iButtonClicked = null)
	{
		mLastFileUIParamSelected = iButtonClicked.GetComponent<UIElementGridParam>();
	}

	private void OnClickOpen()
	{
		if (mLastFileUIParamSelected == null)
			return;

		DataScene lDataScene = DataScene.Unserialize(mLastFileUIParamSelected.Text.text);
		GameManager.Instance.LoadScene(lDataScene);
		XV_UI.Instance.UnlockGUI();
		HideBrowser();
	}

	private void OnClickNewScene()
	{
		HideBrowser();
		newSceneTitle.StartForResult((iTypeResult, iValue) => {
			if (iTypeResult == UISceneTitleResult.OK_RESULT) {

				DataScene lDataScene = new DataScene();
				lDataScene.SceneName = iValue;
				lDataScene.Serialize();
				DisplayBrowser();

			} else if (iTypeResult == UISceneTitleResult.CANCEL_RESULT) {
				DisplayBrowser();
			} else {
				Debug.LogError(iTypeResult + " : " + mFileNames.ToArray() + " " + iValue);
				Utils.PrintStackTrace();
			}


		}, mFileNames.ToArray());
	}

	public void DisplayBrowser()
	{
		// Display
		if (!mDisplayed) {
			gameObject.SetActive(true);
			UpdateFiles();
			mDisplayed = true;
			gameObject.SetActive(true);
			if (mCanvasGroup != null)
				StartCoroutine(Utils.FadeToAsync(1F, 0.2F, mCanvasGroup));
		}
	}


	public void HideBrowser()
	{
		if (mDisplayed) {
			mDisplayed = false;
			if (mCanvasGroup != null) {
				StartCoroutine(Utils.FadeToAsync(0F, 0.2F, mCanvasGroup, () => {
					ClearFiles();
					gameObject.SetActive(false);
				}));
			}
		}
	}
}