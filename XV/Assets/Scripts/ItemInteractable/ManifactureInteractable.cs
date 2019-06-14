using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(MovableEntity))]
public class ManifactureInteractable : AInteraction
{

	private MovableEntity mMovableEntity;

	private AEntity mObjectHeld;

	private AEntity mHumanEntity;

	private ItemInteraction mTakeObjectInteraction;

	private UIBubbleInfoButton mTakeOffBubbleButton;

	protected override void Start()
	{
		base.Start();

		mMovableEntity = GetComponent<MovableEntity>();
	}


	protected override void PostPoppingEntity()
	{
		mMovableEntity.OnStartMovement.Add(OnStartMovement);
		mMovableEntity.OnEndMovement.Add(OnEndMovement);

		if (mEntity.EntityParameters.Type == EntityParameters.EntityType.CUPBOARD)
			return;

		mTakeObjectInteraction = CreateInteraction(new ItemInteraction() {
			Name = "Take",
			Help = "Take an object",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.MEDIUM_ITEM, EntityParameters.EntityType.HEAVY_ITEM },
			AnimationImpl = TakeObjectMoveToTargetCallback,
			AInteraction = this,
			Button = new UIBubbleInfoButton() {
				Text = "Take",
				Tag = name + "_TAKE_OBJECT",
				ClickAction = OnClickTakeObject
			}
		});

		mTakeOffBubbleButton = new UIBubbleInfoButton() {
			Text = "Take off",
			Tag = "TakeOffButton",
			ClickAction = OnClickTakeOffObject
		};
	}


	private void OnManifactureMove(AEntity iEntity)
	{

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
							action = TakeObjectPickCallback
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

	private bool TakeObjectPickCallback(object iParams)
	{
		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		if (lTarget == null || mObjectHeld != null)
			return true;

		mObjectHeld = lTarget.GetComponent<AEntity>();
		mObjectHeld.Selected = false;
		mObjectHeld.NavMeshObjstacleEnabled = false;

		lTarget.transform.parent = gameObject.transform;
		lTarget.transform.localPosition = mEntity.EntityParameters.VehiculeHoldPosition;
		lTarget.transform.localRotation = Quaternion.Euler(0, 0, 0);
		OnHold();

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
		//ResetAnimator();
		mObjectHeld.transform.localPosition = mEntity.EntityParameters.VehiculeDropPosition;
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


	public void HoldHuman(HumanInteractable iHuman)
	{
		iHuman.transform.parent = transform;
		iHuman.transform.localPosition = mEntity.EntityParameters.VehiculeSitPosition;
		iHuman.transform.localRotation = Quaternion.Euler(0, 0, 0);
	}


	private void OnStartMovement()
	{

	}

	private void OnEndMovement()
	{

	}
}
