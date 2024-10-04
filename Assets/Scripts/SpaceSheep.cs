using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpaceSheep : Sheep
{
	public override void OnTouched (TouchInput toucher){}

	protected override void baa(float volume)
	{
		if(visible)
		{
			if(state == State.Afraid)
			{
				Audio.FearBaa(volume);
				Speak ("-baa!-");
			}
			else
			{
				Audio.SpaceBaa(volume);
				Speak ("-baa-");
			}
		}
	}
	
	public override void Die(float delay, DeathType deathType, MonoEvent onKilled)
	{
		print(name+" DYING!");
		this.onDeath += stub;
		this.onDeath += onKilled;
		wanderer.enabled = false;
		GetComponent<Rigidbody2D>().angularVelocity = (UnityEngine.Random.Range(0,2)==0 ? UnityEngine.Random.Range(-400f,-300f) : UnityEngine.Random.Range(300f,400f));
		GetComponent<Rigidbody2D>().velocity *= 0.3f;
		state = State.Dead;
		this.deathType = deathType;
		switch(deathType)
		{
		case DeathType.Knife:
			break;
		case DeathType.BlackHole:
			break;
		case DeathType.Combustion:
			break;
		case DeathType.Collapse:
			break;
		case DeathType.Crash:
			Audio.SpaceDeath(1f);
			break;
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		LogMessage("SheepDeath");
	}

	protected override void endDeath()
	{
		spriteAnimator.ShowDeath();
		switch(deathType)
		{
		case DeathType.Knife:
			break;
		case DeathType.BlackHole:
			break;
		case DeathType.Combustion:
			break;
		case DeathType.Collapse:
			break;
		case DeathType.Crash:
			Audio.Crash(1f);
			break;
		}
		onDeath();
		disableNonSprites();
		enabled = false;
		print(name+" DEATH!");
	}
}
