using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class Joystick : CustomInteractible {
	public Transform Stick; //moving part of joystick
	public Vector2 value; //current position in %
	public Vector2 clamp=new Vector2(60,60); //limits of incline

	public Vector2 angle; //incline angle
	float handleDistance; //distance to handle/grip
	Quaternion rotation;
	public bool normalize; // square or circle limitation
	public bool returnToZero; // return to default position
	public enum TypeHandGrabRotation{
		free,
		vertical,
		horizontal,
	}
	public TypeHandGrabRotation typeHandGrabRotation; // hands grip behaviour
	// Use this for initialization
	void Start () {
		if (grabPoints!=null&&grabPoints.Count>0)
			handleDistance = grabPoints[0].transform.localPosition.magnitude;

		enabled = false;
	}

	// Update is called once per frame

	public void Update(){
		if (leftHand || rightHand)
			enabled = false;
		if (returnToZero) {
			value = Vector2.MoveTowards (value, Vector2.zero, Time.deltaTime);
			if (value == Vector2.zero)
				enabled = false;
			Stick.localRotation = Quaternion.LookRotation(Vector3.SlerpUnclamped (Vector3.SlerpUnclamped (new Vector3 (-1, -1, 1), new Vector3 (-1, 1, 1), value.x*clamp.x/90+.5f),Vector3.SlerpUnclamped (new Vector3 (1, -1, 1), new Vector3 (1, 1, 1), value.x*clamp.x/90+.5f),value.y*clamp.y/90+.5f),Vector3.up);


			Transform tempPoser = grabPoints[0].transform;
			if (typeHandGrabRotation == TypeHandGrabRotation.vertical) {
				tempPoser.rotation = Quaternion.LookRotation (Stick.forward, Stick.up);
			} else {
				if (typeHandGrabRotation == TypeHandGrabRotation.horizontal) {
					tempPoser.rotation = Quaternion.LookRotation (Stick.up, Stick.forward);
				}
			}
			tempPoser.position = Stick.TransformPoint(new Vector3(0,0, handleDistance));
		}
	}

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable (hand);
		hand.SkeletonUpdate ();
		Grab.Invoke ();
	}

	public void GrabUpdate(CustomHand hand){
		Transform tempPoser = GetMyGrabPoserTransform (hand);
		tempPoser.position = hand.PivotPoser.position;
		tempPoser.localPosition = new Vector3 (tempPoser.localPosition.x, tempPoser.localPosition.y, Mathf.Abs(tempPoser.localPosition.z));

		angle.x = Vector2.SignedAngle(new Vector2(tempPoser.localPosition.y,tempPoser.localPosition.z),Vector2.up);
		angle.y = Vector2.SignedAngle(new Vector2(tempPoser.localPosition.x,tempPoser.localPosition.z),Vector2.up);

		angle = new Vector2 (Mathf.Clamp (angle.x, -clamp.x, clamp.x), Mathf.Clamp (angle.y, -clamp.y, clamp.y));
		value = new Vector2 (angle.x / (clamp.x + Mathf.Epsilon), angle.y / (clamp.y + Mathf.Epsilon));
		if (normalize)
			value=Vector2.ClampMagnitude(value,1);

		Stick.localRotation = Quaternion.LookRotation(Vector3.SlerpUnclamped (Vector3.SlerpUnclamped (new Vector3 (-1, -1, 1), new Vector3 (-1, 1, 1), value.x*clamp.x/90+.5f),Vector3.SlerpUnclamped (new Vector3 (1, -1, 1), new Vector3 (1, 1, 1), value.x*clamp.x/90+.5f),value.y*clamp.y/90+.5f),Vector3.up);

		if (typeHandGrabRotation == TypeHandGrabRotation.vertical) {
			tempPoser.rotation = Quaternion.LookRotation (Stick.forward, hand.PivotPoser.up);
		} else {
			if (typeHandGrabRotation == TypeHandGrabRotation.horizontal) {
				tempPoser.rotation = Quaternion.LookRotation (Stick.up, hand.PivotPoser.up);
			} else {
				tempPoser.rotation = hand.PivotPoser.rotation;
			}
		}
		tempPoser.position = Stick.TransformPoint(new Vector3(0,0, handleDistance));


	}

	public void GrabEnd(CustomHand hand){
		DettachHand (hand);
		if (returnToZero) {
			enabled = true;
		}
		ReleaseHand.Invoke ();
	}

}
