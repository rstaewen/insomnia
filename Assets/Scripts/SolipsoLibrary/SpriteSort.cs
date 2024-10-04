using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

//[ExecuteInEditMode]
//i could have done this with execute in edit mode, so that sorting is contantly updated, but
//other asset packages i've seen sometimes abuse this and make the editor run super slow.
//its fucking annoying to have a package designed to make development easier actually slow the
//whole thing down.
public class SpriteSort : MonoBehaviour
{

	[System.Serializable] public class SortNode
	{
		public string name;
		public int depthOffset;
		public Component sorted;
		public Transform transform {get {if(sorted) return sorted.transform; else return null;}}
		[HideInInspector] public SpriteRenderer unitySprite;
		[HideInInspector] public UIWidget widget;
		private float baseAlpha;
		public float BaseAlpha {get {return baseAlpha;} set {baseAlpha = value;}}

		public void SetDepth (int depth)
		{
			if(widget)
				widget.depth = depth;
			else if(unitySprite)
				unitySprite.sortingOrder = depth;
		}
	}
	public List<SortNode> sortedObjects = new List<SortNode>();
	public bool sortInEditMode = true;
	public bool sortOnce = true;
	public enum SortMethod {ByZ, ByDrawOrder, ByZ_and_DrawOrder}
	public SortMethod sortMethod;
	private float minY;
	private float maxY;
	private float rangeY;
	public int integerDepthRange;
	private Vector3 position;
	private float minZ;
	private float maxZ;
	private float rangeZ;
	private bool allowSort = true;
	private Dictionary<string, SortNode> sNodeDict = new Dictionary<string, SortNode>();
	private int depthOffset;
	private int bandSize = 10;
	private int randomOffset = 0;

	//try to prevent oninspectorvalidate from triggering until after level has loaded. doesn't seem to work
	//all the time.
	//causes annoying null refs if sorting happens before the level is finished loading. it can't find
	//the UI anchors, for example, that are used to find the top and bottom of the screen.
	void OnLevelHasLoaded()
	{
		allowSort = true;
	}

#if UNITY_EDITOR
	void OnValidate()					{	setup(); }
	public void OnInspectorValidate()	{	setup(); }
	public void SetBandSize(int size)	{	bandSize = size; }

	void setup()
	{
		if(allowSort && !Application.isPlaying && gameObject.activeInHierarchy)
		{
			getValues();
			setDimensions();
			if(sortInEditMode)
				doSort();
			randomOffset = UnityEngine.Random.Range
				(0, (bandSize - (depthOffset%bandSize)));
		}
	}
#endif

	public void SetWidget (string widgetName, UIWidget widget)
	{
		sNodeDict[widgetName].sorted = widget;
		sNodeDict[widgetName].widget = widget;
	}

	void Start()
	{
		if(!sortInEditMode)
		{
			getValues();
			setDimensions();
			if(sortOnce)
			{
				doSort();
				enabled = false;
			}
			randomOffset = UnityEngine.Random.Range
				(0, (bandSize - (depthOffset%bandSize)));
		} else
			enabled = false;
	}

	void Update()
	{
		doSort();
	}

	void setDimensions()
	{
		UIAnchor topAnchor = GameObject.FindGameObjectWithTag("TopAnchor").GetComponent<UIAnchor>();
		UIAnchor botAnchor = GameObject.FindGameObjectWithTag("BottomAnchor").GetComponent<UIAnchor>();
		minY = botAnchor.transform.position.y;
		maxY = topAnchor.transform.position.y;
		rangeY = maxY - minY;
		minZ = Camera.main.nearClipPlane;
		maxZ = Camera.main.farClipPlane;
		rangeZ = maxZ - minZ;
		sNodeDict.Clear();
		foreach(SortNode SO in sortedObjects)
			if(SO.name != null && SO.name != "")
				sNodeDict.Add(SO.name, SO);
	}
	void getValues()
	{
		if(sortedObjects.Count == 0)
		{
			SortNode newNode = new SortNode();
			newNode.sorted = this;
			newNode.name = name;
			sortedObjects.Add(newNode);
		}
		for(int i = 0; i< sortedObjects.Count; i++)
		{
			if(sortedObjects[i].sorted)
			{
				sortedObjects[i].unitySprite = sortedObjects[i].sorted.GetComponent<SpriteRenderer>();
				sortedObjects[i].widget = sortedObjects[i].sorted.GetComponent<UIWidget>();
				sortedObjects[i].BaseAlpha = (sortedObjects[i].unitySprite ? sortedObjects[i].unitySprite.color.a : sortedObjects[i].widget.alpha);
				//print("sort sprite "+name+" at alpha: "+sortedObjects[i].BaseAlpha.ToString());
			}
		}
	}
	
