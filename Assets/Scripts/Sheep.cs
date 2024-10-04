#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Sheep : AnimatedCharacter
{
	[SerializeField] protected bool CanBeHerded = true;
	[SerializeField] protected bool soloHerding = false;
	public virtual bool SoloHerd {get {return soloHerding;}}
	[SerializeField] protected float HerdReleaseTime = 0.5f;
	protected float herdReleaseTimer;
	[SerializeField] protected float MagnetReleaseTime = 0.5f;
	protected float magnetReleaseTimer;
	[SerializeField] protected float FearReleaseTime = 0.5f;
	protected float fearReleaseTimer;
	public float speed = 5f;
	public float maximumHerdedSpeed;
	public float Speed {get {return speed * 0.1f;}}
	public enum State {Idle, Unresponsive, Afraid, Herded, Jumping, Counted, Dead}
	private State _st = State.Idle;
	private bool wasCounted = false;
	protected State state {get {return _st;} set {if(value == State.Counted) wasCounted = true; _st = value;}}
	public float testSpeedVal;
	public virtual bool CanJump {get {return state == State.Herded || state == State.Idle;}}
	public bool IsIdle {get {return state == State.Idle;}}
	public virtual bool IsHerdable{get {return CanBeHerded && (state == State.Idle || state == State.Herded || state == State.Afraid); }}
	public bool IsMobile {get {return state != State.Dead;}}
	public virtual bool IsKillable {get {return state == State.Afraid || state == State.Herded || state == State.Idle;}}
	public bool IsVocal {get {return state == State.Afraid || state == State.Herded || state == State.Idle || state == State.Counted;}}
	public virtual bool IsAfraid {get {return state == State.Afraid;}}
	public bool isCounted {get {return state == State.Counted || wasCounted;}}
	public virtual bool isCountable {get {return IsHerdable;}}
	public override bool isDead {get {return state == State.Dead;}}
	public bool isCircle { get; set; }
	public bool IsState (Sheep.State checkedState) {return state == checkedState;}
	[SerializeField] bool canFall = true;
	[HideInInspector] public bool isFalling;
	public virtual bool CanFall {get {return canFall && !isFalling;}}
	public void SetDead (){state = State.Dead;}

	protected Wander wanderer;
	protected FaceSpriteCardinalDirection spriteAnimator;
	protected float distance;
	public float baaOnCollisionChance;
	public float baaIdleTimer;
	public float baaIdleChance = 0.3f; 
	protected float minSFXspacing = 1.5f;
	public float outsideForceMultiplier = 1.0f;
	protected float sfxSpacingTimer;
	protected ScoreEvent onJump;
	protected Vector2 jumpForceAdder;
	protected List<Sheep> toNudge;
	protected Rigidbody2D rb;
	public float flipChance;
	public Vector2 normalizedMoveDir {get { return rb.velocity.normalized; } }
	public ParticleSystem jumpParticles;
	protected virtual ParticleSystem jumpPS {get {return jumpParticles; } set {jumpParticles = value;}}
	protected float colliderRadius;
	public float deathTimer;
	public bool hasLeftSpawnArea = false;
	protected bool _isScary;
	public bool isScary {get {return _isScary;} set {if(!_isScary && value || _isScary && !value) scaredSheep.Clear(); _isScary = value;}}
	public NotifyOnCollision sightArea;
	protected List<Sheep> scaredSheep = new List<Sheep>();
	protected bool isPaused = false;
	protected Vector2 currentVelocity;
	protected Vector2 savedVelocity;
	protected Vector3 currentForce;
	protected UIRoot uiRoot;
	protected Vector2 herdForceVector = Vector2.zero;
	protected Vector2 magnetForceVector = Vector2.zero;
	protected Vector2 fearForceVector = Vector2.zero;
	protected Vector2 centerOffset;
	public Vector2 interactiveCenter {get {return (Vector2)physicalCollider.transform.position + centerOffset;} set {transform.position = value - interactiveCenter;}}
	protected SpriteSort spriteSorter;
	public Vector2 physicalColliderOffset {get {return centerOffset;}}

	public CircleCollider2D physicalCollider;
	[HideInInspector] public bool isDestroying = false;
	protected int _clouds = 0;
	[HideInInspector] public int onClouds {
		get {return _clouds;} 
		set{
			if(value > _clouds) 
			{
				fallTimer = fallTimerMax; 
				fallCount = false;
			}
			else if(value < _clouds)
			{
				if(value <= 0 && _clouds > 0)
				{
					fallTimer = fallTimerMax;
					fallCount= true;
				}
			}
			_clouds = value;
			DebugUtility.AddLine("new clouds: "+name+":"+_clouds.ToString(), name+"clouds");
		}
	}
	protected float _fallt;
	protected virtual float fallTimer {get {return _fallt;} set{_fallt = value;}}
	protected float fallTimerMax = 0.5f;
	public bool fallTrigger {get {return fallTimer <= 0f && fallCount;}}
	protected bool fallCount = false;

	void stub(int points, Vector3 position) {}
	protected void stub() {}

	protected override void Awake()
	{
		base.Awake();
		spriteSorter = GetComponent<SpriteSort>();
		TimeKeeper.OnPause += onPause;
		TimeKeeper.OnResume += onResume;
		wanderer = GetComponent<Wander>();
		spriteAnimator = GetComponent<FaceSpriteCardinalDirection>();
		this.name = (this.name + "_" + UnityEngine.Random.Range(0,9999).ToString());
		//print(name+" awake. found label: "+(speechBubbleLabel!=null?"yep":"nope"));
		onJump += stub;
		sfxSpacingTimer = minSFXspacing;
		rb = GetComponent<Rigidbody2D>();
		physicalCollider = GetComponentsInChildren<CircleCollider2D>().Where(c => !c.isTrigger).First();
		colliderRadius = physicalCollider.radius * transform.lossyScale.x;
		uiRoot = GameObject.FindGameObjectWithTag("UIRoot").GetComponent<UIRoot>();
		rb.centerOfMass = (Vector2)physicalCollider.transform.localPosition;
		isCircle = false;
		centerOffset = Vector2.Scale(physicalCollider.offset, (Vector2)physicalCollider.transform.lossyScale);
	}

	protected override void Start()
	{
		base.Start();
		centerOffset = Vector2.Scale(physicalCollider.offset, (Vector2)physicalCollider.transform.lossyScale);
	}
	public virtual void OnTouched(TouchInput toucher)
	{
		if (!isCircle)
			rb.velocity = Vector2.zero;
	}
	public virtual void OnUntouched()
	{

	}

	public virtual void SetCounted ()
	{
		state = State.Counted;
	}

	public void ClearHerding ()
	{
		herdReleaseTimer = 0f;
	}
	protected override void OnEnable()
	{
		if(baaIdleTimer > 0f)
			StartCoroutine("baaTimer");
		if(deathTimer > 0f)
			StartCoroutine(dieOnTimer());
		base.OnEnable();
	}
	protected virtual void onPause()
	{
		isPaused = true;
		spriteAnimator.Pause();
		savedVelocity = rb.velocity;
		rb.Sleep();
		//currentForce = rb.constantForce.force;
	}
	protected virtual void onResume()
	{
		isPaused = false;
		spriteAnimator.Resume();
		rb.velocity = savedVelocity;
		rb.WakeUp();
		//rb.constantForce.force = currentForce;
	}
	protected virtual void OnCollisionEnter2D(Collision2D collision)
	{
		if(state == State.Idle || state == State.Herded)
			if(UnityEngine.Random.Range(0f, 1f) < baaOnCollisionChance)
				baa (0.6f);
	}
	public virtual void AddFearTarget(Collider2D col)
	{
		Sheep sheep = ColliderLinks.FindSheep(col);
		if(sheep && sheep.isScary && (state == State.Idle || state == State.Herded))
		{
			state = State.Afraid;
			sheep.AddScared(this);
		}
	}

	void AddScared (Sheep sheep)
	{
		scaredSheep.Add(sheep);
	}

	protected IEnumerator dieOnTimer()
	{
		yield return new WaitForSeconds(deathTimer);
		Die(0f, DeathType.Collapse, null);
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
			Audio.NormalDeath(1f);
			break;
		case DeathType.Explosive:
			Audio.NormalDeath(1f);
			break;
		case DeathType.Fall:
			Audio.NormalFall(1f);
			break;
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		playDeathEffects(deathType);
		LogMessage("SheepDeath");
	}
	protected IEnumerator delayedDeath(float delay)
	{
		yield return new WaitForSeconds(delay);
		endDeath();
	}
	protected virtual void endDeath()
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
		disableNonSprites();
		enabled = false;
		DebugUtility.AddLine(name+" DEATH!", name);
	}
    protected virtual void disableNonSprites()
	{
		spriteSorter.enabled = false;
		for(int i = 0 ;i<transform.childCount; i++)
		{
			GameObject go = transform.GetChild(i).gameObject;
			if(!go.GetComponent<UISprite>())
				go.SetActive(false);
		}
	}

	public void FreezeBaas ()
	{
		StopCoroutine("baaTimer");
	}

	protected IEnumerator baaTimer()
	{
		while(IsVocal)
		{
			yield return new WaitForSeconds(UnityEngine.Random.Range(baaIdleTimer*0.5f, baaIdleTimer*1.5f));
			if(IsVocal && sfxSpacingTimer > minSFXspacing && UnityEngine.Random.Range(0f,1f) < baaIdleChance)
			{
				sfxSpacingTimer = 0f;
				baa (0.6f);
			}
		}
		yield return null;
	}

	protected virtual void baa(float volume)
	{
		if(visible)
		{
			if(state == State.Afraid)
			{
				Audio.FearBaa(volume);
				Speak ("baa!", 0.7f);
			}
			else
			{
				Audio.NormalBaa(volume);
				Speak ("baa");
			}
		}
	}
	public void Jump(ScoreEvent onJumpComplete, List<Sheep> toNudge, Vector2 forceDirection)
	{
		DebugUtility.AddLine(name+" jump toward:"+forceDirection.ToString(), name);
		state = State.Jumping;
		wanderer.RemoveOverrides();
		wanderer.Pause();
		spriteAnimator.Pause();
		physicalCollider.enabled = false;
		GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		GetComponent<Rigidbody2D>().drag = 0f;
		herdForceVector = Vector2.zero;
		this.toNudge = toNudge;
		GetComponent<Rigidbody2D>().velocity = forceDirection*2f/(1.2f);
		//jumpForceAdder = forceDirection;
		onJump += onJumpComplete;
		spriteSorter.SetDepthOffset(20, true);
		spriteSorter.FadeOut("Shadow", 0.3f);
		//GetComponent<SpriteSort>().enabled = false;
		//rb.AddForce((jumpForceAdder-normalizedMoveDir) * 10f);
		StartCoroutine(doJump());
	}
	IEnumerator doJump()
	{
		jumpPS.Emit(20);
		//print(name+": jump 1...");
		Vector3 scaleTargetBase = transform.localScale;
		Vector3 scaleTargetJump = transform.localScale * 1.5f;
		bool flip = UnityEngine.Random.Range(0f,1f) < flipChance;
		float jumpUpTime = 0.7f;
		float jumpDownTime = 0.5f;
		float t = 0f;
		UIMessage("JumpStart");
		while(t <jumpUpTime)
		{
			t += Time.deltaTime;
			transform.localScale = Vector3.Lerp(transform.localScale, scaleTargetJump, t/jumpUpTime);
			//jumpHeight += (Time.deltaTime / jumpUpTime);
			if(flip)
				transform.Rotate (0f, 0f, 180f * Time.deltaTime / jumpUpTime);

			yield return null;
		}
		//print(name+": jump 2...");
		onJump(points, transform.position);
#if UNITY_ANDROID
		PlayGameServices.unlockAchievement(Score.achievementIDs["Setting the Baar"]);
#elif UNITY_WP8
		print("unlock achievement placeholder: WP8");
#elif UNITY_IOS
#endif
		DebugUtility.AddLine(name+" counted!", name);
		baa(1f);
		t = 0f;
		bool enabledShadow = false;
		physicalCollider.enabled = true;

		GetComponent<Rigidbody2D>().drag = 0.5f;
		while(t <jumpDownTime)
		{
			t += Time.deltaTime;
			//jumpHeight -= (Time.deltaTime * 80f / jumpDownTime);
			for(int i = 0; i<toNudge.Count; i++)
			{
				if(toNudge[i] == null)
				{
					toNudge.RemoveAt(i);
					i--;
				}
				else
					toNudge[i].GetComponent<Rigidbody2D>().AddForce(((Vector2)(toNudge[i].transform.position - transform.position)).normalized * 0.2f);
			}
			transform.localScale = Vector3.Slerp(transform.localScale, scaleTargetBase, t/jumpDownTime);
			if(flip)
				transform.Rotate (0f, 0f, 180f * Time.deltaTime / jumpDownTime);
			if(t < 0.3f && !enabledShadow)
			{
				spriteSorter.FadeIn("Shadow", 0.7f);
				enabledShadow = true;
			}
			//rb.AddForce((jumpForceAdder - normalizedMoveDir) * 20f);
			yield return null;
		}
		spriteSorter.BringEven();
		//print(name+": jump 3...");
		transform.localRotation = Quaternion.identity;
		state = State.Counted;
		spriteAnimator.Resume();
		wanderer.SetGoal(null);
		wanderer.Resume();
		wanderer.FindNewHome(0f);
		GetComponent<Rigidbody2D>().drag = 1.5f;
		physicalCollider.enabled = true;
		jumpPS.Emit(20);
		spriteSorter.enabled = true;
	}

	protected virtual void OnDestroy()
	{
		foreach(Sheep scared in scaredSheep)
			scared.ReturnToIdle();
		scaredSheep.Clear();
		TimeKeeper.OnPause -= onPause;
		TimeKeeper.OnResume -= onResume;
	}
	
	void OnTriggerExit2D(Collider2D c)
	{
		DestroySheep ds = c.GetComponent<DestroySheep>();
		if(ds)
			hasLeftSpawnArea = true;
	}
	void OnCollisionExit2D(Collision2D clsn)
	{
		DestroySheep ds = clsn.collider.GetComponent<DestroySheep>();
		if(ds)
			hasLeftSpawnArea = true;
	}


	protected override void Update()
	{
//		int sortDepth = (int)(transform.localPosition.y/5f);
//		//for draw order only (z depth). not optimized but it seems to work. 400f is the hardcoded max height in this particular local space.
		//		//terrible solution.
//		if (IsMobile)
//		{
//			spriteAnimator.animation.GetComponent<UISprite>().depth = -80 - sortDepth;
//			transform.localPosition = new Vector3(transform.localPosition.x, transform.localPosition.y, sortDepth/80f - 1f - jumpHeight);
//		}
		sfxSpacingTimer += Time.deltaTime;
		currentVelocity = rb.velocity;
		magnetReleaseTimer -= Time.deltaTime;
		if(magnetReleaseTimer < 0f)
			magnetForceVector = Vector2.zero;

		switch(state)
		{
		case State.Idle:
			testSpeedVal = rb.velocity.magnitude;
			break;
		case State.Herded:
			wasAfraid = false;
			herdReleaseTimer -= Time.deltaTime;
			if(herdReleaseTimer < 0f)
			{
				ReturnToIdle();
				herdForceVector = Vector2.zero;
			}
			magnetReleaseTimer -= Time.deltaTime;
			if(magnetReleaseTimer < 0f)
			{
				magnetForceVector = Vector2.zero;
			}
			rb.AddForce((herdForceVector+magnetForceVector) * Time.deltaTime * 20f * rb.mass, ForceMode2D.Force);
			if(rb.velocity.magnitude > maximumHerdedSpeed)
				rb.velocity = rb.velocity.normalized * maximumHerdedSpeed;
			testSpeedVal = rb.velocity.magnitude;
			break;
		case State.Afraid:
			fearReleaseTimer -= Time.deltaTime;
			if(fearReleaseTimer < 0f)
			{
				ReturnToIdle();
				fearForceVector = Vector2.zero;
			}
			rb.AddForce((fearForceVector) * Time.deltaTime * 20f * rb.mass, ForceMode2D.Force);
			if(rb.velocity.magnitude > maximumHerdedSpeed)
				rb.velocity = rb.velocity.normalized * maximumHerdedSpeed;
			testSpeedVal = rb.velocity.magnitude;
			break;
		}
		if(isScary)
		{
			for(int i = 0; i<scaredSheep.Count; i++)
			{
				if(!scaredSheep[i] || !scaredSheep[i].IsAfraid)
				{
					scaredSheep.RemoveAt(i);
					i--;
				}
				else
					scaredSheep[i].SetFearDirection((scaredSheep[i].transform.position - transform.position).normalized);
			}
		}
		else if (scaredSheep.Count > 0)
		{
			foreach(Sheep scared in scaredSheep)
				scared.ReturnToIdle();
			scaredSheep.Clear();
		}
		if(fallCount)
			fallTimer -= Time.deltaTime;
		base.Update();
	}

	public void SetHerdForce(Vector2 herdVector)
	{
		if(IsHerdable)
		{
			if(isCircle)
			{
				magnetReleaseTimer = 2f * MagnetReleaseTime;
				herdReleaseTimer = 2f * HerdReleaseTime;
			}
			else
			{
				magnetReleaseTimer = MagnetReleaseTime;
				herdReleaseTimer = HerdReleaseTime;
			}
			herdForceVector = herdVector;
			if(state != State.Herded)
				wanderer.Pause();
			state = State.Herded;
		}
	}

	public void SetMagnetForce (Vector2 magnetForceAdder)
	{
		if(IsHerdable)
		{
			magnetForceVector = magnetForceAdder;
			if(state != State.Herded)
				wanderer.Pause();
			state = State.Herded;
		}
	}

	private bool wasAfraid = false;
	public void SetFearDirection(Vector2 fearVector)
	{
		if(!wasAfraid)
		{
			wanderer.Pause();
			StartCoroutine("delayFearBaa");
			DebugUtility.AddLine(name+"FEAR!!!");
		}
		isCircle = false;
		wasAfraid = true;
		fearForceVector = fearVector;
		fearReleaseTimer = FearReleaseTime;
	}

	IEnumerator delayFearBaa()
	{
		yield return new WaitForSeconds(UnityEngine.Random.Range(0f,0.2f));
		if(UnityEngine.Random.Range(0f,1f) < 0.5f)
			baa(1f);
	}

	public virtual void ReturnToIdle()
	{
		wanderer.FindNewHome(0.2f);
		wanderer.RemoveOverrides();
		state = State.Idle;
	}

	/*
	//world distance is the true distance (independent of local transform) between the last touch and this sheep.
	//max multiplier is the maximum amount of force that this function can return. force will climb quickly toward this value as the touch is closer.
	//min radius is the minimum radius where the multiplier becomes a flat value (max multiplier). this is so we dont get infinite force when touching the sheeps exact position.
	float InverseGravityForce(float worldDistance, float maxMultiplier, float minRadius)
	{
		float worldMaxInfluence = TouchInput.HerdDistance*transform.parent.lossyScale.x;
		if(worldMaxInfluence <= minRadius)
			return maxMultiplier;
		if(worldDistance <= minRadius)
			return 0f;
		//this number will approach 1 at the limit of the herding range, and zero at the minimum radius.
		float distanceRatio = Mathf.Clamp((worldDistance-minRadius) / (worldMaxInfluence-minRadius), 0f, 1f);
		//we take 1 minus the ratio so that it approaches 1 as the sheep nears the minimum radius instead.
		//if 1 is squared we still get 1, so it will match the "max multiplier" value inside the minimum radius.
		//the force approaches 0 at the limit of the herding range.
		//we subtract 1 from max multiplier and add it back later so that the force approaches 1 at the herding limit
		//instead of 0, so that sheep can actually escape the range. "1" also matches the sheep's base speed,
		//so it will look like they smoothly settle into their normal movement after escape.
		float force = (maxMultiplier-1f) * Mathf.Pow((1f - distanceRatio), 2f) + 1f;
		return force;
	}
	*/
}
