using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


[RequireComponent(typeof(MovableEntity))]
public class HumanInteractable : AInteraction
{
	private MovableEntity mMovableEntity;

	protected override void Start()
	{
		base.Start();

		mMovableEntity = GetComponent<MovableEntity>();
	}

	protected override void PostPoppingEntity()
	{
		CreateInteraction(new ItemInteraction() {
			Name = "TakeObject",
			Help = "Take an object",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.SMALL_ITEM, EntityParameters.EntityType.MEDIUM_ITEM },
			AnimationImpl = TakeObject,
			Button = new UIBubbleInfoButton() {
				Text = "TakeObject",
				Tag = name + "_TAKE_OBJECT",
				ClickAction = OnClickTakeObject
			}
		});
	}

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
                            action = TakeObject
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

	private bool TakeObject(object iAnimInfo)
	{
        // premier coup ok 
        // deuxieme coup parameters est vide
        Debug.Log(iAnimInfo.Parameters.GetHashCode());
        GameObject lTarget = (GameObject)iAnimInfo.Parameters.AnimationTarget;

		if (mMovableEntity.Move(lTarget.transform.position, iAnimInfo) == false)
			return false;

		return false;
	}

}
