using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(MovableEntity))]
public class ManifactureInteractable : AInteraction
{

	private MovableEntity mMovableEntity;

	private AEntity mObjectHeld;

	private AEntity mHumanEntity;

	protected override void Start()
	{
		base.Start();

		mMovableEntity = GetComponent<MovableEntity>();
	}


	protected override void PostPoppingEntity()
	{
		
	}






}
