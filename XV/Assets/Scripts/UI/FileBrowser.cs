using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using UnityEngine;
using UnityEngine.UI;

// Finalize the OpenSelectedFile function - update pool, etc ...
// Update BuildObject function with AssetBundle

public sealed class FileBrowser : MonoBehaviour
{
    private const string SELECTION_FIELD = "Please select a model file to open";

    private readonly string STARTING_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

    private readonly Color32 ROYAL_BLUE = new Color32(65, 105, 255, 255);

    private readonly Color32 ROYAL_GREY = new Color32(80, 80, 80, 255);

	private string mSavedDataPath;

    private bool mDisplayed;

    private CanvasGroup mCanvasGroup;

    private GameObject mFileUITemplate;

    private UIElementGridParam mFileUIParam;

    private string mPath;

    // Assets

    private Sprite mSpriteFile;

    private Sprite mSpriteFolder;

    [Header("Extern Reference")]
    [Header("Please attach: UIModelManager/UIModelManagerGrid/GridELements")]
    [SerializeField]
    private UIModelManager UILibModel;

    [Header("Intern Reference")]
    [Header("Please attach: FileBrowser/Middle/FileContainer/GridWithElements")]
    [SerializeField]
    private GameObject UIGridElement;

    [Header("Please attach: FileBrowser/Top/ButtonContainer/GoBackPath")]
    [SerializeField]
    private Button GoBack;

    [Header("Please attach: FileBrowser/Top/ButtonContainer/GoToHome")]
    [SerializeField]
    private Button GoHome;

    [Header("Please attach: FileBrowser/Bottom/ButtonContainer/OpenFile")]
    [SerializeField]
    private Button OpenFile;

    [Header("Please attach: FileBrowser/Bottom/ButtonContainer/Cancel")]
    [SerializeField]
    private Button Cancel;

    [Header("Please attach: FileBrowser/Top/PWD/Text")]
    [SerializeField]
    private Text PWD;

    [Header("Please attach: FileBrowser/Bottom/SelectedFile/Text")]
    [SerializeField]
    private Text SelectedFile;

    // Use this for initialization
    private void Start()
    {
        mSavedDataPath = Application.dataPath + "/Resources/SavedData/Models/";
        // Init some variables
        mPath = STARTING_PATH;
        mCanvasGroup = GetComponent<CanvasGroup>();
        mCanvasGroup.alpha = 0F;
        mCanvasGroup.blocksRaycasts = false;
        enabled = false;
        mDisplayed = false;
        // Init selected file text
        SelectedFile.text = SELECTION_FIELD;
        SelectedFile.color = ROYAL_GREY;

        // Load Prefab
        mFileUITemplate = Resources.Load<GameObject>(GameManager.UITemplatePath + "UIFileElementGrid");
        // Get the Script that give acces to settings
        mFileUIParam = mFileUITemplate.GetComponent<UIElementGridParam>();

        // Load Assets
        mSpriteFile = Resources.Load<Sprite>(GameManager.UIIconPath + "FileBrowser/File");
        mSpriteFolder = Resources.Load<Sprite>(GameManager.UIIconPath + "FileBrowser/Folder");

        // Button setting
        GoBack.onClick.AddListener(GoToParentDir);
        GoHome.onClick.AddListener(GoToHome);
        OpenFile.onClick.AddListener(OpenSelectedFile);
        Cancel.onClick.AddListener(HideBrowser);
    }

