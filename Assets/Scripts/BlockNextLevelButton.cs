using UnityEngine;
using System.Collections;

public class BlockNextLevelButton : MonoBehaviour {

	public GameObject nextLevelButton;
	private bool lockLevel;

	void OnEnable()
	{
		if(!nextLevelButton)
			nextLevelButton = gameObject;
		int currLevel = PlayerPrefsWrapper.GetInt("LevelCode", 1);
		int currAct = PlayerPrefsWrapper.GetInt("ActCode", 1);
		int nextLevel = LockActs.nextLevel(currAct, currLevel);
		int nextAct = LockActs.nextAct(currAct, currLevel);
		lockLevel = LockActs.isBlocked(nextAct, nextLevel);
		if(lockLevel)
		{
			//print("next level is blocked!!");
			nextLevelButton.SetActive(false);
		}
		else
			nextLevelButton.SetActive(true);
	}
}
