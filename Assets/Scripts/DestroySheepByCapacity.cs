using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DestroySheepByCapacity : DestroySheep
{
	public int capacity;
	protected List<Sheep> heldSheep = new List<Sheep>();

	protected override void OnTriggerEnter2D(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(s && !destroyingSheep.Contains(s) && !heldSheep.Contains(s) && s.spawnArea != GetComponent<Collider2D>())
		{
			heldSheep.Add(s);
			Utilities.RemoveDuplicates(heldSheep);
			Utilities.RemoveNulls(heldSheep);
			if(heldSheep.Count > capacity)
			{
				RemoveSheep(heldSheep[0]);
				heldSheep.RemoveAt(0);
			}
		}
	}
	protected override void OnCollisionEnter2D(Collision2D clsn)
	{
		Sheep s = ColliderLinks.FindSheep(clsn.collider);
		if(s && !destroyingSheep.Contains(s) && !heldSheep.Contains(s) && s.spawnArea != GetComponent<Collider2D>())
		{
			heldSheep.Add(s);
			Utilities.RemoveDuplicates(heldSheep);
			Utilities.RemoveNulls(heldSheep);
			if(heldSheep.Count > capacity)
			{
				RemoveSheep(heldSheep[0]);
				heldSheep.RemoveAt(0);
			}
		}
	}
	
}
