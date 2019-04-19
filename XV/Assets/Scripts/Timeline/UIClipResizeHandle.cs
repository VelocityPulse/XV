using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIClipResizeHandle : MonoBehaviour, IPointerDownHandler, IDragHandler {

	private RectTransform mRectTransform;

	public enum HandleSide { HANDLE_LEFT, HANDLE_RIGHT }

	[SerializeField]
	private HandleSide side;

	private float mSizeMin;
	private float mSizeMax;
	private Vector2 mCurrentPointerPos;
	private Vector2 mPreviousPointerPos;

	private void Start()
	{
		mRectTransform = transform.parent.GetComponent<RectTransform>();
		mSizeMin = 25.0F;
		mSizeMax = 300.0F;
	}

	public void OnPointerDown(PointerEventData iData)
	{
		mRectTransform.SetAsLastSibling();
		RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, iData.position, iData.pressEventCamera, out mPreviousPointerPos);
	}

	public void OnDrag(PointerEventData iData)
	{
		float lSizeDelta = mRectTransform.sizeDelta.x;
		float lResizeValue = 0F;

		RectTransformUtility.ScreenPointToLocalPointInRectangle(mRectTransform, iData.position, iData.pressEventCamera, out mCurrentPointerPos);

		if (side == HandleSide.HANDLE_LEFT) {
			lResizeValue = mPreviousPointerPos.x - mCurrentPointerPos.x;
		}
		else {
			lResizeValue = mCurrentPointerPos.x - mPreviousPointerPos.x;
		}

		lSizeDelta += lResizeValue;

		float lTmpSizeDelta = Mathf.Clamp(lSizeDelta, mSizeMin, mSizeMax);
		lResizeValue -= (lSizeDelta - lTmpSizeDelta);
		lSizeDelta = lTmpSizeDelta;

		if (lSizeDelta != mRectTransform.sizeDelta.x) {

			mRectTransform.sizeDelta = new Vector2(lSizeDelta, mRectTransform.sizeDelta.y);
			if (side == HandleSide.HANDLE_RIGHT) {
				mRectTransform.anchoredPosition += new Vector2(lResizeValue / 2.0F, 0F);
			}
			else {
				mRectTransform.anchoredPosition -= new Vector2(lResizeValue / 2.0F, 0F);
			}
		}
		mPreviousPointerPos = mCurrentPointerPos;
	}
}
