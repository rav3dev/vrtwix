using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Toggle : CustomInteractible
{
	public UnityEvent SwithOn, SwithOff; 
	public float angle,distance; //angle to hand, hand distance ( temporaty not using )
	public Vector2 Switch; //limits
	public bool onOrOff; //switched on/off
	public Transform MoveObject; //moving part

    void Start()
    {
		distance = grabPoints [0].transform.localPosition.magnitude;
		if (onOrOff) {
            MoveObject.localEulerAngles = new Vector3 (Switch.x, 0);
		} else {
            MoveObject.localEulerAngles = new Vector3 (Switch.y, 0);
		}
    }

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable(hand);
		hand.SkeletonUpdate();
		Grab.Invoke ();
	}


	public void GrabUpdate(CustomHand hand){
		angle = -Vector2.SignedAngle (new Vector2(transform.InverseTransformPoint(hand.PivotPoser.position).y, transform.InverseTransformPoint(hand.PivotPoser.position).z),Vector2.up);
        MoveObject.localEulerAngles = new Vector3 (Mathf.Clamp(angle,Switch.x,Switch.y), 0);
        //hand position, if you need them not rotating
        //GetMyGrabPoserTransform (hand).position = RotationObject.position+ RotationObject.forward * distance; 
    }

    public void GrabEnd(CustomHand hand){
        onOrOff = angle < 0;
        if (onOrOff)
            SwithOn.Invoke();
        else
            SwithOff.Invoke();
        MoveObject.localEulerAngles = new Vector3(angle<0?Switch.x:Switch.y, 0);
        DettachHand (hand);
		ReleaseHand.Invoke ();
	}
}
