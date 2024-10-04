using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Cyclops : AnimatedCharacter
{
	public List<string> speechSequence;
	public float speechLineDelay;
	public float speechSequenceDelay;
	public List<SpriteRenderer> renderers;
	bool dead = false;
	public override bool isDead {get{return dead;}}

	protected override void Awake ()
	{
		base.Awake ();
	}

	protected override void Start ()
	{
		base.Start ();
	}

	public override void ApplyDamage (int dmg, DeathType deathType)
	{
		HP -= dmg;
		if(HP <= 0)
		{
			//print(name+" apply damage DEATH: "+deathType.ToString());
			Die(0f, deathType, null);
			dead = true;
		}
		DebugUtility.AddLine(name+".HP:"+HP.ToString(), name);
		StartCoroutine("flash");
	}

	protected override void OnEnable ()
	{
		base.OnEnable ();
		if(speechSequence.Count > 0)
			StartCoroutine("FeeFieFoFum");
	}

	protected IEnumerator flash()
	{
		ChangeColor(Color.yellow);
		yield return new WaitForSeconds(0.6f);
		ChangeColor(Color.white);
	}

	protected void ChangeColor(Color c)
	{
		foreach(SpriteRenderer sr in renderers)
			sr.color = c;
	}

	protected IEnumerator FeeFieFoFum()
	{
		while(true)
		{
			yield return new WaitForSeconds(speechSequenceDelay);
			for(int i = 0; i< speechSequence.Count; i++)
			{
				Speak(speechSequence[i], speechLineDelay);
				yield return new WaitForSeconds(speechLineDelay);
			}
		}
	}

	public override void Die (float delay, DeathType deathType, MonoEvent onKilled)
	{
		deathEffects.ForEach(def => def.transform.parent = null);
		DebugUtility.AddLine(name+" Killed!", name);
		this.deathType = deathType;
		foreach(NotifyOnCollision nc in GetComponentsInChildren<NotifyOnCollision>())
			nc.enabled = false;
		GetComponentInChildren<Laserbeam>().gameObject.SetActive(false);
		GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);
		foreach(SpriteRenderer sr in GetComponentsInChildren<SpriteRenderer>())
		{
			sr.transform.parent = transform;
			if(sr.GetComponent<Collider2D>())
				sr.GetComponent<Collider2D>().isTrigger = true;
			if(sr.GetComponent<RandomlyMoveTransform>())
				sr.GetComponent<RandomlyMoveTransform>().enabled = false;
			Rigidbody2D rb = sr.gameObject.AddComponent<Rigidbody2D>();
			rb.gravityScale = 0.3f;
			rb.fixedAngle = true;
			rb.AddForce(Utilities.RandomVector());
		}
		switch(deathType)
		{
		case DeathType.Explosive:
			//Audio.NormalDeath(1f);
			break;
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		playDeathEffects(deathType);
	}

	
	protected IEnumerator delayedDeath(float delay)
	{
		yield return new WaitForSeconds(delay);
		endDeath();
	}
	protected virtual void endDeath()
	{
		switch(deathType)
		{
		case DeathType.Explosive:
			//Audio.ExplosiveDeath(1f);
			break;
		}
		enabled = false;
		DebugUtility.AddLine(name+" DEATH!", name);
		if(GetComponent<ScoreCounter>())
			GetComponent<ScoreCounter>().AddScore(points, speechBubble.transform.position);
	}
}
