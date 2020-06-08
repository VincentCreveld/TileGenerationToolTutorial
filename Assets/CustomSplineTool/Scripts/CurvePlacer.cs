using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierTool
{
	// proof of concept for placing a tile with offset in mind to road start
	public class CurvePlacer : MonoBehaviour
	{
		public SplineBezierCurve curve0, curve1;

		[ContextMenu("Match Curves")]
		public void Match()
		{
			Vector3 curveOffset = curve1.GetPoint(0) - curve1.transform.position;
			//Vector3 curveOffset = curve1.transform.position - curve1.GetPoint(0);
			curve1.transform.position = curve0.GetLastPoint() - curveOffset;
			curve1.transform.rotation = curve0.GetOrientedPoint(1f).rotation;
			curve1.transform.localEulerAngles = new Vector3(0, curve1.transform.localEulerAngles.y - 90, 0);
		}
	}
}
