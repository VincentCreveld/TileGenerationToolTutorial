using UnityEngine;

namespace BezierTool
{
	public interface ICombinableCurve
	{
		Vector3 GetPoint(float t);
		OrientedPoint GetOrientedPoint(float t);
		Vector3 GetVelocityDirection(float t);
		float GetCurveLength();

		void MoveFirstPointAndAnchor(Vector3 pos, Vector3 anchorPos, CardinalDirections dir, Vector2 dims);
		void MoveLastPointAndAnchor(Vector3 pos, Vector3 anchorPos, CardinalDirections dir, Vector2 dims);

		Vector3 GetLastAnchor(bool local = false);
		Vector3 GetLastRotAnchor(bool local = false);
		Vector3 GetLastPoint(bool local = false);
		void MimicPreviousSplineSettings(Vector3 anchorPrev, Vector3 rotAnchorPrev, Vector3 splinePointPrev, Vector3 localEulerPrev);

		Vector3 GetClosestPointOnSpline(Vector3 pos, out float stepMoment, float accuracy = 100f);
	}
}
