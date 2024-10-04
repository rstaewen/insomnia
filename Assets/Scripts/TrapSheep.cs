using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class TrapSheep : BlackSheep
{
	[System.Serializable] public class Appearance
	{
		public UISpriteAnimation spriteAnimation;
		public GameObject visualHandle;
		public ParticleSystem jumpParticles;
		public GameObject speechBubble;
		public string sheepTypePrefix;
		public List<GameObject> deathEffects;
	}
	public Appearance startingAppearance;
	public Appearance revealedAppearance;
	private Appearance usingAppearance;
	public override bool CanJump {get {return (state == State.Herded || state == State.Idle) && !isMurderous && usingAppearance == startingAppearance;}}
	protected override ParticleSystem jumpPS {
		get {
			return usingAppearance.jumpParticles;
		}
		set {
			usingAppearance.jumpParticles = value;
		}
	}
	protected override UISpriteAnimation spriteAnim {
		get {
			return usingAppearance.spriteAnimation;
		}
		set {
			usingAppearance.spriteAnimation = value;
		}
	}
	protected override IEnumerator toggleMurderMode()
	{
		while(state != State.Jumping && !isCounted)
		{
			yield return new WaitForSeconds(MurderTimer);
			if(state != State.Jumping && !isCounted)
			{
				isMurderous = !isMurderous;
				setupMurderMode();
			}
		}
	}
	protected override void Awake ()
	{
		usingAppearance = startingAppearance;
		deathEffects = usingAppearance.deathEffects;
		GetComponent<FaceSpriteCardinalDirection>().animation = usingAppearance.spriteAnimation;
		base.Awake ();
	}
	public override void OnTouched (TouchInput toucher)
	{
		if(usingAppearance == startingAppearance && CanBeHerded)
			rb.velocity = Vector2.zero;
		else
			base.OnTouched (toucher);
	}
	protected override void OnKillSheep()
	{
		if(usingAppearance == startingAppearance)
		{
			usingAppearance.visualHandle.SetActive(false);
			usingAppearance = revealedAppearance;
			usingAppearance.visualHandle.SetActive(true);
			spriteSorter.SetWidget("Sheep", usingAppearance.spriteAnimation.GetComponent<UISprite>());
			speechBubble = usingAppearance.speechBubble;
			speechBubble.SetActive(true);
			speechBubbleLabel = speechBubble.GetComponentInChildren<UILabel>();
			spriteSorter.SetWidget("SpeechBubbleLabel", speechBubbleLabel);
			spriteSorter.SetWidget("SpeechBubbleSprite", speechBubble.GetComponentInChildren<UISprite>());
			speechBubble.SetActive(false);
			animCtl.animation = usingAppearance.spriteAnimation;
			animCtl.sheepTypePrefix = usingAppearance.sheepTypePrefix;
			animCtl.animation.onAnimationUpdate -= animCtl.animation.onAnimationUpdate;
			animCtl.animation.onAnimationUpdate += OnAnimationUpdate;
			deathEffects = usingAppearance.deathEffects;
			jumpPS.Emit(20);
			Audio.SmokeRelease(1f);
		}
		base.OnKillSheep();
	}
	public override void Die(float delay, DeathType deathType, MonoEvent onKilled)
	{
		deathEffects.ForEach(def => def.transform.parent = null);
		print(name+" DYING!");
		this.onDeath += stub;
		this.onDeath += onKilled;
		rb.velocity = Vector2.zero;
		wanderer.enabled = false;
		state = State.Dead;
		physicalCollider.enabled = false;
		this.deathType = deathType;
		if(usingAppearance == startingAppearance)
		{
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
				Audio.NormalDeath(1f);
				break;
			case DeathType.Explosive:
				Audio.NormalDeath(1f);
				break;
			}
		}
		else
		{
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
			spriteAnim.gameObject.SetActive(false);
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		base.playDeathEffects(deathType);
	}
	protected override void disableNonSprites()
	{
		for(int x = 0; x<transform.childCount; x++)
		{
			Transform t = transform.GetChild(x);
			for(int i = 0 ;i<t.childCount; i++)
			{
				GameObject go = t.GetChild(i).gameObject;
				if(!go.GetComponent<UISprite>())
					go.SetActive(false);
			}
		}
	}
	protected override void setupMurderMode()
	{
		base.setupMurderMode();
		if(isMurderous)
			Speak("...");
	}
	protected override void Start ()
	{
		base.Start();
		state = State.Idle;
		startingAppearance.speechBubble.GetComponent<FollowTransformRemotely>().SetParent();
		revealedAppearance.speechBubble.GetComponent<FollowTransformRemotely>().SetParent();
		revealedAppearance.speechBubble.SetActive(false);
	}
	protected override void OnEnable()
	{
		if(usingAppearance == startingAppearance)
		{
			if(baaIdleTimer > 0f)
				StartCoroutine(baaTimer());
			if(deathTimer > 0f)
				StartCoroutine(dieOnTimer());
		}
		base.OnEnable();
	}
	protected override void OnCollisionEnter2D(Collision2D collision)
	{
		if(usingAppearance == startingAppearance)
		{
			if(state == State.Idle || state == State.Herded)
				if(UnityEngine.Random.Range(0f, 1f) < baaOnCollisionChance)
					baa (0.6f);
		}
		base.OnCollisionEnter2D(collision);
	}

	protected override void baa (float volume)
	{
		if(spriteAnimator.animation.GetComponent<UISprite>().isVisible)
		{
			if(usingAppearance == startingAppearance)
			{
				Audio.NormalBaa(volume);
				base.Speak("baa.");
			}
			else
			{
				Audio.BlackBaa(volume);
				base.Speak("BAA.");
			}
			StopCoroutine("unSpeak");
			StartCoroutine("unSpeak");
		}
	}
}
