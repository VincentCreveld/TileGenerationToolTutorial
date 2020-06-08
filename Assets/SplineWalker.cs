using BezierTool;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SplineWalker : MonoBehaviour
{
	public BezierAccesser spline;
	public float moveDuration = 5f;
	[SerializeField, Range(0,1)] private float progress = 0f;
	public bool lookForward = true;

	public bool follow = false;
	public Transform followObject;

	public bool freeze = false;
	[Range(5f,100f)]
	public float accuracy = 100f;
	private void Update()
	{
		//float point = 0f;
		//if(follow)
		//{
		//	Vector3 position = spline.GetClosestPointOnSpline(followObject.position, out point, accuracy);
		//	transform.localPosition = position;
		//}
		//else
		{
			if(!freeze)
			{
				progress += Time.deltaTime / moveDuration;
				if(progress >= 1f)
					progress = 0f;
			}
			progress = Mathf.Clamp01(progress);
			Vector3 position2 = spline.GetCurve().GetPoint(progress);
			transform.position = position2;
			if(lookForward)
				transform.forward = spline.GetCurve().GetVelocityDirection(progress);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if(other.tag == "Spline")
			Debug.Log("entered");
	}

	private void OnTriggerStay(Collider other)
	{
		if(other.tag == "Spline")
			Debug.Log("stay");
	}

	private void OnTriggerExit(Collider other)
	{
		if(other.tag == "Spline")
			Debug.Log("exited");
	}
}
