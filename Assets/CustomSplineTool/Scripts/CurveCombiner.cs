using System.Collections.Generic;
using UnityEngine;

namespace BezierTool
{
	[RequireComponent(typeof(BezierAccesser))]
	public class CurveCombiner : MonoBehaviour, ICombinableCurve
	{
		public List<BezierAccesser> splines = null;

		public Vector3 GetPoint(float t)
		{
			int targetCurve = GetTargetedCurveByTimestamp(t);
			float tVal = t;

			if(targetCurve == splines.Count)
				tVal -= 0.0001f;
			t = Mathf.Clamp01(t);
			return splines[targetCurve].GetCurve().GetPoint(MapTimestampToCurve(tVal));
		}

		public OrientedPoint GetOrientedPoint(float t)
		{
			int targetCurve = GetTargetedCurveByTimestamp(t);
			float tVal = t;

			if(targetCurve == splines.Count)
				tVal -= 0.0001f;
			return splines[targetCurve].GetCurve().GetOrientedPoint(MapTimestampToCurve(tVal));
		}

		public Vector3 GetVelocityDirection(float t)
		{
			int targetCurve = GetTargetedCurveByTimestamp(t);
			float tVal = t;

			if(targetCurve == splines.Count)
				tVal -= 0.0001f;
			return splines[targetCurve].GetCurve().GetVelocityDirection(MapTimestampToCurve(tVal));
		}

		public float GetCurveLength()
		{
			float length = 0f;
			foreach(ICombinableCurve c in splines)
			{
				// Prevents infinite recursion.
				if(c == this as ICombinableCurve)
					continue;
				length += c.GetCurveLength();
			}
			return length;
		}

		public void MoveFirstPointAndAnchor(Vector3 pos, Vector3 anchorPos, CardinalDirections dir, Vector2 dims)
		{
			splines[0].GetCurve().MoveFirstPointAndAnchor(pos, anchorPos, dir, dims);
		}

		public void MoveLastPointAndAnchor(Vector3 pos, Vector3 anchorPos, CardinalDirections dir, Vector2 dims)
		{
			splines[splines.Count - 1].GetCurve().MoveFirstPointAndAnchor(pos, anchorPos, dir, dims);
		}

		public Vector3 GetLastAnchor(bool local = false)
		{
			return splines[splines.Count - 1].GetCurve().GetLastAnchor(local);
		}

		public Vector3 GetLastRotAnchor(bool local = false)
		{
			return splines[splines.Count - 1].GetCurve().GetLastRotAnchor(local);
		}

		public Vector3 GetLastPoint(bool local = false)
		{
			return splines[splines.Count - 1].GetCurve().GetLastPoint(local);
		}

		public void MimicPreviousSplineSettings(Vector3 anchorPrev, Vector3 rotAnchorPrev, Vector3 splinePointPrev, Vector3 localEulerPrev)
		{
			splines[0].GetCurve().MimicPreviousSplineSettings(anchorPrev, rotAnchorPrev, splinePointPrev, localEulerPrev);
		}

		public Vector3 GetClosestPointOnSpline(Vector3 pos, out float stepMoment, float accuracy = 100f)
		{
			Vector3 closestPoint = GetPoint(0);
			float outMoment = 0f;

			for(int i = 0; i < splines.Count; i++)
			{
				float t = 0f;
				Vector3 v = splines[i].GetCurve().GetClosestPointOnSpline(pos, out t, accuracy);

				if(Vector3.Distance(pos, v) <= Vector3.Distance(closestPoint, v))
				{
					closestPoint = v;
					outMoment = GetNewT(i, t);
				}
			}

			stepMoment = outMoment;
			return closestPoint;
		}

		public float GetNewT(int spline, float t)
		{
			float splineSize = 1f / splines.Count;
			float lowVal = splineSize * (float)spline;
			float highVal = splineSize * (float)(spline + 1f);
			return Mathf.Clamp01(Mathf.Lerp(lowVal, highVal, t));
		}

		// The val is the value on the old curve that should be mapped to the new.
		public void AddCurve(ref float val, BezierAccesser c)
		{
			float v = Mathf.Lerp(0, (float)splines.Count, val);
			float s = val;
			val = Mathf.InverseLerp(0, (float)splines.Count + 1f, v);
			if(splines.Count < 2)
				val = s;
			splines.Add(c);
		}

		public void RemoveFirstCurve(ref float val)
		{
			BezierAccesser oldFirst = splines[0];
			List<BezierAccesser> newList = new List<BezierAccesser>(splines.Count - 1);
			for(int i = 1; i < splines.Count; i++)
			{
				newList.Add(splines[i]);
			}
			splines = newList;

			Destroy(oldFirst.gameObject.GetComponentInParent<GenerationTileController>().gameObject);

			float differenceInStep = (1f / splines.Count) - (1f / (splines.Count + 1));
			bool isInverse = false;
			float n = val;
			if(n <= (1f / (splines.Count + 1)))
			{
				isInverse = true;
				n = 1f - n;
			}

			float v = Mathf.Lerp((float)splines.Count + 1, 0, n);
			float s = val;
			float newN = Mathf.InverseLerp((float)splines.Count, 0, v);
			val = (isInverse)? 1 - newN : newN;
			if(splines.Count < 2)
				val = s;
		}

		private int GetTargetedCurveByTimestamp(float t)
		{
			float curveSegmentSize = 1f / (float)splines.Count;
			int returnVal = Mathf.FloorToInt(t / curveSegmentSize);

			//if(returnVal == splines.Count)
			//	returnVal -= 1;

			return returnVal;
		}

		private float MapTimestampToCurve(float t)
		{
			if(t >= 1)
				t = 1;
			if(t <= 0)
				t = 0;

			float curveSegmentSize = 1f / (float)splines.Count;
			float leftOver = t % curveSegmentSize;
			int curveToTarget = Mathf.FloorToInt(t / curveSegmentSize);

			// edge case if end of spline is reached.
			if(curveToTarget == splines.Count && t == 1f)
				return 1f;

			return Map(leftOver, 0f, curveSegmentSize, 0f, 1f);
		}

		private float Map(float value, float fromSource, float toSource, float fromTarget, float toTarget)
		{
			return (value - fromSource) / (toSource - fromSource) * (toTarget - fromTarget) + fromTarget;
		}
	}
}
