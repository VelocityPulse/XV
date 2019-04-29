using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIClip : MonoBehaviour, IPointerDownHandler, IPointerClickHandler, IDragHandler
{
    private RectTransform mTrackRectTransform;
	private RectTransform mRectTransform;
	private float mOffset;
	public static float sSizeMin = 25F;

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
		get { return mRectTransform.rect.size.x; }
	}

	public UITrack Track { get; private set; }

    private void Awake() {
        mTrackRectTransform = transform.parent as RectTransform;
		mRectTransform = transform as RectTransform;
		Track = transform.GetComponentInParent<UITrack>();
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
			ResizeEvent(Time.deltaTime);
        }
    }

	public void OnPointerClick(PointerEventData iData)
	{
		if (iData.button == PointerEventData.InputButton.Middle) {
			if (Track != null) {
				Track.DeleteClip(this);
			}
		}
	}

	public void Build(float iSize, float iPosition)
	{
		mRectTransform.localPosition = new Vector3(iPosition, 0F, 0F);
		mRectTransform.sizeDelta = new Vector2(iSize, mRectTransform.sizeDelta.y);
	}

	private void FitInPlace()
	{
		float lRightLimit = GetRightLimit();
		float lLeftLimit = GetLeftLimit();
		float lPotentialSize = lRightLimit - lLeftLimit;
		float lOffset;
		if (lPotentialSize < Size) {
			mRectTransform.sizeDelta = new Vector2(lPotentialSize, mRectTransform.sizeDelta.y);
		}
		if (mRectTransform.localPosition.x - Size / 2F < lLeftLimit) {
			lOffset = lLeftLimit - (mRectTransform.localPosition.x - Size / 2F);
			mRectTransform.localPosition += new Vector3(lOffset, 0F, 0F);
		}
		if (mRectTransform.localPosition.x + Size / 2F > lRightLimit) {
			lOffset = (mRectTransform.localPosition.x + Size / 2F) - lRightLimit;
			mRectTransform.localPosition -= new Vector3(lOffset, 0F, 0F);
		}
		ResizeEvent(0F);
	}

	public void ResizeEvent(float iStartGrow)
	{
		TimelineEvent.Data lEventData = new TimelineEvent.Data(Track.ID);
		lEventData.ClipIndex = Track.GetIndex(this);
		lEventData.ClipLength = TimelineUtility.ClipSizeToDuration(mRectTransform.rect.size.x, Track.Size);
		lEventData.ClipStart = TimelineUtility.ClipPositionToStart(mRectTransform.localPosition.x, Track.GetLimits()) - lEventData.ClipLength / 2F;
		lEventData.ClipStart += iStartGrow;
		TimelineEvent.OnUIResizeClip(lEventData);
	}

	public float GetLeftLimit()
	{
		float lLeftLimit = 0F;
		if (Track != null) {
			int lClipIndex = Track.GetPreviousAtPosition(transform.localPosition.x);
			UIClip lPreviousClip = Track.GetClip(lClipIndex);
			if (lPreviousClip != null) {
				lLeftLimit = lPreviousClip.transform.localPosition.x + lPreviousClip.Size / 2.0F;
			}
			else {
				lLeftLimit = Track.GetLimits().x;
			}
		}
		return lLeftLimit;
	}

	public float GetRightLimit()
	{
		float lRightLimit = 0F;
		if (Track != null) {
			int lClipIndex = Track.GetNextAtPosition(transform.localPosition.x);
			UIClip lNextClip = Track.GetClip(lClipIndex);
			if (lNextClip != null) {
				lRightLimit = lNextClip.transform.localPosition.x - lNextClip.Size / 2.0F;
			}
			else {
				lRightLimit = Track.GetLimits().y;
			}
		}
		return lRightLimit;
	}
}
