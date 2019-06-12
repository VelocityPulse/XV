using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChariotPlateForme : AInteraction
{


	protected override void PostPoppingEntity()
	{
		// Exemple of an interaction creation
		CreateInteraction(new ItemInteraction() {
			Name = "TakeAnObject",
			Help = "Will carry the next object you click on.",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.HEAVY_ITEM },
			AnimationImpl = TakeAnObject,
			Button = new UIBubbleInfoButton() {
				Text = "TakeAnObject",
				Tag = name + "_TAKE_OBJECT",
				ClickAction = OnClickTakeObject,
			},
		});

		CreateInteraction(new ItemInteraction() {
			Name = "DropAnObject",
			Help = "Will drop the carried object to the next position you click on.",
			InteractWith = new EntityParameters.EntityType[] { EntityParameters.EntityType.HEAVY_ITEM },
			AnimationImpl = DropAnObject,
			Button = new UIBubbleInfoButton() {
				Text = "DropObject",
				Tag = name + "_DROP_OBJECT",
				ClickAction = OnClickDropObject,
			},
		});
	}

	/*
    *   Idées & Notes de dev
    * 
    *   1 - click sur un bouton d'animation, run OnClick
    *   2 - base class passe en mode Entity selection
    *       Raycast a chaque frame sur la souris
    *       Si entity hit, highlight de l'entity
    *       Si click sur entity, utilisation de cette entity en target pour l'animation
    *       Si click & target valide, fin du mode selection entity et ajout de l'animation a la TimeLine
    */

	// =================== ANIMATION CODE ==================

	/// <summary>
	/// Get Entity that will be use for the animation, and add the animation to the timeline.
	/// </summary>
	/// <param name="iEntity"></param>
	private void OnClickTakeObject(AEntity iEntity)
	{
		Debug.LogWarning("Click on TakeObject from " + name);
		StartCoroutine(InteractionWaitForTargetAsync("TakeAnObject"));
	}

	/// <summary>
	/// This function will wait for the user to select an Entity.
	/// Then check the Entity clicked can interact with the Interaction pass in argument, before add it to the Timeline.
	/// </summary>
	/// <returns>The user select target async.</returns>
	/// <param name="iInteractionName">I interaction name.</param>
	private IEnumerator InteractionWaitForTargetAsync(string iInteractionName)
	{
		yield return new WaitWhile(() => {

			if (Input.GetMouseButtonDown(0)) {

				RaycastHit lHit;
				Ray lRay = Camera.main.ScreenPointToRay(Input.mousePosition);
				if (Physics.Raycast(lRay, out lHit, 100000, LayerMask.GetMask("dropable"))) {

					Debug.DrawRay(lRay.origin, lRay.direction * lHit.distance, Color.red, 1);

					// If we don't found EntityParameters, stop with an error.
					EntityParameters lEntityParam;
					if ((lEntityParam = lHit.collider.gameObject.GetComponentInParent<EntityParameters>()) == null) {
						Debug.LogError("[TARGET SELECTOR] Click on Entity !");
						return false;
					}

					// If Subscribers contain the clicked Entity type use it as target and add animation to timeline
					if (IsInteractionCanInteractType(iInteractionName, lEntityParam.Type)) {
						AnimationParameters lAnimationParameters = new AnimationParameters() {
							TargetType = AnimationParameters.AnimationTargetType.ENTITY,
							AnimationTarget = lEntityParam.gameObject,
						};
						TimelineManager.Instance.AddAnimation(gameObject, TakeAnObject, lAnimationParameters);
						//TimelineManager.Instance.AddAnimation(gameObject, TakeAnObject);
						return false;
					}
					Debug.LogError("[TARGET SELECTOR] The object you click on is not interactable with this object !");
				}

				return false;
			}
			return true;
		});
	}

	// Attention aux dependances ... quitter proprement si destroy de la target ...
	// penser au virtual / override pour la visibilite et comprendre direct qu'il y a du code dans l'Abstraite
	// find suffix / prefix pour que ca soit clair que c'est appeler par une coroutine dans la timeline
	private bool TakeAnObject(object iAnimInfo)
	{
		Debug.Log("---- TAKE AN OBJECT ANIMATIONS ----");
		return true;
	}

	private void OnClickDropObject(AEntity iEntity)
	{
		Debug.LogWarning("Click on DropObject from " + name);
	}

	private bool DropAnObject(object iAnimInfo)
	{
		Debug.Log("---- DROP AN OBJECT ANIMATIONS ----");
		return true;
	}
}
