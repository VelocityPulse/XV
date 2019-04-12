using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class FileBrowser : MonoBehaviour
{
    private readonly string STARTING_PATH = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
 
    private bool mDisplayed;

    private CanvasGroup mCanvasGroup;

	private GameObject mFileEntryTemplate;

    private Sprite mSpriteFile;

    private Sprite mSpriteFolder;

    private Text mFileText;

    private Image mFileIcon;

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
        mCanvasGroup = GetComponent<CanvasGroup>();
        mCanvasGroup.alpha = 0F;
        mCanvasGroup.blocksRaycasts = false;
        enabled = false;
        mDisplayed = false;
        mFileEntryTemplate = Resources.Load<GameObject>("Prefabs/UI/UIFileBrowserElementGrid");
        mSpriteFile = Resources.Load<Sprite>("Sprites/UI/Icons/File");
        mSpriteFolder = Resources.Load<Sprite>("Sprites/UI/Icons/Folder");

        if ((mFileText = mFileEntryTemplate.GetComponentInChildren<Text>()) == null)
            return;

        Image[] lImages;
        // Get all image
        if ((lImages = mFileEntryTemplate.GetComponentsInChildren<Image>()) == null)
            return;
        // Store the children img, ignore the img attach to this gameobject
        foreach (Image lImg in lImages) {
            if (lImg.gameObject.GetInstanceID() != gameObject.GetInstanceID())
                mFileIcon = lImg;
        }
    }

    private void UpdateFiles()
    {
        PWD.text = "PATH:" + STARTING_PATH;
        List<string> lDirs = new List<string>(Directory.GetFileSystemEntries(STARTING_PATH));

        foreach (string lFile in lDirs)
        {
            FileAttributes lAttr = File.GetAttributes(lFile);
            if ((lAttr & FileAttributes.Hidden) == FileAttributes.Hidden)
                continue;
            else if ((lAttr & FileAttributes.Directory) == FileAttributes.Directory) {
                    mFileText.color = Color.blue;
                    mFileIcon.sprite = mSpriteFolder;
            } else {
                    mFileText.color = Color.black;
                    mFileIcon.sprite = mSpriteFile;
            }
            mFileText.text = lFile;
            Instantiate(mFileEntryTemplate, UIGridElement.transform);            
        }
    }

    private void ClearFiles()
    {
        foreach (Transform lChild in UIGridElement.transform) {
            Destroy(lChild.gameObject);
        }
    }

    public void GoToParentDir()
    {
    }

    public void GoToHome()
    {
        
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
            StartCoroutine(FadeTo(1F, 0.4F, null));
        } 
        // Hide
        else {
            mDisplayed = false;
            mCanvasGroup.blocksRaycasts = false;
            StartCoroutine(FadeTo(0F, 0.4F, ClearFiles));
        }
    }

    IEnumerator FadeTo(float iValue, float iTime, Action iOnEndFade)
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