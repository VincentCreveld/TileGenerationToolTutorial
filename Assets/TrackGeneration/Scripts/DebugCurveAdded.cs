using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierTool;

public class DebugCurveAdded : MonoBehaviour
{
	public CurveCombiner target;
	public BezierAccesser toAdd;

	[Range(0f,1f)]
	public float progress = 0f;

	public Transform tracker = null;

	private void Update()
	{
		progress = Mathf.Clamp01(progress);
		tracker.position = target.GetPoint(progress);
	}

	[ContextMenu("aaa")]
	public void AddCurve()
	{
		target.AddCurve(ref progress, toAdd);
	}
}
