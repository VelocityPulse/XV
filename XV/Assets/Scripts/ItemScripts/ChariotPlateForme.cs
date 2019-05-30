using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChariotPlateForme : AInteraction {

    /*
    **  To code animation on an item, inherit from AInteraction 
    **  Then your animation code need to be code into a Predicate<float>
    **  Call 'CreateAnimation' with the name of the animation and it's code
    */

    /*
    ** 1 - start l'item contient toute ses animations dans son AInteraction
    ** 2 - A la fin du build object on check qu'un AInteraction est present, si c'est le cas on appelle son SetObjectEntity
    */


    protected override void PostPoppingEntity()
    {
        // Exemple of an animation creation
        CreateAnimation(new ItemAnimation() {
            Name = "TakeAnObject",
            Help = "Will carry the next object you click on.",
            Subscriptions = new EntityParameters.EntityType[] { EntityParameters.EntityType.HEAVY_ITEM },
            AnimationImpl = TakeAnObject,
            Button = new UIBubbleInfoButton() {
                Text = "TakeAnObject",
                Tag = name + "_TAKE_OBJECT",
                ClickAction = OnClickTakeObject,
            },
        });

        CreateAnimation(new ItemAnimation() {
            Name = "DropAnObject",
            Help = "Will drop the carried object to the next position you click on.",
            Subscriptions = new EntityParameters.EntityType[] { EntityParameters.EntityType.HEAVY_ITEM },
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
        mMode = InteractionMode.TARGET;
        Debug.LogWarning("Click on TakeObject from " + name);
    }

    private bool TakeAnObject(AnimationInfo iAnimInfo, AnimationParameters iParameters)
    {
        Debug.Log("---- TAKE AN OBJECT ANIMATIONS ----");
        return true;
    }

    private void OnClickDropObject(AEntity iEntity)
    {
        Debug.LogWarning("Click on DropObject from " + name);
    }

    private bool DropAnObject(AnimationInfo iAnimInfo, AnimationParameters iParameters)
    {
        Debug.Log("---- DROP AN OBJECT ANIMATIONS ----");
        return true;
    }
}
