using System.Collections.Generic;
using System;
using UnityEditor;
using UnityEngine;

namespace BezierTool
{
	[Serializable, RequireComponent(typeof(BezierAccesser))]
	public class SplineBezierCurve : MonoBehaviour, ICombinableCurve
	{
		[SerializeField] private Vector3[] points;
		[SerializeField] private Vector3[] rotationAnchors;
		[SerializeField] BezierControlPointMode[] controlPointModes;
		[HideInInspector] public bool DrawVelocity = false;
		[HideInInspector] public bool DrawGizmos = false;
		[SerializeField] private bool isLoop;
		[SerializeField] private float splineLength = 0f;
		private const int LINE_STEPS = 10;

		public bool IsLoop
		{
			get { return isLoop; }
			set
			{
				isLoop = value;
				if(value == true)
				{
					controlPointModes[controlPointModes.Length - 1] = controlPointModes[0];
					SetControlPoint(0, points[0]);
				}
			}
		}

		public int ControlPointCount
		{
			get
			{
				return points.Length;
			}
		}

		public int CurveCount
		{
			get
			{
				return (points.Length - 1) / 3;
			}
		}

		private void Start()
		{
			splineLength = GetSplineLength();
		}

		public void Reset()
		{
			points = new Vector3[]
			{
			new Vector3(0f,1f,0f),
			new Vector3(10f,1f,0f),
			new Vector3(40f,1f,0f),
			new Vector3(50f,1f,0f)
			};

			rotationAnchors = new Vector3[]
			{
				points[0] + new Vector3(0f,3f,0f),
				points[3] + new Vector3(0f,3f,0f)
			};

			controlPointModes = new BezierControlPointMode[]
			{
				BezierControlPointMode.Free,
				BezierControlPointMode.Free
			};
		}

		public float GetCurveLength()
		{
			return splineLength;
		}

		// Gives a time moment on the spline which is closest to the given object point.
		public Vector3 GetClosestPointOnSpline(Vector3 pos, out float stepMoment, float accuracy = 100f)
		{
			Vector3 returnVal = GetPoint(0);
			stepMoment = -1f;
			float val = accuracy;
			if(accuracy <= 5f)
				val = 5f;

			float stepSize = Mathf.Clamp(1f / val, 0.001f, 0.2f);
			float closestDistance = Mathf.Infinity;

			for(float i = 0f; i < 1f; i += stepSize)
			{
				Vector3 currentPoint = GetPoint(i);
				float dist = Vector3.Distance(currentPoint, pos);
				if(dist < closestDistance)
				{
					closestDistance = dist;
					stepMoment = i;
					returnVal = currentPoint;
				}
			}
			return returnVal;
		}

		// Returns an approximated length of the bezier curve. Accuracy defines the amount of subdivisions along the spline used to determine length.
		public float GetSplineLength(float accuracy = 100f)
		{
			float returnVal = 0f;
			float val = accuracy;
			if(accuracy <= 5f)
				val = 5f;

			float stepSize = Mathf.Clamp(1f / val, 0.001f, 0.2f);

			Vector3 previousPoint = GetPoint(0f);
			for(float i = 0f; i < 1f; i += stepSize)
			{
				Vector3 currentPoint = GetPoint(i);
				returnVal += Vector3.Distance(previousPoint, currentPoint);
				previousPoint = currentPoint;
			}
			splineLength = returnVal;
			return returnVal;
		}

		// Generates a pseudo-transform at a given point on the spline.
		public OrientedPoint GetOrientedPoint(float t)
		{
			Vector3 localUp = GetClosestUp(t);
			return new OrientedPoint(GetPoint(t), GetOrientation(t, localUp), GetTangent(t), GetBinormal(t, localUp), GetNormal(t, localUp));
		}

		#region BezierSpline functions
		// Gets a non-tangent point along the spline.
		public Vector3 GetCurvePointByIndex(int i)
		{
			return points[i * 3];
		}

		/// <summary>
		/// Gets a point in global space at point T along the spline.
		/// </summary>
		public Vector3 GetPoint(float t)
		{
			int i;
			if(t >= 1f)
			{
				t = 1f;
				i = points.Length - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return transform.TransformPoint(Bezier.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t));
		}

		// Gets the tangent at a given point along the spline. Also defined as velocity. (direction where the curve is pointing at that point. the "forward")
		public Vector3 GetTangent(float t)
		{
			int i;
			if(t >= 1f)
			{
				t = 1f;
				i = points.Length - 4;
			}
			else
			{
				t = Mathf.Clamp01(t) * CurveCount;
				i = (int)t;
				t -= i;
				i *= 3;
			}

			return transform.TransformPoint(Bezier.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t)) - transform.position;
		}

