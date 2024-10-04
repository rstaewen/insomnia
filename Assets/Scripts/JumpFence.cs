#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class JumpFence : MonoBehaviour
{
	private ScoreCounter scoreCounter;
	//note that this is the minimum speed percentage necessary (should be between 0 and 1).
	//for instance, a sheep at 90% of their top speed would meed a .85 threshold but not a 0.95 threshold.
	public float minSpeedForJump;
	public Transform goalTransform;
	private List<Sheep> caughtSheep = new List<Sheep>();
	void Awake()
	{
		scoreCounter = GetComponent<ScoreCounter>();
	}
	void OnTriggerEnter2D(Collider2D other)
	{
		Sheep sheep = ColliderLinks.FindSheep(other);
		if(		sheep && 
		   		sheep.CanJump && 
		   			( sheep.GetComponent<FaceSpriteCardinalDirection>().SpeedPercentage *
		   			Mathf.Clamp(1f - Vector2.Distance(sheep.normalizedMoveDir, jumpForceDirection(sheep)), 0f, 1f))
		   		> minSpeedForJump)
		{
			//print("sheep: "+sheep.name+" jump attempt...");
			if(scoreCounter)
				sheep.Jump(scoreCounter.AddScore, caughtSheep.ToList(), jumpForceDirection(sheep)*(goalTransform.position-transform.position).magnitude);
			else
				sheep.Jump(null, caughtSheep.ToList(),  jumpForceDirection(sheep)*(goalTransform.position-sheep.transform.position).magnitude);
			caughtSheep.Add(sheep);
		}
//		else if (sheep && !sheep.isCounted)
//		{
//			if(!sheep.CanJump)
//				print(sheep.name+".fail: can't jump");
//			else if(( sheep.GetComponent<FaceSpriteCardinalDirection>().SpeedPercentage *
//			          Mathf.Clamp(1f - Vector2.Distance(sheep.normalizedMoveDir, jumpForceDirection(sheep)), 0f, 1f) *
//			         sheep.maximumHerdedSpeed) * 2.5f
//			        < minSpeedForJump)
//				print(sheep.name+".fail: not moving fast enough");
//		}
	}
	Vector2 jumpForceDirection (Sheep sheep)
	{
		//average the directions of the goal to the fence (fence forward direction)
		//with the sheep's own trajectory to the goal.
		//this aught to seem the most natural desired jump direction.
		return (
			(goalTransform.position - sheep.transform.position).normalized + 
			(goalTransform.position - transform.position).normalized
			).normalized;
	}
}