	public void FadeOut (string spriteName, float overTime)
	{
		DebugUtility.AddLine(name+" fade out: "+spriteName+" over time: "+overTime.ToString(), name);
		if(sNodeDict.ContainsKey(spriteName))
			StartCoroutine(fadeOut(sNodeDict[spriteName], overTime));
		else
			Debug.LogWarning(name+" cannot fade out sprite: "+spriteName+", not present in sorter!");
	}

	IEnumerator fadeOut(SortNode sn, float overTime)
	{
		float t = overTime;
		if(sn.widget)
		{
			while(t > 0f)
			{
				sn.widget.alpha -= (sn.BaseAlpha * Time.deltaTime/overTime);
				t -= Time.deltaTime;
				yield return null;
			}
			sn.widget.alpha = 0f;
		}
		else if(sn.unitySprite)
		{
			Color c = sn.unitySprite.color;
			while(t > 0f)
			{
				c.a -= (sn.BaseAlpha * Time.deltaTime/overTime);
				sn.unitySprite.color = c;
				t -= Time.deltaTime;
				yield return null;
			}
			c.a = 0f;
			sn.unitySprite.color = c;
		}
	}

	public void FadeIn (string spriteName, float overTime)
	{
		if(sNodeDict.ContainsKey(spriteName))
			StartCoroutine(fadeIn(sNodeDict[spriteName], overTime));
		else
			Debug.LogWarning(name+" cannot fade in sprite: "+spriteName+", not present in sorter!");
	}

	IEnumerator fadeIn (SortNode sn, float overTime)
	{
		float t = overTime;
		if(sn.widget)
		{
			while(t > 0f)
			{
				sn.widget.alpha += (sn.BaseAlpha * Time.deltaTime/overTime);
				t -= Time.deltaTime;
				yield return null;
			}
		}
		else if(sn.unitySprite)
		{
			Color c = sn.unitySprite.color;
			while(t > 0f)
			{
				c.a += (sn.BaseAlpha * Time.deltaTime/overTime);
				sn.unitySprite.color = c;
				t -= Time.deltaTime;
				yield return null;
			}
		}
	}

	void doSort()
	{
		switch(sortMethod)
		{
		case SortMethod.ByDrawOrder:		orderSortAll();				break;
		case SortMethod.ByZ:				zSortAll();					break;
		case SortMethod.ByZ_and_DrawOrder:	orderSortAll(); zSortAll();	break;
		}
	}

	void orderSortAll()
	{
		foreach(SortNode ss in sortedObjects)
			if(ss.sorted)
				orderSort(ss);
	}

	void zSortAll()
	{
		foreach(SortNode ss in sortedObjects)
			if(ss.sorted)
				zSort(ss);
	}

	void zSort(SortNode ss)
	{
		float positionRatio = ((ss.transform.position.y - minY)/(rangeY))*0.6f+0.2f;
		position = ss.transform.position;
		position.z = (positionRatio * rangeZ)+minZ;
		ss.transform.position = position;
	}

	void orderSort(SortNode ss)
	{
		float positionRatio = ((transform.position.y - minY)/(rangeY))*0.6f+0.2f;
		ss.SetDepth((int)((1f - positionRatio) * integerDepthRange)*bandSize + ss.depthOffset + randomOffset + depthOffset);
	}
	
	public void SetDepthOffset (int depth, bool multiplyByBandSize)
	{
		depthOffset = (multiplyByBandSize ? bandSize : 1) * depth;
	}
	
	public void BringForward ()
	{
		depthOffset = bandSize;
	}
	
	public void BringEven()
	{
		depthOffset = 0;
	}
	
	public void BringBackward()
	{
		depthOffset = -bandSize;
	}
}
