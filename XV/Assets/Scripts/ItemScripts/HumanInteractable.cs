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

	private Vector3 mItemPosition;

	private AEntity mObjectHeld;

	protected override void Start()
	{
		base.Start();

		mMovableEntity = GetComponent<MovableEntity>();
		mAnimator = GetComponent<Animator>();

		mItemPosition = new Vector3(0F, 0.813F, 0.308F);
	}

	protected override void PostPoppingEntity()
	{
		mMovableEntity.OnStartMovement.Add(OnStartMovement);
		mMovableEntity.OnEndMovement.Add(OnEndMovement);

		CreateInteraction(new ItemInteraction() {
			Name = "TakeObject",
			Help = "Take an object",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.SMALL_ITEM, EntityParameters.EntityType.MEDIUM_ITEM },
			AnimationImpl = TakeObjectMoveToTarget,
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
		StartCoroutine(InteractionWaitForTargetAsync("TakeObject"));
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
							action = TakeObjectMoveToTarget
						});

						lInteractionSteps.Add(new InteractionStep {
							tag = lAnimationParameters,
							action = TakeObjectAnimationTake
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

	private bool TakeObjectMoveToTarget(object iParams)
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

	private bool TakeObjectAnimationTake(object iParams)
	{
		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		if (lTarget == null || mObjectHeld != null)
			return true;

		mAnimator.SetTrigger("PickUp");

		if (mAnimator.GetCurrentAnimatorStateInfo(0).IsName("PickUp")) {
			mAnimator.SetBool("IdleWithBox", true);
			mAnimator.ResetTrigger("PickUp");
		}

		if (mAnimator.GetCurrentAnimatorStateInfo(0).IsName("IdleWithBox")) {
			mObjectHeld = lTarget.GetComponent<AEntity>();
			mObjectHeld.Selected = false;
			mObjectHeld.NavMeshObjstacleEnabled = false;

			lTarget.transform.parent = gameObject.transform;
			lTarget.transform.localPosition = mItemPosition;
			return true;
		}

		return false;
	}

	#endregion TakeObject

	private void OnHumanMove(AEntity iEntity)
	{
		mMovableEntity.OnMoveClick();
	}

	public void ResetWorldState()
	{
		mObjectHeld.NavMeshObjstacleEnabled = true;
		mObjectHeld = null;
		mAnimator.SetFloat("Forward", 0F);
		mAnimator.ResetTrigger("PickUp");
		mAnimator.SetBool("WalkingWithBox", false);
		mAnimator.SetBool("IdleWithBox", false);
		mAnimator.SetBool("Pushing", false);
	}

	private void OnEndMovement()
	{
		if (mObjectHeld == null)
			mAnimator.SetFloat("Forward", 0F);
		else
			mAnimator.SetBool("WalkingWithBox", true);
	}

	private void OnStartMovement()
	{
		if (mObjectHeld == null)
			mAnimator.SetFloat("Forward", 0.8F);
		else
			mAnimator.SetBool("WalkingWithBox", true);
	}
}
