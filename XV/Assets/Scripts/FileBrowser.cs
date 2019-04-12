using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileBrowser : MonoBehaviour
{
    private bool mDisplayed;
    private CanvasGroup mCanvasGroup;

    [Header("Please attach: UIFileBrowserElementGrid")]
    [SerializeField]
    private GameObject UIFileElement;

    // Use this for initialization
    private void Start()
    {
        mCanvasGroup = GetComponent<CanvasGroup>();
        mCanvasGroup.alpha = 0F;
        mCanvasGroup.blocksRaycasts = false;
        enabled = false;
        mDisplayed = false;
    }

    public void DisplayToogle()
    {
        if (mDisplayed == false) {
            Debug.Log("BUBBLE : display");
            mDisplayed = true;
            enabled = true;
            mCanvasGroup.blocksRaycasts = true;
            StartCoroutine(FadeTo(1F, 0.4F));
        } 
        else {
            mDisplayed = false;
            mCanvasGroup.blocksRaycasts = false;
            StartCoroutine(FadeTo(0F, 0.4F));            
        }
    }

    IEnumerator FadeTo(float iValue, float iTime)
    {
        float lAlpha = mCanvasGroup.alpha;

        for (float lTime = 0F; lTime < 1F; lTime += Time.deltaTime / iTime) {
            float newAlpha = Mathf.SmoothStep(lAlpha, iValue, lTime);
            mCanvasGroup.alpha = newAlpha;
            yield return null;
        }
    }
}