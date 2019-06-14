﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

[RequireComponent(typeof(MovableEntity))]
[RequireComponent(typeof(Animator))]
public class HumanInteractable : AInteraction
{
	private MovableEntity mMovableEntity;

	private Animator mAnimator;

	private Vector3 mItemTakenPosition;

	private Vector3 mItemPutPosition;

	private AEntity mObjectHeld;

	private ItemInteraction mTakeObjectInteraction;

	private UIBubbleInfoButton mTakeOffBubbleButton;

	protected override void Start()
	{
		base.Start();

		mMovableEntity = GetComponent<MovableEntity>();
		mAnimator = GetComponent<Animator>();

		mItemTakenPosition = new Vector3(0F, 0.813F, 0.308F);
		mItemPutPosition = new Vector3(0F, 0.039F, 0.7F);
	}

	protected override void PostPoppingEntity()
	{
		mMovableEntity.OnStartMovement.Add(OnStartMovement);
		mMovableEntity.OnEndMovement.Add(OnEndMovement);

		mTakeObjectInteraction = CreateInteraction(new ItemInteraction() {
			Name = "Take",
			Help = "Take an object",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.SMALL_ITEM, EntityParameters.EntityType.MEDIUM_ITEM },
			AnimationImpl = TakeObjectMoveToTargetCallback,
			AInteraction = this,
			Button = new UIBubbleInfoButton() {
				Text = "TakeObject",
				Tag = name + "_TAKE_OBJECT",
				ClickAction = OnClickTakeObject
			}
		});

		Button lButton = mEntity.CreateBubbleInfoButton(new UIBubbleInfoButton() {
			Text = "Move",
			ClickAction = OnHumanMove
		});
		mMovableEntity.SetMoveButton(lButton);