		// Gets the normal at any given point along the spline. (the "up" of the point relative to the given up vector)
		public Vector3 GetNormal(float t, Vector3 up)
		{
			// local z
			Vector3 tangent = GetTangent(t).normalized;
			// local x
			Vector3 binormal = Vector3.Cross(up, tangent).normalized;
			// local y
			Vector3 localNormal = Vector3.Cross(tangent, binormal);
			return localNormal;
		}

		// Gets the Binormal. Also known as the "right" of the point.
		public Vector3 GetBinormal(float t, Vector3 up)
		{
			// local z
			Vector3 tangent = GetTangent(t).normalized;
			// local x
			Vector3 binormal = Vector3.Cross(up, tangent).normalized;
			return binormal;
		}

		public Quaternion GetOrientation(float t, Vector3 up)
		{
			return Quaternion.LookRotation(GetTangent(t).normalized, GetNormal(t, up).normalized);
		}

		public Vector3 GetVelocityDirection(float t)
		{
			return GetTangent(t).normalized;
		}

		public Vector3 GetControlPoint(int index)
		{
			return points[index];
		}

		// Sets position of a non-tangent point in the spline.
		public void SetControlPoint(int index, Vector3 point)
		{
			if(index % 3 == 0)
			{
				Vector3 delta = point - points[index];
				if(IsLoop)
				{
					if(index == 0)
					{
						points[1] += delta;
						points[points.Length - 2] += delta;
						points[points.Length - 1] = point;
						rotationAnchors[0] += delta;
					}
					else if(index == points.Length - 1)
					{
						points[0] = point;
						points[1] += delta;
						points[index - 1] += delta;
						rotationAnchors[rotationAnchors.Length - 1] += delta;
					}
					else
					{
						points[index - 1] += delta;
						points[index + 1] += delta;
						rotationAnchors[index / 3] += delta;
					}
				}
				else
				{
					if(index > 0)
						points[index - 1] += delta;
					if(index + 1 < points.Length)
						points[index + 1] += delta;
					rotationAnchors[index / 3] += delta;
				}
			}

			points[index] = point;
			EnforceMode(index);
		}

		public BezierControlPointMode GetControlPointMode(int index)
		{
			return controlPointModes[(index + 1) / 3];
		}

		// Sets the restraint on a tangent point along the spline.
		public void SetControlPointMode(int index, BezierControlPointMode mode)
		{
			int modeIndex = (index + 1) / 3;
			controlPointModes[modeIndex] = mode;
			if(isLoop)
			{
				if(modeIndex == 0)
					controlPointModes[controlPointModes.Length - 1] = mode;
				else if(modeIndex == controlPointModes.Length - 1)
					controlPointModes[0] = mode;
			}
			EnforceMode(index);
		}

		// Enforces the control mode of the tangent points surrounding a non-tangent point.
		public void EnforceMode(int index)
		{
			int modeIndex = (index + 1) / 3;
			BezierControlPointMode mode = controlPointModes[modeIndex];
			if(mode == BezierControlPointMode.Free || !IsLoop && (modeIndex == 0 || modeIndex == controlPointModes.Length - 1))
				return;

			int middleIndex = modeIndex * 3;
			int fixedIndex;
			int enforcedIndex;

			if(index <= middleIndex)
			{
				fixedIndex = middleIndex - 1;
				if(fixedIndex < 0)
					fixedIndex = points.Length - 2;
				enforcedIndex = middleIndex + 1;
				if(enforcedIndex >= points.Length)
					enforcedIndex = 1;
			}
			else
			{
				fixedIndex = middleIndex + 1;
				if(fixedIndex >= points.Length)
					fixedIndex = 1;
				enforcedIndex = middleIndex - 1;
				if(enforcedIndex < 0)
					enforcedIndex = points.Length - 2;
			}

			Vector3 middle = points[middleIndex];
			Vector3 enforcedTangent = middle - points[fixedIndex];

			if(mode == BezierControlPointMode.Aligned)
			{
				enforcedTangent = enforcedTangent.normalized * Vector3.Distance(middle, points[enforcedIndex]);
			}

			points[enforcedIndex] = middle + enforcedTangent;
		}
		#endregion

		#region Custom editor support functions
		// Smoothes out the curve by placing anchors on a point on a line between the non-tangents.
		public void AlignAnchors()
		{
			for(int i = 0; i < points.Length - 2; i += 3)
			{
				Vector3 start = points[i];
				Vector3 an1 = points[i + 1];
				Vector3 an2 = points[i + 2];
				Vector3 end = points[i + 3];

				float newY1 = Vector3.Lerp(start, end, 0.2f).y;
				float newY2 = Vector3.Lerp(start, end, 0.8f).y;

				points[i + 1].y = newY1;
				points[i + 2].y = newY2;
			}
		}

