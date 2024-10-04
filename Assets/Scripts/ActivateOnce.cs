using UnityEngine;
using System.Collections;

public class ActivateOnce : MonoBehaviour {

	public string PrefsName;
	// Use this for initialization
	void Start () {
		bool activated = PlayerPrefsWrapper.GetInt(PrefsName, -1) == 1;
		if(activated)
			gameObject.SetActive(false);
	}

	void OnDisable()
	{
		PlayerPrefsWrapper.SetInt(PrefsName, 1);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
