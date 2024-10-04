using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class FollowTransformRemotely : MonoBehaviour
{
	public Transform followed;
	public string tagOfReassignedParent;
	public enum PostAction {DestroyThisAlso, StopFollow, Nothing}
	public PostAction followedDestructionAction;
	public Vector3 offsetPosition;
	private bool parentSet = false;
	
	protected void Start()
	{
		if(!parentSet)
			SetParent();
	}
	public void SetParent()
	{
		if(tagOfReassignedParent == "")
			return;
		GameObject parent = GameObject.FindGameObjectWithTag(tagOfReassignedParent);
		if(parent)
		{
			reassignLayerWithChildren(transform, parent.layer);
			Vector3 totalScale = transform.lossyScale;
			//			Vector3 targetParentScale = parent.transform.lossyScale;
			//			Vector3 finalLocalScale = new Vector3(totalScale.x/targetParentScale.x, totalScale.y/targetParentScale.y, totalScale.z/targetParentScale.z);
			
			transform.parent = parent.transform;
			//transform.localScale = finalLocalScale;
			parentSet = true;
		}
		else
			Debug.LogWarning("WARNING: Needs a parent with stated tag to attach to.");
	}
	void reassignLayerWithChildren(Transform xform, int parentLayer)
	{
		xform.gameObject.layer = parentLayer;
		for(int i = 0 ; i<xform.childCount; i++)
			reassignLayerWithChildren(xform.GetChild(i), parentLayer);
	}
	void OnEnable()
	{
		if(followed)
		{
			transform.position = followed.position + offsetPosition;
			transform.rotation = Quaternion.identity;
		}
	}
	protected void Update()
	{
		if(!followed)
		{
			switch(followedDestructionAction)
			{
			case PostAction.DestroyThisAlso:
				GameObject.Destroy(gameObject);
				break;
			case PostAction.StopFollow:
				enabled = false;
				break;
			}
			return;
		}
		transform.position = followed.position + offsetPosition;
	}
	
}
