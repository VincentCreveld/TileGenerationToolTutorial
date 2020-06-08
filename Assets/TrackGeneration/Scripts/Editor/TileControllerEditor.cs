using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GenerationTileController))]
public class TileControllerEditor : Editor
{
	GenerationTileController tile = null;
	public override void OnInspectorGUI()
	{
		tile = target as GenerationTileController;

		if(GUILayout.Button("Generate Tile"))
		{
			Undo.RecordObject(tile, "Generated tile " + tile.transform.name);
			tile.CompletelyRegenerateTile();
			EditorUtility.SetDirty(tile);
		}
		base.OnInspectorGUI();
	}
}
