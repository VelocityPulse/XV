using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIClip : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler {

    private RectTransform mTrackRectTransform;
	private RectTransform mRectTransform;
	private UITrack mTrack;

	public float Size
	{
		get
		{
			return mRectTransform.sizeDelta.x;
		}
	}

    private void Start() {
        mTrackRectTransform = transform.parent as RectTransform;
		mRectTransform = transform as RectTransform;
		mTrack = transform.parent.GetComponent<UITrack>();
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

	public void OnPointerClick(PointerEventData iData)
	{
		if (iData.button == PointerEventData.InputButton.Middle) {
			if (mTrack != null) {
				mTrack.DeleteClip(this);
			}
		}
	}

	public float GetLeftLimit()
	{
		float lLeftLimit = 0F;
		if (mTrack != null) {
			int lClipIndex = mTrack.GetPreviousAtPosition(transform.localPosition.x);
			UIClip lPreviousClip = mTrack.GetClip(lClipIndex);
			if (lPreviousClip != null) {
				lLeftLimit = lPreviousClip.transform.localPosition.x + lPreviousClip.Size / 2.0F;
			}
			else {
				// Clamp to track limits ?
			}
		}
		return lLeftLimit;
	}

	public float GetRightLimit()
	{
		float lRightLimit = 0F;
		if (mTrack != null) {
			int lClipIndex = mTrack.GetNextAtPosition(transform.localPosition.x);
			UIClip lNextClip = mTrack.GetClip(lClipIndex);
			if (lNextClip != null) {
				lRightLimit = lNextClip.transform.localPosition.x - lNextClip.Size / 2.0F;
			}
			else {
				// Clamp to track limits ?
			}
		}
		return lRightLimit;
	}
}
