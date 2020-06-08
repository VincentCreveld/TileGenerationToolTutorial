using UnityEngine;
using System;
using UnityEditor;

[CreateAssetMenu]
public class MeshCrossection : ScriptableObject
{
	[Serializable]
    public class Vertex
	{
		public Vector2 point;
		public Vector2 normal;
		public float u;
	}
	public Vertex[] vertices;
	public int[] lines;

	public int VertexCount
	{
		get { return vertices.Length; }
	}
	public int LineCount
	{
		get { return lines.Length; }
	}

	public void SetNormals()
	{
		for(int i = 0; i < lines.Length - 1; i += 2)
		{
			Vector2 p0 = vertices[lines[i]].point;
			Vector2 p1 = vertices[lines[i+1]].point;

			Vector2 v = p0 - p1;
			Vector2 n = new Vector2(v.y, -v.x);

			vertices[lines[i]].normal = n.normalized;
			vertices[lines[i + 1]].normal = n.normalized;
		}
	}

	public float GetLinesLength()
	{
		float dist = 0f;
		for(int i = 0; i < LineCount; i+=2)
		{
			Vector2 a = vertices[lines[i]].point;
			Vector2 b = vertices[lines[i+1]].point;
			dist += (a - b).magnitude;
		}
		return dist;
	}
}
