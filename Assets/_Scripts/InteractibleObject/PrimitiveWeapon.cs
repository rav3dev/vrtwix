using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class PrimitiveWeapon : PhysicalObject
{
	[Space]

	public Trigger trigger;
	public SteamVR_Skeleton_Poser triggerPoser;
	public Transform magazineAttachPoint,bulletInsidePoint,reloadBulletSpawn;
	[Header("Recoil")]
	public Transform recoil;
	public float recoilAngle, recoilAngleReturn, recoilMaxAngle,recoilDistance,recoilDistanceReturn,recoilMaxDistance;
	public float recoilCurrentAngle;
	[Space]
	public bool detachableMag,armed,typeRevolver;
	public string ammoType;
	public Magazine attachMagazine;
	public Bullet bulletInside;
	public Vector3 outBulletSpeed;
//	[HideInInspector]
	public Collider[] myCollidersToIgnore; //для игнора магазина
    // Start is called before the first frame update
    void Start()
    {
		Initialize ();
		trigger= GetComponentInChildren<Trigger> ();

		if (!detachableMag) {
			attachMagazine = GetComponentInChildren<Magazine> ();
		}
    }


	public void GrabStart(CustomHand hand){
		GrabStartCustom (hand);
	}

	public void GrabUpdate(CustomHand hand){
		GrabUpdateCustom (hand);

		if (GetMyGrabPoser(hand)==triggerPoser)
		trigger.customUpdate (hand);
		if (recoil) {
			MyRigidbody.velocity += transform.TransformPoint (recoil.localPosition/Time.fixedDeltaTime);
			MyRigidbody.angularVelocity += PhysicalObject.GetAngularVelocities (transform.rotation, recoil.rotation);
		}


		RecoilReturn ();
	}

	public void GrabEnd(CustomHand hand){
		recoilCurrentAngle = 0;
		recoil.localPosition = Vector3.zero;
		GrabEndCustom (hand);
	}

	public void LoadBullet(){
		if (attachMagazine&& attachMagazine.ammo>0){
			bulletInside = attachMagazine.GetBullet ();
			bulletInside.transform.parent = bulletInsidePoint;
			bulletInside.transform.localPosition = Vector3.zero;
			bulletInside.transform.localRotation = Quaternion.identity;
			armed = true;
		}
	}

	public void RevolverArmed(){
		armed = true;
		attachMagazine.canLoad = false;
	}

	public void RevolverNoArmed(){
		armed = false;
		attachMagazine.canLoad = true;
	}

	public void UnloadBullet(){
		if (bulletInside) {
			bulletInside.transform.parent = null;
			bulletInside.transform.position = reloadBulletSpawn.position;
			bulletInside.transform.rotation = reloadBulletSpawn.rotation;

			bulletInside.OutMagazine ();
			bulletInside.MyRigidbody.AddRelativeForce (outBulletSpeed, ForceMode.VelocityChange);
			bulletInside=null;
			armed = false;
		}
	}
	public void Recoil(){
			recoil.localPosition -= Vector3.forward * recoilDistance;
			recoilCurrentAngle -= recoilAngle;	
	}

	void RecoilReturn(){
		if (recoil) {
			recoilCurrentAngle = Mathf.Clamp (recoilCurrentAngle + recoilAngleReturn, -recoilMaxAngle, 0);
			recoil.localPosition = new Vector3 (0, 0, Mathf.Clamp (recoil.localPosition.z + recoilDistanceReturn, -recoilMaxDistance, 0));
			recoil.localEulerAngles = new Vector3 (-recoilCurrentAngle, 0, 0);
		}
	}

	public bool Shoot(){
		bool IsShoot = false;
		if (detachableMag) {
			if (bulletInside && bulletInside.armed) {
				Recoil ();
				bulletInside.ChangeModel ();
				IsShoot = true;
			}
		}else{
			if (typeRevolver) {
				if (attachMagazine.ShootFromMagazine ()) {
					Recoil ();
					IsShoot = true;
				}
			} else {
				if (bulletInside && bulletInside.armed) {
					Recoil ();
					bulletInside.ChangeModel ();
					IsShoot = true;
				}
			}
		}
		return IsShoot;
	}

	public void UnloadMagazine(){
		attachMagazine.UnloadMagazine (outBulletSpeed);
	}

	void OnTriggerEnter(Collider c){
		if (detachableMag&&!attachMagazine) {
			Magazine tempMagazine = c.GetComponentInParent<Magazine> ();
			if (tempMagazine&&tempMagazine.ammoType==ammoType) {
				//игнор колайтеров магазина
				myCollidersToIgnore = GetComponentInParent<PrimitiveWeapon> ().gameObject.GetComponentsInChildren<Collider> ();
				for (int j = 0; j < myCollidersToIgnore.Length; j++) {
					for (int k = 0; k < tempMagazine.MagazineColliders.Length; k++) {
						Physics.IgnoreCollision(myCollidersToIgnore[j],tempMagazine.MagazineColliders[k]);
					}
				}

				tempMagazine.GetComponent<PhysicalObject> ().DettachHands ();
				tempMagazine.GetComponent<PhysicalObject> ().MyRigidbody.isKinematic = true;
				tempMagazine.transform.parent = magazineAttachPoint;
				tempMagazine.transform.localPosition = Vector3.zero;
				tempMagazine.transform.localRotation = Quaternion.identity;
				attachMagazine = tempMagazine;
				tempMagazine.primitiveWeapon = this;
				tempMagazine.canLoad = false;

				return;
			}
				
		}
	}
}
