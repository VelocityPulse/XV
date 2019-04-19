using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class AInteraction : MonoBehaviour
{
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
    **               Une interaction comporte plusieurs animation d'objets.
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
    **  ------- CLASSIFICATION DES ITEMS: GOD_APPROACH -------
    **
    **  SMALL_ITEM:
    **  Objets ramassables par l'humain et qui seront present dans un inventaire.
    **  (Ex: un document)
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation de ramassage generique puis depop de l'objet dans la scene
    **      - Ajout de cet objet dans l'inventaire de l'humain
    **      - Les objets dans l'inventaire donnent acces a de nouvelle interaction a l'humain
    **
    **  MEDIUM_ITEM:
    **  Objets ramassables par l'humain mais trop gros pour l'inventaire.
    **  (Ex: un carton)
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation de port de l'objet generique. (L'humain place ses bras en avant et l'objet est deplace / repop dessus)
    **      - L'objet porte par l'humain lui donne acces a de nouvelle interactions.
    **
    **  HEAVY_ITEM:
    **  Objets deplacables / portables, uniquement avec l'utilisation d'un objet de type TROLLEY et/ou VEHICLE.
    **  (Ex: palette)
    **
    **  FIX_ITEM:
    **  Objets non deplacables mais qui comporte quand meme des animations / interactions.
    **  (Ex: Table d'emballage -> 'Emballer/Etiquetter un carton')
    **
    **  TROLLEY:
    **  Objets asismilable a un 'chariot' (trolley), utilisables par l'humain et qui peut donner ou non la possibilite
    **  de deplacer les HEAVY_ITEM / MEDIUM_ITEM. (Ex: un chariot donne acces a -> 'deplacer palette')
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation generique pour pousser un chariot. (Bras en avant)
    **      - Deplacement humain + objet
    **  
    **  VEHICLE:
    **  Objets utilisables par l'humain et qui donne la possibilite de deplacer les HEAVY_ITEM.
    **  (Ex: un chariot donne acces a -> 'deplacer palette')
    **
    **
    **
    **  ------ CLASSIFICATION DES ITEMS - HUMAN_APPROACH: -------
    **
    **  SMALL_ITEM:
    **  Objets ramassables par l'humain et qui seront present dans un inventaire.
    **  (Ex: un document)
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation de ramassage generique puis depop de l'objet dans la scene
    **      - Ajout de cet objet dans l'inventaire de l'humain
    **      - Les objets dans l'inventaire donnent acces a de nouvelle interaction a l'humain
    **
    **  MEDIUM_ITEM:
    **  Objets ramassables par l'humain mais trop gros pour l'inventaire.
    **  (Ex: un carton)
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation de port de l'objet generique. (L'humain place ses bras en avant et l'objet est deplace / repop dessus)
    **      - L'objet porte par l'humain lui donne acces a de nouvelle interactions.
    **
    **  HEAVY_ITEM:
    **  Objets deplacables / portables, uniquement avec l'utilisation d'un objet de type TROLLEY et/ou VEHICLE.
    **  (Ex: palette)
    **
    **  FIX_ITEM:
    **  Objets non deplacables mais qui comporte quand meme des animations / interactions.
    **  (Ex: Table d'emballage -> 'Emballer/Etiquetter un carton')
    **
    **  TROLLEY:
    **  Objets asismilable a un 'chariot' (trolley), utilisables par l'humain et qui peut donner ou non la possibilite
    **  de deplacer les HEAVY_ITEM / MEDIUM_ITEM. (Ex: un chariot donne acces a -> 'deplacer palette')
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation generique pour pousser un chariot. (Bras en avant)
    **      - Deplacement humain + objet
    **  
    **  VEHICLE:
    **  Objets utilisables par l'humain et qui donne la possibilite de deplacer les HEAVY_ITEM.
    **  (Ex: un chariot donne acces a -> 'deplacer palette')
    **
    */


    // Code generique permettant de feed la Timeline
    // (Ajout / Suppression d'animation/interaction, etc ...)
}