		public void RemoveLoop()
		{
			SetControlPoint(points.Length - 1, -GetVelocityDirection(1));
			SetControlPoint(points.Length - 2, -GetVelocityDirection(1) * 2);
		}

		// Adds a new section of curve.
		public void AddCurve()
		{
			int lastIndex = points.Length - 1;

			float placement = 1f - 1f / (float)rotationAnchors.Length;

			Vector3 point = transform.InverseTransformPoint(GetPoint(placement));
			Vector3 direction = GetTangent(placement).normalized * 5;
			Array.Resize(ref points, points.Length + 3);

			points[points.Length - 1] = points[lastIndex];
			points[points.Length - 2] = points[lastIndex - 1];

			points[lastIndex] = point;
			points[lastIndex - 1] = point - direction;
			points[lastIndex + 1] = point + direction;

			//point += direction;
			//points[points.Length - 3] = point; //tan point old
			//point += direction;
			//points[points.Length - 2] = point; // tan point new
			//point += direction;
			//points[points.Length - 1] = point; // anchor new
			Array.Resize(ref rotationAnchors, rotationAnchors.Length + 1);
			rotationAnchors[rotationAnchors.Length - 2] = point + new Vector3(0f, 3f, 0f);
			rotationAnchors[rotationAnchors.Length - 1] = points[points.Length - 1] + new Vector3(0f, 3f, 0f);// point + new Vector3(0f, 3f, 0f);


			Array.Resize(ref controlPointModes, controlPointModes.Length + 1);
			controlPointModes[controlPointModes.Length - 2] = controlPointModes[controlPointModes.Length - 1];
			EnforceMode(points.Length - 4);

			if(isLoop)
			{
				points[points.Length - 1] = points[0];
				controlPointModes[controlPointModes.Length - 1] = controlPointModes[0];
				EnforceMode(0);
			}
		}

		// Removes the last curve section.
		public void RemoveCurve()
		{
			if(points.Length <= 4)
				return;
			Vector3[] newPoints = new Vector3[points.Length - 3];
			int threshold = newPoints.Length - 3;
			for(int i = 0; i < newPoints.Length; i++)
			{
				if(i >= threshold - 1)
				{
					newPoints[i] = points[i + 3];
				}
				else
				{
					newPoints[i] = points[i];
				}
			}
			points = newPoints;

			Vector3[] newRot = new Vector3[rotationAnchors.Length - 1];
			threshold = newRot.Length - 1;
			for(int i = 0; i < newRot.Length; i++)
			{
				if(i >= threshold - 1)
				{
					newRot[i] = rotationAnchors[i + 1];
				}
				else
				{
					newRot[i] = rotationAnchors[i];

				}
			}
			rotationAnchors = newRot;
		}
		#endregion

		#region Curve alignment functions
		public Vector3 GetClosestUp(float t)
		{
			Vector3 returnVal = new Vector3();

			int index = Mathf.FloorToInt((float)CurveCount * t);

			if(index == CurveCount)
			{
				returnVal = GetRotationPoint(index) - GetCurvePointByIndex(index);
			}
			else
			{
				float t0 = (1f / (float)CurveCount) * index;
				float t1 = (1f / (float)CurveCount) * index + 1f;
				float tCur = Mathf.InverseLerp(t0, t1, t);
				Vector3 a = GetCurvePointByIndex(index);
				Vector3 b = GetRotationPoint(index);
				Vector3 c = GetCurvePointByIndex(index + 1);
				Vector3 d = GetRotationPoint(index + 1);
				Vector3 e = b - a;
				Vector3 f = d - c;
				returnVal = Vector3.Lerp(e, f, tCur).normalized;
			}
			return returnVal;
		}

		public Vector3 GetRotationPoint(int index)
		{
			return rotationAnchors[index];
		}

		public void SetRotPoint(int index, Vector3 pos)
		{
			rotationAnchors[index] = pos;
		}

		/// <summary>
		/// Pseudo-enforces control point mode based on external spline. Also mimics the rotation of the road at the point of connection.
		///This function is applied to the new connecting tile.This function thus works on the owner of the script.
		/// </summary>
		public void MimicPreviousSplineSettings(Vector3 anchorPrev, Vector3 rotAnchorPrev, Vector3 splinePointPrev, Vector3 localEulerPrev)
		{
			Vector3 aP = transform.InverseTransformPoint(anchorPrev);
			Vector3 aPR = transform.InverseTransformPoint(rotAnchorPrev);
			Vector3 aPS = transform.InverseTransformPoint(splinePointPrev);

			float yAxisDiff = transform.localEulerAngles.y - localEulerPrev.y;

			Vector3 v0 = (aPR - points[0]).normalized * 5;
			v0 = Quaternion.AngleAxis(yAxisDiff, Vector3.up) * v0;
			rotationAnchors[0] = points[0] + v0;

			Vector3 direction = (aP - aPS).normalized;
			points[1] = points[0] - direction * Vector3.Distance(aP, aPS);
		}

