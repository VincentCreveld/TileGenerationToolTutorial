using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierTool;

[RequireComponent(typeof(GenerationTile)), System.Serializable]
public class GenerationTileController : MonoBehaviour
{
	private GenerationTile tileId = null;
	private IGenerateElement[] generationElements = null;

	[HideInInspector] public int TilePlacerMathingId = -1;

	public Transform entryPoint = null;
	public Transform exitPoint = null;

	public Vector2 tileDimensions = new Vector2(50, 50);

	public BezierAccesser progressCurve = null;
	public RoadSegment road = null;

	public float rampAngle = 0f;

	public GenerationTilePropController propController = null;

	public bool placeHazards = false;
	public bool placeRewards = false;

	[SerializeField] private GenerationRuntimeTrigger endTrigger = null;

	public void SetupEndTrigger(System.Action ac, bool singleFire = true, bool isEnter = true, bool isExit = false)
	{
		if(endTrigger == null)
		{
			GenerationRuntimeTrigger endTriggerInstance = new GameObject().AddComponent<GenerationRuntimeTrigger>();
			endTrigger = endTriggerInstance;
			endTrigger.transform.parent = transform;
		}
		endTrigger.transform.position = progressCurve.GetCurve().GetPoint(1f);
		endTrigger.transform.forward = progressCurve.GetCurve().GetOrientedPoint(1f).forward;
		endTrigger.Init(ac, singleFire, isEnter, isExit);
		endTrigger.SetBounds(new Vector3(8f * transform.lossyScale.x, 10f, 3f));
	}

	public void CompletelyRegenerateTile()
	{
		SetTilePointsAndAnchors();
		AlignAnchors();
		SetMesh();
		SetProps();
	}

	public void AlignAnchors()
	{
		road.values.spline.AlignAnchors();
	}

	public void SetMesh()
	{
		road.GenerateMeshAndInit();
	}

	public void SetProps()
	{
		if(propController != null)
			propController.PlaceAllPropsOnLocals(this, placeHazards, placeRewards);
	}

	public void Init(Vector3 pos, Vector3 rotationDirection)
	{
		generationElements = GetComponentsInChildren<IGenerateElement>();
		foreach(IGenerateElement iGen in generationElements)
		{
			iGen.SetUpComponent();
		}
		SetPosition(pos);

		entryPoint.position = progressCurve.GetCurve().GetPoint(0);
		entryPoint.rotation = progressCurve.GetCurve().GetOrientedPoint(0).rotation;

		exitPoint.position = progressCurve.GetCurve().GetPoint(1);
		exitPoint.rotation = progressCurve.GetCurve().GetOrientedPoint(1).rotation;
	}

	// Places the tile with respect to the pivot offset.
	public void SetPosition(Vector3 pos)
	{
		Vector3 offset = entryPoint.position - transform.position;
		transform.position = pos - offset;
	}

	// Sets rotation
	public void SetTileRotation(Quaternion faceDirection)
	{
		transform.rotation = faceDirection;
	}

	public Vector3 GetConnectionPoint()
	{
		return exitPoint.position;
	}

	// base this off of the difference in entry/exit between the pieces.
	public Vector3 GetConnectionDirection(CardinalDirections dir)
	{
		return exitPoint.forward;
	}

	public Vector3 GetExitDir()
	{
		return -exitPoint.forward;
	}

	public Vector3 GetEntryDir()
	{
		return entryPoint.forward;
	}

	public CardinalDirections GetEntryDirection()
	{
		return tileId.GetConnectionId().GetEntry().dir;
	}

	public CardinalDirections GetExitDirection()
	{
		return tileId.GetConnectionId().GetExit().dir;
	}

	public void SetEntryAndExitPos()
	{
		entryPoint.transform.position = progressCurve.GetCurve().GetPoint(0f);
		exitPoint.transform.position = progressCurve.GetCurve().GetPoint(1f);
	}

