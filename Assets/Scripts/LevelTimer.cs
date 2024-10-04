using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class LevelTimer : SolipsoBehavior
{
	float timeLimit;
	float minuteDPS;
	float hourDPS;
	public Rotate minutehand;
	public Rotate hourhand;
	public TweenScale marker;
	public SpriteRenderer clockFace;
	public UILabel bonusTimeLabel;
	bool ticking = false;
	bool victorious = false;
	bool useTimer = true;
	bool bonusTime = false;

	protected virtual void Start()
	{
		SubscribeEngineMessage();
		timeLimit = LevelPicker.CurrentLevel.timeLimit;
		if(timeLimit <= 0f)
		{
			timeLimit = LevelPicker.CurrentLevel.parTime;
			clockFace.color = new Color(0.4196f,1f,1f, 0.4375f);
			enabled = false;
			minutehand.degreesPerSecond = 0f;
			hourhand.degreesPerSecond = 0f;
			useTimer = false;
			bonusTime = true;
			bonusTimeLabel.color = Color.cyan;
		}
		else
		{
			bonusTime = false;
			bonusTimeLabel.color = Color.white;
		}
		hourDPS = -360f/timeLimit;
		minuteDPS = hourDPS * 60f;
		minutehand.degreesPerSecond = minuteDPS;
		hourhand.degreesPerSecond = hourDPS;
		StartCoroutine("countdown");
	}

	protected override void OnEngineMessage (object[] data)
	{
		if((string)data[0] == "QueueVictory" && !victorious)
		{
			if(LevelPicker.CurrentLevel.allowExcessScore)
			{
				Audio.Victory(1f);
				victorious = true;
			}
			else
			{
				victorious = true;
				EndLevel();
			}
		}
	}

	public void EndLevel()
	{
		ticking = false;
		minutehand.degreesPerSecond = 0f;
		hourhand.degreesPerSecond = 0f;
		if(bonusTime)
		{
			if((LevelPicker.CurrentLevel.parTime - Time.timeSinceLevelLoad) > 0)
				bonusTimeLabel.text = "Bonus: "+((LevelPicker.CurrentLevel.parTime - Time.timeSinceLevelLoad)*LevelPicker.CurrentLevel.timeBonusMultiplier).ToString("F0");
		}
		else if(!victorious)
			bonusTimeLabel.text = "Time: "+((LevelPicker.CurrentLevel.timeLimit - Time.timeSinceLevelLoad)*LevelPicker.CurrentLevel.timeBonusMultiplier).ToString("F0");
		StopAllCoroutines();
		if(victorious)
			EngineMessage("Victory");
		else
			EngineMessage("Defeat");
		Score.ShowFinalScore();
	}

	IEnumerator countdown()
	{
		float t = timeLimit;
		while(t > 0f)
		{
			t -= Time.deltaTime;
			if(bonusTime)
				bonusTimeLabel.text = "Bonus: "+((LevelPicker.CurrentLevel.parTime - Time.timeSinceLevelLoad)*LevelPicker.CurrentLevel.timeBonusMultiplier).ToString("F0");
			else if(!victorious)
				bonusTimeLabel.text = "Time: "+((LevelPicker.CurrentLevel.timeLimit - Time.timeSinceLevelLoad)*LevelPicker.CurrentLevel.timeBonusMultiplier).ToString("F0");
			if(!ticking && t < (timeLimit/6f))
			{
				StartCoroutine(tick(t));
			}
			yield return null;
		}
		if(useTimer)
			EndLevel();
		else
		{
			ticking = false;
			minutehand.degreesPerSecond = 0f;
			hourhand.degreesPerSecond = 0f;
			StopAllCoroutines();
			clockFace.color = Color.white;
			minutehand.Reset();
			hourhand.Reset();
			bonusTimeLabel.text = "Bonus time over!";
			//bonusTimeLabel.gameObject.SetActive(false);
		}
		yield return null;
	}

	IEnumerator tick(float remainingTime)
	{
		float initialRemainingTime = remainingTime;
		ticking = true;
		while(ticking)
		{
			Audio.Tick((initialRemainingTime - remainingTime)/initialRemainingTime);
			marker.ResetToBeginning();
			marker.PlayForward();
			remainingTime -= 1f;
			yield return new WaitForSeconds(1f);
		}
	}
}
