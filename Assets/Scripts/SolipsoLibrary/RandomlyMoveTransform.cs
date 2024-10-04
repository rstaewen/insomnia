#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RandomlyMoveTransform : MonoBehaviour
{
	//datastructure for targetting a waypoint within a certain random tolerance.
	[System.Serializable] public class TransformWaypoint
	{
		public abstract class Node
		{
			//the "tolerance" for moving the value around.
			[SerializeField] protected float randomDeviationTolerance;
			[SerializeField] protected Vector3 averageValue;
			//return a random adder as a float value, used up to 3 times on created vector3
			protected float RandomModifier 		{get {return UnityEngine.Random.Range(-randomDeviationTolerance, randomDeviationTolerance);}}
			public void setAvgValue (Vector3 newVal) {averageValue = newVal;}
		}
		[System.Serializable] public class VectorNode : Node
		{
			//by default, dont bother randomizing z. preset for 2D.
			[SerializeField] bool randomizeZ = false;

			public Vector3 RandomizedVector 	
			{get 
				{return new Vector3(
						averageValue.x + RandomModifier, 
						averageValue.y + RandomModifier, 
						averageValue.z + (randomizeZ ? RandomModifier : 0f));
				}
			}
		}
		[System.Serializable] public class QuaternionNode : Node
		{
			public Quaternion RandomizedQuaternion
			{get 
				{return Quaternion.Euler(new Vector3(
						averageValue.x + RandomModifier, 
						averageValue.y + RandomModifier, 
						averageValue.z + RandomModifier));
				}
			}
		}
		[SerializeField] VectorNode position;
		[SerializeField] VectorNode scale;
		[SerializeField] QuaternionNode rotation;
		[SerializeField] float seekTime;
		public float SeekTime {get {return seekTime;} }
		public Vector3 RandomPosition 				{get {return position.RandomizedVector;} }
		public Vector3 RandomScale 					{get {return scale.RandomizedVector;} }
		public Quaternion RandomQuaternion			{get {return rotation.RandomizedQuaternion;} }
	}
	public List<TransformWaypoint> waypoints;
	private int waypointIndex;
	private TransformWaypoint currentWaypoint {get {return waypoints[waypointIndex];}}
	private Vector3 positionTarget;
	private Vector3 positionVelocity;
	private Vector3 scaleTarget;
	private Vector3 scaleVelocity;
	private Quaternion rotationTarget;
	private Quaternion rotationVelocity;
	public enum SpaceType {Local, World}
	public SpaceType spaceType;
	void OnEnable()
	{
		if(waypoints.Count > 0)
			StartCoroutine(moveToWaypoint());
	}
	IEnumerator moveToWaypoint()
	{
		while(enabled)
		{
			positionTarget = currentWaypoint.RandomPosition;
			scaleTarget = currentWaypoint.RandomScale;
			rotationTarget = currentWaypoint.RandomQuaternion;
			float t = 0f;
			while(t < currentWaypoint.SeekTime && !waypointReached(positionTarget, scaleTarget, rotationTarget))
			{
				if(spaceType == SpaceType.Local)
				{
					transform.localPosition = Vector3.Lerp(transform.localPosition, positionTarget, Time.deltaTime * t / currentWaypoint.SeekTime*2f);
					transform.localScale = Vector3.SmoothDamp(transform.localScale, scaleTarget, ref scaleVelocity, currentWaypoint.SeekTime*0.35f);

					float rotationStep = Quaternion.Angle(rotationTarget, transform.localRotation) / (currentWaypoint.SeekTime / Time.deltaTime);
					transform.localRotation = Quaternion.RotateTowards(transform.localRotation, rotationTarget, rotationStep);
				} 
				else if (spaceType == SpaceType.World)
				{
					transform.position = Vector3.Lerp(transform.localPosition, positionTarget, Time.deltaTime * t / currentWaypoint.SeekTime*2f);
					transform.localScale = Vector3.SmoothDamp(transform.localScale, scaleTarget, ref scaleVelocity, currentWaypoint.SeekTime*0.35f);
					
					float rotationStep = Quaternion.Angle(rotationTarget, transform.rotation) / (currentWaypoint.SeekTime / Time.deltaTime);
					transform.rotation = Quaternion.RotateTowards(transform.rotation, rotationTarget, rotationStep);
				}
				t += Time.deltaTime;
				yield return null;
			}
			waypointIndex = (waypointIndex + 1) % waypoints.Count;
			yield return null;
		}
	}

	bool waypointReached(Vector3 positionTarget, Vector3 scaleTarget, Quaternion rotationTarget)
	{
		if(spaceType == SpaceType.Local)
			return positionTarget == transform.localPosition && scaleTarget == transform.localScale && transform.localRotation == rotationTarget;
		else
			return positionTarget == transform.position && scaleTarget == transform.lossyScale && transform.rotation == rotationTarget;
	}
}
