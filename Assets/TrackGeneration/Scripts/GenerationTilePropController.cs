using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierTool;

public class GenerationTilePropController : MonoBehaviour
{
	public List<GenerationProp> DecorativeGenerationProps;
	public List<GenerationProp> HazardGenerationProps;
	public List<GenerationProp> RewardGenerationProps;

	public BezierAccesser spline;

	[SerializeField] private Transform propsParentObj = null;

	[ContextMenu("Remove props")]
	public void RemoveProps()
	{
		foreach(GenerationProp gp in DecorativeGenerationProps)
		{
			gp.RemoveProp();
		}
	}

	[ContextMenu("Place props")]
	public void PlaceAllPropsOnLocals(GenerationTileController gtc, bool placeHazards, bool placeRewards)
	{
		if(propsParentObj == null)
		{
			propsParentObj = new GameObject().transform;
			propsParentObj.parent = transform;
		}
		if(DecorativeGenerationProps == null)
			DecorativeGenerationProps = new List<GenerationProp>();
		foreach(GenerationProp gp in DecorativeGenerationProps)
		{
			if(gp.randomisePlacement)
			{
				gp.RandomisePos(Vector3.one);// propsParentObj.lossyScale);
			}
			OrientedPoint p = spline.GetCurve().GetOrientedPoint(gp.GetPropLocalPos().positionOnSpline);
			gp.localPropPos.localOffset.x *= (gtc.road.GetScaleAtPoint(gp.localPropPos.positionOnSpline)) * gp.transform.lossyScale.x;
			float angleOffset = Vector3.SignedAngle(Vector3.right, p.right, p.up);

			Vector3 rotatedOffset = Quaternion.AngleAxis(angleOffset, p.up) * gp.GetPropLocalPos().localOffset;
			gp.SpawnProp(propsParentObj, new Vector3(1f * transform.lossyScale.x, 1f * transform.lossyScale.y, 1f * transform.lossyScale.z), p.position + rotatedOffset, p.forward);
		}
		if(placeHazards)
		{
			if(HazardGenerationProps == null)
				HazardGenerationProps = new List<GenerationProp>();
			foreach(GenerationProp gp in HazardGenerationProps)
			{
				if(gp.randomisePlacement)
				{
					gp.RandomisePos(propsParentObj.lossyScale);
				}
				OrientedPoint p = spline.GetCurve().GetOrientedPoint(gp.GetPropLocalPos().positionOnSpline);
				gp.localPropPos.localOffset.x *= (gtc.road.GetScaleAtPoint(gp.localPropPos.positionOnSpline)) * gp.transform.lossyScale.x;
				float angleOffset = Vector3.SignedAngle(Vector3.right, p.right, p.up);

				Vector3 rotatedOffset = Quaternion.AngleAxis(angleOffset, p.up) * gp.GetPropLocalPos().localOffset;
				gp.SpawnProp(propsParentObj, new Vector3(1f * transform.lossyScale.x, 1f * transform.lossyScale.y, 1f * transform.lossyScale.z), p.position + rotatedOffset, p.forward);
			}
		}
		if(placeRewards)
		{
			if(RewardGenerationProps == null)
				RewardGenerationProps = new List<GenerationProp>();
			foreach(GenerationProp gp in RewardGenerationProps)
			{
				if(gp.randomisePlacement)
				{
					gp.RandomisePos(propsParentObj.lossyScale);
				}
				OrientedPoint p = spline.GetCurve().GetOrientedPoint(gp.GetPropLocalPos().positionOnSpline);
				gp.localPropPos.localOffset.x *= (gtc.road.GetScaleAtPoint(gp.localPropPos.positionOnSpline)) * gp.transform.lossyScale.x;
				float angleOffset = Vector3.SignedAngle(Vector3.right, p.right, p.up);

				Vector3 rotatedOffset = Quaternion.AngleAxis(angleOffset, p.up) * gp.GetPropLocalPos().localOffset;
				gp.SpawnProp(propsParentObj, new Vector3(1f * transform.lossyScale.x, 1f * transform.lossyScale.y, 1f * transform.lossyScale.z), p.position + rotatedOffset, p.forward);
			}
		}
	}
}

public enum GenerationPropTypes
{
	Decoration,
	Hazard,
	Reward
}
