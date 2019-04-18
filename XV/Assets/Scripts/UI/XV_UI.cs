using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class XV_UI : MonoBehaviour {

    [SerializeField]
    private UINotifier notifier;

    private UINotifier Notifier { get { return notifier; }}

    private static XV_UI sInstance;

    private static bool sLockInstance;

    static public XV_UI Instance
    {
        get
        {
            if (sInstance == null) {
                sLockInstance = true;
                sInstance = Resources.Load<GameObject>(GameManager.UI_TEMPLATE_PATH + "XV_UI").GetComponent<XV_UI>();
            }
            return sInstance;
        }
    }

	private void Start()
    {
        if (sInstance == null)
            sInstance = this;
        else if (!sLockInstance) {
            Destroy(gameObject);
            throw new Exception("An instance of this singleton already exists.");
        }
        sLockInstance = false;
        enabled = false;
    }

    public void Notify(float iDuration, string iMessage)
    {
        if (Notifier != null)
            Notifier.Notify(iDuration, iMessage);
    }
}
