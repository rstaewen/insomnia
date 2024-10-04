using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

//By Randal, 2015
//Direct complaints to Randal

[ExecuteInEditMode]
public class PlayerPrefsWrapper : MonoBehaviour
{
	static PlayerPrefsWrapper inst;
    public string ArrayNameSuffix = "_ARRAY";
	
	[System.Serializable] public class PrefsProperty
	{
		public string name;
		public object val;
		public dataType dType;
		public PrefsProperty(string name, object val, dataType dType) {this.val = val; this.name = name; this.dType = dType;}
	}
	[SerializeField] List<PrefsProperty> prefsContents = new List<PrefsProperty>();
	public int PrefsCount {get {return prefsContents.Count;}}
	public PrefsProperty getProp (int index) {return prefsContents[index];}
	[SerializeField] Dictionary<string, object> prefsDictionary = new Dictionary<string, object>();
	public enum dataType {STRING, INT, FLOAT, STRING_ARRAY, INT_ARRAY, FLOAT_ARRAY}

	void Awake()
	{
		if(!inst)
		{
			inst = this;
			GameObject.DontDestroyOnLoad(gameObject);
			PopulatePreferences();
		}
		else
			GameObject.Destroy(gameObject);
	}
	public void PopulatePreferences()
	{
		int count = PlayerPrefs.GetInt("PrefsCount", 0);
		prefsContents.Clear();
		prefsDictionary.Clear();
		string showMessage = "Repopulating, prefs count: "+count.ToString()+"\n";
		try
		{
			if(count > 0)
			{
				string[] props = new string[count];
				for(int i = 0; i< count; i++)
				{
					props[i] = PlayerPrefs.GetString("PrefsName"+(i+1).ToString());
					showMessage += "#"+i.ToString()+":"+props[i]+"-";
					string[] split = props[i].Split('♦');
					string propName = split[0];
					dataType dType = (dataType)Enum.Parse(typeof(dataType), split[1]);
					object data = null;
					switch(dType)
					{
					case dataType.FLOAT:	data = PlayerPrefs.GetFloat(propName);		break;
					case dataType.STRING:	data = PlayerPrefs.GetString(propName);		break;
					case dataType.INT:		data = PlayerPrefs.GetInt(propName);		break;
					}
					AddElement(propName, data, dType);
					showMessage+=data.ToString()+" ";
				}
			}
		} catch(Exception e) {Debug.LogError("ERROR! "+showMessage);};
		PlayerPrefs.Save();
	}
	void OnDestroy()
	{
		inst = null;
	}
	void AddElement(string propName, object val, dataType dType)
	{
		//Debug.Log("adding property name: "+propName+"♦"+dType.ToString());
		prefsDictionary.Add(propName,val);
		prefsContents.Add(new PrefsProperty(propName, val, dType));
		PlayerPrefs.SetInt("PrefsCount", prefsContents.Count);
		PlayerPrefs.SetString("PrefsName"+prefsContents.Count.ToString(), propName+"♦"+dType.ToString());
		//Debug.Log("PrefsName"+prefsContents.Count.ToString()+":"+propName+"♦"+dType.ToString());
	}
    void SetElement(string propName, object val)
	{
		prefsDictionary[propName] = val;
		prefsContents.Where(x => x.name == propName).First().val = val;
	}
	void SetMetaInformation(string propName, object val, dataType dType)
	{
		if(!prefsDictionary.ContainsKey(propName))
			AddElement(propName, val, dType);
		else
			SetElement(propName, val);
	}
    void setObject(string propName, object value, dataType dt)
    {
        inst.SetMetaInformation(propName, value, dt);
        switch(dt)
        {
            case dataType.STRING:            PlayerPrefs.SetString(propName, (string)value);          break;
            case dataType.INT:               PlayerPrefs.SetInt(propName, (int)value);                break;
            case dataType.FLOAT:             PlayerPrefs.SetFloat(propName, (float)value);            break;
        }
        PlayerPrefs.Save();
    }
    void setObjectArray(string propName, object[] values, dataType dt)
    {
        for(int i = 0; i<values.Length; i++)
        {
            string pn = propName + ArrayNameSuffix + values[i].ToString();
            inst.SetMetaInformation(pn, values, dt);
            switch (dt)
            {
                case dataType.STRING_ARRAY:   PlayerPrefs.SetString(pn, (string)(values[i])); break;
                case dataType.INT_ARRAY:      PlayerPrefs.SetInt(pn, (int)(values[i])); break;
                case dataType.FLOAT_ARRAY:    PlayerPrefs.SetFloat(pn, (float)(values[i])); break;
            }
        }
        PlayerPrefs.Save();
    }

