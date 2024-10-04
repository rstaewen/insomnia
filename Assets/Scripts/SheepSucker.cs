using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SheepSucker : SolipsoBehavior
{
	protected List<Rigidbody2D> suckedBodies = new List<Rigidbody2D>();
	public float suckForce = 10f;
	private float suckRange;
	public List<Rigidbody2D> ignoredBodies;

	protected virtual void Awake()
	{
		//SubscribeEngineMessage();
		suckRange = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
	}

	public void RemoveSheep(Rigidbody2D body)
	{
		suckedBodies.Remove(body);
	}

	protected void OnTriggerEnter2D(Collider2D col)
	{
		Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
		if(rb && !ignoredBodies.Contains(rb) && !rb.GetComponent<Debris>())
			suckedBodies.Add(rb);
	}

	protected void OnTriggerExit2D(Collider2D col)
	{
		Rigidbody2D rb = col.GetComponent<Rigidbody2D>();
		suckedBodies.Remove(rb);
	}
	
//	protected override void OnEngineMessage (object[] data) {}
	
	protected virtual void Update()
	{
		for(int i = 0; i<suckedBodies.Count; i++)
		{
			if(suckedBodies[i] == null)
			{
				suckedBodies.RemoveAt(i);
				i--;
			}
			else
			{
				Rigidbody2D rb = suckedBodies[i];
				float distance = Vector2.Distance(rb.transform.position, transform.position);
				rb.AddForce((transform.position - rb.transform.position).normalized * (Mathf.Clamp(1f-distance/suckRange, 0.01f, 1f) * suckForce * rb.mass));
			}
		}
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
	
}
