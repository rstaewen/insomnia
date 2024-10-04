#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class LoopTexture : MonoBehaviour
{
	Material loopMaterial;
	public float loopSpeed = 10f;
	float _levelSpeed;
	public float levelSpeed {get {return _levelSpeed;} set {_levelSpeed = value; setShader();}}
	bool _canLoop = true;
	public bool canLoop {get {return _canLoop;} set {_canLoop = value; setShader();}}
	private Vector4 offset;
	private Vector4 initialOffset;

	void Start()
	{
		loopMaterial = GetComponent<MeshRenderer>().material;
	}
	void setShader()
	{
		loopMaterial = GetComponent<MeshRenderer>().material;
		if(_canLoop)
			loopMaterial.SetFloat("_TexMoveSpeedX", levelSpeed*loopSpeed);
		else
			loopMaterial.SetFloat("_TexMoveSpeedX", 0f);
	}

}


