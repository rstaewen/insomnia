using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
public class SaveAllSprites : MonoBehaviour
{
#if UNITY_EDITOR
	public UIAtlas atlas{get {return GetComponent<UIAtlas>();}}
	public string withPrefix = "";
#endif
}
