using UnityEngine;
using UnityEditor;
using System.Security.Cryptography;
using System.Runtime.InteropServices.ComTypes;

namespace BezierTool
{
	[CustomEditor(typeof(SplineBezierCurve))]
	public class SplineBezierCurveEditor : Editor
	{
		private const int LINE_STEPS = 5;
		private const float VELOCITY_DIRECTION_SCALE = 0.5f;
		private const float HANDLE_DISPLAY_SIZE = 0.04f;
		private const float HANDLE_SELECT_SIZE = 0.06f;

		public static Color[] BezierControlPointModeColors =
			{
				Color.white,
				Color.yellow,
				Color.blue
			};

		private SplineBezierCurve bezierSpline;
		private Transform bezierCurveTransform;
		private Quaternion bezierCurveRotation;
		private int selectedIndex = -1;
		private int selectedIndexRot = -1;


		public override void OnInspectorGUI()
		{
			bezierSpline = target as SplineBezierCurve;

			if(selectedIndex >= 0 && selectedIndex < bezierSpline.ControlPointCount)
			{
				DrawSelectedPointInspector();
			}
			else if(selectedIndexRot >= 0 && selectedIndexRot <= bezierSpline.CurveCount)
			{
				DrawSelectedRotPointInspector();
			}

			if(GUILayout.Button("Add Curve"))
			{
				Undo.RecordObject(bezierSpline, "Add curve to spline");
				bezierSpline.AddCurve();
				EditorUtility.SetDirty(bezierSpline);
			}

			if(GUILayout.Button("Remove Curve"))
			{
				Undo.RecordObject(bezierSpline, "Remove curve to spline");
				bezierSpline.RemoveCurve();
				EditorUtility.SetDirty(bezierSpline);
			}

			if(GUILayout.Button("Turn Looping " + ((bezierSpline.IsLoop) ? "Off" : "On")))
			{
				Undo.RecordObject(bezierSpline, "Toggled looping");
				bezierSpline.IsLoop = !bezierSpline.IsLoop;

				if(bezierSpline.IsLoop == false)
				{
					bezierSpline.RemoveLoop();
				}

				EditorUtility.SetDirty(bezierSpline);
			}

			if(GUILayout.Button("Smooth out tangent points"))
			{
				Undo.RecordObject(bezierSpline, "Smoothed out tangents on curve.");
				bezierSpline.AlignAnchors();
				EditorUtility.SetDirty(bezierSpline);
			}

			if(GUILayout.Button("Turn Velocity Lines " + ((bezierSpline.DrawVelocity) ? "Off" : "On")))
			{
				Undo.RecordObject(bezierSpline, "Toggled velocity lines");
				bezierSpline.DrawVelocity = !bezierSpline.DrawVelocity;
				EditorUtility.SetDirty(bezierSpline);
			}

			if(GUILayout.Button("Turn Gizmos " + ((bezierSpline.DrawGizmos) ? "Off" : "On")))
			{
				Undo.RecordObject(bezierSpline, "Toggled gizmos");
				bezierSpline.DrawGizmos = !bezierSpline.DrawGizmos;
				EditorUtility.SetDirty(bezierSpline);
			}
		}

