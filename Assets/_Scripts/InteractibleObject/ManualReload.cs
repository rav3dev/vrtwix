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
	public bool reloadHalf,reloadEnd=true,reloadFinish;
	public bool boltAngleTrue=false,boltSlideTrue = true;

	public TypeReload typeReload;
	public enum TypeReload{
		Slider,
		Cracking,
		LeverAction,
		BoltAction,
	}

	public UnityEvent BulletOff,BulletOn;

	public float returnAddSpeed=0.01f,knockback;

	[Header("shotgun fix")]
	public Transform[] grabColliderObject;
	public Transform[] reloadColliderObject;

	float returnStart,returnSpeed;
    // Start is called before the first frame update
    void Start()
    {
		enabled = false;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
		if (returnAddSpeed <= 0||knockback<=0) {
			enabled = false;
			return;
		}
		if (typeReload==TypeReload.Slider){
			if (reloadHalf) {
				returnSpeed += returnAddSpeed;
				ReloadObject.localPosition = Vector3.MoveTowards (Vector3.forward * returnStart, Vector3.forward * ClampPosition.y, returnSpeed*Time.deltaTime);
				if (ReloadObject.localPosition.z >= ClampPosition.y) {
					enabled = false;
					if (!reloadEnd && reloadHalf) {
						reloadEnd = true;
						reloadHalf = false;
						BulletOn.Invoke ();
						returnSpeed = 0;
					}
				}
			} else {//reloadEnd
				ReloadObject.localPosition = Vector3.MoveTowards (ReloadObject.localPosition, Vector3.forward * ClampPosition.x, knockback*Time.deltaTime);
				if (ReloadObject.localPosition.z <= ClampPosition.x) {
					reloadHalf = true;
					reloadEnd = false;
					BulletOff.Invoke ();
					reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
				}
			}
			reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
		}
    }

	public void GrabStart(CustomHand hand){
		SetInteractibleVariable (hand);
	}

	public void GrabUpdate(CustomHand hand){
		Vector3 localHand=Vector3.zero;
		float tempAngle;
		switch (typeReload) {

		case TypeReload.Slider:
			ReloadObject.transform.position = hand.PivotPoser.position;

			if (!reloadHalf && ReloadObject.localPosition.z < ClampPosition.x) {
				reloadHalf = true;
				reloadEnd = false;
				BulletOff.Invoke ();
			}

			if (!reloadEnd && reloadHalf && ReloadObject.localPosition.z > ClampPosition.y) {
				reloadEnd = true;
				reloadHalf = false;
				BulletOn.Invoke ();
			}
			reloadFinish = ReloadObject.localPosition.z >= ClampPosition.y;
			ReloadObject.localPosition = new Vector3 (0, 0, Mathf.Clamp (ReloadObject.transform.localPosition.z, ClampPosition.x, ClampPosition.y));
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
				reloadHalf = false;
				BulletOn.Invoke ();
			}
			reloadFinish = tempAngle >= ClampAngle.y;
			ReloadObject.localEulerAngles = new Vector3 (-Mathf.Clamp(tempAngle,ClampAngle.x,ClampAngle.y), 0, 0);

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

		//для дробовика с разломаным стволом фикс
		if (grabColliderObject != null && reloadColliderObject != null && grabColliderObject.Length == reloadColliderObject.Length) {
			for (int i = 0; i < grabColliderObject.Length; i++) {
				grabColliderObject [i].SetPositionAndRotation (reloadColliderObject [i].position, reloadColliderObject [i].rotation);
			}
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
