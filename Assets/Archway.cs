using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using UnityEngine;

public class Archway : MonoBehaviour
{
	public Transform pos0, pos1;

	public Transform targetObject;

	public void Update()
	{
		targetObject.transform.position = VectorMidpoint(pos0.position, pos1.position);
		
		Vector3 scaleLocal = targetObject.GetChild(0).localScale;
		scaleLocal.y = Vector3.Distance(pos0.position, pos1.position);
		targetObject.GetChild(0).localScale = scaleLocal;

		targetObject.rotation = Quaternion.FromToRotation(Vector3.up, pos1.position - pos0.position);

		Quaternion q = targetObject.GetChild(0).localRotation;

		float d = Mathf.Acos(Vector3.Dot(Vector3.up, targetObject.forward)) * Mathf.Rad2Deg;
		Debug.Log(Vector3.Dot(Vector3.up, targetObject.forward));
		
		if(targetObject.localRotation.z < 0)
			d *= -1;
		
		targetObject.GetChild(0).localEulerAngles = new Vector3(0, d, 0);

		Debug.DrawLine(targetObject.position, targetObject.position + targetObject.forward, Color.red, 0f);

		//targetObject.GetChild(0).eulerAngles = -targetObject.eulerAngles;

		Quaternion localRot = targetObject.localRotation;
		Debug.Log(localRot);
	}

	private Vector3 VectorMidpoint(Vector3 a, Vector3 b)
	{
		return new Vector3((a.x + b.x) * 0.5f, (a.y + b.y) * 0.5f, (a.z + b.z) * 0.5f);
	}
}
