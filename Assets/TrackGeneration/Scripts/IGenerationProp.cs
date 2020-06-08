using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IGenerationProp
{
	void SpawnProp(Transform parent, Vector3 scale, Vector3 pos, Vector3 r);
	GenerationPropLocalPos GetPropLocalPos();
}
