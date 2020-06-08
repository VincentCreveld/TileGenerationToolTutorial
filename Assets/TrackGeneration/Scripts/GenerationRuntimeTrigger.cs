using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[RequireComponent(typeof(BoxCollider))]
public class GenerationRuntimeTrigger : MonoBehaviour
{
	private BoxCollider boxCol = null;
	private Action callBack = null;
	private bool isEnter, isExit, singleFire, hasFiredEnter, hasFiredExit;

	public void Init(Action ac, bool singleFire = true, bool isEnter = true, bool isExit = false)
	{
		callBack = ac;
		this.isEnter = isEnter;
		this.isExit = isExit;
		this.singleFire = singleFire;

		boxCol = GetComponent<BoxCollider>();
		boxCol.isTrigger = true;
	}

	public void SetBounds(Vector3 b)
	{
		boxCol.size = b;
	}

	private void OnTriggerEnter(Collider other)
	{
		if(!isEnter || (singleFire && hasFiredEnter))
			return;

		callBack.Invoke();
		hasFiredEnter = true;
	}

	private void OnTriggerExit(Collider other)
	{
		if(!isExit || (singleFire && hasFiredExit))
			return;

		callBack.Invoke();
		hasFiredExit = true;
	}
}
