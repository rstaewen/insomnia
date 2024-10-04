#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class WanderSheep : Wander
{
	private Sheep sheep;

//	protected override Vector2 targetPosition {
//		get {	
//
//			if(waypoints.Count == 0 && sheep)
//			{
//				Debug.DrawLine(transform.position, (Vector3)(targetOffset+initialPos-sheep.physicalColliderOffset), Color.green);
//				return targetOffset+initialPos-sheep.physicalColliderOffset;
//			}
//			else return base.targetPosition; }
//	}

	
	protected override void Start()
	{
		sheep = GetComponent<Sheep>();
		circleCollider = sheep.physicalCollider;
		base.Start();
		circleCollider = sheep.physicalCollider;
	}
	
	protected override void OnEnable()
	{
		if(GetComponent<Sheep>().IsIdle)
			StartCoroutine("NewTarget");
	}
	
	protected override void FixedUpdate()
	{
		Vector2 addForce;
		
		if(targetOverride == Vector2.zero)
		{
			if(Vector2.Distance(Position2D, targetPosition) < _tetherWithScale/10f)
			{
				StopAllCoroutines();
				StartCoroutine("NewTarget");
				return;
			}
			addForce = getMoveDirection() * sheep.Speed * rb.mass * speedMultiplier;
		} else
			addForce = getTargetMoveDirection() * sheep.Speed * rb.mass * speedMultiplier;
		rb.AddForce(addForce*Time.timeScale);
	}
}
