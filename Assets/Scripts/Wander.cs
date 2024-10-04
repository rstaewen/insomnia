#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Wander : MonoBehaviour
{
	public float moveSpeed;
	public float targetSelectionTime;
	public float tetherDistance = 100f;
	protected float _tetherWithScale {get {return tetherDistance * transform.lossyScale.x; }}

	Vector2 _initpos;
	protected Vector2 initialPos {get {if(homeXform) return (Vector2) homeXform.position; else return _initpos;} set{_initpos = value;}}

	protected Vector2 targetOffset;
	protected virtual Vector2 targetPosition {
		get {	if(waypoints.Count == 0) return targetOffset+initialPos; 
				else return (Vector2)waypoints[waypointIndex].position; }
	}

	protected Vector2 targetOverride = Vector2.zero;
	public float speedMultiplier = 1f;
	protected Rigidbody2D rb;
	protected virtual Vector2 Position2D {get {return (Vector2)transform.position + circleOffset;}}
	protected List<Transform> waypoints = new List<Transform>();
	protected int waypointIndex;
	protected FaceSpriteCardinalDirection spriteFacer;
	protected Transform homeXform;
	protected CircleCollider2D circleCollider;
	protected Vector2 circleOffset;

	public void SetGoal (List<Transform> assignedWaypoints)
	{
		waypoints.Clear();
		if(assignedWaypoints != null)
		{
			waypoints.AddRange(assignedWaypoints);
		}
	}

	public void assignHomeXform (Transform xform)
	{
		homeXform = xform;
		targetOffset = getTarget();
	}

	protected virtual void Start()
	{
		if(GetComponent<Sheep>())
			circleCollider = GetComponent<Sheep>().physicalCollider;
		else if(GetComponent<CircleCollider2D>())
			circleCollider = GetComponent<CircleCollider2D>();
		else foreach(CircleCollider2D cc in GetComponentsInChildren<CircleCollider2D>()) if(!cc.isTrigger)
		{
			circleCollider = cc;
			break;
		}
		rb = GetComponent<Rigidbody2D>();
		initialPos = Position2D;
		FindNewHome(0.1f);
		targetOffset = getTarget();
		circleOffset = Vector2.Scale(circleCollider.offset, (Vector2)transform.lossyScale);
	}

	public void FindNewHome (float time)
	{
		if(waypoints.Count == 0 && gameObject.activeInHierarchy)
			StartCoroutine(newHome(time));
	}

	public void FindNewHome (float time, float newTether)
	{
		tetherDistance = newTether;
		if(waypoints.Count == 0 && gameObject.activeInHierarchy)
			StartCoroutine(newHome(time));
	}

	public void SetNewHome (Vector2 home, float newTether)
	{
		tetherDistance = newTether;
		initialPos = home;
		targetOffset = Vector2.zero;
	}

	protected IEnumerator newHome (float time)
	{
		yield return new WaitForSeconds(time);
		if((GetComponent<Sheep>() && GetComponent<Sheep>().isVisible) || !GetComponent<Sheep>())
		{
			initialPos = Position2D;
			targetOffset = getTarget();
		}
	}

	Vector2 getTarget()
	{
		return new Vector2(
			UnityEngine.Random.Range(-_tetherWithScale,_tetherWithScale), 
			UnityEngine.Random.Range(-_tetherWithScale,_tetherWithScale));
	}

	protected virtual void OnEnable()
	{
		if(waypoints.Count == 0)
			StartCoroutine("NewTarget");
	}

	public void Pause()
	{
		if(waypoints.Count == 0)
			StopCoroutine("NewTarget");
		enabled = false;
	}

	public void Resume()
	{
		if(waypoints.Count == 0)
		{
			StopCoroutine("NewTarget");
			StartCoroutine("NewTarget");
		}
		enabled = true;
	}

	protected IEnumerator NewTarget()
	{
		while(true)
		{
			int tryTarget = 0;
			do {
				setTarget();
				tryTarget ++;
			} while(tryTarget < 3 && Physics.Raycast((Vector3)targetPosition+new Vector3(0f,0f,-5f), (Vector3)targetPosition, 10f));
			yield return new WaitForSeconds(targetSelectionTime);
		}
	}

	void setTarget()
	{
		targetOffset = 
			new Vector2(
				UnityEngine.Random.Range(-_tetherWithScale,_tetherWithScale), 
				UnityEngine.Random.Range(-_tetherWithScale,_tetherWithScale)
				);
	}

	public void OverrideTarget(Vector2 target) { OverrideTarget(target, speedMultiplier);}
	public void OverrideTarget(Vector2 target, float speedMultiplier)
	{
		targetOverride = target;
		StopAllCoroutines();
		this.speedMultiplier = speedMultiplier;
	}

	public void RemoveOverrides()
	{
		//print(name+" remove overrides.");
		targetOverride = Vector2.zero;
		speedMultiplier = 1f;
		if(waypoints.Count == 0)
		{
			StopCoroutine("NewTarget");
			StartCoroutine("NewTarget");
		}
		enabled = true;
	}

	protected virtual void FixedUpdate()
	{
		Vector2 addForce;
		
		if(targetOverride == Vector2.zero)
		{
			if(Vector2.Distance(Position2D, targetPosition) < _tetherWithScale/10f)
			{
				StopAllCoroutines();
				//print(name+" remove overrides.");
				StartCoroutine("NewTarget");
				return;
			}
			addForce = getMoveDirection() * moveSpeed * rb.mass * speedMultiplier;
		} else
			addForce = getTargetMoveDirection() * moveSpeed * rb.mass * speedMultiplier;
		rb.AddForce(addForce*Time.timeScale);
	}

	protected Vector2 getMoveDirection()
	{
		Vector2 desiredDir = (targetPosition - Position2D).normalized;
//		RaycastHit2D hit = Physics2D.Raycast(Position2D, targetPosition, 0.15f);
//		if(hit)
//		{
//			if(hit.collider.tag == "Obstacle")
//				return (desiredDir + hit.normal).normalized;
//		}
		return desiredDir;
	}
	protected Vector2 getTargetMoveDirection()
	{
		Vector2 desiredDir = (targetOverride - Position2D).normalized;
//		RaycastHit2D hit = Physics2D.Raycast(Position2D, targetOverride, 0.15f);
//		if(hit)
//		{
//			if(hit.collider.tag == "Obstacle")
//				return (desiredDir + hit.normal).normalized;
//		}
		return desiredDir;
	}


	protected virtual void OnTriggered(Collider2D col)
	{
		if(waypoints.Count > 0 && col.transform == waypoints[waypointIndex])
		{
			if(waypointIndex < waypoints.Count-1)
				waypointIndex ++;
			else
			{
				waypoints.Clear();
				FindNewHome(0f);
				StartCoroutine("NewTarget");
			}
		}
	}
}
