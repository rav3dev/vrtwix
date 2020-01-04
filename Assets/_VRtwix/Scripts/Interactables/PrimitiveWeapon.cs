using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;
public class PrimitiveWeapon : PhysicalObject
{
	[Header("PrimitiveWeapon")]
	public Trigger trigger;//скрипт курка
	public SteamVR_Skeleton_Poser triggerPoser;//позер, который стреляет
	public Transform magazineAttachPoint,bulletInsidePoint,reloadBulletSpawn; //точка аттача магазина, пули внутри оружия, выбрасывание пули
	[Header("Recoil")]
	public Transform recoil;//объект просчета отдачи
	public float recoilAngle, recoilAngleReturn, recoilMaxAngle,recoilDistance,recoilDistanceReturn,recoilMaxDistance;
	public float recoilCurrentAngle;
	[Space]
	public bool detachableMag,armed,typeRevolver;//отсоединяется ли магазин, готова ли пушка стрелять, револьвер/дробовик
	public string ammoType; //тип патронов
	public Magazine attachMagazine; //Присоединенный магазин
	public Bullet bulletInside; //пуля внутри
	public Vector3 outBulletSpeed; // скорость выбрасывание пули

	public ManualReload manualReload; // скрипт перезарядки
//	[HideInInspector]
	public Collider[] myCollidersToIgnore; //для игнора магазина
	[Header("Sounds Events")]
	public UnityEvent ShootEvent;
	public UnityEvent ShootEmptyEvent;
	public UnityEvent MagazineLoad,MagazineUnload;

    void Start()
    {
		Initialize ();
		trigger= GetComponentInChildren<Trigger> ();
		manualReload = GetComponentInChildren<ManualReload> ();
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
			MyRigidbody.velocity += transform.TransformPoint (recoil.localPosition/Time.fixedDeltaTime* hand.GetBlend());
			MyRigidbody.angularVelocity += PhysicalObject.GetAngularVelocities (transform.rotation, recoil.rotation, hand.GetBlend());
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
			ShootEvent.Invoke ();
		} else {
			ShootEmptyEvent.Invoke ();
		}
		return IsShoot;
	}

	public void UnloadMagazine(){
		attachMagazine.UnloadMagazine (outBulletSpeed);
		MagazineUnload.Invoke ();
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
                    tempPhysicalObject.MyRigidbody.isKinematic = true;
                }
				tempMagazine.transform.parent = magazineAttachPoint;
				tempMagazine.transform.localPosition = Vector3.zero;
				tempMagazine.transform.localRotation = Quaternion.identity;
				attachMagazine = tempMagazine;
				tempMagazine.primitiveWeapon = this;
				tempMagazine.canLoad = false;
				MagazineLoad.Invoke ();
				return;
			}
				
		}
	}
}
