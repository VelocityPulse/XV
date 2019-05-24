using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBubbleInfo : MonoBehaviour
{
	public static string TAG = "BubbleInfo";

	private bool mDisplayed;
	private CanvasGroup mCanvasGroup;
	private ContentSizeFitter mContentSizeFitter;

	private List<KeyValuePair<string, Button>> mButtons;

	[SerializeField]
	private GameObject GridContainer;

	[SerializeField]
	private GameObject SampleButton;

	[SerializeField]
	private InputField ModelName;

	public AEntity Parent { get; set; }

	// Use this for initialization
	private void Start()
	{
		mButtons = new List<KeyValuePair<string, Button>>();
		mCanvasGroup = GetComponent<CanvasGroup>();
		mContentSizeFitter = GetComponentInChildren<ContentSizeFitter>();
		mCanvasGroup.alpha = 0F;
		mCanvasGroup.blocksRaycasts = false;
	}

	private void Awake()
	{
		RefreshCanvas();
	}

	// Update is called once per frame
	private void Update()
	{
		if (mDisplayed) {
			Quaternion lLookAt = Quaternion.LookRotation(
				transform.position - Camera.main.transform.position);

			transform.rotation = Quaternion.Slerp(
				transform.rotation, lLookAt, Time.deltaTime * 2);

			if (ModelName.isFocused)
				GameManager.Instance.KeyboardDeplacementActive = false;
			else
				GameManager.Instance.KeyboardDeplacementActive = true;
		}
	}

	public Button CreateButton(UIBubbleInfoButton iInfoButton)
	{
		if (iInfoButton == null)
			return null;

		GameObject lNewButton = Instantiate(SampleButton, GridContainer.transform);

		Button lButtonComponant = lNewButton.GetComponent<Button>();
		lButtonComponant.onClick.AddListener(() => {
			if (iInfoButton.ClickAction != null)
				iInfoButton.ClickAction(Parent);
		});

        if (string.IsNullOrEmpty(iInfoButton.Tag))
            mButtons.Add(new KeyValuePair<string, Button>("untagged", lButtonComponant));
        else
            mButtons.Add(new KeyValuePair<string, Button>(iInfoButton.Tag, lButtonComponant));

        lNewButton.GetComponentInChildren<Text>().text = iInfoButton.Text;
		lNewButton.name = iInfoButton.Text;
		lNewButton.SetActive(true);
		Canvas.ForceUpdateCanvases();
		return lButtonComponant;
	}

    public bool ContainsButton(string iTag)
    {
        if (string.IsNullOrEmpty(iTag))
            return false;
        foreach (KeyValuePair<string, Button> lButton in mButtons) {
            if (lButton.Key == iTag) {
                return true;
            }
        }
        return false;
    }

    public void DestroyButton(string iTag)
    {
        if (string.IsNullOrEmpty(iTag))
            return;
        foreach (KeyValuePair<string, Button> lButton in mButtons) {
            if (lButton.Key == iTag) {
                mButtons.Remove(lButton);
                Destroy(lButton.Value.gameObject);
                Canvas.ForceUpdateCanvases();
                return;
            }
        }
    }

    public void SetUIName(string iName)
	{
		ModelName.text = iName;
	}

	public void SetInteractable(bool iInteractable)
	{
		foreach (KeyValuePair<string, Button> lButton in mButtons) {
			lButton.Value.interactable = iInteractable;
		}
		ModelName.interactable = iInteractable;
	}

	public void OnEndEdit()
	{
		if (string.IsNullOrEmpty(ModelName.text))
			ModelName.text = Parent.Name;
		else if (Parent != null)
			Parent.Name = ModelName.text;
	}

	public void Display()
	{
		mDisplayed = true;
		SetInteractable(true);
		mCanvasGroup.blocksRaycasts = true;
		StartCoroutine(FadeToAsync(1F, 0.4F));
	}

	public void Hide()
	{
		mDisplayed = false;
		SetInteractable(false);
		mCanvasGroup.blocksRaycasts = false;
		StartCoroutine(FadeToAsync(0F, 0.4F));
	}

	public void RefreshCanvas()
	{
		if (mContentSizeFitter == null)
			return;

		StartCoroutine(Utils.WaitNextFrameAsync(() => {
			
			StartCoroutine(Utils.WaitNextFrameAsync(() => {
				mContentSizeFitter.enabled = true;
			}));

			mContentSizeFitter.enabled = false;
		}));
	}

	IEnumerator FadeToAsync(float iValue, float iTime)
	{
		float lAlpha = mCanvasGroup.alpha;

		for (float lTime = 0F; lTime < 1F; lTime += Time.deltaTime / iTime) {
			float newAlpha = Mathf.SmoothStep(lAlpha, iValue, lTime);
			mCanvasGroup.alpha = newAlpha;
			yield return null;
		}
	}
}
