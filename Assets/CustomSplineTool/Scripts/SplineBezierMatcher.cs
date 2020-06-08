using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BezierTool
{
	public class SplineBezierMatcher : MonoBehaviour
	{
		public SplineBezierCurve curve0, curve1;

		[ContextMenu("Match Curves")]
		public void Match()
		{
			Vector3 a = curve0.GetLastAnchor(false);
			Vector3 b = curve0.GetLastPoint(false);
			Vector3 c = curve0.GetLastRotAnchor(false);

			curve1.MimicPreviousSplineSettings(a, c, b, curve0.transform.localEulerAngles);
		}
	}
}