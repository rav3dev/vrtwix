using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
public class PrimitiveWeapon : PhysicalObject
{
	[Header("PrimitiveWeapon")]
	public Trigger trigger;//trigger handler ( script )
	public SteamVR_Skeleton_Poser triggerPoser;//shooting poser
	public Transform magazineAttachPoint,bulletInsidePoint,reloadBulletSpawn; //mag attach point, ammo inside of weapon position, sleeve extraction
	[Header("Recoil")]
	public Transform recoil;//recoil calculation object
	public float recoilAngle, recoilAngleReturn, recoilMaxAngle,recoilDistance,recoilDistanceReturn,recoilMaxDistance;
	public float recoilCurrentAngle;
	[Space]
	public bool detachableMag,armed,typeRevolver;//detach mags, gun ready to shoo, revolver/shotgun
	public string ammoType; //ammo type
	public Magazine attachMagazine; //attached mag
	public Bullet bulletInside; //ammo inside
	public Vector3 outBulletSpeed; // casing/ammo extraction speed
	public ManualReload manualReload; // reload handler ( script )
//	[HideInInspector]
	public Collider[] myCollidersToIgnore; //to ignore mag colliders
	[Header("Sounds Events")]
	public UnityEvent onShoot;
	public UnityEvent onEmptyShot;
	public UnityEvent onMagazineLoad,onMagazineUnload;

    void Start()
    {
        Initialize();
        trigger = GetComponentInChildren<Trigger>();
        manualReload = GetComponentInChildren<ManualReload>();
        if (!detachableMag)
        {
            attachMagazine = GetComponentInChildren<Magazine>();
        }
    }


    new public void GrabStart(CustomHand hand){
		GrabStartCustom (hand);
	}

	new public void GrabUpdate(CustomHand hand){
		GrabUpdateCustom (hand);

		if (GetMyGrabPoser(hand)==triggerPoser)
		trigger.customUpdate (hand);
		if (recoil) {
			myRigidbody.velocity += transform.TransformDirection (recoil.localPosition/Time.fixedDeltaTime);
			myRigidbody.angularVelocity += PhysicalObject.GetAngularVelocities (transform.rotation, recoil.rotation, hand.GetBlendPose());
		}


		RecoilReturn ();
	}

	new public void GrabEnd(CustomHand hand){
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
			bulletInside.myRigidbody.AddRelativeForce (outBulletSpeed, ForceMode.VelocityChange);
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
			recoilCurrentAngle = Mathf.Clamp (recoilCurrentAngle + recoilAngleReturn*Time.deltaTime, -recoilMaxAngle, 0);
			recoil.localPosition = new Vector3 (0, 0, Mathf.Clamp (recoil.localPosition.z + recoilDistanceReturn * Time.deltaTime, -recoilMaxDistance, 0));
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
				if (manualReload.typeReload == ManualReload.TypeReload.Revolver) {
					if (attachMagazine.ShootFromMagazineRevolver ()) {
						Recoil ();
						IsShoot = true;
					}
				}
				if (manualReload.typeReload == ManualReload.TypeReload.Cracking) {
					if (attachMagazine.ShootFromMagazine ()) {
						Recoil ();
						IsShoot = true;
					}
				}

			} else {
				if (bulletInside && bulletInside.armed) {
					Recoil ();
					bulletInside.ChangeModel ();
					IsShoot = true;
				}
			}
		}
		if (IsShoot) {
			onShoot.Invoke ();
		} else {
			onEmptyShot.Invoke ();
		}
		return IsShoot;
	}

	public void UnloadMagazine(){
		attachMagazine.UnloadMagazine (outBulletSpeed);
		onMagazineUnload.Invoke ();
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
                PhysicalObject tempPhysicalObject = tempMagazine.GetComponent<PhysicalObject>();
                if (tempPhysicalObject)
                {
                    tempPhysicalObject.DettachHands();
                    tempPhysicalObject.myRigidbody.isKinematic = true;
                }
				tempMagazine.transform.parent = magazineAttachPoint;
				tempMagazine.transform.localPosition = Vector3.zero;
				tempMagazine.transform.localRotation = Quaternion.identity;
				attachMagazine = tempMagazine;
				tempMagazine.primitiveWeapon = this;
				tempMagazine.canLoad = false;
				onMagazineLoad.Invoke ();
				return;
			}
				
		}
	}
}
