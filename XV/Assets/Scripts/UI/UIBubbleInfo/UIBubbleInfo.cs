using System;
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
	private GameObject ColorSelector;

	[SerializeField]
	private InputField ModelName;

    [SerializeField]
    private InputField SpeedInput;

    private float mSpeed;

    /// <summary>
    /// The Speed coeffcient input given by the user.
    /// If the user give an invalid input, or if an error occured a default value is used.
    /// Default Value: 100
    /// </summary>
    public float Speed { get { return mSpeed; } }

    public AEntity Parent { get; set; }

	// Use this for initialization
	private void Start()
	{
		mButtons = new List<KeyValuePair<string, Button>>();
		mCanvasGroup = GetComponent<CanvasGroup>();
		mContentSizeFitter = GetComponentInChildren<ContentSizeFitter>();
		mCanvasGroup.alpha = 0F;
		mCanvasGroup.blocksRaycasts = false;
        if (SpeedInput == null) {
            Debug.LogError("SpeedInput reference is missing in UIBubbleInfo.");
            mSpeed = MovableEntity.DEFAULT_SPEED_COEFF;
        } else {
            SpeedInput.onEndEdit.AddListener(OnEndEditSpeed);
            OnEndEditSpeed(SpeedInput.text);
        }
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
				transform.rotation, lLookAt, Time.deltaTime * 5);

			if (ModelName.isFocused || SpeedInput.isFocused)
				GameManager.Instance.KeyboardDeplacementActive = false;
			else
				GameManager.Instance.KeyboardDeplacementActive = true;
		}
	}

    /// <summary>
    /// Try to parse the Speed InputField, if the input is not a number,
    /// or if it's not in range [0.1;10], the user is Notify and the default speed value is used instead.
    /// </summary>
    /// <param name="iSpeedInput"></param>
    private void OnEndEditSpeed(string iSpeedInput)
    {
        // If the user doesn't give an input, use default value.
        if (string.IsNullOrEmpty(iSpeedInput)) {
            mSpeed = MovableEntity.DEFAULT_SPEED_COEFF;
            return;
        }

        try {
            mSpeed = float.Parse(iSpeedInput);
            if (mSpeed < MovableEntity.MIN_SPEED_COEFF || mSpeed > MovableEntity.MAX_SPEED_COEFF)
                mSpeed = -1F;
        } catch (Exception lException) {
            Debug.LogError("UIBubbleInfo - Error while retrieving float in SpeedInput:" + lException.Message);
            mSpeed = -1;
        }
        if (mSpeed < 0F) {
            XV_UI.Instance.Notify(2.5F, "Enter a speed coefficient between" + MovableEntity.MIN_SPEED_COEFF + "% and " + MovableEntity.MAX_SPEED_COEFF + "%.");
            SpeedInput.text = null;
            mSpeed = MovableEntity.DEFAULT_SPEED_COEFF;
        }
    }

    /// <summary>
    /// Hide the Speed Input field in the BubbleInfo.
    /// </summary>
    public void HideSpeedInput()
    {
        SpeedInput.gameObject.transform.parent.gameObject.SetActive(false);
        mSpeed = MovableEntity.DEFAULT_SPEED_COEFF;
    }

	public Button CreateButton(UIBubbleInfoButton iInfoButton)
	{
		if (iInfoButton == null)
			return null;

		GameObject lNewButton = Instantiate(SampleButton, GridContainer.transform);
		lNewButton.transform.SetSiblingIndex(0); // puts the buttons before the selectorColor line

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

	public void OnResetColorClick()
	{
		Parent.ResetColor();
		Debug.Log("reset");
	}

	public void OnRedColorClick()
	{
		Color lColor;
		ColorUtility.TryParseHtmlString("#E73F3FFF", out lColor);

		Parent.SetColored(lColor);
		Debug.Log("red");
	}

	public void OnGreenColorClick()
	{
		Color lColor;
		ColorUtility.TryParseHtmlString("#13A945FF", out lColor);

		Parent.SetColored(lColor);
		Debug.Log("green");
	}

	public void OnBlueColorClick()
	{
		Color lColor;
		ColorUtility.TryParseHtmlString("#347ADBFF", out lColor);

		Parent.SetColored(lColor);
		Debug.Log("blue");
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
