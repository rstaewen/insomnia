using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Shockwave : SolipsoBehavior
{
	protected TweenScale scaler;
	protected TweenAlpha alpha;
	public float delay;
	public float randomDeviation;
	public float pushForce;
	protected float minRadius;
	protected float maxRadius;
	protected virtual void Start()
	{
		scaler = GetComponent<TweenScale>();
		alpha = GetComponent<TweenAlpha>();
		scaler.enabled = false;
		alpha.enabled = false;
		GetComponent<Collider2D>().enabled = false;
		GetComponent<UISprite>().alpha = 0f;
		//SubscribeEngineMessage();
	}

	protected IEnumerator doShock()
	{
		yield return new WaitForSeconds(delay * (1f+UnityEngine.Random.Range(-randomDeviation, randomDeviation)));
		maxRadius = GetComponent<CircleCollider2D>().radius*transform.lossyScale.x * scaler.to.x;
		minRadius = GetComponent<CircleCollider2D>().radius*transform.lossyScale.x;
		scaler.ResetToBeginning();
		scaler.PlayForward();
		alpha.ResetToBeginning();
		alpha.PlayForward();
		GetComponent<Collider2D>().enabled = true;
		StartCoroutine(doShock());
		yield return new WaitForSeconds(scaler.duration);
		GetComponent<Collider2D>().enabled = false;
	}

	void OnTriggerStay2D(Collider2D c)
	{
		Sheep sheep = ColliderLinks.FindSheep(c);
		if(sheep)
		{
			Vector2 distance = (Vector2)sheep.transform.position - (Vector2)transform.position;
			float forceScalar = 1f - Mathf.Clamp(distance.magnitude / maxRadius, 0f, 1f);
			sheep.GetComponent<Rigidbody2D>().AddForce(distance.normalized * pushForce * (forceScalar));
		} else Physics2D.IgnoreCollision(c, GetComponent<Collider2D>());
	}

	void OnEnable()
	{
		StartCoroutine(doShock());
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
