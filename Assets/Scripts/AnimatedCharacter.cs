#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public delegate void MonoEvent ();
public delegate void ScoreEvent (int points, Vector3 position);

public enum DeathType {Knife, BlackHole, Combustion, Explosive, Crash, Collapse, Fall}
public abstract class IGeneratable : SolipsoBehavior
{
	public Collider2D spawnArea;
	public virtual void AttachSpawnArea(Collider2D c) {spawnArea = c;}
	public virtual bool isDead {get {return false;}}
}
public abstract class AnimatedCharacter : IGeneratable {
	
	protected float speakTime = 1f;
	public GameObject speechBubble;
	protected UILabel speechBubbleLabel;
	public List<GameObject> deathEffects;
	protected DeathType deathType;
	protected MonoEvent onDeath;
	public float HP = 5f;
	public int points = 1;
	public bool isVisible {get {return visible;}}
	private bool _v;
	protected bool visible {get {return _v;} set{if(!value && _v) OnBecameInvisible(); else if(value && !_v) OnBecameInvisible(); _v = value;}}
	protected float activeTimer = 0f;
	public bool immovable = false;

	protected virtual void Awake () {
		speechBubbleLabel = speechBubble.GetComponentInChildren<UILabel>();
	}

	protected virtual void Start () {
		speechBubbleLabel.text = "";
		speechBubble.SetActive(false);
		speechBubble.GetComponent<FollowTransformRemotely>().SetParent();
	}
	
	protected virtual void OnBecameVisible()
	{
		activeTimer = 0f;
	}
	
	protected virtual void OnBecameInvisible()
	{
		activeTimer = 0f;
	}
	
	// Update is called once per frame
	protected virtual void Update ()
	{
		if(!visible)
		{
			activeTimer+=Time.deltaTime;
			if(activeTimer > 6f)
			{
				GameObject.Destroy(gameObject);
			}
		}
	}
	
	public virtual void ApplyDamage (int dmg, DeathType deathType)
	{
		HP -= dmg;
		if(HP <= 0)
		{
			Die(0f, deathType, null);
		}
	}
	
	protected IEnumerator checkVisibility()
	{
		while(enabled)
		{
			Vector2 viewportPos = Camera.main.WorldToViewportPoint(transform.position);
			visible = (viewportPos.x >= -0.2f && viewportPos.x <= 1.2f && viewportPos.y >= -0.2f && viewportPos.y <= 1.2f);
			yield return new WaitForSeconds(0.3f);
		}
	}

	protected virtual void OnEnable()
	{
		StartCoroutine(checkVisibility());
	}
	
	public virtual void OnWillDestroy(float inTime)
	{
		StartCoroutine(windDownCoroutines(inTime*0.8f));
	}
	
	IEnumerator windDownCoroutines(float delay)
	{
		yield return new WaitForSeconds(delay);
		StopAllCoroutines();
	}

	public virtual void Die(float delay, DeathType deathType, MonoEvent onKilled){}

	
	public void playDeathEffects(DeathType dType)
	{
		IEnumerable<GameObject> matchingDeaths = deathEffects.Where(go => go.name == dType.ToString());
		if(matchingDeaths.Count() > 0)
		{
			GameObject useDeath = matchingDeaths.First();
			if(useDeath.GetComponent<EjectRemains>())
			{
				useDeath.GetComponent<EjectRemains>().QueueEjection();
				GameObject.Destroy(gameObject, 0.1f);
			}
			if(useDeath.GetComponent<ParticleSystem>())
			{
				useDeath.transform.parent = null;
				GameObject.Destroy(useDeath.gameObject, useDeath.GetComponent<ParticleSystem>().startLifetime);
				useDeath.GetComponent<ParticleSystem>().Play();
			}
		}
	}
	
	public void Speak (string text)
	{
		Speak (text, 1f);
	}
	
	public void Speak (string text, Vector2 resizeBubble)
	{
		Speak (text, 1f, resizeBubble);
	}
	
	public void Speak (string text, float visibleTime, Vector2 resizeBubble)
	{
		Speak (text, visibleTime);
		speechBubble.GetComponentInChildren<UISprite>().transform.localScale = (Vector3)resizeBubble + Vector3.forward;
	}
	
	public void Speak (string text, float visibleTime)
	{
		if(visible)
		{
			speechBubble.SetActive(true);
			speechBubbleLabel.text = text;
			speakTime = visibleTime;
			StopCoroutine("unSpeak");
			StartCoroutine("unSpeak");
		}
	}
	protected IEnumerator unSpeak()
	{
		yield return new WaitForSeconds(speakTime);
		speechBubbleLabel.text = "";
		speechBubble.SetActive(false);
	}
}