	public void SetTilePointsAndAnchors()
	{
		Quaternion curRot = transform.rotation;
		transform.rotation = Quaternion.identity;
		Vector3 entryDir = GetDirectionFromIdCardinalEntry();
		Vector3 exitDir = GetDirectionFromIdCardinalExit();

		int idSize = tileId.GetConnectionId().GetEntry().id.connectionID.Length;

		ConnectionVariations[] entryId = tileId.GetConnectionId().GetEntry().id.connectionID;
		ConnectionVariations[] exitId = tileId.GetConnectionId().GetExit().id.connectionID;

		// needs offset of half of segment size
		float segmentSizeX = tileDimensions.x / (float)idSize;
		float segmentSizeY = tileDimensions.y / (float)idSize;

		// TO DO: TODO: Make this function smae as identification. Check for "path purity"
		float avgPosEntry = 0f;
		int totalPaths = 0;
		for(int i = 0; i < idSize; i++)
		{
			if(entryId[i] == ConnectionVariations.Path)
			{
				avgPosEntry += i;
				totalPaths++;
			}
		}
		if(totalPaths == 0)
			return;
		avgPosEntry /= totalPaths;
		road.values.scaleAtStart = totalPaths;

		float avgPosExit = 0f;
		totalPaths = 0;
		for(int i = 0; i < idSize; i++)
		{
			if(exitId[i] == ConnectionVariations.Path)
			{
				avgPosExit += i;
				totalPaths++;
			}
		}
		avgPosExit /= totalPaths;

		road.values.scaleAtEnd = totalPaths;

		float tEn = Mathf.InverseLerp(0, (float)idSize - 1, avgPosEntry);
		float tEx = Mathf.InverseLerp(0, (float)idSize - 1, avgPosExit);

		float xPosEntry = Mathf.Lerp(0, tileDimensions.x, tEn);
		float yPosEntry = Mathf.Lerp(0, tileDimensions.y, tEn);

		float xPosExit = Mathf.Lerp(0, tileDimensions.x, tEx);
		float yPosExit = Mathf.Lerp(0, tileDimensions.y, tEx);

		Vector3 offsetEntry = GetDirectionFromIdCardinalEntry();
		Vector3 offsetExit = GetDirectionFromIdCardinalExit();

		switch(GetEntryDirection())
		{
			case CardinalDirections.North:
				entryPoint.localPosition = -offsetEntry + new Vector3(xPosEntry - (tileDimensions.x * 0.5f), 0, 0);
				break;
			case CardinalDirections.East:
				entryPoint.localPosition = -offsetEntry + new Vector3(0, 0, yPosEntry - (tileDimensions.y * 0.5f));
				break;
			case CardinalDirections.South:
				entryPoint.localPosition = -offsetEntry + new Vector3(xPosEntry - (tileDimensions.x * 0.5f), 0, 0);
				break;
			case CardinalDirections.West:
				entryPoint.localPosition = -offsetEntry + new Vector3(0, 0, yPosEntry - (tileDimensions.y * 0.5f));
				break;
			default:
				break;
		}

		entryPoint.forward = offsetEntry;

		switch(GetExitDirection())
		{
			case CardinalDirections.North:
				exitPoint.localPosition = -offsetExit + new Vector3(xPosExit - (tileDimensions.x * 0.5f), 0, 0);
				break;
			case CardinalDirections.East:
				exitPoint.localPosition = -offsetExit + new Vector3(0, 0, yPosExit - (tileDimensions.y * 0.5f));
				break;
			case CardinalDirections.South:
				exitPoint.localPosition = -offsetExit + new Vector3(xPosExit - (tileDimensions.x * 0.5f), 0, 0);
				break;
			case CardinalDirections.West:
				exitPoint.localPosition = -offsetExit + new Vector3(0, 0, yPosExit - (tileDimensions.y * 0.5f));
				break;
			default:
				break;
		}

		exitPoint.forward = offsetExit;

		GetOffSetBasedOnAngle();

		progressCurve.GetCurve().MoveFirstPointAndAnchor(entryPoint.position + Vector3.up, entryPoint.forward, tileId.GetConnectionId().GetEntry().dir, tileDimensions);
		progressCurve.GetCurve().MoveLastPointAndAnchor(exitPoint.position + Vector3.up, exitPoint.forward, tileId.GetConnectionId().GetExit().dir, tileDimensions);
		transform.rotation = curRot;
	}
	
	public void GetOffSetBasedOnAngle()
	{
		Vector3 a = entryPoint.localPosition;
		a.y = exitPoint.localPosition.y;
		Vector3 c = exitPoint.localPosition;

		float ac = Vector3.Distance(a, c);

		float ae = Mathf.Tan(rampAngle * Mathf.Deg2Rad) * ac;

		entryPoint.localPosition = new Vector3(entryPoint.localPosition.x, exitPoint.localPosition.y + ae, entryPoint.localPosition.z);
	}

	public void OnDrawGizmos()
	{
		Color c = Color.red;
		c.a = 0.3f;
		Gizmos.color = c;
		Vector3 gizmoOrigin = Vector3.zero;// transform.position;

		Matrix4x4 rotationMatrix = transform.localToWorldMatrix;
		Gizmos.matrix = rotationMatrix;

		Vector3 v3 = new Vector3(tileDimensions.x, 0, tileDimensions.y);
		Gizmos.DrawCube(gizmoOrigin, v3);
	}

	public Vector3 GetDirectionFromIdCardinalEntry()
	{
		Vector3 returnVector = Vector3.zero;
		CardinalDirections dir = tileId.GetConnectionId().GetEntry().dir;
		switch(dir)
		{
			case CardinalDirections.North:
				returnVector = -Vector3.forward;
				break;
			case CardinalDirections.East:
				returnVector = -Vector3.right;
				break;
			case CardinalDirections.South:
				returnVector = Vector3.forward;
				break;
			case CardinalDirections.West:
				returnVector = Vector3.right;
				break;
		}
		if((int)dir == 0)
			return returnVector;

		if((int)dir == 1 || (int)dir == 3)
			returnVector *= (tileDimensions.y * 0.5f);
		if((int)dir == 2 || (int)dir == 4)
			returnVector *= (tileDimensions.x * 0.5f);

		return returnVector;
	}

	public Vector3 GetDirectionFromIdCardinalExit()
	{
		Vector3 returnVector = Vector3.zero;
		CardinalDirections dir = tileId.GetConnectionId().GetExit().dir;
		switch(dir)
		{
			case CardinalDirections.North:
				returnVector = -Vector3.forward;
				break;
			case CardinalDirections.East:
				returnVector = -Vector3.right;
				break;
			case CardinalDirections.South:
				returnVector = Vector3.forward;
				break;
			case CardinalDirections.West:
				returnVector = Vector3.right;
				break;
		}
		if((int)dir == 0)
			return returnVector;

		if((int)dir == 1 || (int)dir == 3)
			returnVector *= (tileDimensions.y * 0.5f);
		if((int)dir == 2 || (int)dir == 4)
			returnVector *= (tileDimensions.x * 0.5f);

		return returnVector;
	}

	public void OnValidate()
	{
		tileId = GetComponent<GenerationTile>();
	}

	public GenerationTile GetTileId()
	{
		if(tileId == null)
			tileId = GetComponent<GenerationTile>();
		return tileId;
	}
}
