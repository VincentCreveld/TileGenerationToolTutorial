using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSegmentSubmesh))]
public class RoadSegmentSubmeshEditor : Editor
{
	RoadSegmentSubmesh road = null;
	public override void OnInspectorGUI()
	{
		road = target as RoadSegmentSubmesh;
		if(GUILayout.Button("Remove submesh"))
		{
			Undo.RecordObject(road, "Removed submesh");
			road.OnDestroyThis();
		}

		base.OnInspectorGUI();
	}
}
