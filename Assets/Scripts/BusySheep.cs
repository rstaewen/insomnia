using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class BusySheep : Sheep
{
	protected override void baa(float volume)
	{
		if(visible)
		{
			if(state == State.Afraid)
			{
				Audio.FearBaa(volume);
				Speak ("ba!", 0.7f, new Vector2(0.15f, 0.2f));
			}
			else
			{
				int index = Audio.BusyBaa(volume);
				if(index == 0 || index == 1)
					Speak ("bababababa", 2f, new Vector2(0.44f, 0.2f));
				else
					Speak ("ba", new Vector2(0.15f, 0.2f));
			}
		}
	}
	
	public override void Die(float delay, DeathType deathType, MonoEvent onKilled)
	{
		DebugUtility.AddLine(name+" DYING!", name);
		this.onDeath += stub;
		this.onDeath += onKilled;
		rb.velocity = Vector2.zero;
		wanderer.enabled = false;
		state = State.Dead;
		physicalCollider.enabled = false;
		this.deathType = deathType;
		switch(deathType)
		{
		case DeathType.Knife:
			Audio.BusyDeath(1f);
			break;
		case DeathType.BlackHole:
			break;
		case DeathType.Combustion:
			break;
		case DeathType.Collapse:
			Audio.BusyDeath(1f);
			break;
		}
		if(delay == 0f)
			endDeath();
		else
			StartCoroutine(delayedDeath(delay));
		LogMessage("SheepDeath");
		playDeathEffects(deathType);
	}
	
}
