using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(RoadSegment))]
public class RoadSegmentEditor : Editor
{ 
	RoadSegment road = null;
	public override void OnInspectorGUI()
	{
		road = target as RoadSegment;
		if(GUILayout.Button("Generate mesh"))
		{
			Undo.RecordObject(road, "Generated mesh");
			road.GenerateMeshAndInit();
			EditorUtility.SetDirty(road);
		}
		if(GUILayout.Button("Add submesh") || road.currentlyExpanded)
		{
			road.currentlyExpanded = true;

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Mesh crossection desired: ");
			road.newShape = EditorGUILayout.ObjectField(road.newShape, typeof(MeshCrossection), false) as MeshCrossection;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			EditorGUILayout.LabelField("Material: ");
			road.newMaterial = EditorGUILayout.ObjectField(road.newMaterial, typeof(Material), false) as Material;
			EditorGUILayout.EndHorizontal();

			EditorGUILayout.BeginHorizontal();
			if(road.newShape == null)
			{
				EditorGUILayout.HelpBox("Still needs mesh shape.", MessageType.Warning);
			}
			else if(road.newMaterial == null)
			{
				EditorGUILayout.HelpBox("Still needs material.", MessageType.Warning);
			}
			else if(GUILayout.Button("Generate") && road.newMaterial != null && road.newShape != null)
			{
				Undo.RecordObject(road, "Added submesh");
				road.CreateNewSubmesh();
				EditorUtility.SetDirty(road);
				road.currentlyExpanded = false;
			}

			if(GUILayout.Button("Cancel"))
			{
				road.newShape = null;
				road.newMaterial = null;
				road.currentlyExpanded = false;
			}
			EditorGUILayout.EndHorizontal();
		}
		base.OnInspectorGUI();
	}
}
