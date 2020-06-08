using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DebugPropRandomiser : MonoBehaviour
{
	private GenerationProp[] props = null;
	public GenerationProp prop;
	public int amtToSpawn = 150;

	[ContextMenu("GenerateProps")]
	public void Rdm()
	{
		if(props == null || props.Length <= 0)
		{
			props = new GenerationProp[amtToSpawn];

			for(int i = 0; i < amtToSpawn; i++)
			{
				props[i] = Instantiate(prop);
				props[i].transform.parent = transform;
			}
		}
		foreach(GenerationProp p in props)
		{
			p.RandomisePos(Vector3.one);
		}
	}
}
