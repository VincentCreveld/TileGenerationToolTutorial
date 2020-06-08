using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BezierTool;

public class GenerationController : MonoBehaviour
{
	[SerializeField] private TilePlacer tilePlacer = null;
	[SerializeField] private CurveCombiner curveTracker = null;
	[Range(0f,1f)]
	public float progress = 0f;

	public bool IsFinite = false;
	[Range(2, 50)]
	public int trackSize = 5;

	public Transform vehicle = null;
	public Transform tracker = null;

	private bool firstCall = true;

	private void Start()
	{
		if(tilePlacer == null)
			tilePlacer = GetComponentInChildren<TilePlacer>();

		tilePlacer.roadSize = trackSize;

		if(IsFinite)
		{
			tilePlacer.GenerateFiniteTrack(tilePlacer.roadSize);
		}
		else
		{
			tilePlacer.SetupInfiniteGeneration(ScrollOverNextTiles, true, true, false);
		}

		for(int i = 0; i < tilePlacer.spawnedTiles.Length; i++)
		{
			curveTracker.AddCurve(ref progress, tilePlacer.spawnedTiles[i].progressCurve);
		}

		tilePlacer.isSetup = true;

		vehicle.transform.position = curveTracker.GetPoint(0f);
		vehicle.transform.forward = curveTracker.GetOrientedPoint(0f).forward;
		vehicle.transform.position += vehicle.transform.forward * 3f;
		vehicle.transform.position += vehicle.transform.up * 1f;
	}

	private void Update()
	{
		if(tilePlacer.isSetup && !IsFinite)
		{
			if(Input.GetKeyDown(KeyCode.Space))
			{
				curveTracker.AddCurve(ref progress, tilePlacer.PlaceTile().progressCurve);
			}
			if(Input.GetKeyDown(KeyCode.R))
			{
				curveTracker.RemoveFirstCurve(ref progress);
				tilePlacer.RemoveFirstTile();
			}
		}

		tracker.position = curveTracker.GetPoint(progress);
	}

	private void ScrollOverNextTiles()
	{
		curveTracker.AddCurve(ref progress, tilePlacer.PlaceTile().progressCurve);
		if(!firstCall)
		{
			curveTracker.RemoveFirstCurve(ref progress);
			tilePlacer.RemoveFirstTile();
		}
		firstCall = false;

		tilePlacer.InitLastEndTrigger(ScrollOverNextTiles, true, true, false);
	}

	public void OnValidate()
	{
		tilePlacer.roadSize = trackSize;
	}
}
