using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

[CustomEditor(typeof(PlayerPrefsWrapper))]
public class PlayerPrefsWrapperEditor : Editor 
{
	bool fold = false;
	int prefsContentsCount = 0;
	public override void OnInspectorGUI()
	{
		PlayerPrefsWrapper myTarget = (PlayerPrefsWrapper)target;
		if(GUILayout.Button("Wipe Player Prefs"))
		{
			myTarget.DeleteMetaInfo();
			PlayerPrefs.DeleteAll();
		}
		if(GUILayout.Button("Repopulate Viewer"))
			myTarget.PopulatePreferences();
		fold = EditorGUILayout.Foldout(fold, "Player Prefs Contents");
		if(fold)
		for(int i = 0; i<myTarget.PrefsCount; i++)
		{
			EditorGUILayout.BeginHorizontal();
			PlayerPrefsWrapper.PrefsProperty prop = myTarget.getProp(i);
			GUILayout.Label(prop.name, GUILayout.Width(200f));
			object data = prop.val;
			switch(prop.dType)
			{
			case PlayerPrefsWrapper.dataType.STRING:
				GUILayout.Label((string)data);
				break;
			case PlayerPrefsWrapper.dataType.INT:
				GUILayout.Label(((int)data).ToString());
				break;
			case PlayerPrefsWrapper.dataType.FLOAT:
				GUILayout.Label(((float)data).ToString());
				break;
			}
			EditorGUILayout.EndHorizontal();
		}
	}
}
