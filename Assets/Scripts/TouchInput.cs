#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class TouchInput : SolipsoBehavior
{
	private Vector2 lastScreenPosition = Vector2.zero;
	public Vector2 ScreenPosition {get {if(touches.Count > 0) return touches.Last().position; else return lastScreenPosition;}}
	private Vector2 lastShephardPosition = Vector2.zero;
	public Vector2 ShephardPosition {get {if(touches.Count > 0) return (Vector2)Camera.main.ScreenToWorldPoint(touches.Last().position); else return lastShephardPosition;}}
	private Transform centerScreenXform;
	public Vector2 touchVelocity {get {return Utilities.AverageVector(touches.Select<TouchNode, Vector2>(x=> x.velocity));}}
	
	private bool isPressed = false;
	public TouchHerdSheep sheepToucher;
	public float velocityAdderStrength = 2f;
	public float magnetStrength = 2f;
	public float maximumMagnetDistance = 2f;
	public float circleDecayTime = 1f;
	protected UIRoot uiRoot;
	[System.Serializable] class TouchNode
	{
		public Vector2 position;
		public Vector2 delta;
		public Vector2 velocity;
		public TouchNode(Vector2 position, Vector2 delta, Vector2 velocity){this.position = position; this.delta = delta; this.velocity = velocity;}
	}
	private List<TouchNode> touches = new List<TouchNode>();
	private Vector2 averageTouchPosition() {	return Utilities.AverageVector(touches.Select<TouchNode, Vector2>(touch => touch.position)); }
	private double circleRadius;
	private Vector2 centroid;
	private enum TouchForce {Flick, Circle}
	private TouchForce touchForceState = TouchForce.Flick;
	private bool clockwise = true;
	private float circleDecayTimer = 0f;
	public UISprite arrowCircleSprite;
	private int assignedTouchID;
	private float minCircleRadius = 35f;
	public float pullForceScale = 1f;
	public float tangentForceScale = 1f;

	void Awake()
	{
		SubscribeEngineMessage();
	}
	
	void Start()
	{
		centerScreenXform = GameObject.FindGameObjectWithTag("CenterAnchor").transform;
		uiRoot = GameObject.FindGameObjectWithTag("UIRoot").GetComponent<UIRoot>();
		enabled = false;
	}
	protected override void OnEngineMessage (object[] data)
	{
		if((string)data[0] == "Victory" || (string)data[0] == "Defeat")
		{
			gameObject.SetActive(false);
		}
	}
	public void OnPress(int assignTouchID)
	{
		if(gameObject.activeInHierarchy)
		{
			this.assignedTouchID = assignTouchID;
			//print("on touch "+assignedTouchID.ToString());
			onTouchChange(true);
		}
	}
	public void OnRelease()
	{
		if(gameObject.activeInHierarchy)
		{
			//print("on release "+assignedTouchID.ToString());
			onTouchChange(false);
		}
	}
	void onTouchChange(bool isPressed)
	{
		this.isPressed = isPressed;
		enabled = isPressed;
		if(isPressed)
		{
			arrowCircleSprite.gameObject.SetActive(false);
			sheepToucher.Activate();
		}
		else
		{
			arrowCircleSprite.gameObject.SetActive(false);
			touchForceState = TouchForce.Flick;
			circleDecayTimer = 0f;
			sheepToucher.Deactivate();
		}
		touches.Clear();
	}

	//in the update loop, 
	//1) process touches (both for touchscreens and simulated mouse touches)
	//2) get the touch force adder for all sheep (independent of sheep position)
	//3) get the magnet force for each sheep (dependent on sheep pos)
	//4) add the combination of forces to each sheep.
	//	NOTE: this force is applied to the sheep in the form of giving it
	//	a move target that it moves itself towards. this makes more sense
	//	because the sheep won't always follow orders, and can make its own decisions this way.
	//5) test if the most recent touches look like a circle.
	//6) move the sheepToucher collider to where the mouse/finger is.
	//	NOTE: it actually uses "Input.mousePosition" in both cases, but "Input.mousePosition"
	//	is actually the most recent touch location for mobile devices. Easy.
	void Update()
	{
		processTouches();
		setForceVectorForHerdableSheep();
		testCircle();
		sheepToucher.transform.position = ShephardPosition;
		DebugUtility.AddLine(name+": "+touchForceState.ToString(), name+"touchMode");
	}
	void setForceVectorForHerdableSheep()
	{
		Vector2 touchForceAdder = getTouchForceAdder();
		Vector2 magnetForceAdder;
		Vector2 totalForceAdder;
		for(int i = 0; i<sheepToucher.herdable.Count; i++)
		{
			if(sheepToucher.herdable[i] == null || sheepToucher.herdable[i].isDead)
			{
				sheepToucher.herdable.RemoveAt(i);
				i--;
			}
		}

		foreach(Sheep sheep in sheepToucher.herdable)
		{
			magnetForceAdder = getMagnetForceAdder(sheep);
			totalForceAdder = Vector2.zero;
			switch(touchForceState)
			{
			case TouchForce.Circle:
				sheep.isCircle = true;
				totalForceAdder = getCircleForceAdder(sheep);
				break;
			case TouchForce.Flick:
				if(sheepToucher.touchable.Contains(sheep))
				{
					sheep.isCircle = false;
					totalForceAdder = touchForceAdder;
				}
				break;
			}
			#if UNITY_EDITOR
			Debug.DrawLine(sheep.transform.position, sheep.transform.position + (Vector3)totalForceAdder*0.1f, Color.yellow);
			#endif
			///////////////////////////
			/// SEAN!!!!!!!!!!!!!!!!!
			/// SEAN!!!!!!!!!!!!!!!!!
			/// this is where force is set!!!
			/// right here!!!
			/// the force adder is set up above, and sent to the sheep here!
			/// note that sheep will continue adding this force on every frame
			/// for up to 0.5 seconds if the force stops being sent.
			/// after 0.5 seconds, their AI takes over again.
			if(totalForceAdder.magnitude > 0f)
				sheep.SetHerdForce(totalForceAdder);
			else if (!sheepToucher.touchable.Contains(sheep) && touchForceAdder.magnitude > 0.2f*velocityAdderStrength)
				sheep.SetMagnetForce(magnetForceAdder);
		}
	}
	void processTouches()
	{	
		if(assignedTouchID >= 0 && assignedTouchID < Input.touches.Count())
		{
//			List<Touch> usingTouches = Input.touches.Where(touch => Vector2.Distance(touch.position, touches.Last().position) < 0.01f).ToList();
//
//			List<Vector2> touchVelocities = (usingTouches.Select<Touch, Vector2>(t => t.deltaPosition/(t.deltaTime > 0f ? t.deltaTime : Time.deltaTime)).ToList());
//			Vector2 avgTouchVelocity = Utilities.AverageVector(touchVelocities);
//			List<Vector2> touchPositions = (usingTouches.Select<Touch, Vector2>(t => t.position).ToList());
//			Vector2 avgTouchPosition = Utilities.AverageVector(touchPositions);
//			List<Vector2> touchDeltas = (usingTouches.Select<Touch, Vector2>(t => t.deltaPosition).ToList());
//			Vector2 avgTouchDelta = Utilities.AverageVector(touchDeltas);
			Touch t = Input.touches[assignedTouchID];
			touches.Add(new TouchNode(t.position, t.deltaPosition, t.deltaPosition/(t.deltaTime > 0f ? t.deltaTime : Time.deltaTime)));
		}
		else
		{
			Vector2 avgVelocity = Vector2.zero;
			Vector2 sPos = Input.mousePosition;
			Vector2 delta;
			if(touches.Count > 0)
				delta = sPos - touches.Last().position;
			else
				delta = Vector2.zero;
			Vector2 velocity = delta / Time.deltaTime;
			touches.Add(new TouchNode(sPos, delta, velocity));
		}
		lastScreenPosition = ScreenPosition;
		lastShephardPosition = ShephardPosition;
		//keep total touches to 20 or less. we may want to increase this if it seems like its not averaging enough points to be smooth.
		//since it's an average, it can't be too high, or you wouldnt be able to change the vector direction very much
		//after the list gets too long.
		if(touches.Count > 30)
			touches.RemoveAt(0);
	}
	void testCircle()
	{
		if(touches.Count> 10 && circleFit())
		{
			touchForceState = TouchForce.Circle;
			circleDecayTimer = circleDecayTime;
		}

		if (touchForceState == TouchForce.Circle && circleRadius > minCircleRadius)
		{
			#if UNITY_EDITOR
			for(int i = 1; i<touches.Count; i++)
			{
				Vector3 pt1 = Camera.main.ScreenToWorldPoint(touches[i-1].position);
				Vector3 pt2 = Camera.main.ScreenToWorldPoint(touches[i].position);
				Debug.DrawLine(pt1, pt2, Color.red);
			}
			#endif
			Vector2 p1 = touches[touches.Count - 1].position;
			Vector2 p2 = touches[(int)(touches.Count * 0.7)].position;

			Vector2 a = p2 - centroid;
			Vector2 b = p1 - centroid;
			float RorL = a.x * b.y - a.y * b.x;

			float checkAngle = ((p1.x - centroid.x)*(p2.x - centroid.x) - (p1.y - centroid.y)*(p2.y - centroid.y));

			DebugUtility.AddLine("Circle angle: " + RorL, "CircleAngle");
			arrowCircleSprite.gameObject.SetActive(true);
			arrowCircleSprite.transform.position = Camera.main.ScreenToWorldPoint(centroid);
			Vector3 circleScale = new Vector3((float)circleRadius, (float)circleRadius, 1f);
			Vector2 clickDir = (touches.Last().position - centroid).normalized;
			float angle = VectorToDegrees(clickDir) - 130f;
			arrowCircleSprite.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			arrowCircleSprite.transform.localScale = circleScale/minCircleRadius * 0.7f;
			if (RorL > 0)
			{
				arrowCircleSprite.invert = true;
				arrowCircleSprite.flip = UIBasicSprite.Flip.Horizontally;
				clockwise = true;
			}
			else
			{
				arrowCircleSprite.invert = false;
				arrowCircleSprite.flip = UIBasicSprite.Flip.Nothing;
				clockwise = false;
			}
		} else
			arrowCircleSprite.gameObject.SetActive(false);

		circleDecayTimer -= Time.deltaTime;
		if(circleDecayTimer < 0f)
			touchForceState = TouchForce.Flick;
	}
	float VectorToDegrees (Vector2 vec) {return Mathf.Atan2(vec.y, vec.x)*Mathf.Rad2Deg;}

	Vector2 getTouchForceAdder()
	{
		Vector2 avgTouchVelocity = Utilities.AverageVector(touches.Select<TouchNode, Vector2>(touch => touch.velocity));
		//both touches and mouse positions begin in units of screen space.
		//NGUI works where the UI is 1 y unit up and 1 y unit down, and screenspace
		//is divided into that area in terms of height. thats why the UIRoot has a weird tiny scale,
		//it's so UI elements can appear to have the scale set in terms of pixels, but it always takes up exactly 2 y units in virtual space.
		//therefore dividing by (pixelheight / 2) turns screen space into world space. crazy. this should be much more efficient than a
		//screen to world space equation.
		Vector2 finalTouchForceAdder = (avgTouchVelocity * velocityAdderStrength)/((float)uiRoot.activeHeight/2f);
		return finalTouchForceAdder;
	}
	Vector2 getCircleForceAdder(Sheep sheep)
	{
		Vector3 circleCenterWorldSpace = Camera.main.ScreenToWorldPoint(centroid);
		Vector3 radiusLine = (sheep.transform.position - circleCenterWorldSpace);
		Vector3 tangentForce;
		if (clockwise)
			tangentForce = new Vector3(-radiusLine.y, radiusLine.x, radiusLine.z).normalized * tangentForceScale;
		else
			tangentForce = -new Vector3(-radiusLine.y, radiusLine.x, radiusLine.z).normalized * tangentForceScale;
		Vector3 pullForce = (-radiusLine).normalized * pullForceScale;
		pullForce = pullForce * Mathf.Pow((float)(1f/(circleRadius/minCircleRadius)), 0.5f);
		Debug.DrawLine(sheep.transform.position, sheep.transform.position + tangentForce, Color.blue);
		Debug.DrawLine(circleCenterWorldSpace, circleCenterWorldSpace + radiusLine);
		return tangentForce+pullForce;
	}

	Vector2 getMagnetForceAdder(Sheep sheep)
	{
		float distance = Vector2.Distance((Vector2)sheepToucher.transform.position, (Vector2)sheep.transform.position);
		if(distance > maximumMagnetDistance)
			return Vector2.zero;
		Vector2 magnetForceAdder = Vector2.zero;
		//NOTE: the sheep will still forget about being herded once its timer expires,
		//even when trying to pull it with the magnet force. this force is purely to aid with guidance once the finger has moved away.
		//otherwise this would be a magnet game.
		float inverseDistanceRatio = Mathf.Clamp(
			maximumMagnetDistance - distance, 
			0f, maximumMagnetDistance) / maximumMagnetDistance;
		//this is already in world space, so no need to convert. this ought to help with the herding a bit (pulls captured sheep toward the finger).
		magnetForceAdder = (sheepToucher.transform.position - sheep.transform.position).normalized 
			* inverseDistanceRatio
				* magnetStrength;
		return magnetForceAdder;
	}
	bool circleFit()
	{
		List<Vector2> points = touches.Select<TouchNode, Vector2>(touch => touch.position).ToList();
		centroid = averageTouchPosition();
		int k = points.Count;
		circleRadius = 0;
		foreach (Vector2 p in points)
			circleRadius += Vector2.Distance(centroid, p);
		circleRadius /= k;
		if(circleRadius < minCircleRadius)
			return false;
		
		const double eps = 0.45; // 25% difference to average is acceptable lower/raise this on mobile?
		
		foreach (Vector2 p in points)
		{
			double r = Vector2.Distance(centroid, p);
			if (Mathf.Abs(((float)(circleRadius-r))/(float)circleRadius)>eps)
				return false;
		}
		return true;
	}
}
