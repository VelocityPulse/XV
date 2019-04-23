using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIClip : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler {

    private RectTransform mTrackRectTransform;
	private RectTransform mRectTransform;
	private UITrack mTrack;
	private float mOffset;

	public float Size
	{
		get { return mRectTransform.sizeDelta.x; }
	}

    private void Start() {
        mTrackRectTransform = transform.parent as RectTransform;
		mRectTransform = transform as RectTransform;
		mTrack = transform.parent.GetComponent<UITrack>();
    }

    public void OnPointerDown(PointerEventData iData) {
		Vector2 lLocalPointerPosition;
        mRectTransform.SetAsLastSibling ();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mTrackRectTransform, iData.position, iData.pressEventCamera, out lLocalPointerPosition);
		mOffset = mRectTransform.localPosition.x - lLocalPointerPosition.x;
    }

    public void OnDrag(PointerEventData iData) {
        Vector2 lPointerPostion = iData.position;
        Vector2 lLocalPointerPosition;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            mTrackRectTransform, lPointerPostion, iData.pressEventCamera, out lLocalPointerPosition
        )) {
			float lXMin = GetLeftLimit() + Size / 2F - mOffset;
			float lXMax = GetRightLimit() - Size / 2F - mOffset;
			lLocalPointerPosition.x = Mathf.Clamp(lLocalPointerPosition.x, lXMin, lXMax);
			lLocalPointerPosition.y = 0F;
            mRectTransform.localPosition = lLocalPointerPosition + new Vector2(mOffset, 0F);
        }
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
				lLeftLimit = mTrack.GetLimits().x;
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
				lRightLimit = mTrack.GetLimits().y;
			}
		}
		return lRightLimit;
	}
}
