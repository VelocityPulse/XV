using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UiModelManager : MonoBehaviour {

    [Header("UI Element")]
    [SerializeField]
    private GameObject UiElement;

    [Header("Sprites Model")]
    [SerializeField]
    private Sprite[] UiModelSprites;


    [SerializeField]
    private int MaxModelInView;

    private int mStartModel;

    private List<GameObject> mUiElements;

    private void Start()
	{
        if (MaxModelInView <= 0)
            return;
        if ((mUiElements = new List<GameObject>(MaxModelInView)) == null)
            return;
        mStartModel = 0;
        for (int i = 0; i < MaxModelInView; i++)
        {
            GameObject lTmp = Instantiate(UiElement, transform);
            lTmp.GetComponent<Image>().sprite = UiModelSprites[i];
            lTmp.GetComponentInChildren<Text>().text = UiModelSprites[i].name;
            mUiElements.Add(lTmp);
        }
	}

    public void OnScroll()
    {
        Debug.Log("COUCOU!");
    }
}
