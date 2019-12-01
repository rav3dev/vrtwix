using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Toggle : CustomInteractible
{
	public UnityEvent e;
	public float angle,distance;
	public Vector2 Switch;
	public bool onOrOff;
	public Transform RotationObject;
    // Start is called before the first frame update
    void Start()
    {
		distance = grabPoints [0].transform.localPosition.magnitude;
		if (onOrOff) {
			RotationObject.localEulerAngles = new Vector3 (Switch.x, 0);
		} else {
			RotationObject.localEulerAngles = new Vector3 (Switch.y, 0);
		}
    }

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable(hand);
		hand.SkeletonUpdate();
		Grab.Invoke ();
	}


	public void GrabUpdate(CustomHand hand){
		angle = -Vector2.SignedAngle (new Vector2(transform.InverseTransformPoint(hand.PivotPoser.position).y, transform.InverseTransformPoint(hand.PivotPoser.position).z),Vector2.up);
		if (angle<Switch.x)
			RotationObject.localEulerAngles = new Vector3 (Switch.x, 0);
		if (angle>Switch.y)
			RotationObject.localEulerAngles = new Vector3 (Switch.y, 0);
		GetMyGrabPoserTransform (hand).position = RotationObject.position+ RotationObject.forward * distance;
	}

	public void GrabEnd(CustomHand hand){
		DettachHand (hand);
		ReleaseHand.Invoke ();
	}
}
