using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public static class Utilities
{
	public delegate void MonoEvent (MonoBehaviour sender);

	public static Vector2 RandomVectorOnCircle(float radius)
	{
		Vector2 returned;
		float angle = Random.Range(0f,2f*Mathf.PI);
		returned.x = radius * Mathf.Cos(angle);
		returned.y = radius * Mathf.Sin(angle);
		return returned;
	}

	public static Vector3 RandomVector()
	{
		return new Vector3(Random.Range(-1f,1f), Random.Range(-1f,1f), Random.Range(-1f,1f));
	}

	public static Vector3 RandomVector(float maxMagnitude)
	{
		return RandomVector().normalized*Random.Range(0f, maxMagnitude);
	}

	public static Vector3 RandomVector(Vector3 min, Vector3 max)
	{
		return new Vector3(Random.Range(min.x,max.x), Random.Range(min.y,max.y), Random.Range(min.z,max.z));
	}

	public static Vector2 RandomVector(Vector2 min, Vector2 max)
	{
		return new Vector2(Random.Range(min.x,max.x), Random.Range(min.y,max.y));
	}

	public static Vector2 AverageVector (params Vector2[] vectors)
	{
		return AverageVector(vectors.AsEnumerable());
	}
	
	public static Vector2 AverageVector (List<Vector2> vectors)
	{
		return AverageVector(vectors.AsEnumerable());
	}

	public static Vector2 AverageVector (IEnumerable<Vector2> vectors)
	{
		int vectorCount = vectors.Count();
		if(vectorCount == 0)
			return Vector2.zero;
		Vector2 avg = Vector2.zero;
		foreach(Vector2 vec in vectors)
			avg += vec;
		return (avg / (float)vectorCount);
	}

	public static Vector3 PickRandomPoint(Collider2D boundingCollider, bool assumeBoxCollider)
	{
		Vector3 min = boundingCollider.bounds.min;
		Vector3 max = boundingCollider.bounds.max;
		if(assumeBoxCollider)
			return RandomVector(min,max);
		Vector3 offset = new Vector3(0f,0f,-1f);
		bool pointFound = false;
		Vector3 point;
		do
		{
			point = RandomVector(min,max);
			RaycastHit hitInfo;
			bool hit = Physics.Raycast(point + offset, Vector3.forward, out hitInfo);
			if(hit)
				if(hitInfo.collider == boundingCollider)
					pointFound = true;
		} while (pointFound == false);
		return point;
	}

	public static float MaxColliderDimension(Collider c)
	{
		bool e = c.enabled;
		c.enabled = true;
		float d = MaxDimension(c.bounds.size);
		c.enabled = e;
		return d;
	}
	public static float MaxDimension(Vector3 vectorInput)
	{
		return(Mathf.Max (Mathf.Max(vectorInput.x, vectorInput.y), vectorInput.z));
	}

	public static float HighestX (params Vector3[] positions)
	{
		float current = positions[0].x;
		for(int i = 0; i<positions.Length; i++)
		{
			if(positions[i].x > current)
				current = positions[i].x;
		}
		return current;
	}
	public static float LowestX (params Vector3[] positions)
	{
		float current = positions[0].x;
		for(int i = 0; i<positions.Length; i++)
		{
			if(positions[i].x < current)
				current = positions[i].x;
		}
		return current;
	}
	public static float HighestZ (params Vector3[] positions)
	{
		float current = positions[0].z;
		for(int i = 0; i<positions.Length; i++)
		{
			if(positions[i].z > current)
				current = positions[i].z;
		}
		return current;
	}
	public static float LowestZ (params Vector3[] positions)
	{
		float current = positions[0].z;
		for(int i = 0; i<positions.Length; i++)
		{
			if(positions[i].z < current)
				current = positions[i].z;
		}
		return current;
	}

	public static Vector3 toRGB(Color color)						{	return new Vector3(color.r, color.g, color.b); }
	public static Color toColor(Vector3 rgb)						{	return toColor(rgb, 1f); }	
	public static Color toColor(Vector3 rgb, float alpha)	{ return new Color(rgb.x, rgb.y, rgb.z, alpha); }

	public static Color blendColors(params Color[] colors)
	{
		float[] blendAmts = new float[colors.Length];
		for(int i = 0; i<blendAmts.Length; i++)
			blendAmts[i] = 1f;
		return blendColors(blendAmts, colors);
	}

	public static Color blendColors(float[] blendAmounts, params Color[] colors)
	{
		if(colors.Length == 0)
			return Color.black;
		float r=0f, b=0f, g=0f, a=0f;
		float totalBlend = 0f;
		foreach(float f in blendAmounts)
			totalBlend+=f;
		for(int i = 0; i<colors.Length; i++) {	r+=(blendAmounts[i]*colors[i].r); g+=(blendAmounts[i]*colors[i].g); b+=(blendAmounts[i]*colors[i].b); a+=(blendAmounts[i]*colors[i].a);	}
		Color retColor = new Color(r/totalBlend, g/totalBlend, b/totalBlend, a/totalBlend);
		return retColor;
	}

	public static bool AreCollectionsEquivalent(int[] collection1, int[] collection2, bool orderMatters)
	{
		if(collection1.Length != collection2.Length)
			return false;
		if(orderMatters)
		{
			for(int i = 0; i<collection1.Length; i++)
				if(collection1[i] != collection2[i])
					return false;
		} else {
			foreach(int i in collection1)
				if(!collection2.Contains(i))
					return false;
		}
		return true;
	}

	public static bool AreCollectionsEquivalent(string[] collection1, string[] collection2, bool orderMatters)
	{
		if(collection1.Length != collection2.Length)
			return false;
		if(orderMatters)
		{
			for(int i = 0; i<collection1.Length; i++)
				if(collection1[i] != collection2[i])
					return false;
		} else {
			foreach(string i in collection1)
				if(!collection2.Contains(i))
					return false;
		}
		return true;
	}

	public static void RemoveNulls <T>(List<T> collection)
	{
		for(int i = 0; i<collection.Count; i++)
		{
			if(collection[i] == null)
			{
				collection.RemoveAt(i);
				i--;
			}
		}
	}

	public static void RemoveDuplicates <T>(List<T> collection)
	{
		for(int i = 0; i<collection.Count; i++)
		{
			for(int j = i+1; j<collection.Count; j++)
			{
				if(collection[i].Equals(collection[j]))
				{
					collection.RemoveAt(j);
					j--;
				}
			}
		}
	}

	public static Vector3 GetJoystickDirectionFromWorldPosition (Vector3 worldPositionOfScreenCenter, Vector2 joystickDirection)
	{
		Plane p = new Plane(Vector3.up, worldPositionOfScreenCenter);
		float enter;
		Ray r = Camera.main.ScreenPointToRay(new Vector2(Screen.width/2, Screen.height/2)+joystickDirection*100f);
		if(!p.Raycast(r, out enter))
		{
			Debug.Log("no hit!");
			return Vector3.zero;
		}
		else
		{
			Vector3 point = r.origin + (r.direction * enter); 
			return point - worldPositionOfScreenCenter;
		}
	}

	public static Vector3 GetClickDirectionFromWorldPosition (Vector3 worldPositionOfScreenCenter, Vector2 screenPosOfClick)
	{
		Vector3 point = GetWorldPositionOfClick(worldPositionOfScreenCenter, screenPosOfClick);
		if(point == Vector3.zero)
			return Vector3.zero;
		else
			return point - worldPositionOfScreenCenter;
	}

	public static Vector3 GetWorldPositionOfClick (Vector3 worldPositionOfScreenCenter, Vector2 screenPosOfClick)
	{
		Plane p = new Plane(Vector3.up, worldPositionOfScreenCenter);
		float enter;
		Ray r = Camera.main.ScreenPointToRay(screenPosOfClick);
		if(!p.Raycast(r, out enter))
		{
			Debug.Log("no hit!");
			return Vector3.zero;
		}
		else
			return r.origin + (r.direction * enter);
	}
}

