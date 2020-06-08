using UnityEngine;

[CreateAssetMenu]
public class TilePlacableProp : ScriptableObject
{
	[SerializeField]
	private GameObject objectToSpawn = null;

	public GameObject GetObjectToSpawn()
	{
		return objectToSpawn;
	}
}