    // Update the list of element, which are file in FILE mode or GameObject in ASSET_BUNDLE mode
    // iImportGameObject : The list of GameObject precedently import from an AssetBundle
    private void UpdateFiles(GameObject[] iImportGameObject = null)
    {
        PWD.text = "Path:" + mPath.Replace(STARTING_PATH, "");
        List<string> lDirs = new List<string>(Directory.GetFileSystemEntries(mPath));

        foreach (string lFile in lDirs) {
            FileAttributes lAttr = File.GetAttributes(lFile);
            if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden)
                continue;
            else if ((lAttr & FileAttributes.Directory) == FileAttributes.Directory) {
                if (mFileUIParam != null) {
                    mFileUIParam.GetText().color = ROYAL_BLUE;
                    mFileUIParam.GetImage().sprite = mSpriteFolder;
                }
            } else {
                if (mFileUIParam != null) {
                    mFileUIParam.GetText().color = ROYAL_GREY;
                    mFileUIParam.GetImage().sprite = mSpriteFile;
                }
            }
            if (mFileUIParam != null)
                mFileUIParam.GetText().text = lFile.Replace(mPath + "/", "");
            GameObject lElement = Instantiate(mFileUITemplate, UIGridElement.transform);
            lElement.GetComponent<Button>().onClick.AddListener(() => { ElementSelection(lElement); });
        }
    }

    private void ClearFiles()
    {
        foreach (Transform lChild in UIGridElement.transform) {
            Destroy(lChild.gameObject);
        }
    }

    public void ElementSelection(GameObject iButtonClicked = null)
    {

        if (iButtonClicked) {
            Text lButtonText = iButtonClicked.GetComponentInChildren<Text>();
            if (lButtonText != null) {
                FileAttributes lAttr = File.GetAttributes(Path.Combine(mPath, lButtonText.text));
                if ((lAttr & FileAttributes.Directory) != FileAttributes.Directory)
                    SelectedFile.text = lButtonText.text;
                else {
                    mPath = Path.Combine(mPath, lButtonText.text);
                    ClearFiles();
                    UpdateFiles();
                }
            }
        }
    }

    public void GoToParentDir()
    {
        if (mPath == STARTING_PATH)
            return;
        mPath = Directory.GetParent(mPath).FullName;
        ClearFiles();
        UpdateFiles();
    }

    public void GoToHome()
    {
        mPath = STARTING_PATH;
        ClearFiles();
        UpdateFiles();
    }

    public void OpenSelectedFile()
    {
        GameObject[] lAssets = null;
        string lPathSrc = Path.Combine(mPath, SelectedFile.text);
        string lPathDst = Path.Combine(mSavedDataPath, SelectedFile.text);

        // Test if the file exist
        if (File.Exists(lPathDst)) {
            Debug.LogError("[IMPORT MODEL] Error file exist !");
            // Warn user with notifier (TODO)
            return ;
        }

        // Load AssetBundle
        if ((lAssets = Utils.LoadAssetBundle(lPathSrc)) == null)
            return;

        // AssetBundle loading success, and GameObject has been found
        // So save the AssetBundle into SavedData
        try {
            File.Copy(lPathSrc, lPathDst);
        } catch (Exception ex) {
            Debug.LogError("[IMPORT MODEL] Error:" + ex.Message);
            // Warn user with notifier (TODO)
        }

        ModelLoader.Instance.UpdatePool();
        UILibModel.UpdateAvailableModel();

        AssetBundle.UnloadAllAssetBundles(true);
        HideBrowser();
    }

    // This function toogle the display of the UI
    public void DisplayToogle()
    {
        // Display
        if (!mDisplayed) {
            UpdateFiles();
            mDisplayed = true;
            enabled = true;
            mCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeToAsync(1F, 0.4F, null));
        }
        // Hide
        else
            HideBrowser();
    }

    public void HideBrowser()
    {
        if (mDisplayed) {
            mDisplayed = false;
            mCanvasGroup.blocksRaycasts = false;
            StartCoroutine(FadeToAsync(0F, 0.4F, ClearFiles));
        }
    }

    IEnumerator FadeToAsync(float iValue, float iTime, Action iOnEndFade)
    {
        float lAlpha = mCanvasGroup.alpha;

        for (float lTime = 0F; lTime < 1F; lTime += Time.deltaTime / iTime) {
            float newAlpha = Mathf.SmoothStep(lAlpha, iValue, lTime);
            mCanvasGroup.alpha = newAlpha;
            yield return null;
        }
        if (iOnEndFade != null)
            iOnEndFade();
    }
}