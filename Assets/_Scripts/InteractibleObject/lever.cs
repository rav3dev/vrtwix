using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class lever : CustomInteractible {
	public Transform stick;
//	public CustomHand leftHand,rightHand;
	public float value;
	public float clamp=60;

	public float angle;
	public float handleDistance;
	// Use this for initialization
	void Start () {
		if (grabPoints!=null&&grabPoints.Count>0)
			handleDistance = grabPoints[0].transform.localPosition.magnitude;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void OnAttachedToHand(CustomHand hand){
		SetInteractibleVariable (hand);

	}

	public void HandAttachedUpdate(CustomHand hand){
		Transform tempHandle = GetMyGrabPoserTransform (hand);
		tempHandle.position = hand.PivotPoser.position;
		tempHandle.localPosition = new Vector3 (0, tempHandle.localPosition.y, Mathf.Abs(tempHandle.localPosition.z)).normalized*handleDistance;

		value = angle / clamp;

		angle = Vector3.SignedAngle ((tempHandle.position - stick.position).normalized, transform.forward, transform.right);
		angle = -Mathf.Clamp (angle, -clamp, clamp);

		tempHandle.localPosition = new Vector3 (0,Mathf.Cos ((angle+90)* Mathf.Deg2Rad), Mathf.Sin ((angle+90)* Mathf.Deg2Rad)).normalized*handleDistance;
//		Mathf.Cos (f* Mathf.Deg2Rad), Mathf.Sin (f* Mathf.Deg2Rad)

		stick.rotation = Quaternion.LookRotation (tempHandle.position - stick.position, transform.up);
	}
}
