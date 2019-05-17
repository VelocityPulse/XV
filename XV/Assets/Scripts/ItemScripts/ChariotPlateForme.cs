using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChariotPlateForme : AInteraction {

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
            Subscriptions = new EntityParameters.EntityType[] { EntityParameters.EntityType.TROLLEY },
            Animation = TakeAnObject,
        });

        CreateAnimation(new AnimationParameters() {
            Name = "DropAnObject",
            Subscriptions = new EntityParameters.EntityType[] { EntityParameters.EntityType.TROLLEY },
            Animation = DropAnObject,
        });
    }

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
