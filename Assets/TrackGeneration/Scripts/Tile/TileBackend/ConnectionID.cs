using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
public class ConnectionID
{
	public const int CONNECTION_SIZE = 7;
	[SerializeField]
	public ConnectionVariations[] connectionID;
	public ConnectionID(params ConnectionVariations[] vars)
	{
		bool validConnection = true;
		if(vars.Length < CONNECTION_SIZE)
			validConnection = false;

		connectionID = new ConnectionVariations[CONNECTION_SIZE];

		if(validConnection)
		{
			for(int i = 0; i < CONNECTION_SIZE; i++)
			{
				connectionID[i] = vars[i];
			}
		}
	}

	public void Validate()
	{
		int curSize = connectionID.Length;

		if(curSize != CONNECTION_SIZE)
		{
			ConnectionVariations[] newArray = new ConnectionVariations[CONNECTION_SIZE];
			for(int i = 0; i < CONNECTION_SIZE; i++)
			{
				if(i >= curSize)
					newArray[i] = ConnectionVariations.Null;
				else
					newArray[i] = connectionID[i];
			}
			connectionID = newArray;
		}
	}

	public static bool operator ==(ConnectionID lhs, ConnectionID rhs)
	{
		bool isValid = true;

		if(lhs.connectionID.Length != CONNECTION_SIZE || rhs.connectionID.Length != CONNECTION_SIZE)
			return false;

		for(int i = 0; i < CONNECTION_SIZE; i++)
		{
			if(!(lhs.connectionID[i] == rhs.connectionID[i]))
				return false;
		}

		return isValid;
	}

	public static bool operator !=(ConnectionID lhs, ConnectionID rhs)
	{
		return !(lhs == rhs);
	}

	public override bool Equals(object obj)
	{
		return base.Equals(obj);
	}

	public override int GetHashCode()
	{
		return base.GetHashCode();
	}

	public override string ToString()
	{
		return base.ToString();
	}
}