		mTakeOffBubbleButton = new UIBubbleInfoButton() {
			Text = "Take off",
			Tag = "TakeOffButton",
			ClickAction = OnClickTakeOffObject
		};
	}

	private void OnDestroy()
	{
		if (mObjectHeld != null) {
			mObjectHeld.transform.parent = null;
			mObjectHeld.RemoveEntity();
			mObjectHeld.Dispose();
		}
	}

	#region TakeObject

	private void OnClickTakeObject(AEntity iEntity)
	{
		StartCoroutine(InteractionWaitForTargetAsync("Take"));
	}

	private IEnumerator InteractionWaitForTargetAsync(string iInteractionName)
	{
		AEntity.HideNoInteractable(GetItemInteraction(iInteractionName).InteractWith, mEntity);
		yield return new WaitWhile(() => {

			if (Input.GetMouseButtonDown(0)) {

				RaycastHit lHit;
				Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(lRay, out lHit, 100000, LayerMask.GetMask("dropable"))) {

					Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);

					// If we don't found EntityParameters, stop with an error.
					EntityParameters lEntityParam;
					if ((lEntityParam = lHit.collider.gameObject.GetComponentInParent<EntityParameters>()) == null) {
						Debug.LogWarning("[TARGET SELECTOR] Exit target selector !");
						return false;
					}

					// If Subscribers contain the clicked Entity type use it as target and add animation to timeline
					if (IsInteractionCanInteractType(iInteractionName, lEntityParam.Type)) {
						AnimationParameters lAnimationParameters = new AnimationParameters() {
							TargetType = AnimationParameters.AnimationTargetType.ENTITY,
							AnimationTarget = lEntityParam.gameObject,
						};

						List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

						lInteractionSteps.Add(new InteractionStep {
							tag = lAnimationParameters,
							action = TakeObjectMoveToTargetCallback
						});

						lInteractionSteps.Add(new InteractionStep {
							tag = lAnimationParameters,
							action = TakeObjectAnimationTakeCallback
						});

						lInteractionSteps.Add(new InteractionStep {
							tag = lAnimationParameters,
							action = TakeObjectWaitAnimationEndCallback
						});

						TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps);

						return false;
					}
					Debug.LogWarning("[TARGET SELECTOR] The object you click on is not interactable with this object !");
				}

				return false;
			}
			return true;
		});
		AEntity.DisableHideNoInteractable();
	}

	private bool TakeObjectMoveToTargetCallback(object iParams)
	{
		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		if (lTarget == null || mObjectHeld != null)
			return true;

		if (mMovableEntity.Move(lTarget.transform.position, lParams) == false)
			return false;

		// doesnt work
		StartCoroutine(Utils.LookAtSlerpY(gameObject, lTarget));
		return true;
	}

	private bool TakeObjectAnimationTakeCallback(object iParams)
	{
		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		if (lTarget == null || mObjectHeld != null)
			return true;

		mAnimator.SetTrigger("PickUp");

		if (mAnimator.GetCurrentAnimatorStateInfo(0).IsName("PickUp")) {
			mAnimator.SetBool("IdleWithBox", true);
		}

		if (mAnimator.GetCurrentAnimatorStateInfo(0).IsName("IdleWithBox")) {
			mAnimator.ResetTrigger("PickUp");

			mObjectHeld = lTarget.GetComponent<AEntity>();
			mObjectHeld.Selected = false;
			mObjectHeld.NavMeshObjstacleEnabled = false;

			lTarget.transform.parent = gameObject.transform;
			lTarget.transform.localPosition = mItemTakenPosition;
			OnHold();
			return true;
		}

		return false;
	}

	private bool TakeObjectWaitAnimationEndCallback(object iParams)
	{
		if (mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
			return true;
		return false;
	}

	#endregion TakeObject

	#region TakeOffObject

	private void OnClickTakeOffObject(AEntity iEntity)
	{
		AnimationParameters lAnimationParameters = new AnimationParameters() {
			TargetType = AnimationParameters.AnimationTargetType.ENTITY,
			AnimationTarget = mObjectHeld
		};

		List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

		lInteractionSteps.Add(new InteractionStep {
			tag = lAnimationParameters,
			action = TakeOffObjectCallback
		});

		TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps);
	}

	private bool TakeOffObjectCallback(object iParams)
	{
		ResetAnimator();
		mObjectHeld.transform.localPosition = mItemPutPosition;
		mObjectHeld.transform.parent = null;
		OnUnhold();
		return true;
	}

	#endregion TakeOffObject

	private void OnHold()
	{
		mTakeObjectInteraction.Enabled = false;
		if (!mEntity.ContainsBubbleInfoButton(mTakeOffBubbleButton.Tag))
			mEntity.CreateBubbleInfoButton(mTakeOffBubbleButton);
	}

	private void OnUnhold()
	{
		mEntity.DestroyBubbleInfoButton(mTakeOffBubbleButton);
		mTakeObjectInteraction.Enabled = true;
		if (mObjectHeld != null) {
			mObjectHeld.NavMeshObjstacleEnabled = true;
			mObjectHeld = null;
		} else
			Debug.LogWarning("[HUMAN INTERACTABLE] Object Held shouldn't be null in OnUnhold");
	}

	private void OnHumanMove(AEntity iEntity)
	{
		mMovableEntity.OnMoveClick();
	}

	public void ResetWorldState()
	{
		if (mObjectHeld != null)
			OnUnhold();
		ResetAnimator();
	}

	private void ResetAnimator()
	{
		mAnimator.SetFloat("Forward", 0F);
		mAnimator.ResetTrigger("PickUp");
		mAnimator.SetBool("WalkingWithBox", false);
		mAnimator.SetBool("IdleWithBox", false);
		mAnimator.SetBool("Pushing", false);
	}

	private void OnStartMovement()
	{
		if (mObjectHeld == null)
			mAnimator.SetFloat("Forward", 0.8F);
		else
			mAnimator.SetBool("WalkingWithBox", true);
	}

	private void OnEndMovement()
	{
		if (mObjectHeld == null)
			mAnimator.SetFloat("Forward", 0F);
		else
			mAnimator.SetBool("WalkingWithBox", false);
	}

}