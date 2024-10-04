using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DestroyIgnoreable : MonoBehaviour
{
	protected bool ignoreDestroy = false;

	protected void IgnoreDestroy()
	{
		ignoreDestroy = true;
	}
	
}
