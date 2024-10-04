using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Options : SolipsoBehavior
{
	float musicVolume;
	float fxVolume;
	public UISlider musicSlider;
	public UISlider fxSlider;
	bool hasRecievedSetupMusicCall = false;
	bool hasRecievedSetupFXCall = false;

	void Start()
	{
		musicVolume = PlayerPrefsWrapper.GetFloat("MusicVolume", 1f);
		fxVolume = PlayerPrefsWrapper.GetFloat("FXVolume", 1f);
		musicSlider.value = musicVolume;
		fxSlider.value = fxVolume;
	}
	public void OnMusicSliderChange(UISlider slider)
	{
		slider.value = Mathf.Max(slider.value, 0.03f);
		if(hasRecievedSetupMusicCall)
		{
			musicVolume = (slider.value-0.03f)*1.031f;
			Audio.SetMusicVolume(musicVolume);
			PlayerPrefsWrapper.SetFloat("MusicVolume", musicVolume);
		}
		hasRecievedSetupMusicCall = true;
	}
	public void OnFXSliderChange(UISlider slider)
	{
		slider.value = Mathf.Max(slider.value, 0.03f);
		if(hasRecievedSetupFXCall)
		{
			fxVolume = (slider.value-0.03f)*1.031f;
			Audio.SetFXVolume(fxVolume);
			PlayerPrefsWrapper.SetFloat("FXVolume", fxVolume);
		}
		hasRecievedSetupFXCall = true;
	}

}
