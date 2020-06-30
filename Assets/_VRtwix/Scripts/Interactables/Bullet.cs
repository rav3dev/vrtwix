using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PhysicalObject
{
	public string ammoType; // ammo type
	public bool armed=true; // if ammo ready to shoot
	public Mesh shellModel; // object of casing , which will be replaced after shot
    void Start()
    {
		Initialize ();
    }
    
	new void GrabStart(CustomHand hand)
    {
		GrabStartCustom(hand);
    }



	new public void GrabUpdate(CustomHand hand){
		GrabUpdateCustom (hand);
	}


	new public void GrabEnd(CustomHand hand){
		GrabEndCustom(hand);
	}

	public void ChangeModel(){
        MeshFilter myMeshfilter = GetComponentInChildren<MeshFilter>();
        myMeshfilter.mesh = shellModel;
		armed = false;
	}

	public void DettachBullet(){
		DettachHands ();
		saveVariables.LoadProperty (myRigidbody);
	}

	public void EnterMagazine(){
		Collider[] tempCollider= GetComponentsInChildren<Collider> ();
		for (int i = 0; i < tempCollider.Length; i++) {
			tempCollider [i].enabled = false;
		}
		myRigidbody.isKinematic = true;
	}

	public void OutMagazine(){
		Collider[] tempCollider= GetComponentsInChildren<Collider> ();
		for (int i = 0; i < tempCollider.Length; i++) {
			tempCollider [i].enabled = true;
		}
		myRigidbody.isKinematic = false;
	}
}