		public Vector3 GetLastAnchor(bool local = false)
		{
			Vector3 returnVal = (local) ? points[points.Length - 2] : transform.TransformPoint(points[points.Length - 2]);
			return returnVal;
		}

		public Vector3 GetLastRotAnchor(bool local = false)
		{
			Vector3 returnVal = (local) ? rotationAnchors[rotationAnchors.Length - 1] : transform.TransformPoint(rotationAnchors[rotationAnchors.Length - 1]);
			return returnVal;
		}

		public Vector3 GetLastPoint(bool local = false)
		{
			Vector3 returnVal = (local) ? points[points.Length - 1] : transform.TransformPoint(points[points.Length - 1]);
			return returnVal;
		}
		#endregion

		public void MoveFirstPointAndAnchor(Vector3 pos, Vector3 anchorPos, CardinalDirections dir, Vector2 dims)
		{
			Vector3 relativePosForRot = rotationAnchors[0] - points[0];
			points[0] = transform.InverseTransformPoint(pos);
			Vector3 newAnchorPos = transform.InverseTransformPoint(pos + (anchorPos.normalized * 5f));
			switch(dir)
			{
				case CardinalDirections.North:
					newAnchorPos.z = dims.y * 0.5f - dims.y * 0.35f;
					break;
				case CardinalDirections.East:
					newAnchorPos.x = dims.x * 0.5f - dims.x * 0.35f;
					break;
				case CardinalDirections.South:
					newAnchorPos.z = dims.y * 0.5f - dims.y * 0.65f;
					break;
				case CardinalDirections.West:
					newAnchorPos.x = dims.x * 0.5f - dims.x * 0.65f;
					break;
			}
			points[1] = newAnchorPos;
			rotationAnchors[0] = points[0] + relativePosForRot;
		}

		public void MoveLastPointAndAnchor(Vector3 pos, Vector3 anchorPos, CardinalDirections dir, Vector2 dims)
		{
			Vector3 relativePosForRot = rotationAnchors[rotationAnchors.Length - 1] - points[points.Length - 1];
			points[points.Length - 1] = transform.InverseTransformPoint(pos);
			Vector3 newAnchorPos = transform.InverseTransformPoint(pos + (anchorPos.normalized * 5f));
			switch(dir)
			{
				case CardinalDirections.North:
					newAnchorPos.z = dims.y * 0.5f - dims.y * 0.35f;
					break;
				case CardinalDirections.East:
					newAnchorPos.x = dims.x * 0.5f - dims.x * 0.35f;
					break;
				case CardinalDirections.South:
					newAnchorPos.z = dims.y * 0.5f - dims.y * 0.65f;
					break;
				case CardinalDirections.West:
					newAnchorPos.x = dims.x * 0.5f - dims.x * 0.65f;
					break;
			}

			points[points.Length - 2] = newAnchorPos;
			rotationAnchors[rotationAnchors.Length-1] = points[points.Length-1] + relativePosForRot;
		}
	}

	public enum BezierControlPointMode
	{
		Free,
		Aligned,
		Mirrored
	}

	public struct PointWithTangent
	{
		public Vector3 point;
		public Vector3 tangent;

		public PointWithTangent(Vector3 pos, Vector3 tan)
		{
			point = pos;
			tangent = tan;
		}
	}

	[Serializable]
	// This struct functions as a pseudo-transform containing position and rotation information for a point along a spline.
	public struct OrientedPoint
	{
		public Vector3 position;
		public Quaternion rotation;
		public Vector3 forward, right, up;

		public OrientedPoint(Vector3 pos, Quaternion rot, Vector3 fwd, Vector3 r, Vector3 u)
		{
			position = pos;
			rotation = rot;
			forward = fwd;
			right = r;
			up = u;
		}

		public Vector3 LocalToWorldPosition(Vector3 point)
		{
			return position + rotation * point;
		}

		public Vector3 WorldToLocal(Vector3 point)
		{
			return Quaternion.Inverse(rotation) * (point - position);
		}

		public Vector3 LocalToWorldVector(Vector3 dir)
		{
			return rotation * dir;
		}
	}
}

