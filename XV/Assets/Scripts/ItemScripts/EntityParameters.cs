using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityParameters : MonoBehaviour {

    public enum EntityType
    {
        SMALL_ITEM,
        MEDIUM_ITEM,
        HEAVY_ITEM,
        FIX_ITEM,
        TROLLEY,
        VEHICLE,
        HUMAN,
        COUNT,
    }

    [Header("Choose a type for this entity")]
    [SerializeField]
    private EntityType TypeOf;

    public EntityType Type
    {
        get { return TypeOf; }
    }

    [Header("Active or not, movement or rotation animation")]
    [SerializeField]
    private bool CanMove;

    public bool Movable
    {
        get { return CanMove; }
    }

    [Header("Setting up the orientation of this model")]
    [SerializeField]
    private Vector3 RotationOffset;

    public Vector3 Orientation
    {
        get { return RotationOffset; }
    }
}
