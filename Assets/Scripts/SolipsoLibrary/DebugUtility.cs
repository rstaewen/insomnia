using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public delegate void LogEvent (string str);
public delegate void TaggedLogEvent (string str, string tag);
//public delegate void MonoEvent ();
public class DebugUtility : MonoBehaviour
{
	static DebugUtility Inst;
	UILabel text;
	List<DebugLogLine> lines = new List<DebugLogLine>();
	public bool duplicateTextInLog = false;
	public int maxLines = 5;
	public float lineRemovalTime = 0f;
	public int maxPixelWidth = 300;
	public int maxPixelHeight = 200;
	List<UILabel> debugLabels = new List<UILabel>();
	bool showText = true;
	static LogEvent OnAddLine;
	static TaggedLogEvent OnAddTaggedLine;
	public UISprite background;
	private int baseWidth;
	private int baseHeight;

	class DebugLogLine
	{
		public string text;
		public string tag;
		public float timeLeft;
		public bool timed;
		public DebugLogLine(string text, string tag, float timeLeft) {this.text = text; this.tag = tag; this.timeLeft = timeLeft; timed = (timeLeft > 0f);}
	}

	// Use this for initialization
	void Awake ()
	{
		OnAddLine = (string str) => {};
		OnAddTaggedLine = (string str, string logTag) => {};
		if(!Debug.isDebugBuild)
		{
			Inst = null;
			GameObject.Destroy(gameObject);
			return;
		}
		Inst = this;
		text = GetComponentInChildren<UILabel>();
		OnAddLine += _addLine;
		OnAddTaggedLine += _addLineTagged;
		Clear();
		if(background)
		{
			baseWidth = background.width;
			baseHeight = background.height;
		}
	}

	void Clear ()
	{
		lines.Clear();
		text.text = "";
	}

	public static void AddLine (string str)
	{
		OnAddLine(str);
	}

	public static void AddLine (string str, string logTag)
	{
		OnAddTaggedLine(str, logTag);
	}

	void _addLine(string str)
	{
		_addLineTagged(str, "");
	}

	void _addLineTagged(string str, string logTag)
	{
		if(logTag == "")
		{
			if(duplicateTextInLog)
				Debug.Log(str);
			lines.Add(new DebugLogLine(str, logTag, lineRemovalTime));
		}
		else
		{
			if(duplicateTextInLog)
				Debug.Log(logTag+": "+str);
			IEnumerable<DebugLogLine> dlines = lines.Where(dl => dl.tag == logTag);
			if(dlines.Count() > 0)
			{
				dlines.First().text = str;
				dlines.First().timeLeft = lineRemovalTime;
			}
			else
				lines.Add(new DebugLogLine(str, logTag, lineRemovalTime));
		}
		if(lines.Count > maxLines)
			lines.RemoveAt(0);
		if(Inst.showText)
		{
			text.text = createText();
			if(background)
				resizeBackground();
		}
		else
			text.text = "";
	}

	void resizeBackground()
	{
		if(lines.Count > 0)
		{
			float maxChars = (float)maxCharacters();
			if(maxChars > 0)
			{
				float fontSizeModifier = (float)text.fontSize/18f;
				background.width = (int)((231f / 20f) * maxChars * fontSizeModifier);
				background.height = (int)(157f / 8f * (float)lines.Count * fontSizeModifier)+15;
			}
		}
	}

	int maxCharacters()
	{
		return lines.Select<DebugLogLine, int> (dl => dl.text.Length).Max();
	}

	void Update()
	{
		bool changed = false;
		for(int i = 0; i<lines.Count; i++)
		{
			if(lines[i].timed)
			{
				lines[i].timeLeft -= Time.deltaTime;
				if(lines[i].timeLeft <= 0f)
				{
					lines.RemoveAt(i);
					changed = true;
					i--;
				}
			}
		}
		if(changed && showText)
			text.text = createText();
		else if(changed && !showText)
			text.text = "";
	}

	string createText()
	{
		return string.Join("\n", lines.Select<DebugLogLine, string>(dl => dl.text).ToArray());
	}

	void OnDestroy()
	{
		Inst = null;
		OnAddLine -= OnAddLine;
		OnAddTaggedLine -= OnAddTaggedLine;
		OnAddLine = (string str) => {};
		OnAddTaggedLine = (string str, string logTag) => {};
	}

	public static void AddDebugLabel(UILabel label)
	{
		if(Inst)
		{
			Inst.debugLabels.Add(label);
			label.enabled = Inst.showText;
		}
	}

	public static void RemoveDebugLabel (UILabel label)
	{
		if(Inst)
			Inst.debugLabels.Remove(label);
	}

	public void OnDebugButton()
	{
		Inst.showText = !Inst.showText;
		if(!Inst.showText)
		{
			text.text = "";
			if(background)
			{
				background.width = baseWidth;
				background.height = baseHeight;
			}
		}
		else
		{
			text.text = createText();
			if(background)
				resizeBackground();
		}
		foreach(UILabel label in Inst.debugLabels)
			label.enabled = Inst.showText;
	}
}
