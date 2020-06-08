using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierTool;

[RequireComponent(typeof(MeshFilter)), RequireComponent(typeof(MeshCollider)), RequireComponent(typeof(MeshRenderer))]
public class RoadSegmentSubmesh : RoadSegment
{
	[SerializeField, HideInInspector]
	private RoadSegment parentScript = null;
	[SerializeField] public bool onlyUseScaleAsOffset = false;

	public void Init(RoadSegment p, RoadSegmentValues vals, MeshCrossection shape, Material mat)
	{
		this.parentScript = p;
		this.values = vals;
		this.materialToAssign = mat;
		this.shape2D = shape;

		GenerateMeshAndInit();
	}

	public override void OnValidate()
	{
		if(parentScript != null)
			parentScript.SubscribeSubmesh(this);
	}

	public void OnDestroyThis()
	{
		parentScript.RemoveEntry(this);
		DestroyImmediate(gameObject);
	}

	public void SetValuesToParent(RoadSegmentValues vals)
	{
		values = vals;
	}
}
