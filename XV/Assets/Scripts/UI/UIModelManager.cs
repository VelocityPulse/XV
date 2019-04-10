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

        GameObject lPaddElement = new GameObject();
        lPaddElement.name = "PaddingElement";
        lPaddElement.AddComponent<LayoutElement>();

        // Instantiate all UI element to drag and drop model
        // Setting up the sprite of the UI
        // Setting up the text of the UI
        // Store the Model to use at drag and drop,
        Instantiate(lPaddElement, transform);
        for (int i = 0; i < lModels.Count; i++) {
            GameObject lUiElement = Instantiate(UiElement, transform);
            lUiElement.GetComponent<Image>().sprite = lModels[i].Sprite;
            lUiElement.GetComponentInChildren<Text>().text = lModels[i].Sprite.name;
            lUiElement.GetComponent<UIElement>().Model = lModels[i];
        }
        Instantiate(lPaddElement, transform);
	}
}
