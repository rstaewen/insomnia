using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[RequireComponent (typeof(UILabel))]
public class Tips : SolipsoBehavior
{
	public List<string> tipMessages;
	private UILabel label;

	void OnEnable()
	{
		label = GetComponent<UILabel>();
		label.text = tipMessages[UnityEngine.Random.Range(0, tipMessages.Count)];
	}
	
	protected override void OnDestroy()
	{
		base.OnDestroy();
	}
	
}
