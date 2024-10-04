using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LockLevelGroup : MonoBehaviour 
{
	protected List<LockLevelButton> levelButtons = new List<LockLevelButton>();
	[SerializeField] int actNumber;
	public int ActNumber {get {return actNumber;} }
	public int LastAvailableLevel {get {return levelButtons.Count-1;}}
	public GameObject actButton;
	protected UISprite buttonBG;
	protected UISprite buttonSprite;
	protected string spriteName;
	protected UISprite actSprite;
	protected UILabel completionLabel;
	protected UIButton button;
	bool isLocked = true;
	public bool IsLocked {get {return isLocked;} }
	public float LevelProgress {get {return (float)(levelButtons.Where(btn => btn.IsLocked == false).Count()- (IsComplete ? 0 : 1)) / (float)levelButtons.Count;}}
	public string completionText;
	[HideInInspector] public bool IsComplete = false;
	public string AdditionalAccessVariable = "";

	void Awake ()
	{
		levelButtons.Clear();
		levelButtons.AddRange(GetComponentsInChildren<LockLevelButton>());
		CheckUnlocked();
	}

	public void CheckUnlocked()
	{
		IsComplete = PlayerPrefsWrapper.GetInt("ACT_"+ActNumber.ToString()+"_COMPLETE", 0) == 1;
		foreach(LockLevelButton btn in levelButtons)
			btn.CheckUnlocked();
		isLocked = levelButtons.Where(btn => btn.IsLocked == false).Count() == 0;
		if(AdditionalAccessVariable != "")
			isLocked = isLocked || PlayerPrefsWrapper.GetInt(AdditionalAccessVariable, 0) == 0;
		bool buttonOn = actButton.activeSelf;
		bool buttonParentOn = actButton.transform.parent.gameObject.activeSelf;
		actButton.SetActive(true);
		actButton.transform.parent.gameObject.SetActive(true);
		buttonSprite = actButton.GetComponentsInChildren<UISprite>().Where(sprite => sprite.name == "Sprite").First();
		spriteName = buttonSprite.spriteName;
		buttonBG = actButton.GetComponentsInChildren<UISprite>().Where(sprite => sprite.name == "BG").First();
		actSprite = actButton.GetComponentsInChildren<UISprite>().Where(label => label.name == "Sprite").First();
		completionLabel = actButton.GetComponentsInChildren<UILabel>().Where(label => label.name == "ProgressLabel").First();
		button = actButton.GetComponentsInChildren<UIButton>().Where(btn => btn.name == "BG").First();
		actButton.SetActive(buttonOn);
		if(actButton.name.Contains("Secret"))
		{
			bool wasActive = actButton.activeSelf;
			actButton.SetActive(PlayerPrefsWrapper.GetInt("SECRETLEVELS")==1);
			if(!wasActive && actButton.activeSelf)
			{
				Audio.Victory(1f);
			}
		}
		setupActButton();
		actButton.transform.parent.gameObject.SetActive(buttonParentOn);
	}

	void setupActButton()
	{
		button.GetComponent<Collider>().enabled = !isLocked;
		buttonSprite.spriteName = (isLocked ? "question" : spriteName);
		buttonSprite.MakePixelPerfect();
		if(!isLocked)
		{
			completionLabel.alpha = 0.7f + (0.3f * LevelProgress);
			completionLabel.text = (LevelProgress * 100f).ToString("N2")+completionText;
			actSprite.alpha = 1.0f;
		}
		else
		{
			completionLabel.text = "";
			actSprite.alpha = 0.5f;
		}
	}

	public LockLevelButton At(int index)
	{
		return levelButtons[index];
	}
}
