using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;

public class SteeringWheel : CustomInteractible {
	public float angle,clamp;//угол руля, ограничение вращения
	float angleLeft,angleRight; //угол от руля до рук
	Vector2 oldPosLeft,oldPosRight; //старые позиции рук
	public Transform RotationObject; //Вращающийся объект

	public float radius; //радиус руля
	bool ReversHand; //выворачивать руки если с другой стороны взялся

	void Start () {
		if (grabPoints!=null&&grabPoints.Count>0)
			radius = grabPoints [0].transform.localPosition.magnitude;
	}
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
		ReversHand = Vector3.Angle (transform.forward, hand.PivotPoser.forward) < 90;
		Grab.Invoke ();
	}

	public void GrabUpdate(CustomHand hand){
		Transform tempPoser = GetMyGrabPoserTransform (hand);
		Vector3 HandTolocalPos = transform.InverseTransformPoint (hand.PivotPoser.position);
		HandTolocalPos.z = 0;
		tempPoser.localPosition = HandTolocalPos;


		if (hand.handType == SteamVR_Input_Sources.LeftHand) {
				angle-=Vector2.SignedAngle (tempPoser.localPosition, oldPosLeft)*(leftHand&&rightHand?leftHand.Squeeze==rightHand.Squeeze?.5f:hand.Squeeze/(Mathf.Epsilon+(leftHand.Squeeze+rightHand.Squeeze)):1f);
			
			oldPosLeft = new Vector2 (HandTolocalPos.x, HandTolocalPos.y);
		} else {
			if (hand.handType == SteamVR_Input_Sources.RightHand) {
					angle-=Vector2.SignedAngle (tempPoser.localPosition, oldPosRight)*(leftHand&&rightHand?leftHand.Squeeze==rightHand.Squeeze?.5f:hand.Squeeze/(Mathf.Epsilon+(leftHand.Squeeze+rightHand.Squeeze)):1f);
				
				oldPosRight = new Vector2 (HandTolocalPos.x, HandTolocalPos.y);
			} 
		}
		angle = Mathf.Clamp (angle, -clamp, clamp);
		RotationObject.localEulerAngles=new Vector3 (0, 0, angle);
		tempPoser.localPosition = tempPoser.localPosition.normalized * radius;
		tempPoser.rotation = Quaternion.LookRotation (ReversHand? transform.forward:-transform.forward, tempPoser.position-transform.position);

	}

	public void GrabEnd(CustomHand hand){
		DettachHand (hand);
		ReleaseHand.Invoke ();
	}
}
