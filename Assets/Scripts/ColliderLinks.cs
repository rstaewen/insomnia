using UnityEngine;
using System.Collections;

public class ColliderLinks : MonoBehaviour
{
	public AnimatedCharacter character;
	public Sheep sheep;
	public static Sheep FindSheep (Collider2D c)
	{
		Sheep s = c.GetComponent<Sheep>();
		if(s)
			return s;
		else
		{
			ColliderLinks cl = c.GetComponent<ColliderLinks>();
			if(cl)
				return cl.sheep;
			return null;
		}
	}
	public static AnimatedCharacter FindCharacter (Collider2D c)
	{
		AnimatedCharacter ac = c.GetComponent<AnimatedCharacter>();
		if(ac)
			return ac;
		else
		{
			ColliderLinks cl = c.GetComponent<ColliderLinks>();
			if(cl)
				return cl.character;
			return null;
		}
	}
}
