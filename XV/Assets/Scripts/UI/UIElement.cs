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

    private Vector3 mCentroid;

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

    // If a selectedElement exist, cast a ray from the camera to the mouse,
    // Just cast on dropable element
    // On hit, update the selectedElement position
    public void OnDrag(PointerEventData eventData)
    {
        Debug.Log("DRAG");
        if (mSelectedElement != null)
        {
            if (!mSelectedElement.activeSelf)
                mSelectedElement.SetActive(true);
            RaycastHit hit;
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit, 1000, LayerMask.GetMask("dropable")))
            {
                Debug.DrawRay(ray.origin, ray.direction * hit.distance, Color.red, 1);
                mSelectedElement.transform.position = hit.point;
                mSelectedElement.transform.position += (mSelectedElement.transform.position - mSelectedElement.transform.TransformPoint(mCentroid));
                mSelectedElement.transform.position = new Vector3(mSelectedElement.transform.position.x, hit.point.y, mSelectedElement.transform.position.z);
                Debug.Log("TEST:" + mCentroid);
            }
            else
                Debug.Log("[ELEMENT_DRAG/DROP] RayCast hit nothing.");
        }
        else
            Debug.Log("[ELEMENT_DRAG/DROP] Instantiation of the selected element has failed.");
    }

    // On End restore Layer to dropable, build the object using selectedElement, delete SelectedElement.
    public void OnEndDrag(PointerEventData eventData)
    {
        Debug.Log("DROP");
        Utils.SetLayerRecursively(mSelectedElement, LayerMask.NameToLayer("dropable"));
        Instantiate(mSelectedElement);
		Destroy(mSelectedElement);
    }

    // Instantiate the associated Model, disable it and ignore raycast for this object.
    public void OnBeginDrag(PointerEventData eventData)
    {
		Debug.Log("BEGIN DRAG");
        if (mSelectedElement != null)
            return;
        mSelectedElement = Instantiate(mModelGameObject);
        mSelectedElement.SetActive(false);
        Utils.SetLayerRecursively(mSelectedElement, LayerMask.NameToLayer("Ignore Raycast"));
        mCentroid = FindCentroid(mSelectedElement.GetComponentsInChildren<MeshFilter>());
    }

    private Vector3 FindCentroid ( MeshFilter [] targets ) {
 
        Vector3 centroid;
        Vector3 minPoint = targets[ 0 ].mesh.bounds.center;
        Vector3 maxPoint = targets[ 0 ].mesh.bounds.center;
 
        for ( int i = 1; i < targets.Length; i ++ ) {
            Vector3 pos = targets[ i ].mesh.bounds.center;
             if( pos.x < minPoint.x )
                 minPoint.x = pos.x;
             if( pos.x > maxPoint.x )
                 maxPoint.x = pos.x;
             if( pos.y < minPoint.y )
                 minPoint.y = pos.y;
             if( pos.y > maxPoint.y )
                 maxPoint.y = pos.y;
             if( pos.z < minPoint.z )
                 minPoint.z = pos.z;
             if( pos.z > maxPoint.z )
                 maxPoint.z = pos.z;
         }
 
         centroid = minPoint + 0.5f * ( maxPoint - minPoint );
 
         return centroid;
 
     }  
}
