#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class BlackSheep : Sheep
{
	public Transform trail;
	public Transform trailL;
	public Transform trailR;
	public UISpriteAnimation spriteAnimation;
	protected virtual UISpriteAnimation spriteAnim {get {return spriteAnimation;} set {spriteAnimation = value;}}
	protected FaceSpriteCardinalDirection.MoveDirection movedir;
	protected FaceSpriteCardinalDirection animCtl;
	protected Vector3 posN = new Vector3(0f, 49f, -30f);
	protected Vector3 posNE = new Vector3(18.95349f, 50.87f, -30f);
	protected Vector3 posE = new Vector3(26.43432f, 32.57f, -30f);
	protected Vector3 posSE = new Vector3(20.33409f, 31.77f, -30f);
	protected Vector3 posS = new Vector3(0f, 31.77f, -30f);
	protected Vector3 posSW = new Vector3(-26.43432f, 31.77f, -30f);
	protected Vector3 posW = new Vector3(-28.46773f, 32.57f, -30f);
	protected Vector3 posNW = new Vector3(-26.43432f, 50.87f, -30f);
	protected Vector3 eyeOffset = new Vector3(0f,0f,-100f);
	protected Vector3 eyeOffsetL;
	protected Vector3 eyeOffsetR;
	public float MurderTimer;
	protected bool _murdering;
	protected virtual bool isMurderous {get {if(AlwaysMurderous) return true; if(MurderTimer == 0f) return true; else return _murdering;} set {_murdering = value;}} 
	public bool AlwaysMurderous = false;
	public override bool CanJump {get {return false; } }
	protected List<Sheep> murderTargets = new List<Sheep>();
	protected Sheep MurderMark = null;
	protected bool isKilling = false;
	protected MonoEvent onKill;
	public override bool IsKillable {get {return false;}}
	public float fearModeTimeAfterMurder;
	public bool startInvisible = false;

	protected override void Start()
	{
		base.Start();
		animCtl = GetComponent<FaceSpriteCardinalDirection>();
		spriteAnim.onAnimationUpdate += OnAnimationUpdate;
		eyeOffsetL = trailL.localPosition;
		eyeOffsetR = trailR.localPosition;
		onKill += OnKillSheep;
		setupMurderMode();
		bringForwardTrail(trailR);
		bringForwardTrail(trailL);
		if(startInvisible)
		{
			trail.gameObject.SetActive(false);
			spriteSorter.FadeOut("Sheep", 0f);
		}
	}
	public override void OnTouched(TouchInput toucher)
	{
	}
	protected void bringForwardTrail(Transform trailXform)
	{
		trailXform.GetComponent<TrailRenderer>().sortingLayerName = "Character";
		trailXform.GetComponent<TrailRenderer>().sortingOrder = 2;
	}
	protected void bringBackwardTrail(Transform trailXform)
	{
		trailXform.GetComponent<TrailRenderer>().sortingLayerName = "Background";
		trailXform.GetComponent<TrailRenderer>().sortingOrder = -1;
	}
	protected virtual void OnKillSheep()
	{
		if(this == null)
			return;
		StartCoroutine("laugh");
		animCtl.Resume();
		isMurderous = false;
		setupMurderMode();
		wanderer.RemoveOverrides();
		if(MurderTimer != 0f)
			StartCoroutine("toggleMurderMode");
		sightArea.GetComponent<Collider2D>().enabled = false;
		sightArea.GetComponent<Collider2D>().enabled = AlwaysMurderous;
		isScary = true;
		StartCoroutine("removeFear");
		StartCoroutine("delayedDisableKill");
		if(startInvisible)
		{
			trail.gameObject.SetActive(true);
			spriteSorter.FadeIn("Sheep", 1f);
		}
	}
	protected IEnumerator delayedDisableKill()
	{
		yield return new WaitForSeconds(1f);
		isKilling = false;
		physicalCollider.enabled = true;
	}
	public override void Die(float delay, DeathType deathType, MonoEvent onKilled)
	{
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
			break;
		case DeathType.BlackHole:
			break;
		case DeathType.Combustion:
			break;
		case DeathType.Collapse:
			break;
		case DeathType.Explosive:
			Audio.BlackBaa(1f);
			break;
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		playDeathEffects(deathType);
		spriteAnim.gameObject.SetActive(false);
	}
	protected IEnumerator removeFear()
	{
		yield return new WaitForSeconds(fearModeTimeAfterMurder);
		isScary = false;
	}
	protected virtual IEnumerator laugh()
	{
		yield return new WaitForSeconds(0.7f);
		baa(1f);
	}
	public override void OnWillDestroy(float inTime)
	{
		base.OnWillDestroy(inTime);
		onKill = () => {};
	}
	protected override void OnDestroy()
	{
		base.OnDestroy();
		onKill = () => {};
	}
	protected void OnAnimationUpdate()
	{
		if(!isKilling)
			movedir = animCtl.MoveDir;
		else
			movedir = FaceSpriteCardinalDirection.MoveDirection.N_;
	}
	public void AddMurderTarget(Collider2D col)
	{
		Sheep sheep = ColliderLinks.FindSheep(col);
		if(!MurderMark && sheep && sheep.IsKillable && !murderTargets.Contains(sheep))
			murderTargets.Add(sheep);
	}
	protected override void baa (float volume)
	{
		Audio.BlackBaa(volume);
		base.Speak("BAA.");
	}
	protected virtual void OnCollisionStay2D (Collision2D collision)
	{
		OnCollisionEnter2D(collision);
	}
	protected override void OnCollisionEnter2D (Collision2D collision)
	{
		Sheep hit = ColliderLinks.FindSheep(collision.collider);
		if(hit && !isKilling && hit == MurderMark && MurderMark.IsKillable)
		{
			animCtl.ForceOrientation(FaceSpriteCardinalDirection.MoveDirection.N_);
			bringBackwardTrail(trailR);
			bringBackwardTrail(trailL);
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			wanderer.OverrideTarget(hit.interactiveCenter);
			hit.Die(1.5f, DeathType.Knife, onKill);
			isKilling = true;
			if(MurderTimer != 0f)
				StopCoroutine("toggleMurderMode");
		}
	}
	protected override void OnEnable ()
	{
		if(MurderTimer != 0f)
			StartCoroutine("toggleMurderMode");
		else if(isMurderous)
			setupMurderMode();
		base.OnEnable();
	}
	protected virtual IEnumerator toggleMurderMode()
	{
		while(true)
		{
			yield return new WaitForSeconds(MurderTimer);
			isMurderous = !isMurderous;
			setupMurderMode();
		}
	}
	protected virtual void setupMurderMode()
	{
		murderTargets.Clear();
		MurderMark = null;
		sightArea.GetComponent<Collider2D>().enabled = false;
		sightArea.GetComponent<Collider2D>().enabled = isMurderous;
		state = State.Unresponsive;
//		if(isMurderous)
//			state = State.Unresponsive;
//		else
//			state = State.Idle;
		if(!isKilling)
			wanderer.RemoveOverrides();
	}
	protected override void Update()
	{
		base.Update();
		if(murderTargets.Count > 0)
		{
			MurderMark = murderTargets[UnityEngine.Random.Range(0, murderTargets.Count)];
			murderTargets.Clear();
		}
		if(MurderMark)
		{
			if(!isKilling && (MurderMark.isDead || MurderMark.isCounted))
			{
				MurderMark = null;
				wanderer.RemoveOverrides();
			}
			else
				wanderer.OverrideTarget(MurderMark.interactiveCenter);
		}
		if(isKilling)
			spriteSorter.SetDepthOffset(10, true);
		else
		{
			spriteSorter.BringEven();
			setEyeTrailPosition();
		}
	}

	protected void setEyeTrailPosition ()
	{
		switch(movedir)
		{
		case FaceSpriteCardinalDirection.MoveDirection.E_:
			trail.transform.localPosition = posE;
			bringForwardTrail(trailR);
			bringForwardTrail(trailL);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.SE:
			trail.transform.localPosition = posSE;
			bringForwardTrail(trailR);
			bringForwardTrail(trailL);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.S_:
			trail.transform.localPosition = posS;
			bringForwardTrail(trailR);
			bringForwardTrail(trailL);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.SW:
			trail.transform.localPosition = posSW;
			bringForwardTrail(trailR);
			bringForwardTrail(trailL);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.W_:
			trail.transform.localPosition = posW;
			bringForwardTrail(trailR);
			bringForwardTrail(trailL);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.NW:
			trail.transform.localPosition = posNW;
			bringForwardTrail(trailL);
			bringBackwardTrail(trailR);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.N_:
			trail.transform.localPosition = posN;
			bringBackwardTrail(trailL);
			bringBackwardTrail(trailR);
			break;
		case FaceSpriteCardinalDirection.MoveDirection.NE:
			trail.transform.localPosition = posNE;
			bringForwardTrail(trailR);
			bringBackwardTrail(trailL);
			break;
		}
	}
}
