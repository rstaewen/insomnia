#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LoadScreen : SolipsoBehavior
{
	public delegate void LoadEvent();
	public static LoadEvent OnLevelHasFaded;
	public static LoadEvent OnLevelWillLoad;
	public static LoadEvent OnLevelHasLoaded;
	public float fadeOutTime;
	public float fadeToLoadingScreenTime;
	public float fadeOutLoadingScreenTime;
	public float fadeInTime;
	public float waitTimeBeforeLoad;
	public UISprite blackSprite;
	public UILabel loadingLabel;
	static LoadScreen inst;
	private string levelToLoad;
	private bool isActive = false;
	public Camera uiCam;
	public GameObject loadScreenPanel;
	private bool instanceSelected = false;
	private bool skipScreen = false;

	void Awake()
	{
		if(inst)
		{
			GameObject.Destroy(gameObject);
			return;
		}
		Transform parent = findRoot();
		GameObject.DontDestroyOnLoad(parent.gameObject);
		inst = this;
		instanceSelected = true;
		blackSprite.alpha = 0f;
		NGUITools.SetActiveChildren(gameObject, false);
		resetDelegates();
		uiCam.gameObject.SetActive(false);
	}
	Transform findRoot()
	{
		Transform t = transform;
		while(t.parent != null)
		{
			t = t.parent;
		}
		return t;
	}
	void FunctionStub(){}
	
	public static void LoadLevel(string levelname, LoadEvent onWillLoad, LoadEvent onHasLoaded, LoadEvent onHasFaded, bool skipScreen=false)
	{
		if(!inst)
		{
			if(onWillLoad != null)
				onWillLoad();
			Application.LoadLevel(levelname);
			if(onHasLoaded != null)
				onHasLoaded();
		}
		else
		{
			inst.skipScreen = skipScreen;
			Audio.FadeOutMusic(inst.waitTimeBeforeLoad);
			inst.uiCam.gameObject.SetActive(true);
			NGUITools.SetActive(inst.loadScreenPanel, false);
			inst.isActive = true;
			resetDelegates();
			OnLevelWillLoad += onWillLoad;
			OnLevelHasLoaded += onHasLoaded;
			OnLevelHasFaded += onHasFaded;
			
			inst.levelToLoad = levelname;
			
			inst.UIMessage("LoadScreenStart");
			inst.StartCoroutine(inst.fadeToBlackFromGame(inst.fadeOutTime));
			inst.StartCoroutine(inst.loadingText());
		}
	}
	static void resetDelegates()
	{
		OnLevelWillLoad -= OnLevelWillLoad;
		OnLevelWillLoad += inst.FunctionStub;
		OnLevelHasLoaded -= OnLevelHasLoaded;
		OnLevelHasLoaded += inst.FunctionStub;
		OnLevelHasFaded -= OnLevelHasFaded;
		OnLevelHasFaded += inst.FunctionStub;
	}
	void OnLevelWasLoaded()
	{
		if(!instanceSelected || skipScreen)
			return;
		StopAllCoroutines();
		loadingLabel.text = "LOADED!";
		StartCoroutine(fadeToBlackFromScreen(fadeOutLoadingScreenTime));
		inst.UIMessage("LoadScreenEnd");
	}
	IEnumerator loadingText()
	{
		int periodNumber = 0;
		while(isActive)
		{
			loadingLabel.text = "LOADING";
			for(int i =0; i<periodNumber; i++)
				loadingLabel.text += ".";
			periodNumber = (periodNumber+1)%4;
			yield return new WaitForSeconds(0.2f);
		}
	}
	//part 1. the level that starts the load appears to fade to black.
	//this is where the texture fades to alpha 1.
	IEnumerator fadeToBlackFromGame(float fadeTime)
	{
		NGUITools.SetActive(blackSprite.gameObject, true);
		float t = 0f;
		while(t < fadeTime)
		{
			t += Time.deltaTime;
			blackSprite.alpha += Time.deltaTime/fadeTime;
			yield return new WaitForEndOfFrame();
		}
		OnLevelHasFaded();
		if(skipScreen)
		{
			OnLevelWillLoad();
			Application.LoadLevel(levelToLoad);
			StartCoroutine(fadeToGameFromBlack(fadeInTime));
			inst.UIMessage("LoadScreenEnd");
		}
		else
			StartCoroutine(fadeToScreen(fadeToLoadingScreenTime));
	}
	//part 2. the screen is fully black, so fade the black texture out
	//after making sure to display the loading screen.
	IEnumerator fadeToScreen(float fadeTime)
	{
		NGUITools.SetActive(loadScreenPanel, true);
		NGUITools.SetActive(blackSprite.gameObject, true);
		float t = 0f;
		while(t < fadeTime)
		{
			t += Time.deltaTime;
			blackSprite.alpha -= Time.deltaTime/fadeTime;
			yield return new WaitForEndOfFrame();
		}
		NGUITools.SetActive(blackSprite.gameObject, false);
		StartCoroutine(LoadNextLevel(waitTimeBeforeLoad));
	}
	//part 3. start the actual load after "wait time before load" time
	//has passed. make sure to show some animation before its frozen.
	IEnumerator LoadNextLevel(float time)
	{
		yield return new WaitForSeconds(time);
		OnLevelWillLoad();
		Application.LoadLevel(levelToLoad);
	}
	//part 4. from loading screen, fade up black texture after level has loaded.
	//appear to fade out loading screen.
	IEnumerator fadeToBlackFromScreen(float fadeTime)
	{
		NGUITools.SetActive(blackSprite.gameObject, true);
		float t = 0f;
		while(t < fadeTime)
		{
			t += Time.deltaTime;
			blackSprite.alpha += Time.deltaTime/fadeTime;
			yield return new WaitForEndOfFrame();
		}
		StartCoroutine(fadeToGameFromBlack(fadeInTime));
		OnLevelHasLoaded();
	}
	//part 5. from black screen, fade out black texture and show new level.
	//appears to fade up new level.
	IEnumerator fadeToGameFromBlack(float fadeTime)
	{
		NGUITools.SetActive(loadScreenPanel, false);
		isActive = false;
		NGUITools.SetActive(blackSprite.gameObject, true);
		float t = 0f;
		while(t < fadeTime)
		{
			t += Time.deltaTime;
			blackSprite.alpha -= Time.deltaTime/fadeTime;
			yield return new WaitForEndOfFrame();
		}
		NGUITools.SetActive(blackSprite.gameObject, false);
		uiCam.gameObject.SetActive(false);
	}
	protected override void OnDestroy()
	{
		base.OnDestroy();
		if(instanceSelected)
		{
			OnLevelWillLoad -= OnLevelWillLoad;
			OnLevelHasLoaded -= OnLevelHasLoaded;
			OnLevelHasFaded -= OnLevelHasFaded;
			inst = null;
		}
	}
}
