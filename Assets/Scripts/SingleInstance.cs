using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SingleInstance : DestroyIgnoreable
{
	private static SingleInstance inst;

	protected void Awake()
	{
		if(!inst)
			inst = this;
		else
		{
			SendMessage("IgnoreDestroy");
			GameObject.Destroy(gameObject);
		}
	}
	
	protected void OnDestroy()
	{
		if(!ignoreDestroy)
			inst = null;
	}
	
}
