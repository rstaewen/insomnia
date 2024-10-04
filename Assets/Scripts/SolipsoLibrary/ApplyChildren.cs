using UnityEngine;
using System.Collections;
using System.Collections.Generic;
#if UNITY_EDITOR
using UnityEditor;

[ExecuteInEditMode]
#endif
public class ApplyChildren : MonoBehaviour
{
#if UNITY_EDITOR
	public bool applyAsActive;
	public bool deactivateAfterApply;
	public GameObject[] toApply
	{
		get {
			GameObject[] ret = new GameObject[transform.childCount];
			for(int i = 0; i<transform.childCount; i++)
				ret[i] = transform.GetChild(i).gameObject;
			return ret;
		}
	}
#endif
}
