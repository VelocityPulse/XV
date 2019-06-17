using System.Collections;
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

	private ManifactureInteractable mObjectMounted;

	private ItemInteraction mTakeObjectInteraction;

	private ItemInteraction mMountObjectInteraction;

	//private ItemInteraction mPushObjectInteraction;

	private UIBubbleInfoButton mTakeOffBubbleButton;

	private UIBubbleInfoButton mUnmountBubbleButton;

	private UIBubbleInfoButton mReleasePushBubbleButton;

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
			AnimationImpl = MoveToTargetCallback,
			AInteraction = this,
			Button = new UIBubbleInfoButton() {
				Text = "Take",
				Tag = name + "_TAKE_OBJECT",
				ClickAction = OnClickTakeObject
			}
		});

		mMountObjectInteraction = CreateInteraction(new ItemInteraction() {
			Name = "Mount",
			Help = "Mount an object",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.VEHICLE },
			AnimationImpl = MountObjectCallback,
			AInteraction = this,
			Button = new UIBubbleInfoButton() {
				Text = "Mount",
				Tag = name + "_MOUNT_OBJECT",
				ClickAction = OnClickMountObject
			}
		});

		//mPushObject = CreateInteraction(new ItemInteraction() {
		//	Name = "Handle",
		//	Help = "Handle an object",
		//	InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.SMALL_ITEM, EntityParameters.EntityType.MEDIUM_ITEM },
		//	AnimationImpl = HandleObjectCallback,
		//	AInteraction = this,
		//	Button = new UIBubbleInfoButton() {
		//		Text = "Handle",
		//		Tag = name + "_HANDLE_OBJECT",
		//		ClickAction = OnClickTakeObject
		//	}
		//});

		mTakeOffBubbleButton = new UIBubbleInfoButton() {
			Text = "Take off",
			Tag = "TakeOffButton",
			ClickAction = OnClickTakeOffObject
		};

		mUnmountBubbleButton = new UIBubbleInfoButton() {
			Text = "Unmount",
			Tag = "UnmountButton",
			ClickAction = OnClickUnmount
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


	#region MountObject

	private void OnClickMountObject(AEntity iEntity)
	{

		StartCoroutine(InteractionWaitForTarget("Mount", (iEntityParam) => {
			AnimationParameters lAnimationParameters = new AnimationParameters() {
				TargetType = AnimationParameters.AnimationTargetType.ENTITY,
				AnimationTarget = iEntityParam.gameObject,
			};

			List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

			lInteractionSteps.Add(new InteractionStep {
				tag = lAnimationParameters,
				action = MoveToTargetCallback
			});

			lInteractionSteps.Add(new InteractionStep {
				tag = lAnimationParameters,
				action = MountObjectCallback
			});

			TimelineManager.Instance.AddInteraction(iEntity.gameObject, lInteractionSteps);

		}));
	}

	private bool MountObjectCallback(object iParams)
	{
		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		ManifactureInteractable lMI = lTarget.GetComponent<ManifactureInteractable>();

		mObjectMounted = lMI;
		lMI.HoldHuman(this);
		OnMount();
		return true;
	}

	private void OnClickUnmount(AEntity iEntity)
	{
		AnimationParameters lAnimationParameters = new AnimationParameters() {
			TargetType = AnimationParameters.AnimationTargetType.ENTITY,
		};

		List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

		lInteractionSteps.Add(new InteractionStep {
			tag = lAnimationParameters,
			action = UnmountObjectCallback
		});

		TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps);
	}

	private bool UnmountObjectCallback(object iParams)
	{
		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		mObjectMounted.DropHuman(this);
		OnUnmount();
		return true;
	}

	private void OnMount()
	{
		ResetAnimator();

		mEntity.NavMeshObjstacleEnabled = false;
		mEntity.LockWorldEditorDeplacement = true;

		mMountObjectInteraction.Enabled = false;
		//mPushObjectInteraction.Enabled = false;
		mTakeObjectInteraction.Enabled = false;

		if (!mEntity.ContainsBubbleInfoButton(mUnmountBubbleButton))
			mEntity.CreateBubbleInfoButton(mUnmountBubbleButton);

		mEntity.StashUIBubbleButtons(mUnmountBubbleButton);
	}

	private void OnUnmount()
	{
		ResetAnimator();

		mEntity.NavMeshObjstacleEnabled = true;
		mEntity.LockWorldEditorDeplacement = false;

		mMountObjectInteraction.Enabled = true;
		//mPushObjectInteraction.Enabled = true;
		mTakeObjectInteraction.Enabled = true;

		if (mEntity.ContainsBubbleInfoButton(mUnmountBubbleButton))
			mEntity.DestroyBubbleInfoButton(mUnmountBubbleButton);
		mEntity.StashPopUIBubbleInfoButtons();

		if (mObjectMounted != null)
			mObjectMounted = null;
	}

	#endregion MountObject

	#region TakeObject

	private void OnClickTakeObject(AEntity iEntity)
	{
		StartCoroutine(InteractionWaitForTarget("Take", (iEntityParameter) => {
			AnimationParameters lAnimationParameters = new AnimationParameters() {
				TargetType = AnimationParameters.AnimationTargetType.ENTITY,
				AnimationTarget = iEntityParameter.gameObject,
			};

			List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

			lInteractionSteps.Add(new InteractionStep {
				tag = lAnimationParameters,
				action = MoveToTargetCallback
			});

			lInteractionSteps.Add(new InteractionStep {
				tag = lAnimationParameters,
				action = TakeObjectAnimationTakeCallback
			});

			lInteractionSteps.Add(new InteractionStep {
				tag = lAnimationParameters,
				action = TakeObjectWaitAnimationEndCallback
			});

			TimelineManager.Instance.AddInteraction(iEntity.gameObject, lInteractionSteps);
		}));
	}

	private bool TakeObjectAnimationTakeCallback(object iParams)
	{
		if (mObjectMounted != null)
			return true;

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
		if (mObjectMounted != null)
			return true;

		if (mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
			return true;
		return false;
	}

	private void OnClickTakeOffObject(AEntity iEntity)
	{
		AnimationParameters lAnimationParameters = new AnimationParameters() {
			TargetType = AnimationParameters.AnimationTargetType.ENTITY
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

	private void OnHold()
	{
		mMountObjectInteraction.Enabled = false;
		//mPushObjectInteraction.Enabled = false;
		mTakeObjectInteraction.Enabled = false;

		if (!mEntity.ContainsBubbleInfoButton(mTakeOffBubbleButton))
			mEntity.CreateBubbleInfoButton(mTakeOffBubbleButton);
	}

	private void OnUnhold()
	{
		mEntity.DestroyBubbleInfoButton(mTakeOffBubbleButton);
		mMountObjectInteraction.Enabled = true;
		//mPushObjectInteraction.Enabled = true;
		mTakeObjectInteraction.Enabled = true;
		if (mObjectHeld != null) {
			mObjectHeld.NavMeshObjstacleEnabled = true;
			mObjectHeld = null;
		} else
			Debug.LogWarning("[HUMAN INTERACTABLE] Object Held shouldn't be null in OnUnhold");
	}

	#endregion TakeObject

	private bool MoveToTargetCallback(object iParams)
	{
		if (mObjectMounted != null)
			return true;

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

	public void ResetWorldState()
	{
		if (mObjectHeld != null)
			OnUnhold();
		if (mObjectMounted != null)
			OnUnmount();
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
