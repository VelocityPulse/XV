using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIModel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler ,IDragHandler, IEndDragHandler
{
    private Image mElementColor;

    private Text mElementText;

    public ModelLoader.Model Model;

    private GameObject mSelectedElement;

    private Vector3 mCentroid;

    private void Start()
	{
        mSelectedElement = null;

        if ((mElementColor = GetComponentInChildren<Image>()) == null)
            Debug.LogError("[ERROR] Ui Model element doesn't contain Image!");
        
        if ((mElementText = GetComponentInChildren<Text>()) == null)
            Debug.LogError("[ERROR] Ui Model element doesn't contain Text!");
	}

	public void OnPointerEnter(PointerEventData iEventData)
    {
        if (mElementColor)
            mElementColor.color = Color.green;
        if (mElementText)
            mElementText.color = Color.green;
    }

    public void OnPointerExit(PointerEventData iEventData)
    {
        if (mElementColor)
            mElementColor.color = Color.white;
        if (mElementText)
            mElementText.color = Color.white;
    }

    // If a selectedElement exist, cast a ray from the camera to the mouse,
    // Just cast on dropable element
    // On hit, update the selectedElement position
    public void OnDrag(PointerEventData iEventData)
    {
        if (mSelectedElement != null) {
            if (!mSelectedElement.activeSelf)
                mSelectedElement.SetActive(true);
            RaycastHit lHit;
            Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(lRay, out lHit, 1000, LayerMask.GetMask("dropable"))) {
                Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);

                mSelectedElement.transform.position = lHit.point;
                mSelectedElement.transform.position += 
                    (mSelectedElement.transform.position - mSelectedElement.transform.TransformPoint(mCentroid));
                // new at each drag ... find a way to update position.y without new

                mSelectedElement.transform.position = new Vector3(
                    mSelectedElement.transform.position.x, lHit.point.y, mSelectedElement.transform.position.z);
            }
        }
    }

    // On End restore Layer to dropable, build the object using selectedElement, delete SelectedElement.
    public void OnEndDrag(PointerEventData eventData)
    {
        ObjectDataScene lODS = new ObjectDataScene {
            Name = mElementText.text,
            Type = Model.Type,
            Position = mSelectedElement.transform.position,
            Rotation = Model.GameObject.transform.rotation.eulerAngles,
            Scale = mSelectedElement.transform.localScale,
        };

        GameManager.Instance.BuildObject(lODS);
		Destroy(mSelectedElement);
    }

    // Instantiate the associated Model, disable it and ignore raycast for this object.
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (mSelectedElement != null)
            return;

        // Instantiate temporary object during drag & drop, it will follow mouse
        if ((mSelectedElement = Instantiate(Model.GameObject)) == null)
            return;

        // Retrieve parameters for this item
        EntityParameters lParameters;
        if ((lParameters = mSelectedElement.GetComponent<EntityParameters>()) != null) {
            // Update orientation
            mSelectedElement.transform.eulerAngles = lParameters.Orientation;
        }

        mSelectedElement.SetActive(false);
        Utils.SetLayerRecursively(mSelectedElement, LayerMask.NameToLayer("Ignore Raycast"));

        List<Vector3> lPoints = new List<Vector3>();
        if (lPoints == null) {
            Debug.LogError("[UI_MODEL] Allocation Error.");
            return;
        }

        mCentroid = Utils.ComputeBoundingBox(mSelectedElement).center;
    }
}
