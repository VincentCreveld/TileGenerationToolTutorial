using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GenerationTile : MonoBehaviour
{
	// will contain cosmetic information
	// will contain difficulty coefficient
	// will contain some form of knowing the contents based on keywords based on contents
	// e.g. "jump" if it has a jump or "boost" if it contains a boostpad

	// needs some form of validation
	// algorithm needs to filter for valid tiles
	[SerializeField]
	public GenerationConnectionID connectionID;

	// checks if the "to be fit" tile fits on this tile
	public bool CanConnect(GenerationTile other)
	{
		string name = this.transform.name;
		string name1 = other.transform.name;
		ConnectionID idThis = this.connectionID.GetExit().id;
		ConnectionID idOther = other.GetConnectionId().GetEntry().id;

		List<ConnectionVariations> cVarThis = GetSignature(idThis.connectionID);

		List<ConnectionVariations> cVarOther = GetSignature(idOther.connectionID);

		bool idsMatch = (cVarThis.Count == cVarThis.Count);
		if(!idsMatch)
			return false;

		for(int i = 0; i < cVarThis.Count; i++)
		{
			if(cVarThis[i] != cVarOther[i])
				return false;
		}

		return true;
	}

	public List<ConnectionVariations> GetSignature(ConnectionVariations[] input)
	{
		List<ConnectionVariations> cVar = new List<ConnectionVariations>();
		bool foundPath = false;
		bool pathIsPure = true;
		for(int i = 0; i < input.Length; i++)
		{
			switch(input[i])
			{
				case ConnectionVariations.Null:
					if(foundPath == false)
						break;
					else return cVar;
				case ConnectionVariations.Path:
					if(foundPath == false && pathIsPure == true)
					{
						foundPath = true;
						cVar.Add(input[i]);
						break;
					}
					else if(foundPath == true && pathIsPure == true)
					{
						cVar.Add(input[i]);
						break;
					}
					else if(pathIsPure == false)
					{
						return cVar;
					}
					break;
				case ConnectionVariations.Wall:
					if(!foundPath && pathIsPure)
					{
						cVar.Add(input[i]);
						break;
					}
					else if(foundPath && pathIsPure)
					{
						pathIsPure = false;
						cVar.Add(input[i]);
						break;
					}else if(foundPath == false && pathIsPure == false)
					{
						cVar.Add(input[i]);
						break;
					}
					break;
				case ConnectionVariations.Inaccessible:
					if(foundPath)
						return cVar;
					break;
			}
		}

		return cVar;
	}

	public GenerationConnectionID GetConnectionId()
	{
		return connectionID;
	}

	public void OnValidate()
	{
		if(connectionID == null)
			connectionID = new GenerationConnectionID(CardinalDirections.North, CardinalDirections.South, new ConnectionID(), new ConnectionID());

		connectionID.Validate();
	}
}
