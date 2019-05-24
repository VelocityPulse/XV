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
        CreateAnimation(new AnimationParameters() {
            Name = "TakeAnObject",
            Help = "Will carry the next object you click on.",
            Subscriptions = new EntityParameters.EntityType[] { EntityParameters.EntityType.HEAVY_ITEM },
            Animation = TakeAnObject,
            Button = new UIBubbleInfoButton() {
                Text = "TakeAnObject",
                Tag = name + "_TAKE_OBJECT",
                ClickAction = (iEntity) => { /*ADD TO TIMELINE HERE*/ Debug.LogWarning("Click on TakeObject from " + name); },
            },
        });

        CreateAnimation(new AnimationParameters() {
            Name = "DropAnObject",
            Help = "Will drop an object, the next position you click on.",
            Subscriptions = new EntityParameters.EntityType[] { EntityParameters.EntityType.HEAVY_ITEM },
            Animation = DropAnObject,
            Button = new UIBubbleInfoButton() {
                Text = "DropObject",
                Tag = name + "_DROP_OBJECT",
                ClickAction = (iEntity) => { /*ADD TO TIMELINE HERE*/ Debug.LogWarning("Click on DropObject from " + name); },
            },
        });
    }

    // =================== ANIMATION CODE ==================

    private bool TakeAnObject(AnimationInfo iAnimationInfo)
    {
        Debug.Log("---- TAKE AN OBJECT ANIMATIONS ----");
        return true;
    }

    private bool DropAnObject(AnimationInfo iAnimationInfo)
    {
        Debug.Log("---- DROP AN OBJECT ANIMATIONS ----");
        return true;
    }
}
