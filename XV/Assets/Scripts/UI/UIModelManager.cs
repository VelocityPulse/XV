using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public sealed class UIModelManager : MonoBehaviour {

    [Header("UI Element")]
    [SerializeField]
    private GameObject UiElement;

    private void Start()
	{
        List<ModelManager.Model> lModels;

        if ((lModels = ModelManager.Instance.GetAllModel()) == null)
            return;
        
        for (int i = 0; i < lModels.Count; i++)
        {
            GameObject lUiElement = Instantiate(UiElement, transform);
            lUiElement.GetComponent<Image>().sprite = lModels[i].Sprite;
            lUiElement.GetComponentInChildren<Text>().text = lModels[i].Sprite.name;
            lUiElement.GetComponent<UIElement>().mModelGameObject = lModels[i].GameObject;
        }
	}
}
