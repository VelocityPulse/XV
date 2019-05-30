using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationInfo
{
	public enum State { PLAY, PAUSE, STOP };

	/// <summary>
	/// Global state of the all animations.
	/// </summary>
	public static State sGlobalState;

	/// <summary>
	/// Speed coefficient of the animation.
	/// </summary>
	public float Speed;
}

public class AnimationParameters
{
    public enum AnimationTargetType { ENTITY, POSITION };

    /// <summary>
    /// The target type use for this animation
    /// </summary>
    public AnimationTargetType TargetType;

    /// <summary>
    /// Object use as target for an Animation.
    /// Use AnimationTargetType to cast it later
    /// </summary>
    public object AnimationTarget;
}

/// <summary>
/// Base class for Item interaction script.
/// </summary>
public abstract class AInteraction : MonoBehaviour
{
    protected enum InteractionMode { NONE, TARGET, };

    public delegate bool AnimationPredicate(AnimationInfo iAnimInfo, AnimationParameters iParameters);

    /// <summary>
    /// Parameters for an Animation, with Animation's code, UI Button and Subscriptions...
    /// </summary>
    protected class ItemAnimation
    {
        /// <summary>
        /// Name of this Animation.
        /// </summary>
        public string Name;

        /// <summary>
        /// Info to understand how to use this animation.
        /// </summary>
        public string Help;

        /// <summary>
        /// Subscriptions of this animation, defined what type of Entity this Animation can interact with.
        /// </summary>
        public EntityParameters.EntityType[] Subscriptions;

        /// <summary>
        /// Animation function, which will be called in the Timeline.
        /// </summary>
        public AnimationPredicate AnimationImpl;

        /// <summary>
        /// UIBubbleInfoButton of this Animation, it will be displayed only when one of Subscription is present in the scene.
        /// </summary>
        public UIBubbleInfoButton Button;

        /// <summary>
        /// Will be called when the UI Button is display.
        /// </summary>
        public Action OnDisplay;

        /// <summary>
        /// Will be called when the UI Button is hide.
        /// </summary>
        public Action OnHide;

        private bool mIsDisplayed;

        /// <summary>
        /// Return true if this Animations is displayed.
        /// </summary>
        public bool IsDisplayed { get { return mIsDisplayed; } }

        private ObjectEntity mBindedObjectEntity;

        internal ObjectEntity BindedObjectEntity { set { mBindedObjectEntity = value; } }

        /// <summary>
        /// Parameters for an Animation, with Animation's code, UI Button and Subscriptions.
        /// </summary>
        public ItemAnimation()
        {
            mIsDisplayed = false;
            mBindedObjectEntity = null;
            OnDisplay = null;
            OnHide = null;
            Help = null;
        }

        internal void Display()
        {
            // Check the Button is Hided
            if (!mIsDisplayed) {
                mIsDisplayed = true;
                if (mBindedObjectEntity != null) {
                    mBindedObjectEntity.CreateBubleInfoButton(Button);
                    if (OnDisplay != null)
                        OnDisplay();
                }
            }
        }

        internal void Hide()
        {
            // Check the Button is Displayed
            if (mIsDisplayed) {
                mIsDisplayed = false;
                if (mBindedObjectEntity != null) {
                    mBindedObjectEntity.DestroyBubleInfoButton(Button);
                    if (OnHide != null)
                        OnHide();
                }
            }
        }
    }

    /*
    **  Each index of this array correspond to an EntityType (HUMAN, TROLLEY, MEDIUM_ITEM, ...)
    **  On build object, the builded object increment it's corresponding index, according to it's EntityType.
    **  On destroy, the destroyed object decrement it's  corresponding index, according to it's EntityType.
    **  So we now exactly what type are currently present in the scene, with this information:
    **  We can display or not, in BuildObject, an Animation button, depending of what type are present in the scene.
    */
    private static int[] sEntityCounter;

    public static int[] EntityCounter
    {
        get
        {
            if (sEntityCounter != null)
                return sEntityCounter;
            else
                return new int[0];
        }
    }

