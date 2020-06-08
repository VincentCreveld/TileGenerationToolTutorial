using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// TODO: connect all of this to object pools
public class GenerationProp : MonoBehaviour, IGenerationProp
{
	public GameObject prop;
	public GenerationPropLocalPos localPropPos;
	[SerializeField] private GameObject spawnedObject = null;

	public bool alwaysLookUp = true;
	public bool randomiseScaleSlightly = false;
	public bool randomisePlacement = false;
	public bool randomiseOnlyTPlacement = false;
	public bool canPlaceOnPath = false;
	public bool placedOnNormalBelow = false;

	public GenerationPropLocalPos GetPropLocalPos()
	{
		return localPropPos;
	}

	public void RemoveProp()
	{
		if(spawnedObject != null)
		{
			if(Application.isEditor && !Application.isPlaying)
				DestroyImmediate(spawnedObject);
			else
				Destroy(spawnedObject);
		}
	}

	public void SpawnProp(Transform p, Vector3 scale, Vector3 pos, Vector3 f)
	{
		RemoveProp();
		transform.parent = p;
		spawnedObject = Instantiate(prop);
		spawnedObject.transform.parent = p;

		Vector3 s = scale;

		if(randomiseScaleSlightly)
			s *= UnityEngine.Random.Range(0.8f, 1.2f);
		spawnedObject.transform.localScale = new Vector3(s.x/p.transform.lossyScale.x, s.y / p.transform.lossyScale.y, s.z / p.transform.lossyScale.z);
		SetPosition(pos);
		SetForward(f);
		if(alwaysLookUp)
		{
			spawnedObject.transform.up = p.transform.up;
		}
		
		spawnedObject.transform.position += new Vector3(0, 10f, 0);
		RaycastHit hit;
		Ray ray = new Ray(spawnedObject.transform.position - new Vector3(0, 1, 0), -Vector3.up);
		//Debug.DrawRay(spawnedObject.transform.position - new Vector3(0, 1, 0), -Vector3.up * 500f, Color.red, 10f);
		LayerMask mask = 1 << LayerMask.NameToLayer("Terrain");
		if(Physics.Raycast(ray, out hit, 500f, mask))
		{
			spawnedObject.transform.position = hit.point;
			if(placedOnNormalBelow)
				spawnedObject.transform.up = hit.normal.normalized;
		}
		if(randomiseScaleSlightly)
			spawnedObject.transform.RotateAround(spawnedObject.transform.position, spawnedObject.transform.up, UnityEngine.Random.Range(0f, 360f));
	}

	public void RandomisePos(Vector3 lossy)
	{
		if(randomiseOnlyTPlacement)
		{
			RandomiseOnlyTPlacement();
			return;
		}
		float lo = (canPlaceOnPath) ? 0f : 5f;
		float hi = (canPlaceOnPath) ? 3f : 10f;
		float x = UnityEngine.Random.Range(lo, hi) * lossy.x;
		float t = UnityEngine.Random.Range(0f, 1f);
		bool invert = Mathf.FloorToInt(x) % 2 == 1;
		localPropPos.localOffset.x = (invert) ? -x : x;
		localPropPos.positionOnSpline = t;
	}

	public void RandomiseOnlyTPlacement()
	{
		float t = UnityEngine.Random.Range(0f, 1f);
		localPropPos.positionOnSpline = t;
	}

	public void SetPosition(Vector3 pos)
	{
		spawnedObject.transform.position = pos;
	}

	public void SetForward(Vector3 f)
	{
		spawnedObject.transform.forward = f;
	}
}

[System.Serializable]
public struct GenerationPropLocalPos
{
	public Vector3 localOffset;
	[Range(0, 1)]
	public float positionOnSpline;
}
