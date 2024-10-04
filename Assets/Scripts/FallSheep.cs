using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FallSheep : DestroySheep
{
	protected Transform objectPanelXform;

	protected void Awake()
	{
		objectPanelXform = GameObject.FindGameObjectWithTag("ObjectPanel").transform;
	}

	protected override void OnTriggerEnter2D(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(s)
			s.onClouds++;
		else
			Physics2D.IgnoreCollision(c, GetComponent<Collider2D>());
	}
	protected override void OnCollisionEnter2D(Collision2D clsn)
	{
	}

	void OnTriggerExit2D(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(s)
		{
			s.onClouds--;
			if(s.onClouds <= 0)
			{
				if(s.CanFall)
				{
//					Vector3 pos = s.transform.position;
//					Quaternion quat = s.transform.rotation;
					s.transform.parent = objectPanelXform;
					//s.transform.parent.GetComponent<FollowTransformRemotely>().followed = null;
					checkSheep(s);
					s.isFalling = true;
//					s.transform.position = pos;
//					s.transform.rotation = quat;
					//Debug.Break();
				}
				else
					StartCoroutine(checkFalling(s));
			}
		}
	}

	IEnumerator checkFalling(Sheep s)
	{
		do
		{
			if(s.CanFall)
			{
//				Vector3 pos = s.transform.position;
//				Quaternion quat = s.transform.rotation;
				s.transform.parent = objectPanelXform;
				checkSheep(s);
				s.isFalling = true;
				//				s.transform.position = pos;
//				s.transform.rotation = quat;
				//Debug.Break();
			}
			yield return new WaitForSeconds(0.1f);
		} while (s && !s.isDead && s.onClouds <= 0);
	}

	protected override void fadeWidget(UIWidget w)
	{
	}

	protected override IEnumerator _delayedRemoveSheep(Sheep s)
	{
		s.SendMessage("OnWillDestroy", timeDelay, SendMessageOptions.DontRequireReceiver);
		s.GetComponent<Wander>().Pause();
		Rigidbody2D rb = s.GetComponent<Rigidbody2D>();
		rb.angularVelocity = 360f;
		s.SetDead();
		rb.velocity = (s.transform.position - transform.position).normalized * 0.5f;
		s.Speak("baaa!");
		s.GetComponent<SpriteSort>().FadeOut("Shadow", 0.2f);
		s.GetComponent<SpriteSort>().FadeOut("Sheep", 2f);
		if(s.tag == "Sheep")
			LogMessage("SheepDeath");
		Audio.NormalFall(1f);
		float t = timeDelay;
		while(t > 0)
		{
			if(!s)
				yield break;
			s.transform.localScale *= (1f - (Time.deltaTime/2f));
			t -= Time.deltaTime;
			yield return null;
		}
		destroyingSheep.Remove(s);
		s.playDeathEffects(DeathType.Fall);
		//Debug.Break();
		if(s && s.gameObject)
			GameObject.Destroy(s.gameObject);
	}

}
