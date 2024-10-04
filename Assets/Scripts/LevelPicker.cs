using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

[System.Serializable] public class Level
{
	[HideInInspector] public string name;
	public GameObject GO;
	public int victoryScore = 20;
	public float timeLimit = 60f;
	public float parTime = 30f;
	public float timeBonusMultiplier = 10f;
	public bool allowExcessScore = true;
	[HideInInspector] public LevelPicker parent;
	public int Number {get {return System.Convert.ToInt32(GO.name);}}
}
public class LevelPicker : SolipsoBehavior
{
	private int currLevelIndex;
	[SerializeField] List<Level> levels;
	protected static LevelPicker inst;
	public static Level CurrentLevel {get {return inst.levels[inst.currLevelIndex];}}

	void Awake ()
	{
		inst = this;
		currLevelIndex = PlayerPrefsWrapper.GetInt("LevelCode", 1) - 1;
		foreach(Level lev in levels)
		{
			if(lev.GO)
				lev.GO.SetActive(false);
			lev.parent = this;
		}
		CurrentLevel.GO.SetActive(true);
	}

	void Start()
	{
		#if UNITY_ANDROID
		PlayGameServices.enableDebugLog(true);
		#elif UNITY_WP8
		#elif UNITY_IOS
		#endif
	}

	void OnDestroy()
	{
		inst = null;
	}

	void OnValidate()
	{
		foreach(Level lev in levels)
			if(lev.GO)
				lev.name = lev.GO.name;
	}
}
