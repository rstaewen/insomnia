using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class TitleScore : Score
{
	public int unlockSecretLevelPointThresh;
	public LockActs actLocker;

	protected override void Awake()
	{
		instance = this;
		hiScore = PlayerPrefsWrapper.GetInt(Application.loadedLevelName+"_HiScore", 0);
		unadjustedPoints = hiScore;
		persistentLabel.text = unadjustedPoints.ToString();
		if(unadjustedPoints > unlockSecretLevelPointThresh)
			PlayerPrefsWrapper.SetInt("SECRETLEVELS", 1);
	}

	protected override void Start()
	{
	}

	protected override void addPoints (int points, string message, Vector3 pointsAddedPosition)
	{
		unadjustedPoints ++;
		onScoreIncreased();
		persistentLabel.text = unadjustedPoints.ToString();
		hiScore = Mathf.Max(hiScore, unadjustedPoints);
		
		if(unadjustedPoints > unlockSecretLevelPointThresh)
		{
			
			#if UNITY_ANDROID
			PlayGameServices.unlockAchievement(achievementIDs["Fee Fie Fo Fum"]);
			#elif UNITY_WP8
			print("unlock secret achievement placeholder: WP8");
			#elif UNITY_IOS
			#endif
			PlayerPrefsWrapper.SetInt("SECRETLEVELS", 1);
			actLocker.CheckAllButtons();
		}


		PlayerPrefsWrapper.SetInt(Application.loadedLevelName+"_HiScore", hiScore);

	}

	protected override void OnDestroy ()
	{
		base.OnDestroy ();
	}
}
