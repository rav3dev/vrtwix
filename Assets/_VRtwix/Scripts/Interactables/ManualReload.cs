using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ManualReload : CustomInteractible
{
	[Space]
	public Transform ReloadObject,Zatvor,HummerRevolver; //reload object, bolt, revolver hummer
	public Vector2 ClampPosition; //position limits
	public Vector2 ClampAngle; //angle limits
    //[HideInInspector]
	public bool reloadHalf,reloadEnd=true,reloadFinish=true,handDrop,boltAngleTrue=false,boltSlideTrue = true; //reload maintaince variables
	public bool reloadLikeAR; //AR type ( bolt handle not moving when shooting )
    public bool slideCatch;
    bool noReturn;
    public TypeReload typeReload; //reload type
	public enum TypeReload{
		Slider,
		Cracking,
		LeverAction,
		BoltAction,
		Revolver,
	}

	public UnityEvent BulletOff,BulletOn;

	public float returnAddSpeed=0.01f,knockback; //bolt return, recoil
	[Header("SwingReload")]
	public Transform PointSwingReload; //swing calculation points
	public Vector3 localDirSwing,bulletOffSwingDir; //drum return direction / casings extraction direction
	public float MaxAngleDir=45, substractSpeed=100,  returnSpeedMultiply=500,substractSpeedBullet=300,returnSpeedMultiplyBullet=500;//угол в котором просчитывается направление, мертвая зона если скорость руки ниже этой, умножение скорости
    Vector3 oldPosSwing, speedSwing, oldSpeedSwing, Velosity;

	[Header("shotgun fix")]
	public Transform[] grabColliderObject; //fix of cracked barrel colliders
	public Transform[] reloadColliderObject;

	float PositionReload;
	float returnStart,returnSpeed;
	float tempAngle;
	Magazine magazineRevolver;
	Trigger trigger;
	Vector3 revolverDrumDirection;
	[Header("Sounds Events")]
	public UnityEvent clampReloadHalf;
	public UnityEvent clampReloadEnd;
	public UnityEvent clampX,clampY;
	public UnityEvent boltPosition,boltAngle;
//	[HideInInspector]
	public int revolverBulletID=0;
	public enum TypeHandGrabRotation{
		freeze,
		free,
		vertical,
		horizontal,
	}
	public TypeHandGrabRotation typeHandGrabRotation; //bolt grip type

	bool revolverReadyShoot,clampXCheck,clampYCheck;

    void Start()
    {
		enabled = false;
		if (reloadHalf == reloadEnd) {
			reloadHalf = !reloadEnd;
		}
		magazineRevolver = GetComponentInParent<PrimitiveWeapon> ().GetComponentInChildren<Magazine> ();
		trigger =  GetComponentInParent<PrimitiveWeapon> ().GetComponentInChildren<Trigger> ();
    }
    
    void FixedUpdate()
    {
		if (typeReload == TypeReload.Slider&&returnAddSpeed > 0 || knockback > 0) {		
			if (reloadHalf||handDrop) {
                if (!noReturn)
                {
                    returnSpeed += returnAddSpeed * Time.deltaTime;
                    PositionReload = Mathf.MoveTowards(returnStart, ClampPosition.y, returnSpeed * Time.deltaTime);
                }
                else {
                    enabled = false;
                }
				if (PositionReload >= ClampPosition.y) {
					enabled = false;
					if (!reloadEnd && reloadHalf) {
						handDrop = false;
						reloadEnd = true;
						reloadHalf = false;
						BulletOn.Invoke ();
						clampReloadEnd.Invoke ();
						returnSpeed = 0;
					}
				}
			} else {//reloadEnd
				PositionReload=Mathf.MoveTowards(PositionReload,ClampPosition.x,knockback*Time.deltaTime);
				if (PositionReload <= ClampPosition.x) {
					reloadHalf = true;
					reloadEnd = false;
					BulletOff.Invoke ();
					clampReloadHalf.Invoke ();
					reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
                    if (slideCatch) {
                        if (!trigger.primitiveWeapon.attachMagazine||(trigger.primitiveWeapon.attachMagazine && trigger.primitiveWeapon.attachMagazine.ammo == 0)) {
                            noReturn = true;
                        }
                    }
                }
			}
			if (reloadLikeAR) {
				if (PositionReload > ReloadObject.localPosition.z) {
					ReloadObject.localPosition = Vector3.forward * PositionReload;
				}
			} else {
				ReloadObject.localPosition = Vector3.forward * PositionReload;
			}
            if (Zatvor)
            Zatvor.localPosition = Vector3.forward * PositionReload;

            reloadFinish = PositionReload >= ClampPosition.y;
            if (reloadFinish)
                handDrop = false;
			if (ReloadObject.localPosition.z == ClampPosition.x) {
				if (!clampXCheck) {
					clampX.Invoke ();
					clampXCheck = true;
				}
			} else {
				clampXCheck = false;
			}
			if (ReloadObject.localPosition.z == ClampPosition.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
		} 

		if (typeReload == TypeReload.Cracking&&PointSwingReload) {
			if (!reloadFinish&&tempAngle>ClampAngle.x&&!leftHand&&!rightHand) {
				tempAngle -= returnAddSpeed*Time.deltaTime;
			}
			if (Vector3.Angle (Velosity, transform.parent.TransformDirection (localDirSwing)) < MaxAngleDir) {
				float tempSwingReload = Mathf.Clamp(Velosity.magnitude - substractSpeed*Time.deltaTime,0,float.MaxValue)*returnSpeedMultiply;
				tempAngle += tempSwingReload*Time.deltaTime;
				if (!reloadHalf&&tempAngle < ClampAngle.x) {
					reloadHalf = true;
					reloadEnd = false;
					BulletOff.Invoke ();
					clampReloadHalf.Invoke ();
					reloadFinish = false;
				}
				if (!reloadEnd && reloadHalf && tempAngle >= ClampAngle.y) {
					reloadEnd = true;
					reloadFinish = true;
					reloadHalf = false;
					clampReloadEnd.Invoke ();
					BulletOn.Invoke ();
					enabled = false;
				}


			}
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
			ReloadObject.localEulerAngles = new Vector3 (-tempAngle, 0, 0);
			speedSwing = (PointSwingReload.position - oldPosSwing);
			Velosity = (speedSwing - oldSpeedSwing)/Time.deltaTime;
			oldSpeedSwing = speedSwing;
			oldPosSwing = PointSwingReload.position;

			if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
				for (int i = 0; i < grabColliderObject.Length; i++) {
					grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
				}
			}
			if (tempAngle == ClampAngle.x) {
				if (!clampXCheck) {
					clampX.Invoke ();
					clampXCheck = true;
				}
			} else {
				clampXCheck = false;
			}
			if (tempAngle == ClampAngle.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
		} 

		if (typeReload == TypeReload.LeverAction) {
			tempAngle=Mathf.MoveTowards(tempAngle,ClampAngle.y,returnAddSpeed*Time.deltaTime);
			if (!reloadEnd && reloadHalf && tempAngle >= ClampAngle.y) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				BulletOn.Invoke ();
				clampReloadEnd.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			if (reloadFinish){
				enabled = false;
			}
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
			ReloadObject.localEulerAngles = new Vector3 (tempAngle, 0, 0);
			if (tempAngle == ClampAngle.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
		}

		if (typeReload == TypeReload.Revolver&&PointSwingReload) {
			
			ReloadObject.localEulerAngles = new Vector3 (0, 0, tempAngle);

			if (tempAngle<ClampAngle.y&&!leftHand&&!rightHand&!reloadFinish) {
				tempAngle += returnAddSpeed*Time.deltaTime;
			}
			if (Vector3.Angle (Velosity, transform.parent.TransformDirection (localDirSwing)) < MaxAngleDir) {
				float tempSwingReload = Mathf.Clamp(Velosity.magnitude - substractSpeed*Time.deltaTime,0,float.MaxValue)*returnSpeedMultiply;
				tempAngle -= tempSwingReload*Time.deltaTime;
			}
			if (!reloadFinish&&Vector3.Angle (Velosity, transform.parent.TransformDirection (bulletOffSwingDir)) < MaxAngleDir) {//вылет патронов с магазина
				float tempSwingReload = Mathf.Clamp(Velosity.magnitude - substractSpeedBullet*Time.deltaTime,0,float.MaxValue)*returnSpeedMultiplyBullet;
				if (tempSwingReload > 0) {
					BulletOff.Invoke ();
				}
			}
			if (reloadEnd && !reloadHalf && tempAngle >= ClampAngle.y) {
				reloadHalf = true;
				reloadEnd = false;
				clampReloadHalf.Invoke ();
			}
			if (!reloadEnd && reloadHalf && tempAngle <= ClampAngle.x) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				clampReloadEnd.Invoke ();
			}

			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
		
			speedSwing = PointSwingReload.position - oldPosSwing;
			Velosity = (speedSwing - oldSpeedSwing)/Time.deltaTime;
			oldSpeedSwing = speedSwing;
			oldPosSwing = PointSwingReload.position;
		} 

    }

	public void CustomRevolverUpdate(){
		
		if (trigger.Axis < 0.1f)
			revolverReadyShoot = true;
		if (revolverReadyShoot) {
			HummerRevolver.localEulerAngles = new Vector3 (Mathf.Lerp (ClampPosition.x, ClampPosition.y, trigger.Axis), 0);
			if (reloadFinish)
				ReloadObject.GetChild (0).localEulerAngles = new Vector3 (0, 0, (revolverBulletID - trigger.Axis) * 360 / magazineRevolver.capacity);
		} else {
			HummerRevolver.localEulerAngles = new Vector3 (ClampPosition.x, 0);
		}
	}

	public void RevolverDrunClose(){
		revolverBulletID= magazineRevolver.getBulletIdRevolver (ReloadObject.GetChild (0).localEulerAngles.z);
		ReloadObject.GetChild (0).localEulerAngles = new Vector3 (0, 0, revolverBulletID * 360/magazineRevolver.capacity);
	}

	public void RevolverNextBullet(){
		if (typeReload == TypeReload.Revolver) {
			if (revolverReadyShoot) {
				revolverReadyShoot = false;
				HummerRevolver.localEulerAngles = new Vector3 (ClampPosition.x, 0);
				if (reloadFinish) {
					revolverBulletID = ((revolverBulletID - 1) + magazineRevolver.capacity) % magazineRevolver.capacity;
					ReloadObject.GetChild (0).localEulerAngles = new Vector3 (0, 0, revolverBulletID * 360 / magazineRevolver.capacity);
				}

			}
		}
	}

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable (hand);
		revolverDrumDirection=hand.pivotPoser.InverseTransformDirection (ReloadObject.GetChild (0).up);
	}

	public void GrabUpdate(CustomHand hand){
		Vector3 localHand=Vector3.zero;
		switch (typeReload) {

		case TypeReload.Slider:
			ReloadObject.transform.position = hand.pivotPoser.position;

			if (!reloadHalf && ReloadObject.localPosition.z < ClampPosition.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
				clampReloadHalf.Invoke ();
			}
			handDrop = true;
            noReturn = false;
			if (!reloadEnd && reloadHalf && ReloadObject.localPosition.z > ClampPosition.y) {
				reloadEnd = true;
				reloadHalf = false;
				BulletOn.Invoke ();
				clampReloadEnd.Invoke ();
			}

			reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
			ReloadObject.localPosition = new Vector3 (0, 0, Mathf.Clamp (ReloadObject.transform.localPosition.z, ClampPosition.x, ClampPosition.y));
            if (Zatvor)
            Zatvor.localPosition = ReloadObject.localPosition;

            if (typeHandGrabRotation != TypeHandGrabRotation.freeze) {
				if (typeHandGrabRotation == TypeHandGrabRotation.horizontal) {
					grabPoints [0].transform.rotation = Quaternion.LookRotation (-grabPoints [0].transform.parent.right, hand.pivotPoser.up);
				} else {
					if (typeHandGrabRotation == TypeHandGrabRotation.vertical) {
						grabPoints [0].transform.rotation = Quaternion.LookRotation (grabPoints [0].transform.parent.up, hand.pivotPoser.up);
					} else {
						grabPoints [0].transform.rotation = hand.pivotPoser.rotation;
					}
				}
			}
			if (ReloadObject.localPosition.z == ClampPosition.x) {
				if (!clampXCheck) {
					clampX.Invoke ();
					clampXCheck = true;
				}
			} else {
				clampXCheck = false;
			}
			if (ReloadObject.localPosition.z == ClampPosition.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
			PositionReload = ReloadObject.localPosition.z;
			break;
		case TypeReload.Cracking:
			localHand = transform.InverseTransformPoint (hand.pivotPoser.position);
			tempAngle = -Vector2.SignedAngle (new Vector2 (localHand.z, localHand.y), Vector2.right);

			if (!reloadHalf && tempAngle < ClampAngle.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
				clampReloadHalf.Invoke ();
				reloadFinish = false;
			}
			if (!reloadEnd && reloadHalf && tempAngle > ClampAngle.y) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				BulletOn.Invoke ();
				clampReloadEnd.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
			ReloadObject.localEulerAngles = new Vector3 (0, 0, tempAngle);
			enabled = true;

			//для дробовика с разломаным стволом фикс
			if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
				for (int i = 0; i < grabColliderObject.Length; i++) {
					grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
				}
			}
			if (tempAngle == ClampAngle.x) {
				if (!clampXCheck) {
					clampX.Invoke ();
					clampXCheck = true;
				}
			} else {
				clampXCheck = false;
			}
			if (tempAngle == ClampAngle.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
			break;

		case TypeReload.LeverAction:
			localHand = transform.InverseTransformPoint (hand.pivotPoser.position);
			tempAngle = Vector2.SignedAngle (new Vector2 (localHand.z, localHand.y), Vector2.left);
			ClampPosition.x = tempAngle;
			if (!reloadHalf && tempAngle < ClampAngle.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
				clampReloadHalf.Invoke ();
			}
			if (!reloadEnd && reloadHalf && tempAngle > ClampAngle.y) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				BulletOn.Invoke ();
				clampReloadEnd.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);

			ReloadObject.localEulerAngles = new Vector3 (Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y), 0, 0);
			enabled = true;
			if (tempAngle == ClampAngle.x) {
				if (!clampXCheck) {
					clampX.Invoke ();
					clampXCheck = true;
				}
			} else {
				clampXCheck = false;
			}
			if (tempAngle == ClampAngle.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
			break;
		case TypeReload.BoltAction:
			ReloadObject.position = hand.pivotPoser.position;
			ReloadObject.rotation = Quaternion.LookRotation (transform.forward, hand.pivotPoser.position - transform.position);
			if (boltSlideTrue) {
				if (Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward) < ClampAngle.x) {
					ReloadObject.localEulerAngles = new Vector3 (0, 0, ClampAngle.x);
					if (!reloadEnd && reloadHalf){
						reloadEnd = true;
						reloadFinish = true;
						reloadHalf = false;
						BulletOn.Invoke ();
						clampReloadEnd.Invoke ();
					}
				}
				if (Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward) > ClampAngle.y) {
					ReloadObject.localEulerAngles = new Vector3 (0, 0, ClampAngle.y);
					if (!boltAngleTrue) {
						boltAngle.Invoke ();
					}
					boltAngleTrue = true;
				} else {
					boltAngleTrue = false;
				}

				ReloadObject.localEulerAngles = new Vector3 (0, 0, Mathf.Clamp (Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward), ClampAngle.x, ClampAngle.y));
			} else {
				ReloadObject.localEulerAngles = new Vector3 (0, 0, ClampAngle.y);
			}

			if (boltAngleTrue) {
				if (ReloadObject.localPosition.z < ClampPosition.x) {
					if (!reloadHalf) {
						reloadHalf = true;
						reloadEnd = false;
						BulletOff.Invoke ();
						clampReloadHalf.Invoke ();
					}
				}

				if (ReloadObject.localPosition.z > ClampPosition.y) {
					if (!boltSlideTrue) {
						boltPosition.Invoke ();
					}
					boltSlideTrue = true;
				} else {
					boltSlideTrue = false;
				}
				ReloadObject.localPosition = new Vector3 (0, 0, Mathf.Clamp (ReloadObject.transform.localPosition.z, ClampPosition.x, ClampPosition.y));
			} else {
				ReloadObject.localPosition = new Vector3 (0, 0, ClampPosition.y);
			}

			reloadFinish = Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward) <= ClampAngle.x;
			if (ReloadObject.localPosition.z == ClampPosition.x) {
				if (!clampXCheck) {
					clampX.Invoke ();
					clampXCheck = true;
				}
			} else {
				clampXCheck = false;
			}
			if (ReloadObject.localPosition.z == ClampPosition.y) {
				if (!clampYCheck) {
					clampY.Invoke ();
					clampYCheck = true;
				}
			} else {
				clampYCheck = false;
			}
			break;
		case TypeReload.Revolver:
			localHand = transform.InverseTransformPoint (hand.pivotPoser.position);
			tempAngle = -Vector2.SignedAngle (new Vector2 (localHand.x, localHand.y), Vector2.up);
			if (reloadEnd && !reloadHalf && tempAngle >= ClampAngle.y) {
				reloadHalf = true;
				reloadEnd = false;
				clampReloadHalf.Invoke ();
//				BulletOff.Invoke ();
			}
			if (!reloadEnd && reloadHalf && tempAngle <= ClampAngle.x) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				clampReloadEnd.Invoke ();
			}
			enabled = true;
			reloadFinish = tempAngle <= ClampAngle.x;
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
			ReloadObject.localEulerAngles = new Vector3 (0, 0, tempAngle);
			ReloadObject.GetChild (0).rotation = Quaternion.LookRotation (ReloadObject.forward, hand.pivotPoser.TransformDirection(revolverDrumDirection));
			GetMyGrabPoserTransform(hand).rotation=Quaternion.LookRotation (ReloadObject.forward, hand.pivotPoser.up);

			break;
		default:
			break;
		}


	}



	public void GrabEnd(CustomHand hand){
		if (typeReload == TypeReload.Slider) {
			enabled = true;
			returnSpeed = 0;
			returnStart = ReloadObject.localPosition.z;
		}
		if (typeReload == TypeReload.LeverAction) {
			enabled = true;
		}
		if (typeReload == TypeReload.Revolver) {
			if (reloadFinish) {
				BulletOn.Invoke ();
				ReloadObject.GetChild (0).localEulerAngles = new Vector3 (0, 0, revolverBulletID * 360 / magazineRevolver.capacity);
			}
			if (HummerRevolver) {
				HummerRevolver.localEulerAngles=new Vector3(ClampPosition.x,0);
			}

		}
		DettachHand (hand);
	}
}
