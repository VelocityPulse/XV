using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum UISceneTitleResult
{
	OK_RESULT,

	CANCEL_RESULT,

	ALREADY_DISPLAYING,

	BAD_PARAMETERS
}

public class UINewSceneTitle : MonoBehaviour
{
	[SerializeField]
	private Button createButton;

	[SerializeField]
	private InputField inputField;

	[SerializeField]
	private Text textError;

	private CanvasGroup mCanvasGroup;

	private bool mDisplayed;

	private Action<UISceneTitleResult, string> mResultAction;

	private List<string> mNamesAlreadyUsed;

	// Use this for initialization
	void Start()
	{
		mCanvasGroup = GetComponent<CanvasGroup>();

		createButton.onClick.AddListener(OnClickCreate);
		inputField.onValueChanged.AddListener(OnValueChanged);
		mNamesAlreadyUsed = new List<string>();
		HideError();
		Reset();
	}

	public void StartForResult(Action<UISceneTitleResult, string> iAction, string[] iFileAlreadyPresent)
	{
		if (iAction == null || iFileAlreadyPresent == null) {
			iAction(UISceneTitleResult.BAD_PARAMETERS, string.Empty);
			Reset();
			return;
		}

		if (!mDisplayed) {
			mDisplayed = true;
			mResultAction = iAction;
			Display();

			foreach (string iFileWithExtension in iFileAlreadyPresent) {
				mNamesAlreadyUsed.Add(iFileWithExtension.Replace(".xml", "").ToLower());
				Debug.Log(mNamesAlreadyUsed[0]);
			}
		} else {
			iAction(UISceneTitleResult.ALREADY_DISPLAYING, string.Empty);
			Reset();
			return;
		}
	}

	private void Display()
	{
		GameManager.Instance.KeyboardDeplacementActive = false;
		mCanvasGroup.alpha = 0F;
		mCanvasGroup.blocksRaycasts = true;
		createButton.interactable = false;
		StartCoroutine(Utils.FadeToAsync(1F, 0.8F, mCanvasGroup));
	}

	private void Hide()
	{
		GameManager.Instance.KeyboardDeplacementActive = true;
		mCanvasGroup.alpha = 1F;
		mCanvasGroup.blocksRaycasts = false;
		StartCoroutine(Utils.FadeToAsync(0F, 0.8F, mCanvasGroup));
		Reset();
	}

	private void ShowError(string iErrorMessage)
	{
		textError.text = iErrorMessage;
		textError.gameObject.SetActive(true);
	}

	private void HideError()
	{
		textError.gameObject.SetActive(false);
	}

	private void OnValueChanged(string iValue)
	{
		if (string.IsNullOrEmpty(iValue.Trim()))
			createButton.interactable = false;
		else if (mNamesAlreadyUsed != null && mNamesAlreadyUsed.Contains(iValue.ToLower())) {
			ShowError("Name already used");
			createButton.interactable = false;
		} else {
			HideError();
			createButton.interactable = true;
		}
	}

	private void OnClickCreate()
	{
		mDisplayed = false;
		mResultAction(UISceneTitleResult.OK_RESULT, inputField.text);
		Hide();
	}

	private void Reset()
	{
		mCanvasGroup.alpha = 0F;
		mCanvasGroup.blocksRaycasts = false;
		mNamesAlreadyUsed = new List<string>();
		mResultAction = null;
	}

}
