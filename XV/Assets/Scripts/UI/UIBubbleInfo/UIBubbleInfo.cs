using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIBubbleInfo : MonoBehaviour
{
	public static string TAG = "BubbleInfo";

	private bool mDisplayed;
	private CanvasGroup mCanvasGroup;

	[SerializeField]
	public GameObject GridContainer;

	[SerializeField]
	public GameObject SampleButton;

	public ObjectEntity Parent { get; set; }

	// Use this for initialization
	private void Start()
	{
		mCanvasGroup = GetComponent<CanvasGroup>();
		mCanvasGroup.alpha = 0F;
		enabled = false;
	}

	public void DestroyObject()
	{
		Parent.RemoveEntity();
		Parent.Dispose();
	}

	// Update is called once per frame
	private void Update()
	{
		if (mDisplayed) {
			Quaternion lLookAt = Quaternion.LookRotation(
				transform.position - Camera.main.transform.position);

			transform.rotation = Quaternion.Slerp(
				transform.rotation, lLookAt, Time.deltaTime * 2);
		}
	}

	public void CreateButton(UIBubbleInfoButton iInfoButton)
	{
		if (iInfoButton == null)
			return;
		
		GameObject lNewButton = Instantiate(SampleButton, GridContainer.transform);

		lNewButton.GetComponent<Button>().onClick.AddListener(() => {
			if (iInfoButton.ClickAction != null)
				iInfoButton.ClickAction(Parent);
		});

		lNewButton.GetComponentInChildren<Text>().text = iInfoButton.Text;
		lNewButton.name = iInfoButton.Text;
		lNewButton.SetActive(true);
		Canvas.ForceUpdateCanvases();
	}

	public void Display()
	{
		mDisplayed = true;
		enabled = true;
		StartCoroutine(FadeTo(1F, 0.4F));
	}

	public void Hide()
	{
		mDisplayed = false;
		StartCoroutine(FadeTo(0F, 0.4F));
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
