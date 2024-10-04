using UnityEngine;
using System.Linq;
using System.Collections;
using System.Collections.Generic;

public class GenerateObjects : MonoBehaviour
{
	[System.Serializable] public class GenerateNode
	{
		[HideInInspector] public string name;
		public bool PreGenerate;
		public float startTime;
		public bool repeat;
		public float repeatTime;
		public int maximumActiveSpawns = 0;
		public int maximumCumulativeSpawns = 0;
		public List<ObjectNode> GeneratedObjects;
		public List<PathNode> possiblePaths; 
		[System.Serializable] public class PathNode
		{
			public Collider2D spawnArea;
			public List<Transform> goalTransforms;
		}
		public int minimumToGenerate;
		public int maximumToGenerate;
		public Vector2 minimumAssignedVelocity;
		public Vector2 maximumAssignedVelocity;
		public float minimumAssignedAngularVelocity;
		public float maximumAssignedAngularVelocity;
		public string releaseOnMessage;
		public List<string> sendMessagesOnRelease;
		private GenerateObjects parentBehavior;
		private PathNode usingPath;
		private int cumulativeSpawned;
		[HideInInspector] public List<GameObject> toRelease = new List<GameObject>();
		public List<GameObject> activeSpawns = new List<GameObject>();
		[HideInInspector] public bool released = false;

		[System.Serializable] public class ObjectNode
		{
			public GameObject prefab;
			public float probability;
		}

		public void Init(GenerateObjects parent)
		{
			EventRouter.Subscribe("baa", OnMessageSender);
			parentBehavior = parent;
		}

		public void Destroy()
		{
			EventRouter.Unsubscribe("baa", OnMessageSender);
		}

		void OnMessageSender(EventRouter.Event evt)
		{
			if(evt.HasData)
			{
				string msg = (string)evt.Data[0];
				if(releaseOnMessage != "" && msg == releaseOnMessage)
					Release();
			}
		}

		public void Generate()
		{
			int number = UnityEngine.Random.Range(minimumToGenerate, maximumToGenerate+1);
			usingPath = possiblePaths[UnityEngine.Random.Range(0,possiblePaths.Count)];
			for(int i = 0; i<number; i++)
			{
				if((maximumActiveSpawns > 0 && (activeSpawns.Count+i) >= maximumActiveSpawns) ||
				   (maximumCumulativeSpawns > 0 && cumulativeSpawned >= maximumCumulativeSpawns))
					break;
				for(int j = 0; j<GeneratedObjects.Count; j++)
				{
					bool useThis = UnityEngine.Random.Range(0f, 1f) < GeneratedObjects[j].probability;
					if(useThis)
					{
						GameObject gen = GameObject.Instantiate(GeneratedObjects[j].prefab) as GameObject;
						Vector3 genScale = gen.transform.localScale;
						gen.transform.parent = parentBehavior.parentForGeneratedObjects;
						gen.transform.localScale = genScale;
						gen.transform.position = Utilities.PickRandomPoint(usingPath.spawnArea, true);
						gen.GetComponent<IGeneratable>().AttachSpawnArea(usingPath.spawnArea);
						gen.SetActive(false);
						toRelease.Add(gen);
						cumulativeSpawned++;
						break;
					}
				}
			}
		}
		public void Release()
		{
			for(int i = 0; i<activeSpawns.Count; i++)
			{
				if(activeSpawns[i] == null || activeSpawns[i].GetComponent<IGeneratable>().isDead)
				{
					activeSpawns.RemoveAt(i);
					i--;
				}
			}
			foreach(GameObject obj in toRelease)
			{
				obj.SetActive(true);
				if(usingPath.goalTransforms.Count > 0 && obj.GetComponent<Wander>())
					obj.GetComponent<Wander>().SetGoal(usingPath.goalTransforms);
				obj.GetComponent<Rigidbody2D>().velocity = Utilities.RandomVector(minimumAssignedVelocity, maximumAssignedVelocity);
				if(minimumAssignedAngularVelocity != 0f || maximumAssignedAngularVelocity != 0f)
					obj.GetComponent<Rigidbody2D>().angularVelocity = UnityEngine.Random.Range(minimumAssignedAngularVelocity, maximumAssignedAngularVelocity);
			}
			foreach(string msg in sendMessagesOnRelease)
				EventRouter.Publish("baa", parentBehavior, msg);
			activeSpawns.AddRange(toRelease);
			toRelease.Clear();
			released = true;
		}
	}
	public List<GenerateNode> TimedGeneration;
	public Transform parentForGeneratedObjects;
	private bool beginGeneration = true;
	
	void Awake ()
	{
		for(int i = 0; i< TimedGeneration.Count; i++)
		{
			TimedGeneration[i].Init(this);
			if(TimedGeneration[i].PreGenerate)
				TimedGeneration[i].Generate();
			if(TimedGeneration[i].startTime == 0f && !TimedGeneration[i].repeat)
				TimedGeneration[i].Release();
			else
			{
				if(TimedGeneration[i].startTime != 0f)
					TimedGeneration[i].Release();
				StartCoroutine(doTimedGenerate(TimedGeneration[i], TimedGeneration[i].startTime));
			}
		}
		beginGeneration = false;
	}

	void OnEnable()
	{
		if(beginGeneration)
			for(int i = 0; i< TimedGeneration.Count; i++)
				StartCoroutine(doTimedGenerate(TimedGeneration[i], TimedGeneration[i].startTime));
	}

	void OnDisable()
	{
		beginGeneration = true;
	}

	void OnValidate()
	{
		foreach(GenerateNode node in TimedGeneration)
		{
			float probability = 1f;
			string str = (node.minimumToGenerate.ToString() + "-" + node.maximumToGenerate.ToString() + "       ");
			if(node.GeneratedObjects.Count == 0)
				str += "null";
			for(int i = 0 ; i<node.GeneratedObjects.Count; i++)
			{
				if(!node.GeneratedObjects[i].prefab)
					str += "null";
				else
				{
					str += (node.GeneratedObjects[i].probability * probability*100f)+"% ";
					str += node.GeneratedObjects[i].prefab.name;
				}
				if(i < node.GeneratedObjects.Count-1)
					str+=", ";
				probability = probability - (probability * node.GeneratedObjects[i].probability);
			}
			str += "   starts at time: "+node.startTime.ToString()+" seconds. "+(node.repeat? "REPEATED EVERY "+node.repeatTime.ToString()+" SECONDS." : "NOT REPEATED.") ;
			node.name = str;
		}
	}

	IEnumerator doTimedGenerate(GenerateNode node, float time)
	{
		yield return new WaitForSeconds(time);
		if(!node.PreGenerate || node.released)
			node.Generate();
		node.Release();
		if(node.repeat)
			StartCoroutine(doTimedGenerate(node, node.repeatTime));
	}
}
