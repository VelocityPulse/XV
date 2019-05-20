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

	[Header("SceneBrowser/Bottom/ButtonContainer1/NewScene")]
	[SerializeField]
	private Button NewSceneButton;

	[Header("SceneBrowser/Bottom/ButtonContainer1/OpenFile")]
	[SerializeField]
	private Button OpenButton;

	[Header("SceneBrowser/Bottom/ButtonContainer2/Cancel")]
	[SerializeField]
	private Button CancelButton;

	[Header("SceneBrowser/Bottom/ButtonContainer2/Remove")]
	[SerializeField]
	private Button RemoveButton;

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
		CancelButton.onClick.AddListener(OnClickCancel);
		RemoveButton.onClick.AddListener(OnClickRemove);


		DisplayBrowser();
	}

	private void Update()
	{
		if (mLastFileUIParamSelected == null)
			RemoveButton.interactable = false;
		else
			RemoveButton.interactable = true;
	}

	// Update the list of element
	private void UpdateFiles()
	{
		ClearFiles();
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
					mFileUIParam.Text.color = Color.white;
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

		if (GameManager.Instance.CurrentDataScene == null)
			CancelButton.interactable = false;
		else
			CancelButton.interactable = true;
	}

	private void ClearFiles()
	{
		foreach (Transform lChild in UIGridElement.transform) {
			Destroy(lChild.gameObject);
		}
	}

	private void RemoveScene()
	{
		if (mLastFileUIParamSelected == null) {
			File.Delete(mSavedScenePath + mLastFileUIParamSelected.Text.text);

			if (GameManager.Instance.CurrentDataScene != null) {
				string lSelectedDataSceneName = mLastFileUIParamSelected.Text.text;
				string lCurrentDataSceneName = GameManager.Instance.CurrentDataScene.SceneName;
				if (lSelectedDataSceneName == lCurrentDataSceneName)
					GameManager.Instance.UnloadScene();
			}

			UpdateFiles();
			mLastFileUIParamSelected = null;
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
		HideBrowser(true);
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

	private void OnClickCancel()
	{
		HideBrowser();
	}

	private void OnClickRemove()
	{
		HideBrowser(true);
		XV_UI.Instance.UIConfirmPopup.StartForResult((iResult) => {
			if (iResult == UIConfirmPopupResult.LEFT_RESULT)
				DisplayBrowser();
			else if (iResult == UIConfirmPopupResult.RIGHT_RESULT) {
				RemoveScene();
				DisplayBrowser();
			} else
				Utils.PrintStackTrace();
		}, "Confirm", "No", "Yes");
	}

	public void DisplayBrowser()
	{
		if (!mDisplayed) {
			gameObject.SetActive(true);
			UpdateFiles();
			mDisplayed = true;
			XV_UI.Instance.LockGUI();
			gameObject.SetActive(true);
			if (mCanvasGroup != null)
				StartCoroutine(Utils.FadeToAsync(1F, 0.2F, mCanvasGroup, () => {
					GameManager.Instance.KeyboardDeplacementActive = false;
				}));
		}
	}

	public void HideBrowser(bool iKeepKeyboardLocked = false)
	{
		if (mDisplayed) {
			mDisplayed = false;
			if (mCanvasGroup != null) {
				StartCoroutine(Utils.FadeToAsync(0F, 0.2F, mCanvasGroup, () => {
					ClearFiles();
					gameObject.SetActive(false);
					if (!iKeepKeyboardLocked) {
						XV_UI.Instance.UnlockGUI();
						GameManager.Instance.KeyboardDeplacementActive = true;
					}
				}));
			}
		}
	}
}