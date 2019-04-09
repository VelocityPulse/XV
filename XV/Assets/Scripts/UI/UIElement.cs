using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public sealed class UIElement : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IBeginDragHandler ,IDragHandler, IEndDragHandler
{
    private Image mElementColor;

    private Text mElementText;

    public GameObject mModelGameObject;

    private GameObject mSelectedElement;

    private string mLastHit;

    private Vector3 mMousePosition;

    private void Start()
	{
        mSelectedElement = null;
        if ((mElementColor = GetComponent<Image>()) == null)
            Debug.LogError("[ERROR] Ui Model element doesn't contain Image!");
        if ((mElementText = GetComponentInChildren<Text>()) == null)
            Debug.LogError("[ERROR] Ui Model element doesn't contain Text!");
	}

	public void OnPointerEnter(PointerEventData eventData)
    {
        if (mElementColor)
            mElementColor.color = Color.green;
        if (mElementText)
            mElementText.color = Color.green;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (mElementColor)
            mElementColor.color = Color.white;
        if (mElementText)
            mElementText.color = Color.grey;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("DRAG");
        if (mSelectedElement != null)
        {
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("scene")))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1);
                mLastHit = hit.collider.tag;
                if (hit.collider.tag == "Player")
                    mSelectedElement.transform.position = hit.point;
                //else
                //{
                //    mMousePosition = Input.mousePosition;
                //    mMousePosition.z = 10.0F;
                //    mSelectedElement.transform.position = Camera.main.ScreenToWorldPoint(mMousePosition);
                //}
            }
            else
                Debug.Log("[ELEMENT_DRAG/DROP] RayCast hit nothing.");
        }
        else
            Debug.Log("[ELEMENT_DRAG/DROP] Instantiation of the selected element has failed.");
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("DROP");
        //if (mLastHit != "Player")
            //Destroy(mSelectedElement);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
		Debug.Log("BEGIN DRAG");
        if (mSelectedElement != null)
            return;
        mSelectedElement = Instantiate(mModelGameObject);
    }
}
