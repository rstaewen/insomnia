using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class SpriteSortEdit : EditorWindow
{
	[MenuItem ("Solipsoid/Sprite Sorting")]
	static void SpriteSortWindow() 
	{
		EditorWindow.GetWindow(typeof(SpriteSortEdit));	
	}

	void OnEnable ()
	{ 
		instance = this; 
		bandSize = PlayerPrefs.GetInt(bandSizePropName, 10); 
		bandNumber = PlayerPrefs.GetInt(bandNumberPropName, 200); 
		sortMethod = (SpriteSort.SortMethod)PlayerPrefs.GetInt(sortMethodPropName, 1);
	}
	void OnDisable () { instance = null; }
	static public SpriteSortEdit instance;
	static string bandSizePropName = "SOLIPSOID_SSBANDSIZE";
	static string bandNumberPropName = "SOLIPSOID_SSBANDNUMBER";
	static string sortMethodPropName = "SOLIPSOID_SSSORTMETHOD";
	private int bandSize;
	private int bandNumber;
	private SpriteSort.SortMethod sortMethod;
	private bool setBandSize = true;
	private bool setBandNumber = true;
	private bool setSortMethod = false;

	void OnGUI()
	{
		GUILayout.Space(10);
		GUIStyle style = new GUIStyle();
		style.richText = true;
		GUILayout.Label("<size=20><color=white>\tSprite Sorting Settings</color></size>", style, GUILayout.Height(30f));

		bandSizeGUI();
		bandNumberGUI();
		sortMethodGUI();

		if(GUILayout.Button("Set for ACTIVE sorted scene objects"))
		{
			setAllSortingInScene(false);
			savePlayerPrefs();
		}

		if(GUILayout.Button("Set for ACTIVE and INACTIVE sorted scene objects"))
		{
			setAllSortingInScene(true);
			savePlayerPrefs();
		}
	}

	void bandSizeGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Band size: ");
		GUILayout.FlexibleSpace();
		setBandSize = GUILayout.Toggle(setBandSize, "Apply"); 


		string _bs = GUILayout.TextField(bandSize.ToString(), GUILayout.Width(100f));
		if(!String.IsNullOrEmpty(_bs))
			bandSize = Mathf.Clamp(System.Convert.ToInt32(_bs), 1, 100000);
		else
			bandSize = 0;
		
		GUILayout.EndHorizontal();
	}

	void bandNumberGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Number of Bands: ");
		GUILayout.FlexibleSpace();
		setBandNumber = GUILayout.Toggle(setBandNumber, "Apply"); 
		
		string _bn = GUILayout.TextField(bandNumber.ToString(), GUILayout.Width(100f));
		if(!String.IsNullOrEmpty(_bn))
			bandNumber = Mathf.Clamp(System.Convert.ToInt32(_bn), 1, 100000);
		else
			bandNumber = 0;
		
		GUILayout.EndHorizontal();
	}

	void sortMethodGUI()
	{
		GUILayout.BeginHorizontal();
		GUILayout.Label("Sort method: ");
		GUILayout.FlexibleSpace();
		setSortMethod = GUILayout.Toggle(setSortMethod, "Apply"); 
		sortMethod = (SpriteSort.SortMethod)EditorGUILayout.EnumPopup(sortMethod, GUILayout.Width(100f));
		GUILayout.EndHorizontal();
	}

	void savePlayerPrefs()
	{
		PlayerPrefs.SetInt(bandSizePropName, bandSize); 
		PlayerPrefs.SetInt(bandNumberPropName, bandNumber); 
		PlayerPrefs.SetInt(sortMethodPropName, (int)sortMethod);
	}

	void setAllSortingInScene(bool sortInactive)
	{
		SpriteSort[] sceneSorters;

		if(sortInactive)
		{
			SpriteSort[] allSorters = Resources.FindObjectsOfTypeAll<SpriteSort>();
			sceneSorters = allSorters.Where(sorter => isInScene(sorter.gameObject)).ToArray();
		}
		else
			sceneSorters = GameObject.FindObjectsOfType<SpriteSort>();

		string log = "sorted objects: \n";
		foreach (SpriteSort ss in sceneSorters)
		{
			if(setBandSize)
				ss.SetBandSize(bandSize);
			if(setBandNumber)
				ss.integerDepthRange = bandNumber;
			if(setSortMethod)
				ss.sortMethod = sortMethod;
			log += ss.name+",";
			ss.OnInspectorValidate();
		}
		log = log.Remove(log.Length-1);
		log+=".";
		Debug.Log(log);
	}

	bool isInScene(GameObject go)
	{
		if (go.hideFlags == HideFlags.NotEditable || go.hideFlags == HideFlags.HideAndDontSave)
			return false;
		
		string assetPath = AssetDatabase.GetAssetPath(go.transform.root.gameObject);
		if (!String.IsNullOrEmpty(assetPath))
			return false;
		return true;
	}
}
