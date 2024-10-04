using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LockLevelButton : MonoBehaviour
{
	bool isLocked = true;
	public bool IsLocked {get {return isLocked;} }
	[SerializeField] bool startUnlocked;
	protected UISprite buttonBG;
	protected UILabel levelLabel;
	protected UILabel nameLabel;
	protected UIButton button;
	string levelCode;

	public static int toLevelCode (string levelName) 
	{
		string[] split = levelName.Split('_');
		int i = 0;
		if(split[0] == "Load")
			i++;
		return toLevelCode(System.Convert.ToInt32(split[i]), System.Convert.ToInt32(split[i+1]));
	}
	public static int toLevelCode (int actCode, int levelCode) 
	{
		int lc = (actCode << 4) + levelCode;
		//Debug.Log("act code: "+actCode.ToString()+"level code: "+levelCode.ToString()+"final: "+lc.ToString());
		return lc;
	}

	void Awake()
	{
		buttonBG = GetComponentsInChildren<UISprite>().Where(sprite => sprite.name == "BG").First();
		levelLabel = GetComponentsInChildren<UILabel>().Where(label => label.name == "LevelNumberLabel").First();
		nameLabel = GetComponentsInChildren<UILabel>().Where(label => label.name == "NameLabel").First();
		button = GetComponentsInChildren<UIButton>().Where(btn => btn.name == "BG").First();
	}

	public void CheckUnlocked()
	{
		levelCode = "LVL_"+toLevelCode(name).ToString();
		int lockState = PlayerPrefsWrapper.GetInt(levelCode, -1);
		if(lockState == -1)
		{
			lockState = (startUnlocked ? 1 : 0);
			PlayerPrefsWrapper.SetInt(levelCode, lockState);
		}
		isLocked = (lockState == 0);
		setupLevelButton();
	}

	void setupLevelButton()
	{
		button.GetComponent<Collider>().enabled = !isLocked;
		levelLabel.alpha = (isLocked ? 0.5f : 1f);
		if(isLocked)
			nameLabel.text = "";
	}
}
