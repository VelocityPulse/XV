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
        float Speed;

        EntityParameters.EntityType EntityType;
    }

    protected class AnimationParameters
    {
        public string Name;

        public EntityParameters.EntityType[] Subscriptions;

        public Predicate<AnimationInfo> Animation;

        public UIBubbleInfoButton Button;

        public AnimationParameters()
        {

        }
    }

    private List<AnimationParameters> mAnimations;

    private EntityParameters mParameters;

    private ObjectEntity mObjectEntity;

    /*
    **  Each index of this array correspond to an EntityType (HUMAN, TROLLEY, MEDIUM_ITEM, ...)
    **  On build object, the builded object increment it's corresponding index, according to it's EntityType.
    **  On destroy, the destroyed object decrement it's  corresponding index, according to it's EntityType.
    **  So we now exactly what type are currently present in the scene, with this information:
    **  We can display or not, in BuildObject, an Animation button, depending of what type are present in the scene.
    */
    private static int[] mEntityCounter;

    public static int[] EntityCounter
    {
        get
        {
            if (mEntityCounter != null)
                return mEntityCounter;
            else
                return new int[0];
        }
    }

    private static List<Action<EntityParameters.EntityType>>[] mOnSubPresence;

    private static List<Action<EntityParameters.EntityType>>[] mOnSubAbsence;

    public List<Action<EntityParameters.EntityType>>[] OnSubPresence { get { return mOnSubPresence; } }

    public List<Action<EntityParameters.EntityType>>[] OnSubAbsence { get { return mOnSubAbsence; } }

    // Who Entity to warn when this entity counter is empty or not
    private bool[] mPub = new bool[(int)EntityParameters.EntityType.COUNT];

    // This script is associated to the item so, start run at the very beginning
    private void Start()
    {
        mAnimations = new List<AnimationParameters>();

        int lLenght = (int)EntityParameters.EntityType.COUNT;

        if (mOnSubPresence == null) {
            mOnSubPresence = new List<Action<EntityParameters.EntityType>>[lLenght];
            for (int i = 0; i < lLenght; i++)
                mOnSubPresence[i] = new List<Action<EntityParameters.EntityType>>();
        }

        if (mOnSubAbsence == null) {
            mOnSubAbsence = new List<Action<EntityParameters.EntityType>>[lLenght];
            for (int i = 0; i < lLenght; i++)
                mOnSubAbsence[i] = new List<Action<EntityParameters.EntityType>>();
        }

        if (mEntityCounter == null)
            mEntityCounter = new int[(int)EntityParameters.EntityType.COUNT];
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
        });
        return this;
    }

    protected abstract void PostPoppingEntity();

    // Increase the entity counter with the type of this new ObjectEntity
    private void AddType()
    {
        if (mParameters != null) {
            if (Enum.IsDefined(typeof(EntityParameters.EntityType), mParameters.Type)) {

                int lIndex = (int)mParameters.Type;

                bool lValueWasZero = mEntityCounter[lIndex] == 0;
                mEntityCounter[lIndex]++;
                Debug.LogWarning("-- [+]Typeof(" + mParameters.Type + "):" + mEntityCounter[lIndex] + " --");
                if (lValueWasZero && mEntityCounter[lIndex] == 1) {
                    foreach (Action<EntityParameters.EntityType> lAction in mOnSubPresence[lIndex]) {
                        Debug.LogWarning("--FIRE NEW---");
                        lAction(mParameters.Type);
                    }
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
                mEntityCounter[lIndex]--;
                if (mEntityCounter[lIndex] < 0)
                    mEntityCounter[lIndex] = 0;

                Debug.LogWarning("-- [-]Typeof(" + mParameters.Type + "):" + mEntityCounter[lIndex] + " --");
                if (mEntityCounter[lIndex] == 0) {
                    foreach (Action<EntityParameters.EntityType> lAction in mOnSubPresence[lIndex]) {
                        Debug.LogWarning("--FIRE LOST---");
                        lAction(mParameters.Type);
                    }
                }
            }
        }
    }

    private void OnDestroy()
    {
        RemoveType();
    }

    protected void CreateAnimation(AnimationParameters iAnimationParameters)
    {
        if (iAnimationParameters != null) {

            // Check all field are correctly set
            if (string.IsNullOrEmpty(iAnimationParameters.Name) || iAnimationParameters.Subscriptions == null
                || iAnimationParameters.Subscriptions.Length == 0 || iAnimationParameters.Animation == null) {
                Debug.LogWarning("[INTERACTION] - AnimationParameters not correctly set");
                return;
            }

            if (mOnSubPresence.Length != mOnSubAbsence.Length) {
                Debug.LogError("[INTERACTION] - Callback array doesn't have same size.");
                return;
            }

            // Update Callback for EntityType Counter
            foreach (EntityParameters.EntityType lType in iAnimationParameters.Subscriptions) {

                int lIndex = (int)lType;

                if (lIndex < 0 || lIndex > mOnSubPresence.Length || lIndex > mOnSubAbsence.Length) {
                    Debug.LogError("[INTERACTION] - Callback out of range index access.");
                } else {
                    Debug.LogWarning("--SUB :" + lType);
                    mOnSubPresence[lIndex].Add((iEntityType) => { Debug.LogWarning(iAnimationParameters.Name + "Triggered by new EntityType:" + iEntityType); /*show button*/});
                    mOnSubAbsence[lIndex].Add((iEntityType) => { Debug.LogWarning(iAnimationParameters.Name + "Triggered by lost EntityType:" + iEntityType);/*hide button*/});
                }
            }

            // Add parameters to the list of available animations
            mAnimations.Add(iAnimationParameters);
        }
    }
}