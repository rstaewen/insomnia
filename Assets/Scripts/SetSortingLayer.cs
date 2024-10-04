using UnityEngine;
using System.Collections;

public class SetSortingLayer : MonoBehaviour {

	public string sortLayer;
	public int sortOrder;
	ParticleSystem ps;

	void Awake()
	{
		ps = GetComponent<ParticleSystem>();
		ps.GetComponent<Renderer>().sortingLayerName = sortLayer;
		ps.GetComponent<Renderer>().sortingOrder = sortOrder;
	}
}
