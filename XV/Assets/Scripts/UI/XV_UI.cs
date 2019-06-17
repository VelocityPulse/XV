using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class XV_UI : MonoBehaviour
{

	public bool isGUILocked { get { return mIsGUILocked; } }

	[SerializeField]
	private UINotifier notifier;

	[SerializeField]
	private CanvasGroup UIModelManagerLockerScreen;

	[SerializeField]
	private CanvasGroup UIToolBarLockerScreen;

	[SerializeField]
	private CanvasGroup UITimelinePanelLockerScreen;

	[SerializeField]
	private UIConfirmPopup uIConfirmPopup;

	[SerializeField]
	private Text sceneNameText;

	private UINotifier Notifier { get { return notifier; } }

	private static XV_UI sInstance;

	private bool mIsGUILocked;

	static public XV_UI Instance
	{
		get
		{
			if (sInstance == null) {

				GameObject lGameObject = null;
				if ((lGameObject = GameObject.Find("XV_UI"))) {
					if ((sInstance = lGameObject.GetComponent<XV_UI>()))
						return sInstance;
				}
				sInstance = Resources.Load<GameObject>(GameManager.UI_TEMPLATE_PATH + "XV_UI").GetComponent<XV_UI>();
			}
			return sInstance;
		}
	}

	public Text SceneNameText { get { return sceneNameText; } }

	public UIConfirmPopup UIConfirmPopup { get { return uIConfirmPopup; } }

	private void Start()
	{
		if (sInstance == null)
			sInstance = this;

		enabled = false;
	}

	public void Notify(float iDuration, string iMessage)
	{
		if (Notifier != null)
			Notifier.Notify(iDuration, iMessage);
	}

	public void LockGUI()
	{
		if (!mIsGUILocked) {
			mIsGUILocked = true;
			UIModelManagerLockerScreen.alpha = 0F;
			UIToolBarLockerScreen.alpha = 0F;
			UITimelinePanelLockerScreen.alpha = 0F;

			UIModelManagerLockerScreen.gameObject.SetActive(true);
			UIToolBarLockerScreen.gameObject.SetActive(true);
			UITimelinePanelLockerScreen.gameObject.SetActive(true);

			StartCoroutine(Utils.FadeToAsync(1F, 0.5F, UIModelManagerLockerScreen));
			StartCoroutine(Utils.FadeToAsync(1F, 0.5F, UIToolBarLockerScreen));
			StartCoroutine(Utils.FadeToAsync(1F, 0.5F, UITimelinePanelLockerScreen));
		}
	}

	public void UnlockGUI()
	{
		if (mIsGUILocked) {
			mIsGUILocked = false;

			UIModelManagerLockerScreen.alpha = 1F;
			UIToolBarLockerScreen.alpha = 1f;
			UITimelinePanelLockerScreen.alpha = 1F;

			StartCoroutine(Utils.FadeToAsync(0F, 0.5F, UIModelManagerLockerScreen,
											 () => { UIModelManagerLockerScreen.gameObject.SetActive(false); }));
			StartCoroutine(Utils.FadeToAsync(0F, 0.5F, UIToolBarLockerScreen,
											 () => { UIToolBarLockerScreen.gameObject.SetActive(false); }));
			StartCoroutine(Utils.FadeToAsync(0F, 0.5F, UITimelinePanelLockerScreen,
			                                 () => { UITimelinePanelLockerScreen.gameObject.SetActive(false); }));
		}
	}
}
