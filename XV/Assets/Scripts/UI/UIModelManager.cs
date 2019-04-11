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
        List<ModelLoader.Model> lModels;

        if ((lModels = ModelLoader.Instance.GetAllModel()) == null)
            return;

        // Instantiate all UI element to drag and drop model
        // Setting up the sprite of the UI
        // Setting up the text of the UI
        // Store the Model to use at drag and drop,
        GameObject lPaddElement = new GameObject("PaddingElement");
        lPaddElement.transform.parent = transform;
        lPaddElement.AddComponent<LayoutElement>();
        for (int i = 0; i < lModels.Count; i++) {
            GameObject lUiElement = Instantiate(UiElement, transform);
            lUiElement.GetComponent<Image>().sprite = lModels[i].Sprite;
            lUiElement.GetComponentInChildren<Text>().text = lModels[i].Sprite.name;
            lUiElement.GetComponent<UIModel>().Model = lModels[i];
        }
        Instantiate(lPaddElement, transform);
	}
}
