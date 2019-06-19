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

    private ManifactureInteractable mObjectPushed;

    private ItemInteraction mTakeObjectInteraction;

	private ItemInteraction mMountObjectInteraction;

    private ItemInteraction mPushObjectInteraction;

    private UIBubbleInfoButton mTakeOffBubbleButton;

	private UIBubbleInfoButton mUnmountBubbleButton;

	private UIBubbleInfoButton mReleaseBubbleButton;

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
			},
		});

        mPushObjectInteraction = CreateInteraction(new ItemInteraction() {
            Name = "Handle",
            Help = "Handle an object",
            InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.TROLLEY },
            AnimationImpl = PushObjectCallback,
            AInteraction = this,
            Button = new UIBubbleInfoButton() {
                Text = "Handle",
                Tag = name + "_HANDLE_OBJECT",
                ClickAction = OnClickPushObject
            }
        });

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

        mReleaseBubbleButton = new UIBubbleInfoButton() {
            Text = "Release",
            Tag = "ReleaseButton",
            ClickAction = OnClickRelease
        };

        CheckAndAddInteractionsSaved();
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
		StartCoroutine(InteractionWaitForTarget("Mount", (iTargetEntityParameters) => {

            AnimationParameters lAnimationParameters = new AnimationParameters() {
				TargetType = AnimationParameters.AnimationTargetType.ENTITY,
				AnimationTarget = iTargetEntityParameters.gameObject,
                Speed = mMovableEntity.ComputeSpeed(),
                Acceleration = mMovableEntity.ComputeAcceleration(),
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

			TimelineManager.Instance.AddInteraction(iEntity.gameObject, lInteractionSteps, TimelineManager.Instance.Time);

            GameManager.Instance.TimeLineSerialized.HumanInteractionList.Add(new HumanInteraction() {
                InteractionType = HumanInteractionType.MOUNT,
                TargetGUID = iEntity.AODS.GUID,
                ObjectUseInInteractionGUID = iTargetEntityParameters.gameObject.GetComponent<AEntity>().AODS.GUID,
				Time = TimelineManager.Instance.Time
			});
			GameManager.Instance.CurrentDataScene.Serialize();

		}));
	}

	private bool MountObjectCallback(object iParams)
	{
		if (TimelineManager.sGlobalState == TimelineManager.State.STOP)
			return true;

		if (TimelineManager.sGlobalState == TimelineManager.State.PAUSE)
			return false;

		AnimationParameters lParams = (AnimationParameters)iParams;
		GameObject lTarget = (GameObject)lParams.AnimationTarget;

		ManifactureInteractable lMI = lTarget.GetComponent<ManifactureInteractable>();

		mObjectMounted = lMI;
		lMI.HoldHuman(this, HumanInteractionType.MOUNT);
		OnMount();
		return true;
	}

	private void OnClickUnmount(AEntity iEntity)
	{
		AnimationParameters lAnimationParameters = new AnimationParameters() {
			TargetType = AnimationParameters.AnimationTargetType.ENTITY,
            Speed = mMovableEntity.ComputeSpeed(),
            Acceleration = mMovableEntity.ComputeAcceleration(),
        };

		List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

		lInteractionSteps.Add(new InteractionStep {
			tag = lAnimationParameters,
			action = UnmountObjectCallback
		});

		TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps, TimelineManager.Instance.Time);

		GameManager.Instance.TimeLineSerialized.HumanInteractionList.Add(new HumanInteraction() {
			InteractionType = HumanInteractionType.UNMOUNT,
			Time = TimelineManager.Instance.Time
		});
		GameManager.Instance.CurrentDataScene.Serialize();
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
        mPushObjectInteraction.Enabled = false;
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
        mPushObjectInteraction.Enabled = true;
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
		StartCoroutine(InteractionWaitForTarget("Take", (iTargetEntityParameters) => {
			AnimationParameters lAnimationParameters = new AnimationParameters() {
				TargetType = AnimationParameters.AnimationTargetType.ENTITY,
				AnimationTarget = iTargetEntityParameters.gameObject,
                Speed = mMovableEntity.ComputeSpeed(),
                Acceleration = mMovableEntity.ComputeAcceleration(),
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

			TimelineManager.Instance.AddInteraction(iEntity.gameObject, lInteractionSteps, TimelineManager.Instance.Time);

			GameManager.Instance.TimeLineSerialized.HumanInteractionList.Add(new HumanInteraction() {
				InteractionType = HumanInteractionType.TAKE,
				TargetGUID = iEntity.AODS.GUID,
                ObjectUseInInteractionGUID = iTargetEntityParameters.gameObject.GetComponent<AEntity>().AODS.GUID,
                Time = TimelineManager.Instance.Time
			});
			GameManager.Instance.CurrentDataScene.Serialize();
		}));
	}

	private bool TakeObjectAnimationTakeCallback(object iParams)
	{
		if (mObjectMounted != null)
			return true;

		if (TimelineManager.sGlobalState == TimelineManager.State.STOP)
			return true;

		if (TimelineManager.sGlobalState == TimelineManager.State.PAUSE)
			return false;

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

		if (TimelineManager.sGlobalState == TimelineManager.State.STOP) {
			ResetAnimator();
			return true;
		}

		if (TimelineManager.sGlobalState == TimelineManager.State.PAUSE)
			return false;

		if (mAnimator.GetCurrentAnimatorStateInfo(0).normalizedTime > 1)
			return true;
		return false;
	}

	private void OnClickTakeOffObject(AEntity iEntity)
	{
		AnimationParameters lAnimationParameters = new AnimationParameters() {
			TargetType = AnimationParameters.AnimationTargetType.ENTITY,
            Speed = mMovableEntity.ComputeSpeed(),
            Acceleration = mMovableEntity.ComputeAcceleration(),
        };

		List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

		lInteractionSteps.Add(new InteractionStep {
			tag = lAnimationParameters,
			action = TakeOffObjectCallback
		});

		TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps, TimelineManager.Instance.Time);

		GameManager.Instance.TimeLineSerialized.HumanInteractionList.Add(new HumanInteraction() {
			InteractionType = HumanInteractionType.TAKEOFF,
			Time = TimelineManager.Instance.Time
		});
		GameManager.Instance.CurrentDataScene.Serialize();
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
        mPushObjectInteraction.Enabled = false;
        mTakeObjectInteraction.Enabled = false;

		if (!mEntity.ContainsBubbleInfoButton(mTakeOffBubbleButton))
			mEntity.CreateBubbleInfoButton(mTakeOffBubbleButton);
	}

	private void OnUnhold()
	{
		mEntity.DestroyBubbleInfoButton(mTakeOffBubbleButton);
		mMountObjectInteraction.Enabled = true;
        mPushObjectInteraction.Enabled = true;
        mTakeObjectInteraction.Enabled = true;
		if (mObjectHeld != null) {
			mObjectHeld.NavMeshObjstacleEnabled = true;
			mObjectHeld = null;
		} else
			Debug.LogWarning("[HUMAN INTERACTABLE] Object Held shouldn't be null in OnUnhold");
	}

    #endregion TakeObject

    #region PushObject

    private void OnClickPushObject(AEntity iEntity)
    {
        StartCoroutine(InteractionWaitForTarget("Handle", (iEntityParam) => {

            AnimationParameters lAnimationParameters = new AnimationParameters() {
                TargetType = AnimationParameters.AnimationTargetType.ENTITY,
                AnimationTarget = iEntityParam.gameObject,
                Speed = mMovableEntity.ComputeSpeed(),
                Acceleration = mMovableEntity.ComputeAcceleration(),
            };

            List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

            lInteractionSteps.Add(new InteractionStep {
                tag = lAnimationParameters,
                action = MoveToTargetCallback
            });

            lInteractionSteps.Add(new InteractionStep {
                tag = lAnimationParameters,
                action = PushObjectCallback
            });

            TimelineManager.Instance.AddInteraction(iEntity.gameObject, lInteractionSteps);

            GameManager.Instance.TimeLineSerialized.HumanInteractionList.Add(new HumanInteraction() {
                InteractionType = HumanInteractionType.PUSH,
                TargetGUID = iEntity.AODS.GUID,
                ObjectUseInInteractionGUID = iEntityParam.gameObject.GetComponent<AEntity>().AODS.GUID,
                Time = TimelineManager.Instance.Time
            });
            GameManager.Instance.CurrentDataScene.Serialize();

        }));
    }

    private bool PushObjectCallback(object iParams)
    {
        if (TimelineManager.sGlobalState == TimelineManager.State.STOP)
            return true;

        if (TimelineManager.sGlobalState == TimelineManager.State.PAUSE)
            return false;

        AnimationParameters lParams = (AnimationParameters)iParams;
        GameObject lTarget = (GameObject)lParams.AnimationTarget;

        ManifactureInteractable lMI = lTarget.GetComponent<ManifactureInteractable>();

        mObjectPushed = lMI;
        lMI.HoldHuman(this, HumanInteractionType.PUSH, OnManufactureStartMove, OnManufactureEndMove);
        OnPush();
        return true;
    }

    private void OnManufactureStartMove()
    {
        if (mObjectPushed != null)
            mAnimator.SetBool("Pushing", true);
    }

    private void OnManufactureEndMove()
    {
        if (mObjectPushed != null)
            mAnimator.SetBool("Pushing", false);
    }

    private void OnClickRelease(AEntity iEntity)
    {
        AnimationParameters lAnimationParameters = new AnimationParameters() {
            TargetType = AnimationParameters.AnimationTargetType.ENTITY,
            Speed = mMovableEntity.ComputeSpeed(),
            Acceleration = mMovableEntity.ComputeAcceleration(),
        };

        List<InteractionStep> lInteractionSteps = new List<InteractionStep>();

        lInteractionSteps.Add(new InteractionStep {
            tag = lAnimationParameters,
            action = ReleaseObjectCallback
        });

        TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps);

        GameManager.Instance.TimeLineSerialized.HumanInteractionList.Add(new HumanInteraction() {
            InteractionType = HumanInteractionType.STOP_PUSH,
            Time = TimelineManager.Instance.Time
        });
        GameManager.Instance.CurrentDataScene.Serialize();
    }

    private bool ReleaseObjectCallback(object iParams)
    {
        AnimationParameters lParams = (AnimationParameters)iParams;
        GameObject lTarget = (GameObject)lParams.AnimationTarget;

        mObjectPushed.DropHuman(this, OnManufactureStartMove, OnManufactureEndMove);
        OnRelease();
        return true;
    }

    private void OnPush()
    {
        ResetAnimator();

        mEntity.NavMeshObjstacleEnabled = false;
        mEntity.LockWorldEditorDeplacement = true;

        mMountObjectInteraction.Enabled = false;
        mPushObjectInteraction.Enabled = false;
        mTakeObjectInteraction.Enabled = false;

        if (!mEntity.ContainsBubbleInfoButton(mReleaseBubbleButton))
            mEntity.CreateBubbleInfoButton(mReleaseBubbleButton);

        mEntity.StashUIBubbleButtons(mReleaseBubbleButton);
    }

    private void OnRelease()
    {
        ResetAnimator();

        mEntity.NavMeshObjstacleEnabled = true;
        mEntity.LockWorldEditorDeplacement = false;

        mMountObjectInteraction.Enabled = true;
        mPushObjectInteraction.Enabled = true;
        mTakeObjectInteraction.Enabled = true;

        if (mEntity.ContainsBubbleInfoButton(mReleaseBubbleButton))
            mEntity.DestroyBubbleInfoButton(mReleaseBubbleButton);
        mEntity.StashPopUIBubbleInfoButtons();

        if (mObjectPushed != null)
            mObjectPushed = null;
    }

    #endregion PushObject

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

	public override void ResetWorldState()
	{
		if (mObjectHeld != null)
			OnUnhold();
		if (mObjectMounted != null)
			OnUnmount();
        if (mObjectPushed != null)
            OnRelease();
		ResetAnimator();
	}

	private void ResetAnimator()
	{
		if (mAnimator == null)
			return;
		
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

	private void CheckAndAddInteractionsSaved()
	{
		List<HumanInteraction> lMovableAnimationList = GameManager.Instance.TimeLineSerialized.HumanInteractionList;
		string lMyGUID = mEntity.AODS.GUID;

		foreach (HumanInteraction lInter in lMovableAnimationList) {

			AnimationParameters lAnimationParameters;
			List<InteractionStep> lInteractionSteps;

			switch (lInter.InteractionType) {
			case HumanInteractionType.MOUNT:

                AEntity lEntity = AEntity.FindGUID(lInter.TargetGUID);
                AEntity lObjectToInteractWith = AEntity.FindGUID(lInter.ObjectUseInInteractionGUID);
				if (lEntity == null) {
					Debug.LogError("[HUMAN INTERACTABLE] TargetGUID not found!");
					continue;
				}

				lAnimationParameters = new AnimationParameters() {
					TargetType = AnimationParameters.AnimationTargetType.ENTITY,
					AnimationTarget = lObjectToInteractWith.gameObject,
                    Speed = mMovableEntity.ComputeSpeed(),
                    Acceleration = mMovableEntity.ComputeAcceleration(),
				};

				lInteractionSteps = new List<InteractionStep>();

				lInteractionSteps.Add(new InteractionStep {
					tag = lAnimationParameters,
					action = MoveToTargetCallback
				});

				lInteractionSteps.Add(new InteractionStep {
					tag = lAnimationParameters,
					action = MountObjectCallback
				});

				TimelineManager.Instance.AddInteraction(lEntity.gameObject, lInteractionSteps, lInter.Time);

				break;

			case HumanInteractionType.UNMOUNT:

				lAnimationParameters = new AnimationParameters() {
					TargetType = AnimationParameters.AnimationTargetType.ENTITY,
                    Speed = mMovableEntity.ComputeSpeed(),
                    Acceleration = mMovableEntity.ComputeAcceleration(),
                };

				lInteractionSteps = new List<InteractionStep>();

				lInteractionSteps.Add(new InteractionStep {
					tag = lAnimationParameters,
					action = UnmountObjectCallback
				});

				TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps, lInter.Time);

				break;

			case HumanInteractionType.TAKE:

                lObjectToInteractWith = AEntity.FindGUID(lInter.ObjectUseInInteractionGUID);
                lEntity = AEntity.FindGUID(lInter.TargetGUID);
				if (lEntity == null) {
					Debug.LogError("[HUMAN INTERACTABLE] TargetGUID not found!");
					continue;
				}

				lAnimationParameters = new AnimationParameters() {
					TargetType = AnimationParameters.AnimationTargetType.ENTITY,
					AnimationTarget = lObjectToInteractWith.gameObject,
                    Speed = mMovableEntity.ComputeSpeed(),
                    Acceleration = mMovableEntity.ComputeAcceleration(),
                };

				lInteractionSteps = new List<InteractionStep>();

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

				TimelineManager.Instance.AddInteraction(lEntity.gameObject, lInteractionSteps, lInter.Time);

				break;

			case HumanInteractionType.TAKEOFF:

				lAnimationParameters = new AnimationParameters() {
					TargetType = AnimationParameters.AnimationTargetType.ENTITY,
                    Speed = mMovableEntity.ComputeSpeed(),
                    Acceleration = mMovableEntity.ComputeAcceleration(),
                };

				lInteractionSteps = new List<InteractionStep>();

				lInteractionSteps.Add(new InteractionStep {
					tag = lAnimationParameters,
					action = TakeOffObjectCallback
				});

				TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps, lInter.Time);

				break;

			case HumanInteractionType.PUSH:

                    lObjectToInteractWith = AEntity.FindGUID(lInter.ObjectUseInInteractionGUID);
                    lEntity = AEntity.FindGUID(lInter.TargetGUID);
                    if (lEntity == null) {
                        Debug.LogError("[HUMAN INTERACTABLE] TargetGUID not found!");
                        continue;
                    }

                    lAnimationParameters = new AnimationParameters() {
                        TargetType = AnimationParameters.AnimationTargetType.ENTITY,
                        AnimationTarget = lObjectToInteractWith.gameObject,
                        Speed = mMovableEntity.ComputeSpeed(),
                        Acceleration = mMovableEntity.ComputeAcceleration(),
                    };

                    lInteractionSteps = new List<InteractionStep>();

                    lInteractionSteps.Add(new InteractionStep {
                        tag = lAnimationParameters,
                        action = MoveToTargetCallback
                    });

                    lInteractionSteps.Add(new InteractionStep {
                        tag = lAnimationParameters,
                        action = PushObjectCallback
                    });

                    TimelineManager.Instance.AddInteraction(lEntity.gameObject, lInteractionSteps, lInter.Time);
                    break;

			case HumanInteractionType.STOP_PUSH:

                    lAnimationParameters = new AnimationParameters() {
                        TargetType = AnimationParameters.AnimationTargetType.ENTITY,
                        Speed = mMovableEntity.ComputeSpeed(),
                        Acceleration = mMovableEntity.ComputeAcceleration(),
                    };

                    lInteractionSteps = new List<InteractionStep>();

                    lInteractionSteps.Add(new InteractionStep {
                        tag = lAnimationParameters,
                        action = ReleaseObjectCallback
                    });

                    TimelineManager.Instance.AddInteraction(gameObject, lInteractionSteps, lInter.Time);

                    break;

			}

		}
	}

}
