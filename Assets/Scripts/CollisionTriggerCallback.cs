using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CollisionTriggerCallback : SolipsoBehavior
{
	public string MethodName = "OnTriggered";
	void OnTriggerEnter2D(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(!s)
			c.SendMessage(MethodName, GetComponent<Collider2D>(), SendMessageOptions.DontRequireReceiver);
		else
			s.SendMessage(MethodName, GetComponent<Collider2D>(), SendMessageOptions.DontRequireReceiver);
	}
}
