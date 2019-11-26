using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ManualReload : CustomInteractible
{
	[Space]
	public Transform ReloadObject,HummerRevolver;
	public Vector2 ClampPosition;
	public Vector2 ClampAngle;
	public bool reloadHalf,reloadEnd=true,reloadFinish,handDrop;
	public bool boltAngleTrue=false,boltSlideTrue = true,reloadLikeM4;
	public TypeReload typeReload;
	public enum TypeReload{
		Slider,
		Cracking,
		LeverAction,
		BoltAction,
		Revolver,
	}

	public UnityEvent BulletOff,BulletOn;


	public float returnAddSpeed=0.01f,knockback;
	[Header("SwingReload")]
	public Transform PointSwingReload;
	public Vector3 localDirSwing,bulletOffSwingDir;
	public float MaxAngleDir, substractSpeed, returnSpeedMultiply=100;
	Vector3 oldPosSwing,speedSwing,oldSpeedSwing,Velosity;

	[Header("shotgun fix")]
	public Transform[] grabColliderObject;
	public Transform[] reloadColliderObject;

	float PositionReload;
	float returnStart,returnSpeed;
	float tempAngle;
	Magazine magazineRevolver;
	Trigger trigger;
	Vector3 revolverDrumDirection;
//	[HideInInspector]
	public int revolverBulletID=0;

	bool revolverReadyShoot;
    // Start is called before the first frame update
    void Start()
    {
		enabled = false;
		if (reloadHalf == reloadEnd) {
			reloadHalf = !reloadEnd;
		}
		magazineRevolver = GetComponentInParent<PrimitiveWeapon> ().GetComponentInChildren<Magazine> ();
		trigger =  GetComponentInParent<PrimitiveWeapon> ().GetComponentInChildren<Trigger> ();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if (typeReload == TypeReload.Slider&&returnAddSpeed > 0 || knockback > 0) {		
			if (reloadHalf||handDrop) {
				returnSpeed += returnAddSpeed*Time.deltaTime;

//					ReloadObject.localPosition = Vector3.MoveTowards (Vector3.forward * returnStart, Vector3.forward * ClampPosition.y, returnSpeed * Time.deltaTime);

				PositionReload=Mathf.MoveTowards(returnStart,ClampPosition.y,returnSpeed*Time.deltaTime);
				if (PositionReload >= ClampPosition.y) {
					enabled = false;
					if (!reloadEnd && reloadHalf) {
						handDrop = false;
						reloadEnd = true;
						reloadHalf = false;
						BulletOn.Invoke ();
						returnSpeed = 0;
					}
				}
			} else {//reloadEnd
//					ReloadObject.localPosition = Vector3.MoveTowards (ReloadObject.localPosition, Vector3.forward * ClampPosition.x, knockback * Time.deltaTime);
				PositionReload=Mathf.MoveTowards(PositionReload,ClampPosition.x,knockback*Time.deltaTime);
				if (PositionReload <= ClampPosition.x) {
					reloadHalf = true;
					reloadEnd = false;
					BulletOff.Invoke ();
					reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
				}
			}
			if (reloadLikeM4) {
				if (PositionReload > ReloadObject.localPosition.z) {
					ReloadObject.localPosition = Vector3.forward * PositionReload;
				}
			} else {
				ReloadObject.localPosition = Vector3.forward * PositionReload;
			}
			reloadFinish = PositionReload >= ClampPosition.y;
		} 

		if (typeReload == TypeReload.Cracking&&PointSwingReload) {
			if (!reloadFinish&&tempAngle>ClampAngle.x&&!leftHand&&!rightHand) {
				tempAngle -= returnAddSpeed*Time.deltaTime;
			}
//			PointSwingReload.rotation = Quaternion.LookRotation (PointSwingReload.position - oldPosSwing-oldVelosity);
//			PointSwingReload.localScale = Vector3.one * (PointSwingReload.position - oldPosSwing-oldVelosity).magnitude;
			if (Vector3.Angle (Velosity, transform.parent.TransformDirection (localDirSwing)) < MaxAngleDir) {
				float tempSwingReload = Mathf.Clamp(Velosity.magnitude - substractSpeed*Time.deltaTime,0,float.MaxValue)*returnSpeedMultiply;
				tempAngle += tempSwingReload*Time.deltaTime;
				if (!reloadHalf&&tempAngle < ClampAngle.x) {
					reloadHalf = true;
					reloadEnd = false;
					BulletOff.Invoke ();
					reloadFinish = false;
				}
				if (!reloadEnd && reloadHalf && tempAngle >= ClampAngle.y) {
					reloadEnd = true;
					reloadFinish = true;
					reloadHalf = false;
					BulletOn.Invoke ();
					enabled = false;
				}


			}
			ReloadObject.localEulerAngles = new Vector3 (-Mathf.Clamp(tempAngle,ClampAngle.x,ClampAngle.y), 0, 0);
			speedSwing = (PointSwingReload.position - oldPosSwing);
			Velosity = (speedSwing - oldSpeedSwing)/Time.deltaTime;// PointSwingReload.position - oldPosSwing;
			oldSpeedSwing = speedSwing;
			oldPosSwing = PointSwingReload.position;

			if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
				for (int i = 0; i < grabColliderObject.Length; i++) {
					grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
				}
			}
		} 

		if (typeReload == TypeReload.LeverAction) {
			tempAngle=Mathf.MoveTowards(tempAngle,ClampAngle.y,returnAddSpeed*Time.deltaTime);
			if (!reloadEnd && reloadHalf && tempAngle >= ClampAngle.y) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				BulletOn.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			if (reloadFinish){
				enabled = false;
			}
			ReloadObject.localEulerAngles = new Vector3 (Mathf.Clamp(tempAngle,ClampAngle.x,ClampAngle.y), 0, 0);
		}

		if (typeReload == TypeReload.Revolver&&PointSwingReload) {
			
			ReloadObject.localEulerAngles = new Vector3 (0, 0, tempAngle);

			if (tempAngle<ClampAngle.y&&!leftHand&&!rightHand&!reloadFinish) {
				tempAngle += returnAddSpeed*Time.deltaTime;
			}
//			//			PointSwingReload.rotation = Quaternion.LookRotation (PointSwingReload.position - oldPosSwing-oldVelosity);
//			//			PointSwingReload.localScale = Vector3.one * (PointSwingReload.position - oldPosSwing-oldVelosity).magnitude;
			if (Vector3.Angle (Velosity, transform.parent.TransformDirection (localDirSwing)) < MaxAngleDir) {
				float tempSwingReload = Mathf.Clamp(Velosity.magnitude - substractSpeed*Time.deltaTime,0,float.MaxValue)*returnSpeedMultiply;
				tempAngle -= tempSwingReload*Time.deltaTime;
			}
			if (!reloadFinish&&Vector3.Angle (Velosity, transform.parent.TransformDirection (bulletOffSwingDir)) < MaxAngleDir) {
				float tempSwingReload = Mathf.Clamp(Velosity.magnitude - substractSpeed*Time.deltaTime,0,float.MaxValue)*returnSpeedMultiply;
				if (tempSwingReload > 0) {
					BulletOff.Invoke ();
				}
			}
			if (reloadEnd && !reloadHalf && tempAngle >= ClampAngle.y) {
				reloadHalf = true;
				reloadEnd = false;
//				BulletOff.Invoke ();
			}
			if (!reloadEnd && reloadHalf && tempAngle <= ClampAngle.x) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
			}

			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
		
			speedSwing = PointSwingReload.position - oldPosSwing;
			Velosity = (speedSwing - oldSpeedSwing)/Time.deltaTime;
			oldSpeedSwing = speedSwing;
			oldPosSwing = PointSwingReload.position;
//
//			if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
//				for (int i = 0; i < grabColliderObject.Length; i++) {
//					grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
//				}
//			}
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
		revolverDrumDirection=hand.PivotPoser.InverseTransformDirection (ReloadObject.GetChild (0).up);
	}

	public void GrabUpdate(CustomHand hand){
		Vector3 localHand=Vector3.zero;
		switch (typeReload) {

		case TypeReload.Slider:
			ReloadObject.transform.position = hand.PivotPoser.position;

			if (!reloadHalf && ReloadObject.localPosition.z < ClampPosition.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
			}
			handDrop = true;
			if (!reloadEnd && reloadHalf && ReloadObject.localPosition.z > ClampPosition.y) {
				reloadEnd = true;
				reloadHalf = false;
				BulletOn.Invoke ();
			}

			reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
			ReloadObject.localPosition = new Vector3 (0, 0, Mathf.Clamp (ReloadObject.transform.localPosition.z, ClampPosition.x, ClampPosition.y));
			PositionReload = ReloadObject.localPosition.z;
			break;
		case TypeReload.Cracking:
			localHand = transform.InverseTransformPoint (hand.PivotPoser.position);
			tempAngle = -Vector2.SignedAngle (new Vector2 (localHand.z, localHand.y), Vector2.right);

			if (!reloadHalf&&tempAngle < ClampAngle.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
				reloadFinish = false;
			}
			if (!reloadEnd && reloadHalf && tempAngle > ClampAngle.y) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				BulletOn.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			ReloadObject.localEulerAngles = new Vector3 (0, 0, tempAngle);
			enabled = true;

			//для дробовика с разломаным стволом фикс
			if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
				for (int i = 0; i < grabColliderObject.Length; i++) {
					grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
				}
			}
			break;

		case TypeReload.LeverAction:
			localHand = transform.InverseTransformPoint (hand.PivotPoser.position);
			tempAngle = Vector2.SignedAngle (new Vector2 (localHand.z, localHand.y), Vector2.left);
			ClampPosition.x = tempAngle;
			if (!reloadHalf && tempAngle < ClampAngle.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
			}
			if (!reloadEnd && reloadHalf && tempAngle > ClampAngle.y) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
				BulletOn.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);

			ReloadObject.localEulerAngles = new Vector3 (Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y), 0, 0);
			enabled = true;
			break;
		case TypeReload.BoltAction:
			ReloadObject.position = hand.PivotPoser.position;
			ReloadObject.rotation = Quaternion.LookRotation (transform.forward, hand.PivotPoser.position - transform.position);
			if (boltSlideTrue) {
				if (Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward) < ClampAngle.x) {
					ReloadObject.localEulerAngles = new Vector3 (0, 0, ClampAngle.x);
					if (!reloadEnd && reloadHalf){
						reloadEnd = true;
						reloadFinish = true;
						reloadHalf = false;
						BulletOn.Invoke ();
					}
				}
				if (Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward) > ClampAngle.y) {
					ReloadObject.localEulerAngles = new Vector3 (0, 0, ClampAngle.y);
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
					}
				}
				boltSlideTrue = false;
				if (ReloadObject.localPosition.z > ClampPosition.y) {
					
					boltSlideTrue = true;
				}
				ReloadObject.localPosition = new Vector3 (0, 0, Mathf.Clamp (ReloadObject.transform.localPosition.z, ClampPosition.x, ClampPosition.y));
			} else {
				ReloadObject.localPosition = new Vector3 (0, 0, ClampPosition.y);
			}

			reloadFinish = Vector3.SignedAngle (transform.up, ReloadObject.up, transform.forward) <= ClampAngle.x;

			break;
		case TypeReload.Revolver:
			localHand = transform.InverseTransformPoint (hand.PivotPoser.position);
			tempAngle = -Vector2.SignedAngle (new Vector2 (localHand.x, localHand.y), Vector2.up);
			if (reloadEnd && !reloadHalf && tempAngle >= ClampAngle.y) {
				reloadHalf = true;
				reloadEnd = false;
//				BulletOff.Invoke ();
			}
			if (!reloadEnd && reloadHalf && tempAngle <= ClampAngle.x) {
				reloadEnd = true;
				reloadFinish = true;
				reloadHalf = false;
			}
			enabled = true;
			reloadFinish = tempAngle <= ClampAngle.x;
			tempAngle = Mathf.Clamp (tempAngle, ClampAngle.x, ClampAngle.y);
			ReloadObject.localEulerAngles = new Vector3 (0, 0, tempAngle);
			ReloadObject.GetChild (0).rotation = Quaternion.LookRotation (ReloadObject.forward, hand.PivotPoser.TransformDirection(revolverDrumDirection));
			GetMyGrabPoserTransform(hand).rotation=Quaternion.LookRotation (ReloadObject.forward, hand.PivotPoser.up);

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
