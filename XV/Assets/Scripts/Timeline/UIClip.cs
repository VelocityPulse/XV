using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIClip : MonoBehaviour, IPointerDownHandler, IDragHandler {

    private RectTransform mTrackRectTransform;
	private RectTransform mRectTransform;

    private void Start() {
        mTrackRectTransform = transform.parent as RectTransform;
		mRectTransform = transform as RectTransform;
    }

    public void OnPointerDown(PointerEventData iData) {
        mRectTransform.SetAsLastSibling ();
    }

    public void OnDrag(PointerEventData iData) {
        Vector2 lPointerPostion = ClampToTrack(iData);
        Vector2 lLocalPointerPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle (
            mTrackRectTransform, lPointerPostion, iData.pressEventCamera, out lLocalPointerPosition
        )) {
            mRectTransform.localPosition = lLocalPointerPosition;
        }
    }

    Vector2 ClampToTrack(PointerEventData iData) {
        float lRawPointerPosition = iData.position.x;
        Vector3[] lTrackCorners = new Vector3[4];

        mTrackRectTransform.GetWorldCorners(lTrackCorners);

		float lMinX = lTrackCorners[0].x + mRectTransform.sizeDelta.x / 2.0F;
		float lMaxX = lTrackCorners[2].x - mRectTransform.sizeDelta.x / 2.0F;
        float lClampedX = Mathf.Clamp(lRawPointerPosition, lMinX, lMaxX);

        return new Vector2(lClampedX, mTrackRectTransform.position.y);
    }
}
