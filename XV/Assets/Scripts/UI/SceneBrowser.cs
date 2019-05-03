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

	[Header("Extern Reference")]
	[Header("Please attach: UIModelManager/UIModelManagerGrid/GridELements")]
	[SerializeField]
	private UIModelManager UILibModel;

	[Header("Intern Reference")]
	[Header("Please attach: SceneBrowser/Middle/FileContainer/GridWithElements")]
	[SerializeField]
	private GameObject UIGridElement;

	[Header("Please attach: SceneBrowser/Bottom/ButtonContainer/OpenFile")]
	[SerializeField]
	private Button OpenFile;

	[Header("Please attach: SceneBrowser/Bottom/ButtonContainer/Cancel")]
	[SerializeField]
	private Button Cancel;

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
		OpenFile.onClick.AddListener(OpenSelectedFile);
		Cancel.onClick.AddListener(HideBrowser);
	}

	// Update the list of element
	private void UpdateFiles()
	{
		//List<string> lDirs = new List<string>(Directory.GetFileSystemEntries(mPath));

		//foreach (string lFile in lDirs) {
		//FileAttributes lAttr = File.GetAttributes(lFile);
		//if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden)
		//	continue;
		//else if ((lAttr & FileAttributes.Directory) == FileAttributes.Directory) {
		//	if (mFileUIParam != null) {
		//		mFileUIParam.Text.color = Utils.ROYAL_BLUE;
		//		mFileUIParam.Icon.sprite = mSpriteFolder;
		//	}
		//} else {
		//	if (mFileUIParam != null) {
		//		mFileUIParam.Text.color = Utils.ROYAL_GREY;
		//		mFileUIParam.Icon.sprite = mSpriteFile;
		//	}
		//}
		//if (mFileUIParam != null)
		//mFileUIParam.Text.text = lFile.Replace(mPath + "/", "");
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

	public void OpenSelectedFile()
	{
		//GameObject[] lAssets = null;
		////string lPathSrc = Path.Combine(mPath, SelectedFile.text);
		////string lPathDst = Path.Combine(mSavedDataPath, SelectedFile.text);

		//// Test if the file exist
		//if (File.Exists(lPathDst)) {
		//	Debug.LogError("[IMPORT MODEL] Error file exist !");
		//	XV_UI.Instance.Notify(NOTIFIER_DURATION, "The file has already been imported.");
		//	return;
		//}

		//// Load AssetBundle to test if the file is correct
		//if ((lAssets = Utils.LoadAssetBundle<GameObject>(lPathSrc, (iErrorMessage) => { XV_UI.Instance.Notify(NOTIFIER_DURATION, iErrorMessage); })) == null)
		//	return;

		//// AssetBundle loading success, and GameObject has been found
		//// So save the AssetBundle into SavedData
		//try {
		//	File.Copy(lPathSrc, lPathDst);
		//} catch (Exception ex) {
		//	Debug.LogError("[IMPORT MODEL] Error:" + ex.Message);
		//	XV_UI.Instance.Notify(NOTIFIER_DURATION, "An error occurred while copying the file");
		//}

		//ModelLoader.Instance.UpdatePool();
		//UILibModel.UpdateAvailableModel();

		//AssetBundle.UnloadAllAssetBundles(true);
		//HideBrowser();
		//XV_UI.Instance.Notify(NOTIFIER_DURATION, "Your file has been imported.");
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
			if (mCanvasGroup != null)
				StartCoroutine(Utils.FadeToAsync(1F, 0.4F, mCanvasGroup));
		}
		// Hide
		else
			HideBrowser();
	}

	public void HideBrowser()
	{
		if (mDisplayed) {
			mDisplayed = false;
			if (mCanvasGroup != null) {
				StartCoroutine(Utils.FadeToAsync(0F, 0.4F, mCanvasGroup, () => {
					ClearFiles();
					gameObject.SetActive(false);
				}));
			}
		}
	}
}