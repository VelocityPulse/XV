using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
**  (Idee en vrac: Ajouter une GUI hierarchie)
**
**
**  
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
**      GOD_APPROACH / 
**  FIRST_PERSON_APPROACH: Toute les interactions sont independante, et se font via les objets concernes
**                          +: Independant de l'humain
**                          +: Plus ergonomique
**                          -: Permet d'animer des scene illogique par rapport a la realite (mais osef .. ?)
**                          -: Comment gerer les objets uniquement ramassable ? (Ex: etiquette, document)
**
**  HUMAN_APPROACH  : Toute les interactions se font via l'humain.
**                  +:  Proche de la realite,
**                  +:  Centralise tout les infos possible sur l'humain
**                  -:  Allourdit l'ergonomie pour animer une scene
**                  -:  Penalise une scene ou on pourrait se passer de l'humain (Ex: un chariot amene des cartons sur un convoyeur)
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


// Classe de base contenant le managing des animations pour un item
// Voir interfacage avec TimeLineManager qui prend un AnimationClip pour le moment

public abstract class AInteraction : MonoBehaviour
{
    protected struct AnimationInfo
    {
        /// <summary>
        /// Speed coefficient of the animation.
        /// </summary>
        float Speed;

    }

    /// <summary>
    /// Parameters for an Animation, with Animation's code, UI Button and Subscriptions...
    /// </summary>
    protected class AnimationParameters
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
        public Predicate<AnimationInfo> Animation;

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
        public AnimationParameters()
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

    private static List<AnimationParameters>[] sOnSubPresence;

    private static List<AnimationParameters>[] sOnSubAbsence;

    private EntityParameters mParameters;

    private ObjectEntity mObjectEntity;

    private List<AnimationParameters> mAnimationParameters;

    protected List<AnimationParameters>[] OnSubPresence { get { return sOnSubPresence; } }

    protected List<AnimationParameters>[] OnSubAbsence { get { return sOnSubAbsence; } }

    private void Start()
    {
        mAnimationParameters = new List<AnimationParameters>();

        int lLenght = (int)EntityParameters.EntityType.COUNT;

        if (sOnSubPresence == null) {
            sOnSubPresence = new List<AnimationParameters>[lLenght];
            for (int i = 0; i < lLenght; i++)
                sOnSubPresence[i] = new List<AnimationParameters>();
        }

        if (sOnSubAbsence == null) {
            sOnSubAbsence = new List<AnimationParameters>[lLenght];
            for (int i = 0; i < lLenght; i++)
                sOnSubAbsence[i] = new List<AnimationParameters>();
        }

        if (sEntityCounter == null)
            sEntityCounter = new int[(int)EntityParameters.EntityType.COUNT];
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
        foreach (AnimationParameters lAnimationParameter in mAnimationParameters) {

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
                    foreach (AnimationParameters lAnimationParameters in sOnSubPresence[lIndex])
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
                    foreach (AnimationParameters lAnimationParameters in sOnSubAbsence[lIndex])
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
    /// <param name="iAnimationParameters"></param>
    protected void CreateAnimation(AnimationParameters iAnimationParameters)
    {
        if (iAnimationParameters != null) {

            // Check all field are correctly set
            if (string.IsNullOrEmpty(iAnimationParameters.Name) || iAnimationParameters.Subscriptions == null
                || iAnimationParameters.Subscriptions.Length == 0 || iAnimationParameters.Animation == null) {
                Debug.LogError("[INTERACTION] - AnimationParameters not correctly set");
                return;
            }

            if (sOnSubPresence.Length != sOnSubAbsence.Length) {
                Debug.LogError("[INTERACTION] - Callback array doesn't have same size.");
                return;
            }

            // Update Callback for EntityType Counter
            foreach (EntityParameters.EntityType lType in iAnimationParameters.Subscriptions) {

                int lIndex = (int)lType;

                if (lIndex < 0 || lIndex > sOnSubPresence.Length || lIndex > sOnSubAbsence.Length) {
                    Debug.LogError("[INTERACTION] - Callback out of range index access.");
                } else {
                    iAnimationParameters.BindedObjectEntity = mObjectEntity;
                    sOnSubPresence[lIndex].Add(iAnimationParameters);
                    sOnSubAbsence[lIndex].Add(iAnimationParameters);
                }
            }

            mAnimationParameters.Add(iAnimationParameters);
        }
    }
}