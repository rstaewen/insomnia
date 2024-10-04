using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Debris : SolipsoBehavior
{
	protected virtual void Awake()
	{
		//SubscribeEngineMessage();
	}
	
	protected virtual void Start()
	{
	}
	
//	protected override void OnEngineMessage (object[] data) {}
	
	protected virtual void Update()
	{
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
	
}
