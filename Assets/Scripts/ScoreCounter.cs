#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ScoreCounter : MonoBehaviour
{
	public void AddScore(int points, Vector3 addedPosition)
	{
		Score.AddPoints(points, "", addedPosition);
	}
}
