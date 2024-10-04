using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class HeroSheep : Sheep
{

	bool charging = false;
	bool chargeComplete = false;
	public float chargeTime = 1.5f;
	public int chargeDamage = 5;
	private float maxHerdSpeedBase;
	public TrailRenderer heroicTrail;
	public ParticleSystem heroicChargeUpParticles;
	public ParticleSystem heroicFullChargeParticles;
	private float cUpPPS;
	private float cFullPPS;
	public Collider2D chargeCollider;
	public override bool CanJump {get {return base.CanJump && CanBeHerded;}}
	public override bool SoloHerd {get {return soloHerding && !CanBeHerded;}}
	public override bool IsKillable {get {return base.IsKillable && CanBeHerded;}}
	protected TouchInput toucher;
	public string sendMessageOnChargeActive;
	public override bool CanFall {get {return base.CanFall && !charging;}}
	public override bool isCountable {get {	return base.isCountable || charging; } }
	public Transform visualCenterXform;
	protected override float fallTimer {get {return base.fallTimer;} set{}}

	protected override void Awake ()
	{
		base.Awake ();
		maxHerdSpeedBase = maximumHerdedSpeed;
		heroicTrail.sortingLayerName = "Character";
		heroicTrail.enabled = false;
		cUpPPS = heroicChargeUpParticles.emissionRate;
		heroicChargeUpParticles.emissionRate = 0f;
		cFullPPS = heroicFullChargeParticles.emissionRate;
		heroicFullChargeParticles.emissionRate = 0f;
	}

	public override void ReturnToIdle()
	{
		if(CanBeHerded && state != State.Dead)
		{
			wanderer.FindNewHome(0.2f);
			wanderer.RemoveOverrides();
			state = State.Idle;
		}
	}

	public override void SetCounted ()
	{
		base.SetCounted ();
		StopCoroutine("doCharge");
		StopCoroutine("chargeUp");
		StopCoroutine("startCharge");
		rb.mass = 1f;
		rb.velocity *= 0.4f;
		rb.drag = 1f;
		heroicTrail.enabled = false;
		maximumHerdedSpeed = maxHerdSpeedBase;
		CanBeHerded = true;
		charging = false;
		chargeCollider.enabled = false;
		wanderer.Resume();
		maximumHerdedSpeed = maxHerdSpeedBase;
		spriteAnimator.Resume();
		spriteAnimator.animation.framesPerSecond = 2;
		heroicChargeUpParticles.emissionRate = 0f;
		heroicFullChargeParticles.emissionRate = 0f;
	}

	public override void Die(float delay, DeathType deathType, MonoEvent onKilled)
	{
		deathEffects.ForEach(def => def.transform.parent = null);
		DebugUtility.AddLine(name+" DYING!", name);
		this.onDeath += stub;
		this.onDeath += onKilled;
		rb.velocity = Vector2.zero;
		wanderer.enabled = false;
		state = State.Dead;
		physicalCollider.enabled = false;
		this.deathType = deathType;
		switch(deathType)
		{
		case DeathType.Knife:
			Audio.NormalDeath(1f);
			break;
		case DeathType.BlackHole:
			break;
		case DeathType.Combustion:
			break;
		case DeathType.Collapse:
			Audio.HeroDeath(1f);
			break;
		case DeathType.Explosive:
			Audio.HeroDeath(1f);
			break;
		case DeathType.Fall:
			Audio.HeroDeath(1f);
			break;
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		playDeathEffects(deathType);
		LogMessage("SheepDeath");
	}

	protected override void endDeath()
	{
		spriteAnimator.ShowDeath();
		switch(deathType)
		{
		case DeathType.Knife:
			Audio.KnifeSlash(1f);
			break;
		case DeathType.BlackHole:
			break;
		case DeathType.Combustion:
			break;
		case DeathType.Collapse:
			break;
		case DeathType.Explosive:
			Audio.ExplosiveDeath(1f);
			break;
		}
		onDeath();
		//enabled = false;
		disableNonSprites();
		DebugUtility.AddLine(name+" DEATH!", name);
	}

	public override void OnTouched (TouchInput toucher)
	{
		if(charging || isCounted)
			return;
		StopCoroutine("doCharge");
		StopCoroutine("chargeUp");
		StopCoroutine("startCharge");
		StartCoroutine("startCharge");
		this.toucher = toucher;
	}

	IEnumerator startCharge()
	{
		yield return new WaitForSeconds(0.2f);
		StartCoroutine("chargeUp");
		spriteAnimator.Pause();
		spriteAnimator.animation.framesPerSecond = 10;
		wanderer.Pause();
		rb.angularVelocity = 0f;
		CanBeHerded = false;
		maximumHerdedSpeed = 0f;
		heroicChargeUpParticles.emissionRate = cUpPPS;
		#if UNITY_ANDROID && !UNITY_EDITOR
		TouchSense.instance.playBuiltinEffect(TouchSense.BUMP_33);
		#endif
	}

	public override void OnUntouched ()
	{
		if(charging || isCounted)
			return;
		base.OnUntouched ();
		StopCoroutine("doCharge");
		StopCoroutine("startCharge");
		StopCoroutine("chargeUp");
		maximumHerdedSpeed = maxHerdSpeedBase;
		spriteAnimator.Resume();
		spriteAnimator.animation.framesPerSecond = 2;
		heroicChargeUpParticles.emissionRate = 0f;
		heroicFullChargeParticles.emissionRate = 0f;
		if(chargeComplete)
		{
			chargeComplete = false;
			heroicTrail.enabled = true;
			StartCoroutine("doCharge");
			Audio.HeroBaa(1f);
		}
		else
		{
			CanBeHerded = true;
			heroicTrail.enabled = false;
			wanderer.Resume();
		}
	}

	IEnumerator chargeUp()
	{
		float t = chargeTime;
		while(t > 0f)
		{
			if(Vector2.Distance(interactiveCenter, toucher.ShephardPosition) > 0.01f)
				transform.position = (Vector3)((toucher.ShephardPosition - interactiveCenter).normalized * 0.01f) + transform.position;
			t -= Time.deltaTime;
			yield return null;
		}
		chargeComplete = true;
		UIMessage(sendMessageOnChargeActive);
		heroicChargeUpParticles.emissionRate = 0f;
		heroicFullChargeParticles.emissionRate = cFullPPS;
		#if UNITY_ANDROID && !UNITY_EDITOR
		TouchSense.instance.playBuiltinEffect(TouchSense.BUMP_100);
		#endif
	}

	IEnumerator doCharge()
	{
		charging = true;
		//transform.parent = null;
		maximumHerdedSpeed = maxHerdSpeedBase * 3f;
		fallTimer = fallTimerMax;
		yield return new WaitForSeconds(0.05f);
		fallTimer = fallTimerMax;
		CanBeHerded = true;
		Vector2 forceLine = (toucher.ShephardPosition - interactiveCenter).normalized * 25f;
		SetHerdForce(forceLine);
		Debug.DrawLine(interactiveCenter, forceLine);
		CanBeHerded = false;
		chargeCollider.enabled = true;
		Speak ("bababaBAA",2f);
		rb.mass = 100f;
		rb.drag = 0f;
		//Debug.Break();
		yield return new WaitForSeconds(1.3f);
		rb.mass = 1f;
		rb.drag = 1f;
		heroicTrail.enabled = false;
		maximumHerdedSpeed = maxHerdSpeedBase;
		CanBeHerded = true;
		charging = false;
		chargeCollider.enabled = false;
		wanderer.Resume();
	}

	protected virtual void OnCollisionEnter2D (Collision2D cln)
	{
		if(charging && !cln.collider.isTrigger && cln.relativeVelocity.magnitude > 0.3f)
		{
			AnimatedCharacter character = ColliderLinks.FindCharacter(cln.collider);
			if(character)
			{
				character.ApplyDamage(chargeDamage, DeathType.Explosive);
				if(character.immovable)
				{
					Die(0f, DeathType.Explosive, null);
					return;
				}
				if((Sheep)character)
					if(character.tag != "Sheep")
						Score.AddKillPoints(character.points, "", transform.position);
				rb.velocity = currentVelocity;
			}
		}
	}
}
