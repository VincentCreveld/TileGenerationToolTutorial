using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileComparer : MonoBehaviour
{
	public GenerationTile tile1, tile2;

	[ContextMenu("Check")]
	public void Check()
	{
		if(tile1.CanConnect(tile2))
			Debug.Log("Can connect");
		else
			Debug.Log("Cannot connect");
	}
}
