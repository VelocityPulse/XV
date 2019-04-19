using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBubbleInfo : MonoBehaviour
{
	public static string TAG = "BubbleInfo";

	private bool mDisplayed;
	private CanvasGroup mCanvasGroup;

	private List<Button> mButtons;
	

	[SerializeField]
	private GameObject GridContainer;

	[SerializeField]
	private GameObject SampleButton;

    [SerializeField]
    private InputField ModelName;

    public ObjectEntity Parent { get; set; }

    // Use this for initialization
    private void Start()
    {
        mButtons = new List<Button>();
        mCanvasGroup = GetComponent<CanvasGroup>();
        mCanvasGroup.alpha = 0F;
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
        mButtons.Add(lButtonComponant);

		lNewButton.GetComponentInChildren<Text>().text = iInfoButton.Text;
		lNewButton.name = iInfoButton.Text;
		lNewButton.SetActive(true);
		Canvas.ForceUpdateCanvases();
        return lButtonComponant;
	}

	public void SetInteractable(bool iInteractable) {
		foreach (Button lButton in mButtons) {
			lButton.interactable = iInteractable;
		}
		ModelName.interactable = iInteractable;
	}

    public void OnEndEdit()
    {
        if (string.IsNullOrEmpty(ModelName.text))
            ModelName.text = Parent.Name;
        else
            Parent.Name = ModelName.text;
    }

	public void Display()
	{
		mDisplayed = true;
		StartCoroutine(FadeToAsync(1F, 0.4F));
	}

	public void Hide()
	{
		mDisplayed = false;
		StartCoroutine(FadeToAsync(0F, 0.4F));
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
