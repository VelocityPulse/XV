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


// Code generique permettant de feed la Timeline
// (Ajout / Suppression d'animation/interaction, etc ...)

public abstract class AInteraction : MonoBehaviour
{
    // Wait for TimelineManager
    List<Predicate<bool>> mTimeline = new List<Predicate<bool>>();

    private bool mIsBusy;

    public bool TimeLineIsBusy { get { return mIsBusy; }}

	private void Start()
	{
        mIsBusy = false;
	}

    protected void AddToTimeline(Predicate<bool> iAction, float iDuration)
    {
        if (mTimeline != null)
            mTimeline.Add(iAction);
        // Code Interface with TimelineManager
    }

    // --------- Debug - Tmp func ----------------
    // Wait for TimelineManager
    protected void PlayTimeline()
    {
        if (mTimeline == null)
            return;
        mIsBusy = true;
 
        StartCoroutine(ActionPlayerAsync(mTimeline));
    }

    // This function browse and execute all of ActionClip
    // When an ActionClip if finished, it return true, then the coroutine launch the next ActionClip
    private IEnumerator ActionPlayerAsync(List<Predicate<bool>> iTimeline)
    {
        if (iTimeline == null)
            yield break;

        foreach (Predicate<bool> lActionClip in iTimeline) {
            yield return new WaitUntil(() => { return lActionClip(true); });
        }
        mIsBusy = false;
        mTimeline.Clear();
    }
}