using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour
{
    private const string SELECTION_FIELD = "Please select a model file to open ...";

    private readonly string STARTING_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
 
    private readonly Color32 ROYAL_BLUE = new Color32(65, 105, 255, 255);

    private readonly Color32 ROYAL_GREY = new Color32(80, 80, 80, 255);

    private bool mDisplayed;

    private CanvasGroup mCanvasGroup;

	private GameObject mFileEntryTemplate;

    private Sprite mSpriteFile;

    private Sprite mSpriteFolder;

    private Text mFileText;

    private Image mFileIcon;

    private string mPath;

    [Header("Please attach: FileBrowser/Middle/FileContainer/GridWithElements")]
    [SerializeField]
    private GameObject UIGridElement;

    [Header("Please attach: FileBrowser/Top/ButtonContainer/GoBackPath")]
    [SerializeField]
    private Button GoBack;

    [Header("Please attach: FileBrowser/Top/ButtonContainer/GoToHome")]
    [SerializeField]
    private Button GoHome;

    [Header("Please attach: FileBrowser/Bottom/OpenSelectedFile")]
    [SerializeField]
    private Button OpenFile;

    [Header("Please attach: FileBrowser/Top/PWD")]
    [SerializeField]
    private Text PWD;

    [Header("Please attach: FileBrowser/Bottom/SelectedFilePath")]
    [SerializeField]
    private Text SelectedFile;

    // Use this for initialization
    private void Start()
    {
        mPath = STARTING_PATH;
        mCanvasGroup = GetComponent<CanvasGroup>();
        mCanvasGroup.alpha = 0F;
        mCanvasGroup.blocksRaycasts = false;
        enabled = false;
        mDisplayed = false;
        mFileEntryTemplate = Resources.Load<GameObject>("Prefabs/UI/UIFileBrowserElementGrid");
        mSpriteFile = Resources.Load<Sprite>("Sprites/UI/Icons/File");
        mSpriteFolder = Resources.Load<Sprite>("Sprites/UI/Icons/Folder");

        // Get the text component of the Element prefab
        if ((mFileText = mFileEntryTemplate.GetComponentInChildren<Text>()) == null)
            return;

        Image[] lImages;
        // Get all image
        if ((lImages = mFileEntryTemplate.GetComponentsInChildren<Image>()) == null)
            return;

        // Get the image component of the Element prefab (Button icon)
        foreach (Image lImg in lImages) {
            // Ignore the component of this gameobject, use the children component.
            if (lImg.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                mFileIcon = lImg;
        }

        // Button setting
        GoBack.onClick.AddListener(GoToParentDir);
        GoHome.onClick.AddListener(GoToHome);
        OpenFile.onClick.AddListener(OpenSelectedFile);

        // Init selected file text
        SelectedFile.text = SELECTION_FIELD;
    }

    private void UpdateFiles()
    {
        PWD.text = "Path:" + mPath;
        List<string> lDirs = new List<string>(Directory.GetFileSystemEntries(mPath));

        foreach (string lFile in lDirs)
        {
            FileAttributes lAttr = File.GetAttributes(lFile);
            if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden)
                continue;
            else if ((lAttr & FileAttributes.Directory) == FileAttributes.Directory) {
                mFileText.color = ROYAL_BLUE;
                mFileIcon.sprite = mSpriteFolder;
            } else {
                mFileText.color = ROYAL_GREY;
                    mFileIcon.sprite = mSpriteFile;
            }
            mFileText.text = lFile.Replace(mPath + "/", "");
            GameObject lElement = Instantiate(mFileEntryTemplate, UIGridElement.transform);
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
                FileAttributes lAttr = File.GetAttributes(mPath + "/" + lButtonText.text);
                if ((lAttr & FileAttributes.Directory) != FileAttributes.Directory)
                    SelectedFile.text = lButtonText.text;
                else {
                    mPath = mPath + "/" + lButtonText.text;
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
        
    }

    // This function toogle the display of the UI
    public void DisplayToogle()
    {
        // Display
        if (mDisplayed == false) {
            UpdateFiles();
            mDisplayed = true;
            enabled = true;
            mCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeToAsync(1F, 0.4F, null));
        } 
        // Hide
        else {
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