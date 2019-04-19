using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public sealed class ChariotPlateFormeEntity : AInteraction 
{
    /*
    **  (Idee en vrac: Ajouter une GUI hierarchie)
    **
    **
    **  
    **  GLOSSAIRE:
    **
    **  deplacable: Dans le contexte des interactions le mot deplacable est utilise pour designer une interaction.
    **              (Ex: 'poser le carton sur la table').
    **              Il ne designe donc pas la translation d'objet mais son deplacement via une interaction / animations.
    **
    **  CLASSIFICATION DES ITEMS:  
    **
    **  SMALL_ITEM:
    **  Objets ramassables par l'humain et qui seront present dans un inventaire. (Ex: un document)
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation de ramassage generique puis depop de l'objet dans la scene
    **      - Ajout de cet objet dans l'inventaire de l'humain
    **      - Les objets dans l'inventaire donnent acces a de nouvelle interaction a l'humain
    **
    **  MEDIUM_ITEM:
    **  Objets ramassables par l'humain mais trop gros pour l'inventaire. (Ex: un carton)
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - Animation de port de l'objet generique. (L'humain place ses bras en avant et l'objet est deplace / repop dessus)
    **      - L'objet porte par l'humain lui donne acces a de nouvelle interactions.
    **
    **  BIG_ITEM:
    **  Objets non portable, ni deplacable via un ACTUATOR, mais deplacable a la main par l'humain (Ex: Transpalette, Servante)
    **
    **  HEAVY_ITEM:
    **  Objets deplacables / portables, uniquement avec l'utilisation d'un objet de type ACTUATOR. (Ex: palette)
    **
    **  FIX_ITEM:
    **  Objets non deplacables mais qui comporte quand meme des animations / interactions. (Ex: Table d'emballage -> 'Emballer/Etiquetter un carton')
    **
    **  ACTUATOR:
    **  Objets utilisables par l'humain et qui donne la possibilite de deplacer les HEAVY_ITEM. (Ex: un chariot donne acces a -> 'deplacer palette')
    **      - Deplacement de l'humain jusqu'a l'objet
    **      - 
    **  
    **
    **  Une fonction d'ajout par interaction
    **  Une interaction pouvant comporter plusieurs clip d'animation
    */
}
