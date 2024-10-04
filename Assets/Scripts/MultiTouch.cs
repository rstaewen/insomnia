using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MultiTouch : SolipsoBehavior
{
	public List<TouchInput> touchInputs;
	public float minimumTouchSeparation;
	List<Vector2> activeTouchLocations = new List<Vector2>();

	public void OnPress()
	{
		int currentTouchID = UICamera.currentTouchID;
		//if touch id is -1, this is a computer emulator.
		if(currentTouchID < 0)
			touchInputs[0].OnPress(currentTouchID);
		else if(currentTouchID < touchInputs.Count)
			touchInputs[currentTouchID].OnPress(currentTouchID);
	}
	public void OnRelease()
	{
		int currentTouchID = UICamera.currentTouchID;
		//if touch id is -1, this is a computer emulator.
		if(currentTouchID < 0)
			touchInputs[0].OnRelease();
		else if(currentTouchID < touchInputs.Count)
			touchInputs[currentTouchID].OnRelease();
	}
}
