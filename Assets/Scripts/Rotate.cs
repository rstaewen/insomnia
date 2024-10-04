using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{
	public float degreesPerSecond;
	private Vector3 rotation;
	private Vector3 homeRotation;

	void Start ()
	{
		rotation = transform.rotation.eulerAngles;
		homeRotation= rotation;
	}

	void Update () {
		rotation.z += Time.deltaTime * degreesPerSecond;
		transform.rotation = Quaternion.Euler(rotation);
	}

	public void Reset ()
	{
		rotation = homeRotation;
		transform.rotation = Quaternion.Euler(rotation);
	}
}
