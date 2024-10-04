using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class PauseAnimation : SolipsoBehavior
{
	private UISpriteAnimation spriteAnimation;
	private int framerate;

	protected virtual void Awake()
	{
		spriteAnimation = GetComponent<UISpriteAnimation>();
		framerate = spriteAnimation.framesPerSecond;
		spriteAnimation.framesPerSecond = 0;
	}

	public void Pause()
	{
		spriteAnimation.framesPerSecond = 0;
	}

	public void Unpause()
	{
		spriteAnimation.framesPerSecond = framerate;
	}
}
