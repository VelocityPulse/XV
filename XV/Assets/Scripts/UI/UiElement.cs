using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UiElement : MonoBehaviour {


    [SerializeField]
    private Color OverlayColor;


    public void OnPointerEnter()
    {
        GetComponent<Image>().color = OverlayColor;
    }

    public void OnPointerExit()
    {
        GetComponent<Image>().color = Color.white;
    }

}
