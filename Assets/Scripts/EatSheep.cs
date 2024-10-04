using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EatSheep : SolipsoBehavior
{
	protected Vector3 baseScale;
	protected Vector3 currentScale;
	protected Vector3 targetScale;
	protected Vector3 scaleVelocity = Vector3.zero;
	public float scaleIncreasePctPerSheep;
	public float scaleAdjustmentTime;
	public SheepSucker sucker;
	private ScoreCounter scoreCounter;
	private List<Rigidbody2D> eatenBodies = new List<Rigidbody2D>();
	public List<Rigidbody2D> ignoredBodies;

	protected virtual void Awake()
	{
		baseScale = transform.localScale;
		currentScale = baseScale;
		targetScale = baseScale;
		scoreCounter = GetComponent<ScoreCounter>();
		//SubscribeEngineMessage();
	}

	public void SetNewBaseScale(Vector3 newBase)
	{
		baseScale = newBase;
	}

	protected void OnTriggerEnter2D(Collider2D col)
	{
		Rigidbody2D body = col.GetComponent<Rigidbody2D>();
		if(body && !body.isKinematic && !eatenBodies.Contains(body) && !ignoredBodies.Contains(body) && !body.GetComponent<Debris>())
		{
			print("eating body: "+body.name);
			sucker.RemoveSheep(body);
			Sheep sheep = body.GetComponent<Sheep>();
			if(sheep)
			{
				if(GetComponent<EjectRemains>())
					GetComponent<EjectRemains>().QueueEjection();
				if(sheep.IsKillable)
				{
					if(scoreCounter)
						scoreCounter.AddScore(sheep.points, body.transform.position);
					else
						LogMessage("SheepDeath");
					body.GetComponent<FaceSpriteCardinalDirection>().Pause();
					body.GetComponent<Wander>().enabled = false;
					sheep.Speak("ba~");
					Audio.SpaceDeath(1f);
					StartCoroutine(eatSheep(sheep));
				}
				else
					StartCoroutine(eatBody(body));
			}
			else
				StartCoroutine(eatBody(body));
			eatenBodies.Add(body);
			body.angularVelocity = 720f;
			body.GetComponent<Collider2D>().enabled = false;
			targetScale += (baseScale*scaleIncreasePctPerSheep);
		}
	}
	IEnumerator eatBody(Rigidbody2D rb)
	{
		rb.velocity = Vector2.zero;
		float t = 2f;
		UISprite sprite = rb.GetComponentInChildren<UISprite>();
		Color c = sprite.color;
		float alpha = c.a;
		TweenAlpha tween = sprite.gameObject.AddComponent<TweenAlpha>();
		tween.from = alpha;
		tween.to = 0f;
		tween.duration = 2f;
		tween.ignoreTimeScale = false;
		tween.PlayForward();
		while(t > 0f)
		{
			rb.transform.position = rb.transform.position + (transform.position - rb.transform.position) * 0.2f;
			rb.transform.localScale = rb.transform.localScale * 0.98f;
			t-=Time.deltaTime;
			yield return null;
		}
		eatenBodies.Remove(rb);
		GameObject.Destroy(rb.gameObject);
	}
	IEnumerator eatSheep(Sheep sheep)
	{
		sheep.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		float t = 2f;
		UISprite sprite = sheep.GetComponent<FaceSpriteCardinalDirection>().animation.GetComponent<UISprite>();
		Color c = sprite.color;
		float alpha = c.a;
		TweenAlpha tween = sprite.gameObject.AddComponent<TweenAlpha>();
		tween.from = alpha;
		tween.to = 0f;
		tween.duration = 2f;
		tween.ignoreTimeScale = false;
		tween.PlayForward();
		sheep.SendMessage("OnWillDestroy", t);
		while(t > 0f)
		{
			sheep.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			sheep.transform.position = sheep.transform.position + (transform.position - sheep.transform.position) * 0.2f;
			sheep.transform.localScale = sheep.transform.localScale * 0.98f;
			sheep.speechBubble.transform.localScale *= 0.98f;
			sheep.speechBubble.GetComponent<FollowTransformRemotely>().offsetPosition *= 0.98f;
			t-=Time.deltaTime;
			yield return null;
		}
		eatenBodies.Remove(sheep.GetComponent<Rigidbody2D>());
		GameObject.Destroy(sheep.gameObject);
	}
	
//	protected override void OnEngineMessage (object[] data) {}
	
	protected virtual void Update()
	{
		currentScale = Vector3.SmoothDamp(currentScale, targetScale, ref scaleVelocity, scaleAdjustmentTime);
		targetScale = Vector3.Max(targetScale - (Time.deltaTime*currentScale.magnitude*scaleAdjustmentTime*baseScale), baseScale);
		transform.localScale = currentScale;
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
	
}
