using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class DestroySheep : SolipsoBehavior
{
	public float timeDelay;
	public bool fadeOutOverTime;
	protected List<Sheep> destroyingSheep = new List<Sheep>();
	public bool checkForState = false;
	public Sheep.State checkedState = Sheep.State.Counted;

	protected virtual void OnTriggerEnter2D(Collider2D c)
	{
		Sheep s = ColliderLinks.FindSheep(c);
		if(s)
			checkSheep(s);
		else
			Physics2D.IgnoreCollision(c, GetComponent<Collider2D>());
	}
	protected virtual void OnCollisionEnter2D(Collision2D clsn)
	{
		Sheep s = ColliderLinks.FindSheep(clsn.collider);
		if(s)
			checkSheep(s);
		else
			Physics2D.IgnoreCollision(clsn.collider, GetComponent<Collider2D>());
	}
	protected void checkSheep(Sheep s)
	{
		if(!destroyingSheep.Contains(s) && s.spawnArea != GetComponent<Collider2D>() && !s.isDestroying)
		{
			if((checkForState && s.IsState(checkedState)) || !checkForState)
			{
				DebugUtility.AddLine(s.name+" removal...", s.name);
				s.isDestroying = true;
				RemoveSheep(s);
			}
		}
	}
	protected void RemoveSheep(Sheep s)
	{
		if(timeDelay != 0f)
		{
			if(fadeOutOverTime)
				foreach(UIWidget widget in s.GetComponent<SpriteSort>().sortedObjects.Select(so => so.widget))
					fadeWidget(widget);
			destroyingSheep.Add(s);
			StartCoroutine(_delayedRemoveSheep(s));
		}
		else
			GameObject.Destroy(s.gameObject);
	}
	protected virtual void fadeWidget(UIWidget w)
	{
		Color c = w.color;
		float alpha = c.a;
		TweenAlpha tween = w.gameObject.AddComponent<TweenAlpha>();
		tween.from = alpha;
		tween.to = 0f;
		tween.duration = timeDelay;
		tween.ignoreTimeScale = false;
		tween.PlayForward();
	}
	protected virtual IEnumerator _delayedRemoveSheep(Sheep s)
	{
		s.SendMessage("OnWillDestroy", timeDelay, SendMessageOptions.DontRequireReceiver);
		yield return new WaitForSeconds(timeDelay);
		destroyingSheep.Remove(s);
		if(s && s.gameObject)
			GameObject.Destroy(s.gameObject);
	}
}