    public static void SetString(string propName, string val){          if (inst) inst.setObject(propName, (object)val, dataType.STRING);                               else { PlayerPrefs.SetString(propName, val);    PlayerPrefs.Save(); } }
    public static void SetInt(string propName, int val) {               if (inst) inst.setObject(propName, (object)val, dataType.INT);                                  else { PlayerPrefs.SetInt(propName, val);       PlayerPrefs.Save(); } }
    public static void SetFloat(string propName, float val) {           if (inst) inst.setObject(propName, (object)val, dataType.STRING);                               else { PlayerPrefs.SetFloat(propName, val);     PlayerPrefs.Save(); } }

    public static void SetStringArray(string propName, string[] vals) { if (inst) inst.setObjectArray(propName, (object[])vals, dataType.STRING_ARRAY);                 else { throw new System.NotImplementedException(); } }
    public static void SetIntArray(string propName, int[] vals) {       if (inst) inst.setObjectArray(propName, vals.Cast<object>().ToArray(), dataType.INT_ARRAY);     else { throw new System.NotImplementedException(); } }
    public static void SetFloatArray(string propName, float[] vals) {   if (inst) inst.setObjectArray(propName, vals.Cast<object>().ToArray(), dataType.FLOAT_ARRAY);   else { throw new System.NotImplementedException(); } }

    public static string GetString(string propName)
	{
		string val = PlayerPrefs.GetString(propName);
		if(inst)
			inst.SetMetaInformation(propName, val, dataType.STRING);
		return val;
	}
	public static string GetString(string propName, string defaultValue)
	{
		string val = PlayerPrefs.GetString(propName, defaultValue);
		if(inst)
			inst.SetMetaInformation(propName, val, dataType.STRING);
		return val;
	}
	public static int GetInt(string propName)
	{
		int val = PlayerPrefs.GetInt(propName);
		if(inst)
			inst.SetMetaInformation(propName, val, dataType.INT);
		return val;
	}
    public static int GetIntArray(string propName)
    {
        int[] val = PlayerPrefs.GetInt(propName);
        if (inst)
            inst.SetMetaInformation(propName, val, dataType.INT);
        return val;
    }
    public static int GetInt(string propName, int defaultValue)
	{
		int val = PlayerPrefs.GetInt(propName, defaultValue);
		if(inst)
			inst.SetMetaInformation(propName, val, dataType.INT);
		return val;
	}
	public static float GetFloat(string propName)
	{
		float val = PlayerPrefs.GetFloat(propName);
		if(inst)
			inst.SetMetaInformation(propName, val, dataType.FLOAT);
		return val;
	}
	public static float GetFloat(string propName, float defaultValue)
	{
		float val = PlayerPrefs.GetFloat(propName, defaultValue);
		if(inst)
			inst.SetMetaInformation(propName, val, dataType.FLOAT);
		return val;
	}
	public void DeleteMetaInfo()
	{
		prefsContents.Clear();
		prefsDictionary.Clear();
	}
	public static void DeleteAll()
	{
		DebugUtility.AddLine("DELETE ALL!");
		if(inst)
		{
			foreach(PrefsProperty prop in inst.prefsContents)
				PlayerPrefs.DeleteKey(prop.name);
			inst.DeleteMetaInfo();
		}
		PlayerPrefs.DeleteAll();
		PlayerPrefs.Save();
	}
	public static void DeleteKey(string key)
	{
		if(inst)
		{
			inst.prefsDictionary.Remove(key);
			inst.prefsContents.Remove(inst.prefsContents.Where(x => x.name == key).First());
		}
		PlayerPrefs.DeleteKey(key);
		PlayerPrefs.Save();
	}
}
