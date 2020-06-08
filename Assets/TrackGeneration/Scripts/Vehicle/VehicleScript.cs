using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class VehicleScript : MonoBehaviour
{

	public float forwardAcl = 10000f;
	public float backwardAcl = 2500f;
	private float currentThrust = 0f;

	public float turnStr = 300f;
	private float currentTurn = 0f;

	public float hoverForce = 900f;
	public float hoverHeight = 1f;
	public GameObject[] rayPoints;

	private float inputDeadzone = 0.35f;

	private LayerMask layerMask;
	private Rigidbody rb = null;

	private void Start()
	{
		rb = GetComponent<Rigidbody>();
		layerMask = 1 << LayerMask.NameToLayer("Player");
		layerMask = ~layerMask;
	}

	private void Update()
	{
		currentThrust = 0f;
		float mp = 1f;
		if(Input.GetKey(KeyCode.LeftAlt))
			mp = 3f;
		float fwdInput = Input.GetAxis("Vertical");
		if(fwdInput > inputDeadzone)
			currentThrust = fwdInput * forwardAcl * mp;
		else if(fwdInput < inputDeadzone)
			currentThrust = fwdInput * backwardAcl * mp;

		currentTurn = 0f;
		float turnInput = Input.GetAxis("Horizontal");
		if(Mathf.Abs(turnInput) > inputDeadzone)
			currentTurn = turnInput;
	}

	private void FixedUpdate()
	{
		if(Mathf.Abs(currentThrust) > 0)
			rb.AddForce(transform.forward * currentThrust);

		if(Mathf.Abs(currentTurn) > inputDeadzone)
		{
			rb.AddRelativeTorque(Vector3.up * currentTurn * turnStr);
		}

		RaycastHit hit;
		for(int i = 0; i < rayPoints.Length; i++)
		{
			GameObject hoverPoint = rayPoints[i];
			if(Physics.Raycast(hoverPoint.transform.position, -Vector3.up, out hit, hoverHeight, layerMask))
			{
				rb.AddForceAtPosition(Vector3.up * hoverForce * (1f - (hit.distance / hoverHeight)), hoverPoint.transform.position);
			}
			else
			{
				rb.AddForceAtPosition(Vector3.down * hoverForce * 3 * (1f - (hit.distance / hoverHeight)), hoverPoint.transform.position);

				if(transform.position.y > hoverPoint.transform.position.y)
				{
					rb.AddForceAtPosition(hoverPoint.transform.up * hoverForce, hoverPoint.transform.position);
				}
				else
				{
					rb.AddForceAtPosition(hoverPoint.transform.up * -hoverForce, hoverPoint.transform.position);
				}
			}
		}

		if(Physics.Raycast(transform.position, -Vector3.up, out hit, layerMask))
		{
			if(Input.GetKey(KeyCode.LeftShift))
				rb.velocity *= 0.98f;
			else
				rb.velocity *= 0.9f;
		}
		else
		{
			rb.velocity *= 0.95f;
		}

		rb.angularVelocity *= 0.95f;

	}
}
