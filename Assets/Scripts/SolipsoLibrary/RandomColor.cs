#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class RandomColor : MonoBehaviour
{
	//datastructure for targetting a waypoint within a certain random tolerance.
	[System.Serializable] public class ColorWaypoint
	{
		[System.Serializable] public class ColorNode
		{
			//the "tolerance" for moving the value around.
			[SerializeField] protected float randomDeviationTolerance;
			[SerializeField] protected Color averageColor = Color.white;
			//return a random adder as a float value, used up to 3 times on created vector3
			protected float RandomModifier 		{get {return UnityEngine.Random.Range(-randomDeviationTolerance, randomDeviationTolerance);}}
			//by default, dont bother randomizing z. preset for 2D.
			[SerializeField] bool randomizeAlpha = false;
			
			public Color RandomizedColor 	
			{get 
				{return new Color(
						averageColor.r + RandomModifier, 
						averageColor.g + RandomModifier, 
						averageColor.b + RandomModifier,
						averageColor.a + (randomizeAlpha ? RandomModifier : 0f));
				}
			}
		}
		[SerializeField] ColorNode colorValue;
		[SerializeField] float seekTime;
		public float SeekTime {get {return seekTime;} }
		public Color RandomColor 				{get {return colorValue.RandomizedColor;} }
	}
	public List<ColorWaypoint> waypoints;
	private int waypointIndex;
	private ColorWaypoint currentWaypoint {get {return waypoints[waypointIndex];}}
	private Color colorTarget;
	private Vector3 colorVelocity;
	private float alphaVelocity;
	private UISprite nguiSprite;
	private SpriteRenderer unitySprite;
	private enum SpriteType {NGUI, Unity, None}
	private SpriteType spriteType;
	void Awake()
	{
		nguiSprite = GetComponent<UISprite>();
		unitySprite = GetComponent<SpriteRenderer>();
		if(nguiSprite)
			spriteType = SpriteType.NGUI;
		else if(unitySprite)
			spriteType = SpriteType.Unity;
		else
			spriteType = SpriteType.None;
	}
	void OnEnable()
	{
		if(waypoints.Count > 0)
			StartCoroutine(moveToWaypoint());
	}
	IEnumerator moveToWaypoint()
	{
		while(true)
		{
			colorTarget = currentWaypoint.RandomColor;
			Vector3 targetRGB = Utilities.toRGB(colorTarget);
			float targetAlpha = colorTarget.a;
			Vector3 currentRGB;
			float currentAlpha;
			Color currentColor;
			switch(spriteType)
			{
			case SpriteType.Unity:
				currentColor = unitySprite.color;
				currentRGB = Utilities.toRGB(unitySprite.color);
				currentAlpha = unitySprite.color.a;
				break;
			case SpriteType.NGUI:
				currentColor = nguiSprite.color;
				currentRGB = Utilities.toRGB(nguiSprite.color);
				currentAlpha = nguiSprite.color.a;
				break;
			default:
				currentColor = Color.black;
				currentRGB = Vector3.zero;
				currentAlpha = 1f;
				break;
			}
			float t = 0f;
			while(t < currentWaypoint.SeekTime)
			{
				currentRGB = Vector3.SmoothDamp(currentRGB, targetRGB, ref colorVelocity, currentWaypoint.SeekTime*0.35f);
				currentAlpha = Mathf.SmoothDamp(currentAlpha, targetAlpha, ref alphaVelocity, currentWaypoint.SeekTime*0.35f);
				currentColor.a = currentAlpha;
				currentColor.r = currentRGB.x;
				currentColor.g = currentRGB.y;
				currentColor.b = currentRGB.z;
				switch(spriteType)
				{
				case SpriteType.Unity:
					unitySprite.color = currentColor;
					break;
				case SpriteType.NGUI:
					nguiSprite.color = currentColor;
					break;
				case SpriteType.None:
					break;
				}
				t += Time.deltaTime;
				yield return null;
			}
			waypointIndex = (waypointIndex + 1) % waypoints.Count;
			yield return null;
		}
	}
}
