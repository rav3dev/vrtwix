using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PhysicalObject
{
	public string ammoType;
	public bool armed=true;
	public Mesh shellModel;
	MeshFilter myMeshfilter;
//	public Rigidbody MyRigidbody;
//
//	public PhysicalObject.SaveVariables saveVariables;

    // Start is called before the first frame update
    void Start()
    {
		Initialize ();
//		if (GetComponent<Rigidbody> ()) {
//			MyRigidbody = GetComponent<Rigidbody> ();
//			saveVariables.SaveProperty (MyRigidbody);
//		}
		myMeshfilter = GetComponentInChildren<MeshFilter> ();

    }

    // Update is called once per frame
	void GrabStart(CustomHand hand)
    {
//		if (!TwoHanded && (leftHand || rightHand)) {
//			DettachHands ();
//		}
//		MyRigidbody.useGravity = false;
//		MyRigidbody.isKinematic = false;
//		MyRigidbody.maxAngularVelocity = float.MaxValue;
//		SetInteractibleVariable (hand);
		GrabStartCustom(hand);
    }



	public void GrabUpdate(CustomHand hand){
		GrabUpdateCustom (hand);
//		MyRigidbody.centerOfMass = transform.InverseTransformPoint(GetMyGrabPoserTransform(hand).position);
//		MyRigidbody.velocity = (hand.PivotPoser.position - GetMyGrabPoserTransform(hand).position)/Time.fixedDeltaTime;
//		MyRigidbody.angularVelocity = PhysicalObject.GetAngularVelocities (hand.PivotPoser.rotation, GetMyGrabPoserTransform(hand).rotation);
	}


	public void GrabEnd(CustomHand hand){
		GrabEndCustom(hand);
//		DettachHand (hand);
//
//		if (!leftHand && !rightHand) {
//			saveVariables.LoadProperty (MyRigidbody);
//		}
	}

	public void ChangeModel(){
		myMeshfilter.mesh = shellModel;
		armed = false;
	}

	public void DettachBullet(){
		DettachHands ();
		saveVariables.LoadProperty (MyRigidbody);
	}

	public void EnterMagazine(){
		Collider[] tempCollider= GetComponentsInChildren<Collider> ();
		for (int i = 0; i < tempCollider.Length; i++) {
			tempCollider [i].enabled = false;
		}
		MyRigidbody.isKinematic = true;
	}

	public void OutMagazine(){
		Collider[] tempCollider= GetComponentsInChildren<Collider> ();
		for (int i = 0; i < tempCollider.Length; i++) {
			tempCollider [i].enabled = true;
		}
		MyRigidbody.isKinematic = false;
	}
}
