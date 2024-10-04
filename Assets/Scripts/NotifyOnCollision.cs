using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum TriggerType {MurderTarget, FearTarget, GoalReached, AttackCollision}

public class NotifyOnCollision : MonoBehaviour
{
	public bool disableColliderOnCollision = false;
	public List<string> tagsToDetect;
	[SerializeField] private TriggerType triggerType;
	public GameObject toNotify;
	public string functionName;
	public float notifyOnStayFrequency = -1f;
	private bool sendOnStay = false;

	void OnEnable()
	{
		if(notifyOnStayFrequency > 0f)
			StartCoroutine("enableOnStay");
	}

	IEnumerator enableOnStay()
	{
		while(true)
		{
			sendOnStay = true;
			yield return new WaitForSeconds(notifyOnStayFrequency);
		}
	}

	void OnTriggerEnter2D(Collider2D c){ OnCollisionEvent(c); }
	void OnCollisionEnter2D(Collision2D c){	OnCollisionEvent(c.collider); }
	void OnTriggerStay2D(Collider2D c){ if(sendOnStay) {OnCollisionEvent(c); sendOnStay = false;} }
	void OnCollisionStay2D(Collision2D c){ if(sendOnStay) {OnCollisionEvent(c.collider); sendOnStay = false;} }
	
	void OnCollisionEvent(Collider2D c)
	{
		if(!enabled)
			return;
		foreach(string thisTag in tagsToDetect)
			if(thisTag == c.tag)
		{
			toNotify.SendMessage(functionName, c);
			if(disableColliderOnCollision)
				GetComponent<Collider2D>().enabled = false;
			return;
		}
		Physics2D.IgnoreCollision(c, GetComponent<Collider2D>());
	}
	
}
