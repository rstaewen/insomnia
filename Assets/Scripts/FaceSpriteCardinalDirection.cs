#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FaceSpriteCardinalDirection : MonoBehaviour
{
	public UISpriteAnimation animation;
	private Rigidbody2D rb;
	public enum MoveDirection {N_, NE, E_, SE, S_, SW, W_, NW}
	private MoveDirection moveDir;
	public MoveDirection MoveDir {get {return moveDir;}}
	public MoveDirection VisualMoveDir {get {return moveDir;}}
	private Vector2 upRight = new Vector2(0.7f,0.7f);
	private Vector2 upLeft= new Vector2(-0.7f,0.7f);
	private Vector2 botLeft= new Vector2(-0.7f,-0.7f);
	private Vector2 botRight= new Vector2(0.7f,-0.7f);
	private float speed;
	private Vector2 velocity;
	private Vector2 velocityNormalized;
	public int MinFPS;
	public int MaxFPS;
	public float TopVelocity;
	public float SpeedPercentage;
	private bool canUnpause = true;
	private WaitForSeconds waitAnimationAmt;

	public string sheepTypePrefix;
	public string deadPrefix;

	void Awake()
	{
		waitAnimationAmt = new WaitForSeconds(UnityEngine.Random.Range(0.1f,0.2f));
		rb = GetComponent<Rigidbody2D>();
		animation.framesPerSecond = MinFPS;
		if(TopVelocity == 0f)
			SetTopVelocity(0.5f);
	}
	public void SetTopVelocity(float moveForce)
	{
		TopVelocity = ((moveForce / rb.drag) - Time.fixedDeltaTime * moveForce ) / rb.mass;
	}
	public void Pause()
	{
		animation.framesPerSecond = 0;
		enabled = false;
	}
	void OnEnable()
	{
		StartCoroutine(UpdateAnimation());
	}
	public void Resume()
	{
		if(canUnpause)
		{
			animation.enabled = true;
			enabled = true;
		}
	}

	public void ForceOrientation (FaceSpriteCardinalDirection.MoveDirection dir)
	{
		animation.enabled = false;
		animation.GetComponent<UISprite>().spriteName = sheepTypePrefix + dir.ToString()+"2";
		animation.GetComponent<UISprite>().MakePixelPerfect();
		moveDir = dir;
		enabled = false;
	}

	public void ShowDeath()
	{
		animation.enabled = false;
		animation.GetComponent<UISprite>().spriteName = sheepTypePrefix + deadPrefix + moveDir.ToString();
		animation.GetComponent<UISprite>().MakePixelPerfect();
		enabled = false;
		canUnpause = false;
	}
	IEnumerator UpdateAnimation()
	{
		while(enabled)
		{
			speed = velocity.magnitude;
			SpeedPercentage = speed / TopVelocity;

			velocity = rb.velocity;
			velocityNormalized = rb.velocity.normalized;

			if(Vector2.Distance(velocityNormalized, Vector2.up) < 0.4f)
				moveDir = MoveDirection.N_;
			else if(Vector2.Distance(velocityNormalized, Vector2.right) < 0.4f)
				moveDir = MoveDirection.E_;
			else if(Vector2.Distance(velocityNormalized, -Vector2.up) < 0.4f)
				moveDir = MoveDirection.S_;
			else if(Vector2.Distance(velocityNormalized, -Vector2.right) < 0.4f)
				moveDir = MoveDirection.W_;
			else if(Vector2.Distance(velocityNormalized, upRight) < 0.4f)
				moveDir = MoveDirection.NE;
			else if(Vector2.Distance(velocityNormalized, botRight) < 0.4f)
				moveDir = MoveDirection.SE;
			else if(Vector2.Distance(velocityNormalized, botLeft) < 0.4f)
				moveDir = MoveDirection.SW;
			else if(Vector2.Distance(velocityNormalized, upLeft) < 0.4f)
				moveDir = MoveDirection.NW;

			animation.namePrefix = sheepTypePrefix + moveDir.ToString();
			if(rb.velocity.magnitude < TopVelocity * 0.01f)
				animation.framesPerSecond = 0;
			else if (rb.velocity.magnitude < TopVelocity * 0.5f)
				animation.framesPerSecond = MinFPS;
			else
				animation.framesPerSecond = MaxFPS;
			yield return waitAnimationAmt;
		}
	}
}
