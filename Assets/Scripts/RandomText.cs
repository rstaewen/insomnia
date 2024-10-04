using UnityEngine;
using System.Collections;

public class RandomText : MonoBehaviour {

	public System.Collections.Generic.List<string> possibleText;
	// Use this for initialization
	void Start () {
		GetComponent<UILabel>().text = possibleText[Random.Range(0,possibleText.Count)];
	}
}
