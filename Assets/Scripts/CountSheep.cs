using UnityEngine;
using System.Collections;

public class CountSheep : MonoBehaviour
{
	public ScoreCounter scoreCounter;

	void Awake()
	{
		scoreCounter = GetComponent<ScoreCounter>();
	}

	protected void OnTriggerEnter2D(Collider2D col)
	{
		Sheep sheep = ColliderLinks.FindSheep(col);
		if(sheep && sheep.isCountable && !sheep.isCounted && !sheep.isDead)
		{
			if(scoreCounter)
				scoreCounter.AddScore(sheep.points, sheep.transform.position);
			sheep.SetCounted();
			Wander wanderer = sheep.GetComponent<Wander>();
			wanderer.SetGoal(null);
			wanderer.Resume();
			wanderer.SetNewHome((Vector2) transform.position, 50f);
			sheep.Speak("baa");
			Audio.NormalBaa(1f);
		}
	}
}
