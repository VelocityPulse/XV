using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIClip : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler {

    private RectTransform mTrackRectTransform;
	private RectTransform mRectTransform;
	private UITrack mTrack;
	private float mOffset;
	public static float sMinSize = 25F;

	[SerializeField]
	private Text nameText;
	public string Name
	{
		get { return nameText.text; }
		set
		{
			if (value != string.Empty) {
				nameText.text = value;
			}
		}
	}

	public float Size
	{
		get { return mRectTransform.sizeDelta.x; }
		set
		{
			mRectTransform.sizeDelta = new Vector2(value, mRectTransform.sizeDelta.y);
			FitInPlace();
		}
	}

    private void Awake() {
        mTrackRectTransform = transform.parent as RectTransform;
		mRectTransform = transform as RectTransform;
		mTrack = transform.GetComponentInParent<UITrack>();
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

	public void Build(float iSize, float iPosition)
	{
		mRectTransform.localPosition = new Vector3(iPosition, 0F, 0F);
		Size = iSize;
	}

	private void FitInPlace()
	{
		float lPotentialSize = GetRightLimit() - GetLeftLimit();
		if (lPotentialSize < Size) {
			mRectTransform.sizeDelta = new Vector2(lPotentialSize, mRectTransform.sizeDelta.y);
		}
		if (mRectTransform.localPosition.x - Size / 2F < GetLeftLimit()) {
			float lOffset = GetLeftLimit() - (mRectTransform.localPosition.x - Size / 2F);
			mRectTransform.anchoredPosition += new Vector2(lOffset, 0F);
		}
		if (mRectTransform.localPosition.x + Size / 2F > GetRightLimit()) {
			float lOffset = (mRectTransform.localPosition.x + Size / 2F) - GetRightLimit();
			mRectTransform.anchoredPosition -= new Vector2(lOffset, 0F);
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
