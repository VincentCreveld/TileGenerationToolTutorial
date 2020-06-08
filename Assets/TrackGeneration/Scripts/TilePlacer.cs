using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class TilePlacer : MonoBehaviour
{
	public GameObject car = null;
	public Transform spawnPos;

	[Tooltip("Excludes start and end tiles")]
	[HideInInspector] public int roadSize = 10;
	public GenerationTileController startTile = null;
	public GenerationTileController endTile = null;
	[SerializeField] private GenerationTileController[] inputControllers = null;
	[SerializeField, HideInInspector]
	private Dictionary<int, GenerationTileController[]> connectionDictionary = new Dictionary<int, GenerationTileController[]>();

	public bool IsFinite = false;
	[HideInInspector] public bool isSetup = false;
	[SerializeField]
	public GenerationTileController[] spawnedTiles = null;

	private void SetupConnectionDictionary()
	{
		connectionDictionary = new Dictionary<int, GenerationTileController[]>();
		for(int i = 0; i < inputControllers.Length; i++)
		{
			inputControllers[i].TilePlacerMathingId = i;
			connectionDictionary.Add(i, GetAllConnectableTiles(inputControllers[i]));
		}
	}

	private GenerationTileController[] GetAllConnectableTiles(GenerationTileController input)
	{
		List<GenerationTileController> returnList = new List<GenerationTileController>();

		for(int i = 0; i < inputControllers.Length; i++)
		{
			GenerationTile t = input.GetTileId();
			if(t.CanConnect(inputControllers[i].GetTileId()))
				returnList.Add(inputControllers[i]);
		}

		return returnList.ToArray();
	}

	public void InitLastEndTrigger(Action ac, bool singleFire = true, bool isEnter = true, bool isExit = false)
	{
		spawnedTiles[spawnedTiles.Length - 1].SetupEndTrigger(ac, singleFire, isEnter, isExit);
	}

	public void SetupInfiniteGeneration(Action ac, bool singleFire = true, bool isEnter = true, bool isExit = false)
	{
		SetupConnectionDictionary();
		WipeSpawnedTiles();

		spawnedTiles = new GenerationTileController[2];

		SetupFirstTwoTiles();

		SetUpTwoPieces(spawnedTiles[0], spawnedTiles[1]);

		if(spawnedTiles[0].propController != null)
			spawnedTiles[0].propController.PlaceAllPropsOnLocals(spawnedTiles[0], false, false);
		if(spawnedTiles[1].propController != null)
			spawnedTiles[1].propController.PlaceAllPropsOnLocals(spawnedTiles[1], false, false);

		for(int i = 0; i < 2; i++)
		{
			PlaceTile();
		}

		for(int i = 2; i < spawnedTiles.Length; i++)
		{
			spawnedTiles[i].SetupEndTrigger(ac, singleFire, isEnter, isExit);
		}
	}

	private void SetupFirstTwoTiles()
	{
		spawnedTiles[0] = Instantiate(startTile.gameObject).GetComponent<GenerationTileController>();
		spawnedTiles[0].transform.parent = transform;
		spawnedTiles[0].transform.localScale = Vector3.one;
		spawnedTiles[0].SetEntryAndExitPos();

		GenerationTileController[] entryOptions = GetAllConnectableTiles(startTile);
		int randomIndex = UnityEngine.Random.Range(0, entryOptions.Length);
		spawnedTiles[1] = Instantiate(entryOptions[randomIndex].gameObject).GetComponent<GenerationTileController>();
		spawnedTiles[1].transform.parent = transform;
		spawnedTiles[1].transform.localScale = Vector3.one;
		spawnedTiles[1].SetEntryAndExitPos();

		
	}

	private GenerationTileController GetNextTile(GenerationTileController prevTile)
	{
		int idToMatch = prevTile.TilePlacerMathingId;
		int index = UnityEngine.Random.Range(0, connectionDictionary[idToMatch].Length);
		return connectionDictionary[idToMatch][index];
	}

	public GenerationTileController PlaceTile()
	{
		GenerationTileController gtc = GetNextTile(spawnedTiles[spawnedTiles.Length - 1]);

		Array.Resize(ref spawnedTiles, spawnedTiles.Length + 1);

		spawnedTiles[spawnedTiles.Length - 1] = Instantiate(gtc.gameObject).GetComponent<GenerationTileController>();
		spawnedTiles[spawnedTiles.Length - 1].transform.parent = transform;
		spawnedTiles[spawnedTiles.Length - 1].transform.localScale = Vector3.one;

		SetUpTwoPieces(spawnedTiles[spawnedTiles.Length - 2], spawnedTiles[spawnedTiles.Length - 1]);

		bool rdm0 = UnityEngine.Random.Range(0, 2) == 0;
		bool rdm1 = UnityEngine.Random.Range(0, 2) == 0;
		if(spawnedTiles[spawnedTiles.Length - 1].propController != null)
			spawnedTiles[spawnedTiles.Length - 1].propController.PlaceAllPropsOnLocals(gtc, rdm0, rdm1);

		return spawnedTiles[spawnedTiles.Length - 1];
	}

	private void WipeSpawnedTiles()
	{
		if(spawnedTiles != null)
		{
			for(int i = spawnedTiles.Length - 1; i >= 0; i--)
			{
				if(spawnedTiles[i] == null)
					continue;
				if(spawnedTiles[i].gameObject != null)
					DestroyImmediate(spawnedTiles[i].gameObject);
			}
			spawnedTiles = null;
		}
	}

	public void GenerateFiniteTrack(int roadLength)
	{
		SetupConnectionDictionary();

		WipeSpawnedTiles();

		spawnedTiles = new GenerationTileController[roadLength + 2];

		SetupFirstTwoTiles();

		spawnedTiles[spawnedTiles.Length - 1] = Instantiate(endTile.gameObject).GetComponent<GenerationTileController>();
		spawnedTiles[spawnedTiles.Length - 1].transform.parent = transform;
		spawnedTiles[spawnedTiles.Length - 1].transform.localScale = Vector3.one;
		for(int i = 2; i < roadLength + 1; i++)
		{
			GenerationTileController gtc = GetNextTile(spawnedTiles[i - 1]);
			spawnedTiles[i] = Instantiate(gtc.gameObject).GetComponent<GenerationTileController>();
			spawnedTiles[i].transform.parent = transform;
			spawnedTiles[i].transform.localScale = Vector3.one;
		}

		for(int i = 1; i < spawnedTiles.Length; i++)
		{
			GenerationTileController tile0 = spawnedTiles[i - 1];
			GenerationTileController tile1 = spawnedTiles[i];
			SetUpTwoPieces(tile0, tile1);
		}
		for(int i = 0; i < spawnedTiles.Length; i++)
		{
			bool rdm0 = UnityEngine.Random.Range(0, 2) == 0;
			bool rdm1 = UnityEngine.Random.Range(0, 2) == 0;
			if(spawnedTiles[i].propController != null)
				spawnedTiles[i].propController.PlaceAllPropsOnLocals(spawnedTiles[i], rdm0, rdm1);
			spawnedTiles[i].transform.parent = transform;
		}
	}

	private void SetUpTwoPieces(GenerationTileController tile0, GenerationTileController tile1)
	{
		float angle = Vector3.SignedAngle(tile1.GetEntryDir(), tile1.transform.forward, Vector3.up);
		Quaternion localRotTile1 = Quaternion.Euler(0, -angle, 0);
		Quaternion newRot = Quaternion.LookRotation(tile0.GetExitDir());
		newRot *= Quaternion.Inverse(localRotTile1);

		tile1.SetEntryAndExitPos();
		tile1.SetTileRotation(newRot);
		tile1.SetPosition(tile0.GetConnectionPoint());

		Vector3 a = tile0.progressCurve.GetCurve().GetLastAnchor(false);
		Vector3 b = tile0.progressCurve.GetCurve().GetLastPoint(false);
		Vector3 c = tile0.progressCurve.GetCurve().GetLastRotAnchor(false);

		float angl = tile0.transform.eulerAngles.y - tile1.transform.eulerAngles.y;

		tile1.progressCurve.GetCurve().MimicPreviousSplineSettings(a, c, b, tile0.progressCurve.transform.eulerAngles - new Vector3(0, tile0.transform.eulerAngles.y - angl, 0));

		tile0.road.GenerateMeshAndInit(tile0.road.scaleSubmeshes, tile0.road.shape2D);
		tile1.road.GenerateMeshAndInit(tile1.road.scaleSubmeshes, tile1.road.shape2D);
	}

	[ContextMenu("Place road")]
	// Places entire road.
	public void Place()
	{
		if(spawnPos == null)
		{
			spawnPos = startTile.entryPoint;
		}

		foreach(GenerationTileController t in inputControllers)
		{
			t.transform.localScale = Vector3.one;
		}

		GenerateFiniteTrack(roadSize);

		car.transform.position = spawnPos.position + spawnPos.up.normalized * 2 + spawnPos.forward.normalized * 5;
		car.transform.forward = spawnPos.forward;
	}

	public void RemoveFirstTile()
	{
		GenerationTileController[] newArr = new GenerationTileController[spawnedTiles.Length - 1];
		for(int i = 1; i < spawnedTiles.Length; i++)
		{
			newArr[i - 1] = spawnedTiles[i];
		}
		spawnedTiles = newArr;
	}
}
