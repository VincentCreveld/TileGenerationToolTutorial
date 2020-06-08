using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[RequireComponent(typeof(MeshFilter))]
public class ReadMeshCrossection : MonoBehaviour
{
	[SerializeField]
	private MeshCrossection shape2D;
}