		private void DrawSelectedPointInspector()
		{
			GUILayout.Label("Selected Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", bezierSpline.GetControlPoint(selectedIndex));
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(bezierSpline, "Move " + bezierSpline.name + " point " + selectedIndex);
				EditorUtility.SetDirty(bezierSpline);
				bezierSpline.SetControlPoint(selectedIndex, point);
			}
			EditorGUI.BeginChangeCheck();
			BezierControlPointMode mode = (BezierControlPointMode)EditorGUILayout.EnumPopup("Mode", bezierSpline.GetControlPointMode(selectedIndex));
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(bezierSpline, "Changed " + bezierSpline.name + " restraint " + selectedIndex);
				bezierSpline.SetControlPointMode(selectedIndex, mode);
				EditorUtility.SetDirty(bezierSpline);
			}
		}

		private void DrawSelectedRotPointInspector()
		{
			GUILayout.Label("Selected Rotation Point");
			EditorGUI.BeginChangeCheck();
			Vector3 point = EditorGUILayout.Vector3Field("Position", bezierSpline.GetRotationPoint(selectedIndexRot));
			if(EditorGUI.EndChangeCheck())
			{
				Undo.RecordObject(bezierSpline, "Move " + bezierSpline.name + " rotation point " + selectedIndexRot);
				bezierSpline.SetRotPoint(selectedIndexRot, point);
				EditorUtility.SetDirty(bezierSpline);
			}
			if(GUILayout.Button("Reset rotation point"))
			{
				Undo.RecordObject(bezierSpline, "Reset rotation point");
				bezierSpline.SetRotPoint(selectedIndexRot, bezierSpline.GetCurvePointByIndex(selectedIndexRot) + (Vector3.up * 5));
				EditorUtility.SetDirty(bezierSpline);
			}
		}

		private void OnSceneGUI()
		{
			bezierSpline = target as SplineBezierCurve;
			bezierCurveTransform = bezierSpline.transform;
			bezierCurveRotation = bezierCurveTransform.rotation;

			Vector3 p0 = ShowPoint(0);
			Vector3 rp0 = ShowPointRot(0);
			for(int i = 1; i < bezierSpline.ControlPointCount; i += 3)
			{
				Vector3 p1 = ShowPoint(i);
				Vector3 p2 = ShowPoint(i + 1);
				Vector3 p3 = ShowPoint(i + 2);
				Vector3 rp1 = ShowPointRot((i + 2) / 3);

				Handles.color = Color.red;
				Handles.DrawLine(p0, rp0);
				Handles.DrawLine(p3, rp1);

				Handles.color = Color.gray;
				Handles.DrawLine(p0, p1);
				Handles.DrawLine(p2, p3);

				Handles.DrawBezier(p0, p3, p1, p2, Color.white, null, 2f);
				p0 = p3;
				rp0 = rp1;
			}

			if(bezierSpline.DrawVelocity)
				ShowDirections();
		}

		private Vector3 ShowPoint(int index)
		{
			Vector3 point = bezierCurveTransform.TransformPoint(bezierSpline.GetControlPoint(index));

			float handleSize = HandleUtility.GetHandleSize(point);

			if(index == 0)
				handleSize *= 2f;

			Handles.color = BezierControlPointModeColors[(int)bezierSpline.GetControlPointMode(index)];
			if(Handles.Button(point, bezierCurveRotation, handleSize * HANDLE_DISPLAY_SIZE, handleSize * HANDLE_SELECT_SIZE, Handles.DotHandleCap))
			{
				selectedIndex = index;
				selectedIndexRot = -1;
				Repaint();
			}

			if(selectedIndex == index)
			{
				EditorGUI.BeginChangeCheck();

				Undo.RecordObject(bezierSpline, "Move " + bezierSpline.name + " point " + index);
				EditorUtility.SetDirty(bezierSpline);

				point = Handles.DoPositionHandle(point, bezierCurveRotation);

				if(EditorGUI.EndChangeCheck())
					bezierSpline.SetControlPoint(index, bezierCurveTransform.InverseTransformPoint(point));
			}
			return point;
		}

		private Vector3 ShowPointRot(int index)
		{
			Vector3 point = bezierCurveTransform.TransformPoint(bezierSpline.GetRotationPoint(index));

			float handleSize = HandleUtility.GetHandleSize(point);

			Handles.color = Color.red;

			if(Handles.Button(point, bezierCurveRotation, handleSize * HANDLE_DISPLAY_SIZE, handleSize * HANDLE_SELECT_SIZE, Handles.DotHandleCap))
			{
				selectedIndexRot = index;
				selectedIndex = -1;
				Repaint();
			}

			if(selectedIndexRot == index)
			{
				EditorGUI.BeginChangeCheck();

				Undo.RecordObject(bezierSpline, "Move " + bezierSpline.name + " rot point " + index);
				EditorUtility.SetDirty(bezierSpline);

				point = Handles.DoPositionHandle(point, bezierCurveRotation);

				if(EditorGUI.EndChangeCheck())
				{
					Vector3 pos = point - bezierCurveTransform.TransformPoint(bezierSpline.GetCurvePointByIndex(selectedIndexRot));
					point = bezierCurveTransform.TransformPoint(bezierSpline.GetCurvePointByIndex(selectedIndexRot)) + (pos.normalized * 5);
					bezierSpline.SetRotPoint(index, bezierCurveTransform.InverseTransformPoint(point));
				}
			}
			return point;
		}

		private void ShowDirections()
		{
			Vector3 point = bezierSpline.GetPoint(0);
			Handles.DrawLine(point, point + bezierSpline.GetVelocityDirection(0f) * VELOCITY_DIRECTION_SCALE);
			int steps = LINE_STEPS * bezierSpline.ControlPointCount;
			for(int i = 0; i <= steps; i++)
			{
				Handles.color = Color.green;
				point = bezierSpline.GetPoint(i / (float)steps);
				Handles.DrawLine(point, point + bezierSpline.GetVelocityDirection(i / (float)steps) * VELOCITY_DIRECTION_SCALE);

			}
		}
	}
}