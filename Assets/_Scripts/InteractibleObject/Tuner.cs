using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tuner : CustomInteractible
{
    public Transform RotationObject;
	public float angle;
    Vector3 oldDir;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

	public void GrabStart(CustomHand hand)
    {
        SetInteractibleVariable(hand);
        hand.SkeletonUpdate();
        GetMyGrabPoserTransform(hand).rotation = Quaternion.LookRotation(transform.forward, hand.PivotPoser.up);
		oldDir = transform.InverseTransformDirection(hand.PivotPoser.up);
		GetMyGrabPoserTransform (hand).transform.position = hand.PivotPoser.position;
    }

	public void GrabUpdate(CustomHand hand)
    {
		
		angle+= Vector3.SignedAngle(oldDir, transform.InverseTransformDirection(hand.PivotPoser.up), Vector3.forward);
        RotationObject.localEulerAngles = new Vector3(0, 0, angle);
        GetMyGrabPoserTransform(hand).transform.rotation = Quaternion.LookRotation(transform.forward, transform.InverseTransformDirection(hand.PivotPoser.up));
		GetMyGrabPoserTransform (hand).transform.position = Vector3.MoveTowards (GetMyGrabPoserTransform (hand).transform.position, transform.TransformPoint(Vector3.zero), Time.deltaTime*.5f);
        oldDir = transform.InverseTransformDirection(hand.PivotPoser.up);
    }

	public void GrabEnd(CustomHand hand)
    {
        DettachHand(hand);
    }

	void OnDrawGizmos(){
		Gizmos.DrawRay(transform.position,transform.TransformDirection(oldDir));
		if (rightHand)
		Gizmos.DrawRay(transform.position,transform.TransformDirection(transform.InverseTransformDirection(rightHand.PivotPoser.up)));
	}
}
