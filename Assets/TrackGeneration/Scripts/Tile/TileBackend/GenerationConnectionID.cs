using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class GenerationConnectionID
{
	[SerializeField]
	public GenerationTileConnection entry;
	[SerializeField]
	public GenerationTileConnection exit;

	[SerializeField, HideInInspector]
	private bool isValid = true;

	public GenerationConnectionID(CardinalDirections entry, CardinalDirections exit, ConnectionID enID, ConnectionID exID)
	{
		this.entry.dir = entry;
		this.entry.id = enID;
		this.exit.dir = exit;
		this.exit.id = exID;
	}

	public GenerationTileConnection GetEntry()
	{
		return entry;
	}

	public GenerationTileConnection GetExit()
	{
		return exit;
	}

	public bool IsValid()
	{
		return isValid;
	}

	public void Validate()
	{
		entry.id.Validate();
		exit.id.Validate();

		if(entry.dir == exit.dir)
		{
			exit.dir = CardinalDirections.None;
			isValid = false;
		}

		if(entry.dir == CardinalDirections.None || exit.dir == CardinalDirections.None)
		{
			//Debug.LogError("ConnectionID is invalid in one of the tiles. It won't be added to generation.");
			isValid = false;
			return;
		}

		isValid = true;
	}
}
