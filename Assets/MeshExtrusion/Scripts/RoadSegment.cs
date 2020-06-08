using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierTool;
using System;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshCollider)), RequireComponent(typeof(MeshRenderer))]
public class RoadSegment : MonoBehaviour
{
	public MeshCrossection shape2D = null;
	public Material materialToAssign = null;
	public bool scaleSubmeshes = true;
	public RoadSegmentValues values = null;
	[SerializeField]
	private List<RoadSegmentSubmesh> subMeshes = null;

	// Editor script related variables
	[HideInInspector] public bool currentlyExpanded = false;
	[HideInInspector] public MeshCrossection newShape = null;
	[HideInInspector] public Material newMaterial = null;

	private void Update()
	{
		if(values.generateMeshEveryFrame)
			GenerateMesh();
	}

	public void GenerateMeshAndInit(bool useScaleAsOffset = false, MeshCrossection parentC = null)
	{
		if(subMeshes != null)
			for(int i = 0; i < subMeshes.Count; i++)
			{
				subMeshes[i].GenerateMeshAndInit(!scaleSubmeshes, shape2D);
			}

		values.mesh = new Mesh();
		values.mesh.name = "GeneratedMesh";
		GetComponent<MeshFilter>().sharedMesh = values.mesh;
		GetComponent<MeshCollider>().sharedMesh = values.mesh;

		SetMeshMaterial();

		GenerateMesh(useScaleAsOffset, parentC);
	}

	private void SetMeshMaterial()
	{
		if(materialToAssign != null)
			GetComponent<MeshRenderer>().sharedMaterial = materialToAssign;
	}

	private void GenerateMesh(bool useScaleAsOffset = false, MeshCrossection parentC = null)
	{
		values.mesh.Clear();

		float uSpan = shape2D.GetLinesLength();
		List<Vector3> verts = new List<Vector3>();
		List<Vector3> normals = new List<Vector3>();
		List<Vector2> uvs = new List<Vector2>();

		for(int currentEdgeRing = 0; currentEdgeRing < values.edgeRingCount; currentEdgeRing++)
		{
			float splineLength = values.spline.GetSplineLength(10f);
			float t = currentEdgeRing / (values.edgeRingCount - 1f);

			if(values.scaleToEnd)
				values.scale = Mathf.Lerp(values.scaleAtStart, values.scaleAtEnd, t);
			else if(values.scaleByCurve)
				values.scale = values.scaleCurve.Evaluate(t);
			else
				values.scale = values.scaleAtStart;

			values.scaleToEnd = (values.scaleAtEnd != values.scaleAtStart);

			float multiplier = (values.useMultiplier) ? values.scale : 1f;
			OrientedPoint op = values.spline.GetOrientedPoint(t);

			Vector3 innerNeg = new Vector3();
			Vector3 innerPos = new Vector3();

			if(scaleSubmeshes && parentC != null)
			{
				int inNeg = -1;
				int inPos = -1;
				float inP = 100000f;
				float inN = -100000f;
				for(int u = 0; u < parentC.VertexCount; u++)
				{
					float xpos = parentC.vertices[u].point.x;
					if(xpos >= inN)
					{
						inN = xpos;
						inNeg = u;
					}
					if(xpos <= inP)
					{
						inP = xpos;
						inPos = u;
					}
				}

				innerNeg = parentC.vertices[inNeg].point * transform.lossyScale;
				innerPos = parentC.vertices[inPos].point * transform.lossyScale;
			}

			for(int i = 0; i < shape2D.VertexCount; i++)
			{
				Vector3 p = (Vector3)(shape2D.vertices[i].point);

				if(!useScaleAsOffset)
				{
					p.x = ((values.scaleX) ? p.x * values.scale : p.x) * transform.lossyScale.x;
					p.y = ((values.scaleY) ? p.y * values.scale : p.y) * transform.lossyScale.y;
					p.z = ((values.scaleZ) ? p.z * values.scale : p.z) * transform.lossyScale.z;
				}
				else
				{
					p.x = (p.x == 0) ? p.x : ((p.x < 0) ? p.x - values.scale : p.x + values.scale) * transform.lossyScale.x;
					p.y = ((values.scaleY) ? p.y * values.scale : p.y) * transform.lossyScale.y;
					p.z = ((values.scaleZ) ? p.z * values.scale : p.z) * transform.lossyScale.z;
					if(p.x < 0)
					{
						float o = (innerNeg * values.scale).x;
						p.x = (p.x - o) + innerNeg.x + (values.scale * transform.lossyScale.x);
					}
					else if(p.x > 0)
					{
						float o = (innerPos * values.scale).x;
						p.x = (p.x - o) + innerPos.x - (values.scale * transform.lossyScale.x);
					}
				}
				verts.Add(transform.InverseTransformPoint(op.LocalToWorldPosition(p * multiplier) + values.localOffset));

				normals.Add(op.LocalToWorldVector((Vector3)(shape2D.vertices[i].normal)));

				uvs.Add(
					new Vector2(values.uvOffset.x + (currentEdgeRing / (float)values.edgeRingCount - 1) *
					values.uMultiplier, values.uvOffset.y + (i / (float)shape2D.VertexCount) *
					splineLength / uSpan * (1 / values.uvRepeatScale)
					));
			}
		}

		List<int> triangleIndices = new List<int>();
		for(int r = 0; r < values.edgeRingCount - 1; r++)
		{
			int rootIndex = r * shape2D.VertexCount;
			int rootIndexNext = (r + 1) * shape2D.VertexCount;

			for(int l = 0; l < shape2D.LineCount; l += 2)
			{
				int lineIndexA = shape2D.lines[l];
				int lineIndexB = shape2D.lines[l + 1];

				int currentA = rootIndex + lineIndexA;
				int currentB = rootIndex + lineIndexB;

				int nextA = rootIndexNext + lineIndexA;
				int nextB = rootIndexNext + lineIndexB;

				// tri 1
				triangleIndices.Add(currentA);
				triangleIndices.Add(nextA);
				triangleIndices.Add(nextB);
				// tri 2
				triangleIndices.Add(currentA);
				triangleIndices.Add(nextB);
				triangleIndices.Add(currentB);
			}
		}

		values.mesh.SetVertices(verts);
		values.mesh.SetNormals(normals);
		values.mesh.SetTriangles(triangleIndices, 0);
		values.mesh.RecalculateBounds();
		values.mesh.RecalculateNormals();
		values.mesh.RecalculateTangents();
		values.mesh.uv = uvs.ToArray();
		GetComponent<MeshCollider>().sharedMesh = values.mesh;
		gameObject.layer = values.layerToAssign;
	}

