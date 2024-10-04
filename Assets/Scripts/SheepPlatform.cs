using UnityEngine;
using System.Collections;

public class SheepPlatform : MonoBehaviour
{
	public Transform platXform;
	void Awake()
	{
		if(!platXform)
			platXform = transform;
	}
	void OnTriggerEnter2D(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(s && !s.isCounted && !s.isDead && !s.isFalling)
		{
			s.transform.parent = platXform;
			s.GetComponent<Wander>().assignHomeXform(platXform);
		}
	}
}
