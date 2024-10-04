using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TouchHerdSheep : MonoBehaviour
{
	private CircleCollider2D pusherCollider;
	private Rigidbody2D pusherRB;
	private Vector2 velocity;
	private bool soloMode = false;

	public bool isActive {get {return pusherCollider.enabled;}}
	public float Range {get {return pusherCollider.radius; } }

	//[HideInInspector]
	public List<Sheep> herdable = new List<Sheep>();
	public List<Sheep> touchable = new List<Sheep>();
	void Awake()
	{
		pusherCollider = GetComponent<CircleCollider2D>();
		pusherCollider.enabled = false;
		pusherRB = pusherCollider.GetComponent<Rigidbody2D>();
	}
	
	public void RemoveNullsAndDead (List<Sheep> checkedCollection)
	{
		for(int i = 0; i<checkedCollection.Count; i++)
		{
			if(checkedCollection[i] == null || checkedCollection[i].isDead)
			{
				checkedCollection.RemoveAt(i);
				i--;
			}
		}
	}

	public void Deactivate()
	{
		RemoveNullsAndDead(touchable);
		foreach(Sheep sheep in touchable)
			sheep.OnUntouched();
		herdable.Clear();
		touchable.Clear();
		pusherCollider.enabled = false;
		soloMode = false;
	}
	public void Activate()
	{
		RemoveNullsAndDead(touchable);
		foreach(Sheep sheep in touchable)
			sheep.OnUntouched();
		pusherCollider.enabled = true;
		herdable.Clear();
		touchable.Clear();
	}
	void OnTriggerEnter2D(Collider2D c)
	{
		Sheep sheep = ColliderLinks.FindSheep(c);
		if(sheep)
		{
			if(!soloMode)
			{
				if(sheep.IsHerdable)
				{
					herdable.Add(sheep);
					touchable.Add(sheep);
					
					if(sheep.SoloHerd)
					{
						RemoveNullsAndDead(herdable);
						foreach(Sheep s in herdable)
							if(s != sheep)
						{
							s.ClearHerding();
							s.OnUntouched();
						}
						herdable.Clear();
						touchable.Clear();
						herdable.Add(sheep);
						touchable.Add(sheep);
						soloMode = true;
					}
				}
				sheep.OnTouched(transform.parent.GetComponent<TouchInput>());
			}
		}
		else
			Physics2D.IgnoreCollision(c, pusherCollider);
	}
	void OnTriggerExit2D(Collider2D c)
	{
		Sheep sheep = ColliderLinks.FindSheep(c);
		if(sheep)
		{
			touchable.Remove(sheep);
			sheep.OnUntouched();
		}
	}
	void Update()
	{
		if(!soloMode)
		{
			foreach(Sheep sheep in touchable)
			if(sheep.SoloHerd)
			{
				RemoveNullsAndDead(herdable);
				foreach(Sheep s in herdable)
					if(s != sheep)
				{
					s.ClearHerding();
					s.OnUntouched();
				}
				herdable.Clear();
				touchable.Clear();
				herdable.Add(sheep);
				touchable.Add(sheep);
				soloMode = true;
				return;
			}
		}
		else
		{
			bool noSolo = touchable.TrueForAll(sheep => !sheep.SoloHerd) || touchable.Count == 0;
			soloMode = !noSolo;
		}
	}
}
