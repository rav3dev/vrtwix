using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class steeringWheel : CustomInteractible {
	public float angle,clamp;
	float angleLeft,angleRight;
	Vector2 oldPosLeft,oldPosRight;
	public Transform RotationObject;

	public float radius;
	// Use this for initialization
	void Start () {
		if (grabPoints!=null&&grabPoints.Count>0)
			radius = grabPoints [0].transform.localPosition.magnitude;
		
	}
	
	// Update is called once per frame
//	void Update () {
//		target.localPosition = new Vector3 (target.localPosition.x, target.localPosition.y);
////		float ChangeAngle=oldAngle-Vector2.SignedAngle (target.localPosition, Vector2.up);
//		angle -=Vector2.SignedAngle (target.localPosition, oldPos);
//		angle = Mathf.Clamp (angle, -clamp, clamp);
////		oldAngle = Vector2.SignedAngle (target.localPosition, oldPos);
//		oldPos = new Vector2 (target.localPosition.x, target.localPosition.y);
//		rot.localEulerAngles = new Vector3 (0, 0, angle);
//	}

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable (hand);
		hand.SkeletonUpdate ();
		hand.PivotUpdate ();
		Transform tempPoser=GetMyGrabPoserTransform (hand);
		Vector3 HandTolocalPos = transform.InverseTransformPoint (hand.PivotPoser.position);
		HandTolocalPos.z = 0;
		tempPoser.localPosition = HandTolocalPos;
		if (hand.handType == SteamVR_Input_Sources.LeftHand) {
			oldPosLeft = new Vector2 (HandTolocalPos.x, HandTolocalPos.y);
		} else {
			if (hand.handType == SteamVR_Input_Sources.RightHand) {
				oldPosRight = new Vector2 (HandTolocalPos.x, HandTolocalPos.y);
			} 
		}
	}

	public void GrabUpdate(CustomHand hand){
		Transform tempPoser = GetMyGrabPoserTransform (hand);
		Vector3 HandTolocalPos = transform.InverseTransformPoint (hand.PivotPoser.position);
		HandTolocalPos.z = 0;
		tempPoser.localPosition = HandTolocalPos;

		if (hand.handType == SteamVR_Input_Sources.LeftHand) {
			angle-=Vector2.SignedAngle (tempPoser.localPosition, oldPosLeft)*(leftHand&&rightHand?0.5f:1f);
			oldPosLeft = new Vector2 (HandTolocalPos.x, HandTolocalPos.y);
		} else {
			if (hand.handType == SteamVR_Input_Sources.RightHand) {
				angle-=Vector2.SignedAngle (tempPoser.localPosition, oldPosRight)*(leftHand&&rightHand?0.5f:1f);
				oldPosRight = new Vector2 (HandTolocalPos.x, HandTolocalPos.y);
			} 
		}
		angle = Mathf.Clamp (angle, -clamp, clamp);
		RotationObject.localEulerAngles=new Vector3 (0, 0, angle);
		tempPoser.localPosition = tempPoser.localPosition.normalized * radius;
		tempPoser.rotation = Quaternion.LookRotation (transform.forward, tempPoser.position-transform.position);

	}

	public void GrabEnd(CustomHand hand){
		DettachHand (hand);
	}
}
