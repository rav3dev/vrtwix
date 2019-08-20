using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button : CustomInteractible
{
	public float distanseToPress;
    public float DistanceDettach=.05f;
	float startZCoordinate;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void GrabStart(CustomHand hand){
		SetInteractibleVariable(hand);
		hand.SkeletonUpdate();
		startZCoordinate = transform.InverseTransformPoint (hand.PivotPoser.position).z;
		hand.grabType = CustomHand.GrabType.Select;
	}

	void GrabUpdate(CustomHand hand){
		if ((rightHand || leftHand)&&GetMyGrabPoserTransform (hand)) {
			hand.SkeletonUpdate ();
			GetComponentInChildren<MeshRenderer> ().material.color = Color.grey;
			float tempDistance = Mathf.Clamp (transform.InverseTransformPoint (hand.PivotPoser.position).z - startZCoordinate, 0, distanseToPress);
			if (tempDistance == distanseToPress) {
				GetComponentInChildren<MeshRenderer> ().material.color = Color.blue;
			}
			GetMyGrabPoserTransform (hand).localPosition = new Vector3 (0, 0, tempDistance);
			GetMyGrabPoserTransform (hand).rotation = Quaternion.LookRotation (GetMyGrabPoserTransform (hand).forward, hand.PivotPoser.up);
			hand.GrabUpdateCustom ();
            if (Vector3.Distance(transform.position, hand.PivotPoser.position) > DistanceDettach) {
				GrabEnd (hand);
            }
		}
	}

	void GrabEnd(CustomHand hand){
		if ((rightHand || leftHand)&&GetMyGrabPoserTransform (hand)) {
			GetMyGrabPoserTransform (hand).localPosition = Vector3.zero;
			DettachHand (hand);

			GetComponentInChildren<MeshRenderer> ().material.color = Color.grey;
		}
    }
}