    private static List<ItemAnimation>[] sOnSubPresence;

    private static List<ItemAnimation>[] sOnSubAbsence;

    private EntityParameters mParameters;

    private ObjectEntity mObjectEntity;

    private List<ItemAnimation> mItemAnimations;

    protected List<ItemAnimation>[] OnSubPresence { get { return sOnSubPresence; } }

    protected List<ItemAnimation>[] OnSubAbsence { get { return sOnSubAbsence; } }

    protected InteractionMode mMode;

    private void Start()
    {
        mItemAnimations = new List<ItemAnimation>();

        int lLenght = (int)EntityParameters.EntityType.COUNT;

        if (sOnSubPresence == null) {
            sOnSubPresence = new List<ItemAnimation>[lLenght];
            for (int i = 0; i < lLenght; i++)
                sOnSubPresence[i] = new List<ItemAnimation>();
        }

        if (sOnSubAbsence == null) {
            sOnSubAbsence = new List<ItemAnimation>[lLenght];
            for (int i = 0; i < lLenght; i++)
                sOnSubAbsence[i] = new List<ItemAnimation>();
        }

        if (sEntityCounter == null)
            sEntityCounter = new int[(int)EntityParameters.EntityType.COUNT];

        mMode = InteractionMode.NONE;
    }

    private void Update()
    {
        if (mMode == InteractionMode.NONE)
            return;
        if (mMode == InteractionMode.TARGET) {

            }
    }

    public AInteraction SetEntityParameters(EntityParameters iParameters)
    {
        mParameters = iParameters;
        return this;
    }

    public AInteraction SetObjectEntity(ObjectEntity iObjectEntity)
    {
        mObjectEntity = iObjectEntity;
        // Add all this code to the PostPopping callback of ObjectEntity
        mObjectEntity.PostPoppingAction.Add(() => {
            // Increase the entity counter with the type of this new ObjectEntity
            AddType();

            // Child post popping
            PostPoppingEntity();

            UpdateAvailableInteraction();
        });
        return this;
    }

    protected abstract void PostPoppingEntity();

    /// <summary>
    /// Display UI of available interaction according to Entities in the scene.
    /// </summary>
    public void UpdateAvailableInteraction()
    {
        foreach (ItemAnimation lAnimationParameter in mItemAnimations) {

            foreach (EntityParameters.EntityType lType in lAnimationParameter.Subscriptions) {
                if (sEntityCounter[(int)lType] > 0)
                    lAnimationParameter.Display();
            }

        }
    }

    // Increase the entity counter with the type of this new ObjectEntity
    private void AddType()
    {
        if (mParameters != null) {
            if (Enum.IsDefined(typeof(EntityParameters.EntityType), mParameters.Type)) {

                int lIndex = (int)mParameters.Type;

                bool lValueWasZero = sEntityCounter[lIndex] == 0;
                sEntityCounter[lIndex]++;
                //Debug.LogWarning("-- [+]Typeof(" + mParameters.Type + "):" + mEntityCounter[lIndex] + " --");

                if (lValueWasZero && sEntityCounter[lIndex] == 1) {
                    foreach (ItemAnimation lAnimationParameters in sOnSubPresence[lIndex])
                        lAnimationParameters.Display();
                }
            }
        }
    }

    // Decrease the entity counter with the type of this new ObjectEntity
    private void RemoveType()
    {
        if (mParameters != null) {
            if (Enum.IsDefined(typeof(EntityParameters.EntityType), mParameters.Type)) {

                int lIndex = (int)mParameters.Type;
                sEntityCounter[lIndex]--;
                if (sEntityCounter[lIndex] < 0)
                    sEntityCounter[lIndex] = 0;
                //Debug.LogWarning("-- [-]Typeof(" + mParameters.Type + "):" + mEntityCounter[lIndex] + " --");

                if (sEntityCounter[lIndex] == 0) {
                    foreach (ItemAnimation lAnimationParameters in sOnSubAbsence[lIndex])
                        lAnimationParameters.Hide();
                }
            }
        }
    }

