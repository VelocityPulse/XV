using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIElementGridParam : MonoBehaviour {

    [SerializeField]
    private Image Icon;

    [SerializeField]
    private Text Text;

    public Image GetImage()
    {
        return Icon;
    }

    public Text GetText()
    {
        return Text;
    }
}
