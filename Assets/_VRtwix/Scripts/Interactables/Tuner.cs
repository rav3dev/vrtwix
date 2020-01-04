using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tuner : CustomInteractible
{
    public Transform RotationObject; //вращающийся объект
	public float angle; //угол поворота
	public Vector2 clamp; //ограничения поворота, если 0 то нету ограничений
    Vector3 oldDir; //старый поворот руки

	public void GrabStart(CustomHand hand)
    {
        SetInteractibleVariable(hand);
        hand.SkeletonUpdate();
        GetMyGrabPoserTransform(hand).rotation = Quaternion.LookRotation(transform.forward, hand.PivotPoser.up);
		oldDir = transform.InverseTransformDirection(hand.PivotPoser.up);
		GetMyGrabPoserTransform (hand).transform.position = hand.PivotPoser.position;
		Grab.Invoke ();
    }

	public void GrabUpdate(CustomHand hand)
    {
		
		angle+= Vector3.SignedAngle(oldDir, transform.InverseTransformDirection(hand.PivotPoser.up), Vector3.forward);
		if (clamp != Vector2.zero)
		angle = Mathf.Clamp (angle, clamp.x, clamp.y);
        RotationObject.localEulerAngles = new Vector3(0, 0, angle);
		GetMyGrabPoserTransform (hand).transform.position = transform.position;// Vector3.MoveTowards (GetMyGrabPoserTransform (hand).transform.position, transform.TransformPoint(Vector3.zero), Time.deltaTime*.5f);
        oldDir = transform.InverseTransformDirection(hand.PivotPoser.up);
    }

	public void GrabEnd(CustomHand hand)
    {
        DettachHand(hand);
		ReleaseHand.Invoke ();
    }

}