    private void OnDestroy()
    {
        RemoveType();
    }

    /// <summary>
    /// Add and handle an animation, it's button will be displayed only when available,
    /// according to Subscriptions field of AnimationParameters.
    /// </summary>
    /// <param name="iAnimation"></param>
    protected void CreateAnimation(ItemAnimation iAnimation)
    {
        if (iAnimation != null) {

            // Check all field are correctly set
            if (string.IsNullOrEmpty(iAnimation.Name) || iAnimation.Subscriptions == null
                || iAnimation.Subscriptions.Length == 0 || iAnimation.AnimationImpl == null) {
                Debug.LogError("[INTERACTION] - AnimationParameters not correctly set");
                return;
            }

            if (sOnSubPresence.Length != sOnSubAbsence.Length) {
                Debug.LogError("[INTERACTION] - Callback array doesn't have same size.");
                return;
            }

            // Update Callback for EntityType Counter
            foreach (EntityParameters.EntityType lType in iAnimation.Subscriptions) {

                int lIndex = (int)lType;

                if (lIndex < 0 || lIndex > sOnSubPresence.Length || lIndex > sOnSubAbsence.Length) {
                    Debug.LogError("[INTERACTION] - Callback out of range index access.");
                } else {
                    iAnimation.BindedObjectEntity = mObjectEntity;
                    sOnSubPresence[lIndex].Add(iAnimation);
                    sOnSubAbsence[lIndex].Add(iAnimation);
                }
            }

            mItemAnimations.Add(iAnimation);
        }
    }
}


/*
**  GLOSSAIRE:
**
**  deplacable: Le mot deplacable est utilise pour designer une interaction / animation.
**              (Ex: 'poser le carton sur la table').
**              Il ne designe donc pas la translation d'objet (edition) mais son deplacement via une interaction / animations.
**
**  animation: Definit une action simple, (Ex: Chariot -> 'Monter les fourche du chariot')
**             Ne pas confondre avec une animation de l'animator, car nous somme dans le contexte de la TIMELINE.
**             (Ex d'animation qui ne sera pas implementer via l'Animator: Humain -> 'aller ici ...')
**
**  interaction: Definit un comportement et implique obligatoirement plusieur objets.
**               Une interaction peut comporter plusieurs animation d'objets.
**               (Ex: Chariot -> 'Prendre la palette' ou Humain -> 'aller a la table d'emballage')
**
**
**  ------ CLASSIFICATION DES ITEMS  -------
**
**  SMALL_ITEM:
**  Objets ramassable, qui se place dans un inventaire. Peut debloquer des interactions pour un HUMAN.
**  (Ex: un document, etiquette)
**
**  MEDIUM_ITEM:
**  Objets deplacable par un HUMAN ou par TROLLEY & portable par un HUMAN.
**  (Ex: un carton)
**
**  HEAVY_ITEM:
**  Objets deplacable uniquement via TROLLEY ou VEHICLE & non portable par un HUMAN.
**  (Ex: palette)
**
**  FIX_ITEM:
**  Objets non deplacables mais qui comporte quand meme des animations / interactions.
**  (Ex: Table d'emballage -> 'Emballer/Etiquetter un carton')
**
**  TROLLEY:
**  Objets assimilable a un 'chariot' (trolley), utilisables et qui peut donner ou non la possibilite
**  de deplacer les HEAVY_ITEM / MEDIUM_ITEM. (Ex: un chariot donne acces a -> 'deplacer palette')
**  
**  VEHICLE:
**  Objets utilisables et qui donne la possibilite de deplacer les HEAVY_ITEM.
**  (Ex: un chariot donne acces a -> 'deplacer palette')
**
**  HUMAN:
**  Humain pouvant ramasser, porter des objets, pousser des TROLLEY ou conduire des VEHICLE.
**
**  ------------- UI -----------
**
**  Les objets deplacable Deplacement / Rotations auront un bouton pour ces features dans leurs UIBubleInfo.
**  Tout les objets comportants animations ou interactions en plus, auront un bouton Animer dans l'UIBubleInfo.
*/
