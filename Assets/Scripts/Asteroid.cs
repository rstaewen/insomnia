using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Asteroid : IGeneratable
{
	public List<string> killTags;

	protected virtual void Awake()
	{
		name = name.Replace("(Clone)","") + UnityEngine.Random.Range(1,10000).ToString();
		//SubscribeEngineMessage();
	}
	
	protected virtual void Start()
	{
	}

	public void OnCollisionEnter2D(Collision2D cln)
	{
		if(killTags.Contains(cln.collider.tag))
		{
			Sheep s = ColliderLinks.FindSheep(cln.collider);
			if(s && s.IsKillable && impactThresholdMet(s.GetComponent<Rigidbody2D>(), GetComponent<Rigidbody2D>(), cln.relativeVelocity))
			{
				s.Die(0f, DeathType.Crash, null);
				s.physicalCollider.isTrigger = true;
			}
		}
	}

	bool impactThresholdMet(Rigidbody2D a, Rigidbody2D b, Vector2 relativeVelocity)
	{
		if(relativeVelocity.magnitude > 1)
		{
			return true;
		}
		return false;
	}
	
//	protected override void OnEngineMessage (object[] data) {}
	
	protected virtual void Update()
	{
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
	
}
