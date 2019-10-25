using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class ManualReload : CustomInteractible
{
	[Space]
	public Transform ReloadObject;
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
	}

	public UnityEvent BulletOff,BulletOn;


	public float returnAddSpeed=0.01f,knockback;
	[Header("SwingReload")]
	public Transform PointSwingReload;
	public Vector3 localDirSwing;
	public float MaxAngleDir, substractSpeed, returnSpeedMultiply=100;
	Vector3 oldPosSwing,oldVelosity;

	[Header("shotgun fix")]
	public Transform[] grabColliderObject;
	public Transform[] reloadColliderObject;

	float PositionReload;
	float returnStart,returnSpeed;
	float tempAngle;
    // Start is called before the first frame update
    void Start()
    {
		enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if (typeReload == TypeReload.Slider&&returnAddSpeed > 0 || knockback > 0) {		
			if (reloadHalf||handDrop) {
				returnSpeed += returnAddSpeed;

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
				tempAngle -= returnAddSpeed;
			}
			PointSwingReload.rotation = Quaternion.LookRotation (PointSwingReload.position - oldPosSwing-oldVelosity);
			PointSwingReload.localScale = Vector3.one * (PointSwingReload.position - oldPosSwing-oldVelosity).magnitude;
			if (Vector3.Angle ((PointSwingReload.position - oldPosSwing), transform.parent.TransformDirection (localDirSwing)) < MaxAngleDir) {
				float tempSwingReload = Mathf.Clamp((PointSwingReload.position - oldPosSwing).magnitude - substractSpeed,0,float.MaxValue)*returnSpeedMultiply;
				if (tempSwingReload > 0) {
					
					tempAngle += tempSwingReload;
				}
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
			oldVelosity = PointSwingReload.position - oldPosSwing;
			oldPosSwing = PointSwingReload.position;

			if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
				for (int i = 0; i < grabColliderObject.Length; i++) {
					grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
				}
			}
		} 
    }

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable (hand);
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
			ReloadObject.localEulerAngles = new Vector3 (-Mathf.Clamp(tempAngle,ClampAngle.x,ClampAngle.y), 0, 0);
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
			if (!reloadHalf&&tempAngle < ClampAngle.x) {
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
			ReloadObject.localEulerAngles = new Vector3 (Mathf.Clamp(tempAngle,ClampAngle.x,ClampAngle.y), 0, 0);

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
		DettachHand (hand);
	}
}
