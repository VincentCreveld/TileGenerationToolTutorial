using System;
using UnityEngine;

[Serializable]
public enum CardinalDirections
{
	None,
	North,
	East,
	South,
	West
}

[Serializable]
public enum ConnectionVariations
{
	Null,
	Path,
	Wall,
	Inaccessible
}