	public float GetScaleAtPoint(float t)
	{
		float scale = 0f;

		if(values.scaleToEnd)
			scale = Mathf.Lerp(values.scaleAtStart, values.scaleAtEnd, t);
		else if(values.scaleByCurve)
			scale = values.scaleCurve.Evaluate(t);
		else
			scale = values.scaleAtStart;

		return scale;
	}

	public void OnDrawGizmos()
	{
		if(!values.drawGizmos)
			return;

		OrientedPoint p = values.spline.GetOrientedPoint(values.t);

		for(int i = 0; i < shape2D.lines.Length; i += 2)
		{
			int v0 = shape2D.lines[i];
			int v1 = shape2D.lines[i + 1];
			Vector3 pos0 = p.LocalToWorldPosition((Vector3)shape2D.vertices[v0].point * values.scale + values.localOffset);
			Vector3 pos1 = p.LocalToWorldPosition((Vector3)shape2D.vertices[v1].point * values.scale + values.localOffset);
			Gizmos.DrawLine(pos0, pos1);
		}
	}

	public void AddNewSubmesh(MeshCrossection m, Material mat)
	{
		newShape = m;
		newMaterial = mat;
		CreateNewSubmesh();
	}

	public void CreateNewSubmesh()
	{
		GameObject go = new GameObject();
		RoadSegmentSubmesh submesh = go.AddComponent<RoadSegmentSubmesh>();
		submesh.Init(this, values, newShape, newMaterial);
		go.transform.parent = transform;
		go.transform.localRotation = Quaternion.identity;
		go.transform.localPosition = Vector3.zero;
		newShape = null;
		newMaterial = null;

		if(subMeshes == null)
			subMeshes = new List<RoadSegmentSubmesh>();

		subMeshes.Add(submesh);
	}

	public void RemoveEntry(RoadSegmentSubmesh submesh)
	{
		subMeshes.Remove(submesh);
	}

	public void SubscribeSubmesh(RoadSegmentSubmesh submesh)
	{
		if(!subMeshes.Contains(submesh))
			subMeshes.Add(submesh);
	}

	public virtual void OnValidate()
	{
		if(subMeshes != null)
		{
			for(int i = subMeshes.Count - 1; i > -1; i--)
			{
				if(subMeshes[i] == null)
					subMeshes.RemoveAt(i);
			}
		}

		if(shape2D != null)
			SetMeshMaterial();
		if(subMeshes != null)
		{
			foreach(RoadSegmentSubmesh s in subMeshes)
			{
				s.SetValuesToParent(values);
				s.OnValidate();
			}
		}
	}
}

[Serializable]
public class RoadSegmentValues
{
	public SplineBezierCurve spline = null;
	
	[Range(0, 1)]
	public float t = 0f;
	public float scale = 1f;
	public float scaleAtStart = 1f;
	public float scaleAtEnd = 2f;
	public Vector3 localOffset = new Vector3();
	public Vector2 uvOffset = new Vector2();
	[Range(1f, 100f)]
	public float uvRepeatScale = 1f;

	public bool scaleToEnd = false;
	public bool scaleByCurve = false;
	public bool useMultiplier = false;

	public AnimationCurve scaleCurve = null;

	[SerializeField, Range(2, 128)]
	public int edgeRingCount = 8;

	public Mesh mesh;
	public float uMultiplier = 5f;

	public bool drawGizmos = false;
	public bool generateMeshEveryFrame = false;

	public bool scaleX = true;
	public bool scaleY = true;
	public bool scaleZ = true;

	public int layerToAssign;
}