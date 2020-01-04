using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : PhysicalObject
{
	public string ammoType; // тип патронов
	public bool armed=true; // готова ли стрелять пуля
	public Mesh shellModel; // Модель на которую заменится при выстреле
    void Start()
    {
		Initialize ();
    }
    
	void GrabStart(CustomHand hand)
    {
		GrabStartCustom(hand);
    }



	public void GrabUpdate(CustomHand hand){
		GrabUpdateCustom (hand);
	}


	public void GrabEnd(CustomHand hand){
		GrabEndCustom(hand);
	}

	public void ChangeModel(){
        MeshFilter myMeshfilter = GetComponentInChildren<MeshFilter>();
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
