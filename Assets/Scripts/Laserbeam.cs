using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Laserbeam : MonoBehaviour
{
	private List<Sheep> targets = new List<Sheep>();
	public float activeTime = 2f;
	public float activeFrequency = 6f;
	public float angleSweep = 20f;
	public Transform eye;
	public GameObject beam;
	private Sheep currentTarget;
	public ParticleSystem chargeParticles;
	// Use this for initialization
	void Start () {
	
	}
	
	protected void OnEnable ()
	{
		beam.SetActive(false);
		StartCoroutine("fireLasers");
	}

	protected IEnumerator fireLasers()
	{
		while(true)
		{
			yield return new WaitForSeconds(activeFrequency/2f);
			RemoveNullsAndDead();
			if(targets.Count > 0)
			{
				currentTarget = targets[UnityEngine.Random.Range(0,targets.Count)];
				StartCoroutine(seekTarget(currentTarget));
			}
			yield return new WaitForSeconds(activeFrequency/2f);
			RemoveNullsAndDead();
			if(currentTarget)
				StartCoroutine(laser(currentTarget));
		}
		yield return null;
	}

	public void RemoveNullsAndDead ()
	{
		for(int i = 0; i<targets.Count; i++)
		{
			if(targets[i] == null || targets[i].isDead)
			{
				targets.RemoveAt(i);
				i--;
			}
		}
	}

	public void LaserHit(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(s)
		{
			if(!s.isDead)
			{
				s.Die(0.3f, DeathType.Explosive, null);
			}
		}
	}

	protected IEnumerator seekTarget(Sheep target)
	{
		chargeParticles.Play();
		float t = activeFrequency/2f;
		Vector2 diff;
		Quaternion targetRotation;
		while(t > 0f && target)
		{
			diff = target.interactiveCenter - (Vector2)transform.position;
			targetRotation = Quaternion.Euler(0f,0f, Mathf.Atan2(diff.y, -diff.x)*Mathf.Rad2Deg - (0.5f*angleSweep));
			eye.transform.rotation = Quaternion.Lerp(eye.transform.rotation, targetRotation, activeFrequency/2f*Time.deltaTime);
			t -= Time.deltaTime;
			yield return null;
		}
		chargeParticles.Stop();
		yield return null;
	}

	protected IEnumerator laser(Sheep target)
	{
		float t = activeTime;
		beam.SetActive(true);
		Vector2 diff = target.interactiveCenter - (Vector2)transform.position;
		float degrees = Mathf.Atan2(diff.y, -diff.x)*Mathf.Rad2Deg;
		eye.transform.rotation = Quaternion.Euler(0f,0f, degrees - (0.5f*angleSweep));
		while(t > 0f)
		{
			eye.transform.rotation = Quaternion.Euler(0f,0f, degrees + ((activeTime-t)/activeTime * angleSweep));
			t -= Time.deltaTime;
			yield return null;
		}
		beam.SetActive(false);
		yield return null;
	}

	public void AddMurderTarget(Collider2D col)
	{
		Sheep s = ColliderLinks.FindSheep(col);
		if(s)
		{
			if(!targets.Contains(s) && !s.isDead)
			{
				targets.Add(s);
			}
		}
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
