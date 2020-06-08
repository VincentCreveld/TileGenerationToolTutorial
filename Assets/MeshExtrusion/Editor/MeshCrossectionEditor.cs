using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(MeshCrossection))]
public class MeshCrossectionEditor : Editor
{
	MeshCrossection mCross;
    public override void OnInspectorGUI()
	{
		mCross = target as MeshCrossection;

		if(GUILayout.Button("Set Normals"))
		{
			Undo.RecordObject(mCross, "SetNormals");
			mCross.SetNormals();
			EditorUtility.SetDirty(mCross);
		}
		base.OnInspectorGUI();
	}
}
