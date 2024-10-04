using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class UIController : SolipsoBehavior
{
	private Dictionary<string, string> nameToCode = new Dictionary<string, string>() {
		{"PlayButton", "Play"},
		{"OptionsButton", "Options"},
		{"QuitButton", "Quit"},
		{"MainButton", "Main"},
		{"ParanoiaButton", "Level1"},
		{"SchizoidButton", "Level2"},
		{"DysphoriaButton", "Level3"},
		{"SecretButton", "Level4"},
		{"LoadMainMenuButton", "LoadMain"},
		{"TitleButton", "LoadMain"},
		{"PauseButton", "Pause"},
		{"UnlockButton", "Unlock"},
		{"RetryButton", "LoadRetry"}};
	[System.Serializable] public class MenuState
	{
		public string name;
		public List<UIPanel> panels;
		public void SetActive(bool activate)
		{
			foreach(UIPanel panel in panels)
				NGUITools.SetActive(panel.gameObject, activate);
		}
		public string backButtonActionName;
	}

	public List<MenuState> menuStates;
	public string ActiveMenuOnLoad;
	private MenuState activeState;
	private Dictionary<string, MenuState> menuStateDict = new Dictionary<string, MenuState>();
	private LoadScreen.LoadEvent quitEvent;
	private bool freezeState = false;
	private int nextLevel;
	private int nextAct;
	private bool delayedResponse = false;
	public float buttonDelay = 0.2f;
	private string buttonName = "";

	void Awake()
	{
		foreach(MenuState menu in menuStates)
		{
			menuStateDict.Add(menu.name, menu);
			menu.SetActive(false);
		}
		activeState = menuStateDict[ActiveMenuOnLoad];
		activeState.SetActive(true);
		quitEvent += Quit;
		SubscribeEngineMessage();
	}

	public void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			backButtonAction();
	}

	void backButtonAction()
	{
		switchMenu(activeState.backButtonActionName);
	}

	void Quit()
	{
		Application.Quit();
	}
	protected override void OnEngineMessage (object[] data)
	{
		if((string)data[0] == "Defeat")
		{
			switchMenu("Defeat");
			//TimeKeeper.Unpause();
			freezeState = true;
			int currLevel = PlayerPrefsWrapper.GetInt("LevelCode", 1);
			int currAct = PlayerPrefsWrapper.GetInt("ActCode", 1);
			nextAct = LockActs.nextAct(currAct, currLevel);
			nextLevel = LockActs.nextLevel(currAct, currLevel);
		}
		else if((string)data[0] == "Victory")
		{
			switchMenu("Victory");
			//TimeKeeper.Unpause();
			freezeState = true;
			int currLevel = PlayerPrefsWrapper.GetInt("LevelCode", 1);
			int currAct = PlayerPrefsWrapper.GetInt("ActCode", 1);
			MetaMessage("UnlockNext", currAct, currLevel);
			nextAct = LockActs.nextAct(currAct, currLevel);
			nextLevel = LockActs.nextLevel(currAct, currLevel);
		}
	}

	public void OnButtonSender(GameObject sender)
	{
		if(!delayedResponse)
		{
			//print("delay button sender: "+sender.name);
			buttonName = sender.name;
			if(Time.timeScale == 0f)
				buttonSender();
			else
			{
				delayedResponse = true;
				StartCoroutine("buttonSenderDelayed");
			}
		}
	}
	
	IEnumerator buttonSenderDelayed()
	{
		yield return new WaitForSeconds(buttonDelay);
		delayedResponse = false;
		//print("delay finished: "+buttonName);
		buttonSender();
	}
	bool attemptShowAchievements = false;
	void buttonSender()
	{
		if(buttonName.StartsWith("Achievements"))
		{
			#if UNITY_ANDROID
			if(PlayGameServices.isSignedIn())
				PlayGameServices.showAchievements();
			else
			{
				attemptShowAchievements = true;
				GPGManager.authenticationSucceededEvent += GooglePlayOnAuthentication;
				PlayGameServices.authenticate();
			}
			#elif UNITY_WP8
			print("show achievements placeholder: WP8");
			#elif UNITY_IOS
			#endif
		}
		else if(buttonName.StartsWith("Load_"))
			LoadScene(buttonName);
		else if(buttonName == "NextButton")
			LoadScene("Load_"+nextAct.ToString()+"_"+nextLevel.ToString(), true);
		else if(buttonName == "ResetButton")
			PlayerPrefsWrapper.DeleteAll();
		else if(buttonName == "UnlockButton")
			LockActs.UnlockAll();
		else
			switchMenu(nameToCode[buttonName]);
	}
	#if UNITY_ANDROID
	void GooglePlayOnAuthentication(string userId)
	{
		if(attemptShowAchievements)
			PlayGameServices.showAchievements();
		attemptShowAchievements = false;
	}
	#endif
	void switchMenu(string code)
	{
		if(code == "Quit")
		{
			TimeKeeper.Unpause();
			LoadScreen.LoadLevel("Main", null, null, quitEvent);
		}
		else if(code.StartsWith("Load"))
		{
			string levelName = code.Substring("Load".Length);
			if(levelName == "Retry")
				levelName = Application.loadedLevelName;
			TimeKeeper.Unpause();
			LoadScreen.LoadLevel(levelName, null, null, null);
		}
		else
		{
			if(freezeState)
				return;
			if(code == "Pause")
			{
				if(activeState.name == "Pause")
				{
					backButtonAction();
					return;
				}
				TimeKeeper.Pause();
			}
			else
				TimeKeeper.Unpause();
			activeState.SetActive(false);
			menuStateDict[code].SetActive(true);
			activeState = menuStateDict[code];
		}
	}

	void LoadScene(string loadCode, bool skipScreen = false)
	{
		string[] split = loadCode.Split('_');
		LoadScene("Level"+split[1], System.Convert.ToInt32(split[1]), System.Convert.ToInt32(split[2]), skipScreen);
	}
	
	void LoadScene(string sceneName, int actCode, int levelCode, bool skipScreen = false)
	{
		PlayerPrefsWrapper.SetInt("ActCode", actCode);
		PlayerPrefsWrapper.SetInt("LevelCode", levelCode);
		LoadScreen.LoadLevel(sceneName, null, null, null, skipScreen);
	}
}
