using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TimeKeeper : DestroyIgnoreable
{
	public static float deltaTime{get {return timeScale * Time.deltaTime;}} 
	public static MonoEvent OnPause;
	public static MonoEvent OnResume;
	private static TimeKeeper inst;
	private static float timeScale = 1f;
	private float timeMarker;

	protected void Awake()
	{
		timeScale = 1f;
		inst = this;
		OnPause = () => {};
		OnResume = () => {};
	}

	public static void Pause()
	{
		if(timeScale != 0f)
		{
			timeScale = 0f;
			Time.timeScale = 0f;
			OnPause();
		}
	}

	public static void Unpause()
	{
		if(timeScale != 1f)
		{
			timeScale = 1f;
			Time.timeScale = 1f;
			OnResume();
		}
	}
	
	protected void OnDestroy()
	{
		if(!ignoreDestroy)
		{
			inst = null;
			OnPause = null;
			OnResume = null;
		}
	}
	
}
