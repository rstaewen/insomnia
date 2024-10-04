using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LockActs : SolipsoBehavior
{
	public List<LockLevelGroup> groupLockers;
	[Tooltip("Unlocks all levels in sequence, using the same function that unlocks each level after a victory. Does not automatically unlock secret level(s).")]
	public bool UnlockAllLevels = false;
	protected string spriteName;
	public static List<int> LevelCountByAct= new List<int>(){8,8,8,2};
	private static LockActs inst;
	[System.Serializable] public class LevelCode
	{
		public int act;
		public int level;
		public LevelCode(int act, int level){this.act = act; this.level = level;}
	}
	public static List<LevelCode> blockedLevels = new List<LevelCode>() 
	{
		new LevelCode(4,2)
	};
	public static bool isBlocked (int act, int level) {return blockedLevels.Where(bl => bl.act == act && bl.level == level).Count()>0;}
	public static bool isCurrentLevelUnlocked = false;
	public static bool isNextLevelUnlocked = false;

	void Awake()
	{
		SubscribeMetaMessage();
		for(int i = 0; i<groupLockers.Count-1; i++)
		{
			if(groupLockers[i] == null)
				return;
			if(!groupLockers[i].IsLocked && !groupLockers[i+1].IsLocked)
				groupLockers[i].IsComplete = true;
		}
		inst = this;
		int currLevel = PlayerPrefsWrapper.GetInt("LevelCode", 1);
		int currAct = PlayerPrefsWrapper.GetInt("ActCode", 1);
		int nLevel = nextLevel(currAct, currLevel);
		int nAct = nextAct(currAct,currLevel);
		isCurrentLevelUnlocked = PlayerPrefsWrapper.GetInt(
			"LVL_"+LockLevelButton.toLevelCode(currAct, currLevel).ToString()
		, -1) == 1;
		isNextLevelUnlocked = PlayerPrefsWrapper.GetInt(
			"LVL_"+LockLevelButton.toLevelCode(nAct, nLevel).ToString()
		, -1) == 1;
	}

	protected override void OnDestroy ()
	{
		base.OnDestroy ();
		isCurrentLevelUnlocked = false;
		isNextLevelUnlocked = false;
		inst = null;
	}

	void OnValidate()
	{
		if(UnlockAllLevels)
		{
			unlockAll();
		}
		UnlockAllLevels = false;
	}

	public static void UnlockAll()
	{
		if(inst)
		{
			PlayerPrefsWrapper.SetInt("SECRETLEVELS", 1);
			inst.unlockAll();
		}
	}

	void unlockAll()
	{
		List<List<int>>levels = new List<List<int>>();
		levels.Add(new List<int>());
		for(int i = 1; i<=8; i++)
			levels.Last().Add(i);
		levels.Add(new List<int>());
		for(int i = 1; i<=9; i++)
			levels.Last().Add(i);
		levels.Add(new List<int>());
		for(int i = 1; i<=8; i++)
			levels.Last().Add(i);
		levels.Add(new List<int>());
		for(int i = 1; i<=2; i++)
			levels.Last().Add(i);
		for(int i = 0; i< levels.Count; i++)
			foreach(int l in levels[i])
				UnlockNext(i+1,l);
	}

	protected override void OnMetaMessage (object[] data)
	{
		string msg = (string)data[0];
		if(msg == "UnlockLevel")
		{
			int actCode = (int)data[1];
			int levelCode = (int)data[2];
			UnlockLevel(actCode, levelCode);
		}
		if(msg == "UnlockNext")
		{
			int actCode = (int)data[1];
			int levelCode = (int)data[2];
			UnlockNext(actCode, levelCode);
		}
	}

	void UnlockLevel (int actCode, int levelCode)
	{
		print("unlock act: "+actCode.ToString()+" level: "+levelCode.ToString());
		PlayerPrefsWrapper.SetInt("LVL_"+LockLevelButton.toLevelCode(actCode, levelCode).ToString(),1);
		CheckAllButtons();
	}

	public void CheckAllButtons()
	{
		foreach(LockLevelGroup grp in groupLockers)
			grp.CheckUnlocked();
	}

	public static int nextAct (int currAct, int currLevel)
	{
		if((currLevel+1) <= LevelCountByAct[currAct-1])
			return currAct;
		else if(currAct+1 <= LevelCountByAct.Count)
			return currAct+1;
		else
			return -1;
	}
	public static int nextLevel (int currAct, int currLevel)
	{
		if((currLevel+1) <= LevelCountByAct[currAct-1])
			return currLevel+1;
		else if(currAct+1 <= LevelCountByAct.Count)
			return 1;
		else
			return -1;
	}

	void UnlockNext (int currentActCode, int currentLevelCode)
	{
		DebugUtility.AddLine("unlock act: "+currentActCode.ToString()+" level: "+currentLevelCode.ToString());
		if((currentLevelCode+1) <= LevelCountByAct[currentActCode-1])
		{
			int actCode = currentActCode;
			int levelCode = currentLevelCode+1;
			PlayerPrefsWrapper.SetInt("LVL_"+LockLevelButton.toLevelCode(actCode, levelCode).ToString(),1);
		}
		else if(currentActCode+1 <= LevelCountByAct.Count)
		{
			PlayerPrefsWrapper.SetInt("ACT_"+currentActCode.ToString()+"_COMPLETE", 1);
			int actCode = currentActCode+1;
			int levelCode = 1;
			PlayerPrefsWrapper.SetInt("LVL_"+LockLevelButton.toLevelCode(actCode, levelCode).ToString(),1);
		}
		#if UNITY_ANDROID
		if(currentActCode == 3 && currentLevelCode == LevelCountByAct[currentActCode])
			PlayGameServices.unlockAchievement(Score.achievementIDs["Shepherd of Dreams"]);
		#elif UNITY_WP8
		print("unlock achievement placeholder: WP8");
		#elif UNITY_IOS
		#endif
	}
}
