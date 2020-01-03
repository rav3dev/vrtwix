using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Magazine : MonoBehaviour
{
	public bool Revolver = false,canLoad=true;  //револьвер это типа можно стрелять с магазина
	public string ammoType;
	public int capacity,ammo;
//	public List<Collider> EnterBullet;
	public List<Bullet> stickingAmmo;
	public Transform[] ContainAmmo;
	public PrimitiveWeapon primitiveWeapon;
	public Collider[] MagazineColliders; //IgnoreCollider
	PrimitiveWeapon primitiveWeaponRevolver;

	public float ang,id;
	[Header("Sounds Events")]
	public UnityEvent addBullet;
    // Start is called before the first frame update
    void Start()
    {
		MagazineColliders = GetComponentsInChildren<Collider> ();
		if (Revolver) {
			primitiveWeaponRevolver = GetComponentInParent<PrimitiveWeapon> ();
			stickingAmmo.AddRange (new Bullet[capacity]);
		}
    }

	void Update(){
		ang = ((id<0?capacity+id:id) * 360 / capacity)%360;
//		id =getBulletIdRevolver(ang);
	}

	public int getBulletIdRevolver(float tempAngle){
		float TempId = capacity/360f *(tempAngle<0?360f+tempAngle:tempAngle);
		return Mathf.RoundToInt (TempId)%capacity;
	}

	public void GrabStart(CustomHand hand){
		if (primitiveWeapon) {
			primitiveWeapon.attachMagazine = null;
			canLoad = true;
			for (int i = 0; i < primitiveWeapon.myCollidersToIgnore.Length; i++) {
				for (int j = 0; j < MagazineColliders.Length; j++) {
					Physics.IgnoreCollision(primitiveWeapon.myCollidersToIgnore[i],MagazineColliders[j],false);
				}
			}
			primitiveWeapon = null;
		}
	}

	void AddBullet(Bullet bullet){
		if (!canLoad)
			return;
		bullet.DettachBullet ();
		stickingAmmo.Add (bullet);
		ammo = stickingAmmo.Count;
		stickingAmmo [ammo - 1].transform.parent = ContainAmmo [ammo - 1];
		stickingAmmo [ammo - 1].transform.localPosition = Vector3.zero;
		stickingAmmo [ammo - 1].transform.localRotation = Quaternion.identity;
		bullet.EnterMagazine ();
		addBullet.Invoke ();
	}

	void AddBulletClose(Bullet bullet){
		if (!Revolver&&!canLoad)
			return;

		float tempCloseDistance=float.MaxValue;
		int closeId=0;
		for (int i = 0; i < ContainAmmo.Length; i++) {
			if (stickingAmmo[i]==null&&Vector3.Distance(bullet.transform.position,ContainAmmo[i].position)<tempCloseDistance){
				tempCloseDistance = Vector3.Distance (bullet.transform.position, ContainAmmo [i].position);
				closeId = i;
			}
		} 
		bullet.DettachBullet ();
		stickingAmmo [closeId] = bullet;
		ammo++;

		stickingAmmo [closeId].transform.parent = ContainAmmo [closeId];
		stickingAmmo [closeId].transform.localPosition = Vector3.zero;
		stickingAmmo [closeId].transform.localRotation = Quaternion.identity;

		bullet.EnterMagazine ();
	}

	public bool ShootFromMagazineRevolver(){
		if (!Revolver)
			return false;

//		int tempId = (primitiveWeaponRevolver.manualReload.revolverBulletID+1)%capacity;
		int tempId = primitiveWeaponRevolver.manualReload.revolverBulletID;
		if (stickingAmmo [tempId] != null&&stickingAmmo[tempId].armed) {
			stickingAmmo [tempId].ChangeModel ();
			return true;
		}
		return false;
	}

	public bool ShootFromMagazine(){//по очередди
		if (!Revolver)
			return false;

		for (int i = 0; i < stickingAmmo.Count; i++) {
			if (stickingAmmo [i] != null&&stickingAmmo[i].armed) {
				stickingAmmo [i].ChangeModel ();
				return true;
			}
		}
		return false;
	}

	public void UnloadMagazine(Vector3 outBulletSpeed){
		if (!Revolver) {
			return;
		}
		for (int i = 0; i < stickingAmmo.Count; i++) {
			if (stickingAmmo [i]) {
				stickingAmmo [i].transform.parent = null;
				stickingAmmo [i].OutMagazine ();
				stickingAmmo [i].MyRigidbody.AddRelativeForce (outBulletSpeed, ForceMode.VelocityChange);
				stickingAmmo [i] = null;
			}
		}
		ammo = 0;
	}

//	[ContextMenu("RemoveBullet")]
	public Bullet GetBullet(){
		Bullet tempReturn=stickingAmmo[ammo-1];
		stickingAmmo.RemoveAt(ammo-1);
		ammo = stickingAmmo.Count;
		return tempReturn;
	}

	void OnTriggerEnter(Collider c){
		if (Revolver) {
			if (c.attachedRigidbody && c.attachedRigidbody.GetComponent<Bullet> ()&&c.attachedRigidbody.GetComponent<Bullet> ().ammoType == ammoType) {
				if (ammo < capacity) {
					AddBulletClose (c.attachedRigidbody.GetComponent<Bullet> ());
				}
			}
//			ContactPoint[] tempContactPoint = c.contacts;
//			for (int i = 0; i < tempContactPoint.Length; i++) {
//				if (EnterBullet.Contains (tempContactPoint [i].thisCollider)) {
//					int tempIdEnter = EnterBullet.IndexOf (tempContactPoint [i].thisCollider);
//					if (tempContactPoint [i].otherCollider.attachedRigidbody && tempContactPoint [i].otherCollider.attachedRigidbody.GetComponent<Bullet> ()) {
//						if (tempContactPoint [i].otherCollider.attachedRigidbody.GetComponent<Bullet> ().ammoType == ammoType) {
//
//							return;
//						
//						}
//					}
//				}
//			}
		} else {
			if (c.attachedRigidbody && c.attachedRigidbody.GetComponent<Bullet> () && c.attachedRigidbody.GetComponent<Bullet> ().ammoType == ammoType) {
				if (ammo < capacity) {
					AddBullet (c.attachedRigidbody.GetComponent<Bullet> ());
				}
			}
		}
	}
}
