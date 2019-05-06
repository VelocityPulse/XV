using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public sealed class SceneBrowser : MonoBehaviour
{
	private const float NOTIFIER_DURATION = 1F;

	private string mSavedScenePath;

	private bool mDisplayed;

	private CanvasGroup mCanvasGroup;

	private GameObject mFileUITemplate;

	private UIElementGridParam mFileUIParam;

	// Assets

	private Sprite mSpriteFile;

	private Sprite mSpriteFolder;

	[Header("Intern Reference")]
	[Header("Please attach: SceneBrowser/Middle/FileContainer/GridWithElements")]
	[SerializeField]
	private GameObject UIGridElement;

	[Header("Please attach: SceneBrowser/Bottom/ButtonContainer/Cancel")]
	[SerializeField]
	private Button CancelButton;

	[Header("Please attach: SceneBrowser/Bottom/ButtonContainer/OpenFile")]
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
		mSpriteFolder = Resources.Load<Sprite>(GameManager.UI_ICON_PATH + "FileBrowser/Folder");

		// Button setting
		OpenButton.onClick.AddListener(OnClickOpen);
		CancelButton.onClick.AddListener(OnClickCancel);

		DisplayBrowser();
	}

	// Update the list of element
	private void UpdateFiles()
	{

		GameObject lElement = Instantiate(mFileUITemplate, UIGridElement.transform);
		lElement.GetComponent<Button>().onClick.AddListener(() => { ElementSelection(lElement); });
	}

	private void ClearFiles()
	{
		foreach (Transform lChild in UIGridElement.transform) {
			Destroy(lChild.gameObject);
		}
	}

	public void ElementSelection(GameObject iButtonClicked = null)
	{

	}

	private void OnClickOpen()
	{

	}

	private void OnClickCancel()
	{
		HideBrowser();
	}

	// This function toogle the display of the UI
	public void DisplayBrowser()
	{
		// Display
		if (!mDisplayed) {
			gameObject.SetActive(true);
			UpdateFiles();
			mDisplayed = true;
			gameObject.SetActive(true);
			if (mCanvasGroup != null) {
				StartCoroutine(Utils.WaitForAsync(1F, () => {
					StartCoroutine(Utils.FadeToAsync(1F, .5F, mCanvasGroup));
				}));
			}
		}
		// Hide
		else
			HideBrowser();
	}

	public void HideBrowser()
	{
		//if (mDisplayed) {
		//	mDisplayed = false;
		//	if (mCanvasGroup != null) {
		//		StartCoroutine(Utils.FadeToAsync(0F, 0.4F, mCanvasGroup, () => {
		//			ClearFiles();
		//			gameObject.SetActive(false);
		//		}));
		//	}
		//}
	}
}