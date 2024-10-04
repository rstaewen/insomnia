using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DestroyObject : MonoBehaviour
{
	public void Destroy()
	{
		GameObject.Destroy(gameObject);
	}
}
