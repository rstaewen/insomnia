using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class EjectRemains : SolipsoBehavior
{
	public List<GameObject> remainsPrefabList;
	public float delayTime;
	public float ejectionForce;

	public void QueueEjection()
	{
		StartCoroutine(queueDelayed());
	}

	IEnumerator queueDelayed()
	{
		yield return new WaitForSeconds(delayTime);
		foreach(GameObject chosenPrefab in remainsPrefabList)
		{
			Vector3 localScale = chosenPrefab.transform.localScale;
			GameObject remains = GameObject.Instantiate(chosenPrefab, transform.position, Quaternion.identity) as GameObject;
			remains.transform.parent = GameObject.FindGameObjectWithTag("ObjectPanel").transform;
			remains.transform.position = transform.position+Utilities.RandomVector(0.1f);
			remains.transform.localScale = localScale;
			foreach(Rigidbody2D rb in remains.GetComponentsInChildren<Rigidbody2D>())
			{
				if(transform.parent && transform.parent.GetComponent<Rigidbody2D>())
					rb.velocity = Utilities.RandomVectorOnCircle(ejectionForce) + transform.parent.GetComponent<Rigidbody2D>().velocity;
				else
					rb.velocity = Utilities.RandomVectorOnCircle(ejectionForce);
				rb.angularVelocity = UnityEngine.Random.Range(-360f, 360f);
			}
		}
		//Debug.Break();
	}
	
}
