using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class AssignVelocity2D : SolipsoBehavior
{
	public Vector2 velocity;
	
	protected virtual void Start()
	{
		GetComponent<Rigidbody2D>().velocity += velocity;
	}
}
