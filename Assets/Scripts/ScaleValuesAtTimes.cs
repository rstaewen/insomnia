using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class ScaleValuesAtTimes : SolipsoBehavior
{
	[System.Serializable] public class IncreaseEvent
	{
		public float levelTimeMarker;
		public bool repeat;
		public float repeatDelay;
		public float overTime;
	}
	[System.Serializable] public class VelocityIncreaseEvent : IncreaseEvent
	{
		public float velocityScalar;
	}
	[System.Serializable] public class SuckerIncreaseEvent : IncreaseEvent
	{
		public float suckForceScalar;
	}
	[System.Serializable] public class ColorEvent : IncreaseEvent
	{
		public Color newColor;
	}
	[System.Serializable] public class TransformScaleEvent : IncreaseEvent
	{
		public Vector3 newScale;
	}
	[System.Serializable] public class ActivateObjectEvent : IncreaseEvent
	{
		public GameObject go;
		public bool activate;
	}
	public List<VelocityIncreaseEvent> velocityIncreaseEvents;
	public List<SuckerIncreaseEvent> suckerIncreaseEvents;
	public List<ColorEvent> colorChangeEvents;
	public List<TransformScaleEvent> scaleChangeEvents;
	public List<ActivateObjectEvent> activationEvents;

	void OnEnable()
	{
		foreach(VelocityIncreaseEvent evt in velocityIncreaseEvents)
			StartCoroutine(doVelocityEvent(evt));
		foreach(SuckerIncreaseEvent evt in suckerIncreaseEvents)
			StartCoroutine(doSuckerEvent(evt));
		foreach(ColorEvent evt in colorChangeEvents)
			StartCoroutine(doColorEvent(evt));
		foreach(TransformScaleEvent evt in scaleChangeEvents)
			StartCoroutine(doScaleEvent(evt));
		foreach(ActivateObjectEvent evt in activationEvents)
			StartCoroutine(doActivationEvent(evt));
	}

	IEnumerator doSuckerEvent(SuckerIncreaseEvent evt)
	{
		yield return new WaitForSeconds(evt.levelTimeMarker*LevelPicker.CurrentLevel.timeLimit);
		StartCoroutine(scaleSucker(evt.suckForceScalar, evt.overTime));
		if(evt.repeat)
		{
			while(true)
			{
				yield return new WaitForSeconds(evt.repeatDelay*LevelPicker.CurrentLevel.timeLimit);
				StartCoroutine(scaleSucker(evt.suckForceScalar, evt.overTime));
			}
		}
	}

	IEnumerator doVelocityEvent(VelocityIncreaseEvent evt)
	{
		yield return new WaitForSeconds(evt.levelTimeMarker*LevelPicker.CurrentLevel.timeLimit);
		StartCoroutine(scaleVelocity(evt.velocityScalar, evt.overTime));
		if(evt.repeat)
		{
			while(true)
			{
				yield return new WaitForSeconds(evt.repeatDelay*LevelPicker.CurrentLevel.timeLimit);
				StartCoroutine(scaleVelocity(evt.velocityScalar, evt.overTime));
			}
		}
	}

	IEnumerator doColorEvent(ColorEvent evt)
	{
		yield return new WaitForSeconds(evt.levelTimeMarker*LevelPicker.CurrentLevel.timeLimit);
		StartCoroutine(changeColor(evt.newColor, evt.overTime));
		if(evt.repeat)
		{
			while(true)
			{
				yield return new WaitForSeconds(evt.repeatDelay*LevelPicker.CurrentLevel.timeLimit);
				StartCoroutine(changeColor(evt.newColor, evt.overTime));
			}
		}
	}

	IEnumerator doScaleEvent(TransformScaleEvent evt)
	{
		yield return new WaitForSeconds(evt.levelTimeMarker*LevelPicker.CurrentLevel.timeLimit);
		StartCoroutine(changeScale(evt.newScale, evt.overTime));
		if(evt.repeat)
		{
			while(true)
			{
				yield return new WaitForSeconds(evt.repeatDelay*LevelPicker.CurrentLevel.timeLimit);
				StartCoroutine(changeScale(evt.newScale, evt.overTime));
			}
		}
	}

	IEnumerator doActivationEvent(ActivateObjectEvent evt)
	{
		yield return new WaitForSeconds(evt.levelTimeMarker*LevelPicker.CurrentLevel.timeLimit);
		evt.go.SetActive(evt.activate);
		if(evt.repeat)
		{
			while(true)
			{
				yield return new WaitForSeconds(evt.repeatDelay*LevelPicker.CurrentLevel.timeLimit);
				evt.go.SetActive(evt.activate);
			}
		}
	}

	IEnumerator scaleVelocity(float scalar, float overTime)
	{
		Vector2 velocity = GetComponent<Rigidbody2D>().velocity;
		Vector2 normalizedVelocity = velocity.normalized;
		float scaledMagnitude = scalar * velocity.magnitude;
		float currMagnitude = velocity.magnitude;
		float magVelocity = 0f;
		Rigidbody2D rb = GetComponent<Rigidbody2D>();
		if(overTime == 0f)
		{
			rb.velocity = normalizedVelocity*scaledMagnitude;
			yield break;
		}
		float t = 0;
		while(t < overTime)
		{
			currMagnitude = Mathf.SmoothDamp(currMagnitude, scaledMagnitude, ref magVelocity, overTime*0.35f);
			rb.velocity = normalizedVelocity*currMagnitude;
			t += TimeKeeper.deltaTime;
			yield return null;
		}
	}

	IEnumerator scaleSucker(float scalar, float overTime)
	{
		float suckForce = GetComponent<SheepSucker>().suckForce;
		float targetSuckForce = suckForce * scalar;
		float suckVelocity = 0f;
		SheepSucker ss = GetComponent<SheepSucker>();
		if(overTime == 0f)
		{
			ss.suckForce = targetSuckForce;
			yield break;
		}
		float t = 0;
		while(t < overTime)
		{
			suckForce = Mathf.SmoothDamp(suckForce, targetSuckForce, ref suckVelocity, overTime*0.35f);
			ss.suckForce = suckForce;
			t += TimeKeeper.deltaTime;
			yield return null;
		}
	}

	IEnumerator changeColor(Color newColor, float overTime)
	{
		UISprite sprite = GetComponent<UISprite>();
		Vector3 currRGB = Utilities.toRGB(sprite.color);
		Vector3 targetRGB = Utilities.toRGB(newColor);
		float currAlpha = sprite.alpha;
		float targetAlpha = newColor.a;
		Color currColor;
		Vector3 colorVelocity = Vector3.zero;
		float alphaVelocity = 0f;
		if(overTime == 0f)
		{
			sprite.color = newColor;
			yield break;
		}
		float t = 0;
		while(t < overTime)
		{
			currRGB = Vector3.SmoothDamp(currRGB, targetRGB, ref colorVelocity, overTime*0.35f);
			currAlpha = Mathf.SmoothDamp(currAlpha, targetAlpha, ref alphaVelocity, overTime*0.35f);
			currColor = Utilities.toColor(currRGB);
			currColor.a = currAlpha;
			sprite.color = currColor;
			t += TimeKeeper.deltaTime;
			yield return null;
		}
	}

	IEnumerator changeScale(Vector3 newScale, float overTime)
	{
		Vector3 currScale = transform.localScale;
		Vector3 scaleVelocity = Vector3.zero;
		Transform mTransform = transform;
		EatSheep sheepEater = GetComponent<EatSheep>();
		if(overTime == 0f)
		{
			transform.localScale = currScale;
			yield break;
		}
		float t = 0;
		while(t < overTime)
		{
			currScale = Vector3.SmoothDamp(currScale, newScale, ref scaleVelocity, overTime*0.35f);
			if(sheepEater)
				sheepEater.SetNewBaseScale(currScale);
			else
				mTransform.localScale = currScale;
			t += TimeKeeper.deltaTime;
			yield return null;
		}
	}
}
