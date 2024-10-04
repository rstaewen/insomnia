#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class Score : SolipsoBehavior
{
	public static Dictionary<string, string> achievementIDs = new Dictionary<string, string> ()
	{
		{"Setting the Baar", "CgkI_u-Vm5QaEAIQJQ"},
		{"Fee Fie Fo Fum", "CgkI_u-Vm5QaEAIQAQ"},
		{"The Bigger They Are", "CgkI_u-Vm5QaEAIQAg"},
		{"Shepherd of Dreams", "CgkI_u-Vm5QaEAIQAw"},
		{"Collective Dreamer", "CgkI_u-Vm5QaEAIQBA"},
		{"No Holds Baarred", "CgkI_u-Vm5QaEAIQBQ"},
		{"Raising the Baar", "CgkI_u-Vm5QaEAIQBg"},
		{"Clearing the Baar", "CgkI_u-Vm5QaEAIQBw"}
	};
	protected static Score instance;
	public UILabel countingLabel;
	protected int unadjustedPoints = 0;
	public int CurrentUnadjusted {get {return unadjustedPoints;}}
	protected int adjustedPoints = 0;
	protected int deadSheep = 0;
	protected int hiScore = 0;
	protected float comboTime;
	protected int currComboBonus = 0;
	public float minComboTime = 2f;
	public string temporaryPrefix;
	public string temporaryPostfix;
	public UILabel persistentLabel;
	public UILabel goalLabel;
	public UILabel pointLabel;
	public UILabel adjustedScoreLabel;
	public UISprite victoryCheckmark;
	public GameObject nextLevelButton;
	public GameObject endLevelArrow;
	public UILabel victoryStatus;
	public UILabel victoryTime;
	public UILabel victoryScore;
	public GameObject submitButton;
	protected bool victoryTriggered = false;
	protected bool lockAddPoints = false;
	protected string hiScorePropName {get {return Application.loadedLevelName+"_"+LevelPicker.CurrentLevel.Number.ToString()+"_HiScore";}}
	protected int totalSheep;
	protected bool hasGenerator;
	public LevelTimer levelTimer;
	private int timeBonus;
	public GameObject socialScoresObject;
	private Dictionary<int, string> leaderboardIDs;
	public MonoEvent onScoreIncreased = () => {};

	protected virtual void Awake()
	{
		instance = this;
		hiScore = PlayerPrefsWrapper.GetInt(hiScorePropName, 0);
		persistentLabel.text = unadjustedPoints.ToString();
		goalLabel.text = "/"+LevelPicker.CurrentLevel.victoryScore.ToString("D2");
		countingLabel.text = "";
		pointLabel.text = "";
		adjustedScoreLabel.text = adjustedPoints.ToString();
		SubscribeLogMessage();
		SubscribeEngineMessage();
		if(Application.loadedLevelName.Contains("Level"))
		{
			leaderboardIDs = new Dictionary<int,string>()
			{
				{17, "CgkI_u-Vm5QaEAIQCA"},
				{18, "CgkI_u-Vm5QaEAIQCQ"},
				{19, "CgkI_u-Vm5QaEAIQCg"},
				{20, "CgkI_u-Vm5QaEAIQCw"},
				{21, "CgkI_u-Vm5QaEAIQDA"},
				{22, "CgkI_u-Vm5QaEAIQDQ"},
				{23, "CgkI_u-Vm5QaEAIQDg"},
				{24, "CgkI_u-Vm5QaEAIQDw"},

				{33, "CgkI_u-Vm5QaEAIQEA"},
				{34, "CgkI_u-Vm5QaEAIQEQ"},
				{35, "CgkI_u-Vm5QaEAIQEg"},
				{36, "CgkI_u-Vm5QaEAIQEw"},
				{37, "CgkI_u-Vm5QaEAIQFA"},
				{38, "CgkI_u-Vm5QaEAIQFQ"},
				{39, "CgkI_u-Vm5QaEAIQFg"},
				{40, "CgkI_u-Vm5QaEAIQFw"},

				{49, "CgkI_u-Vm5QaEAIQGQ"},
				{50, "CgkI_u-Vm5QaEAIQGg"},
				{51, "CgkI_u-Vm5QaEAIQGw"},
				{52, "CgkI_u-Vm5QaEAIQHA"},
				{53, "CgkI_u-Vm5QaEAIQHQ"},
				{54, "CgkI_u-Vm5QaEAIQHg"},
				{55, "CgkI_u-Vm5QaEAIQHw"},
				{56, "CgkI_u-Vm5QaEAIQIA"},

				{65, "CgkI_u-Vm5QaEAIQIQ"},
				{66, "CgkI_u-Vm5QaEAIQIg"}
			};
		}
		endLevelArrow.SetActive(LockActs.isNextLevelUnlocked);
		nextLevelButton.SetActive(LockActs.isNextLevelUnlocked);
	}

	private string getLeaderboardID()
	{
		int act = System.Convert.ToInt32(Application.loadedLevelName.Replace("Level","").Trim());
		int lvl = LevelPicker.CurrentLevel.Number;
		int code = LockLevelButton.toLevelCode(act, lvl);
		return leaderboardIDs[code];
	}

	protected virtual void Start()
	{
		if(!LevelPicker.CurrentLevel.GO.GetComponentInChildren<GenerateObjects>())
		{
			totalSheep = LevelPicker.CurrentLevel.GO.GetComponentsInChildren<Sheep>().Where(sheep => sheep.tag == "Sheep").Count();
			hasGenerator = false;
		}
		else
			hasGenerator = true;
	}

	protected override void OnLogMessage (object[] data)
	{
		if((string)data[0] == "SheepDeath")
		{
			deadSheep++;
			if(!hasGenerator)
				if(totalSheep-deadSheep < LevelPicker.CurrentLevel.victoryScore)
				{
					levelTimer.EndLevel();
				}
		}
	}
	protected override void OnEngineMessage (object[] data)
	{
		if((string)data[0] == "Victory")
		{
			lockAddPoints = true;
			nextLevelButton.SetActive(true);
		}
		if((string)data[0] == "Defeat")
		{
			victoryTime.text = "";
			victoryScore.text = "";
			lockAddPoints = true;
			nextLevelButton.SetActive(LockActs.isNextLevelUnlocked);
		}
	}
	public static void AddPoints(int points, string message, Vector3 pointsAddedPosition)
	{
		instance.addPoints(points, message, pointsAddedPosition);
	}
	public static void AddKillPoints(int points, string message, Vector3 pointsAddedPosition)
	{
		instance.addKillPoints(points,message, pointsAddedPosition);
	}
	protected virtual void addKillPoints(int points, string message, Vector3 pointsAddedPosition)
	{
		if(lockAddPoints)
			return;
		if(comboTime > 0f)
			currComboBonus++;
		comboTime = minComboTime;
		adjustedPoints += (points * (currComboBonus+1));
		onScoreIncreased();
		adjustedScoreLabel.text = adjustedPoints.ToString();
		adjustedScoreLabel.GetComponent<UITweener>().ResetToBeginning();
		adjustedScoreLabel.GetComponent<UITweener>().PlayForward();
		pointLabel.gameObject.SetActive(true);
		pointLabel.text = "+"+(points*(currComboBonus+1)).ToString();
		pointLabel.transform.position = pointsAddedPosition + new Vector3(0f,0f, -1f);
		foreach(UITweener tween in pointLabel.GetComponents<UITweener>())
		{
			tween.ResetToBeginning();
			tween.PlayForward();
		}
	}

	protected virtual void addPoints(int points, string message, Vector3 pointsAddedPosition)
	{
		if(lockAddPoints)
			return;
		if(comboTime > 0f)
			currComboBonus++;
		comboTime = minComboTime;
		unadjustedPoints += 1;
		onScoreIncreased();
		adjustedPoints += (points * (currComboBonus+1));
		adjustedScoreLabel.text = adjustedPoints.ToString();
		adjustedScoreLabel.GetComponent<UITweener>().ResetToBeginning();
		adjustedScoreLabel.GetComponent<UITweener>().PlayForward();
		countingLabel.text = buildTemporaryLabelShort(message);
		persistentLabel.text = unadjustedPoints.ToString();
		goalLabel.text = "/"+LevelPicker.CurrentLevel.victoryScore.ToString("D2");
		pointLabel.gameObject.SetActive(true);
		pointLabel.text = "+"+(points*(currComboBonus+1)).ToString();
		pointLabel.transform.position = pointsAddedPosition + new Vector3(0f,0f, -1f);
		foreach(UITweener tween in pointLabel.GetComponents<UITweener>())
		{
			tween.ResetToBeginning();
			tween.PlayForward();
		}
		foreach(UITweener tween in countingLabel.GetComponents<UITweener>())
		{
			tween.ResetToBeginning();
			tween.PlayForward();
		}
		StopCoroutine("hideTemporary");
		StartCoroutine("hideTemporary");
		if(!victoryTriggered && unadjustedPoints >= LevelPicker.CurrentLevel.victoryScore)
		{
			if(LevelPicker.CurrentLevel.timeLimit > 0)
			{
				float timeLeft =(LevelPicker.CurrentLevel.timeLimit - Time.timeSinceLevelLoad);
				timeBonus = Mathf.RoundToInt(timeLeft * LevelPicker.CurrentLevel.timeBonusMultiplier);
				victoryTime.text = "Complete with "+timeLeft.ToString("F1")+" seconds remaining. +"+timeBonus.ToString()+" BONUS!";
				adjustedPoints += timeBonus;
			}
			else
			{
				float bonusTime = Mathf.Clamp(LevelPicker.CurrentLevel.parTime - Time.timeSinceLevelLoad, 0f, LevelPicker.CurrentLevel.parTime);
				timeBonus = Mathf.RoundToInt(bonusTime * LevelPicker.CurrentLevel.timeBonusMultiplier);
				if(timeBonus > 0f)
					victoryTime.text = Time.timeSinceLevelLoad.ToString("F1")+" seconds taken. +"+timeBonus.ToString()+" time bonus!";
				else
					victoryTime.text = Time.timeSinceLevelLoad.ToString("F1")+" seconds taken.";
				adjustedPoints += timeBonus;
			}
			
			EngineMessage("QueueVictory");
			victoryCheckmark.gameObject.SetActive(true);
			endLevelArrow.SetActive(true);
			victoryTriggered = true;
		}
	}
	void Update()
	{
		comboTime -= Time.deltaTime;
		if(comboTime <= 0f)
			currComboBonus = 0;
	}
	public static void ShowFinalScore()
	{
		instance.showFinalScore();
	}
	void showFinalScore()
	{
		victoryStatus.text = "[00ff00]"+unadjustedPoints.ToString()+"[-] sheep counted." +
			"\n[ff0000]"+deadSheep.ToString()+"[-] sheep [ff0000]brutally murdered[-] by nightmares.";
		victoryScore.text = adjustedPoints.ToString()+" POINTS";
		if(adjustedPoints > hiScore)
		{
			PlayerPrefsWrapper.SetInt(hiScorePropName, adjustedPoints);
			
			#if UNITY_ANDROID
			if(adjustedPoints > 1000)
				PlayGameServices.unlockAchievement(achievementIDs["Raising the Baar"]);
			if(adjustedPoints > 10000)
				PlayGameServices.unlockAchievement(achievementIDs["Clearing the Baar"]);
			#elif UNITY_WP8
			print("unlock score achievements placeholder: WP8");
			#elif UNITY_IOS
			#endif
			victoryScore.text += ": [ccffff]NEW HIGH SCORE![-]";
			victoryScore.GetComponent<TweenColor>().ResetToBeginning();
			victoryScore.GetComponent<TweenColor>().PlayForward();
			submitButton.SetActive(true);
			DebugUtility.AddLine("enable submit button...");
		}
		else
			submitButton.SetActive(false);
	}
	public void OnSubmitButton()
	{
		SocialScores ss;
		//public override void SubmitScore(string leaderBoardID, int score) {
		#if UNITY_ANDROID
		ss = socialScoresObject.GetComponent<GooglePlayScores>();
		ss.SubmitScore(getLeaderboardID(), adjustedPoints);
		ss.ShowLeaderboard();
		PlayGameServices.unlockAchievement(achievementIDs["Collective Dreamer"]);
		#elif UNITY_IOS
		ss = socialScoresObject.GetComponent<GameCenterScores>();
		ss.SubmitScore(getLeaderboardID(), adjustedPoints);
		ss.ShowLeaderboard();
		#elif UNITY_WP8
		print("show leaderboard placeholder: WP8");
		#endif
	}


	IEnumerator hideTemporary()
	{
		countingLabel.gameObject.SetActive(true);
		yield return new WaitForSeconds(0.3f);
		countingLabel.text = buildTemporaryLabelFull("");
		yield return new WaitForSeconds(1.2f);
		countingLabel.text = "";
		countingLabel.gameObject.SetActive(false);
		pointLabel.gameObject.SetActive(false);
	}
	string buildTemporaryLabelShort(string extraMessage)
	{
		string text = temporaryPrefix + unadjustedPoints.ToString();
		return text;
	}
	string buildTemporaryLabelFull(string extraMessage)
	{
		string text = temporaryPrefix + unadjustedPoints.ToString() + temporaryPostfix;
		if(extraMessage != "")
			text += "\n\t"+extraMessage;
		return text;
	}
	protected override void OnDestroy()
	{
		instance = null;
		base.OnDestroy();
	}
}
