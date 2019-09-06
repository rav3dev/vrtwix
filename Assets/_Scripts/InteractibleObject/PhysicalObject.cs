using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
[RequireComponent(typeof(Rigidbody))]
public class PhysicalObject : CustomInteractible {
	public List<SteamVR_Skeleton_Poser> handleObject;//count=2
//	public Transform LeftGrab,RightGrab;
//	public CustomHand leftHand,rightHand;
	public Rigidbody MyRigidbody;
	public bool GizmoVisible;
	public Vector2 clampHandlePosZ;
	[Range(0,1)]
	public float SqueezeCheack;

//	Vector3 LocalPositonWithPivotLeft,LocalPositonWithPivotRight;
	Vector3 LocalDirectionWithPivotLeft,LocalDirectionWithPivotRight;
	bool leftIsForvard;

	Vector3 leftHandlePos,rightHandlePos;
	Quaternion leftHandleRot,rightHandleRot;
	[System.Serializable]
	public struct SaveVariables{
		public float maxAngelarVelicity,mass,drag,angularDrag;
		public Vector3 centerOfMass;
		public bool isKinematic,useGravity;

		public void SaveProperty(Rigidbody rigidbody){
			useGravity = rigidbody.useGravity;
			isKinematic = rigidbody.isKinematic;
			maxAngelarVelicity = rigidbody.maxAngularVelocity;
			centerOfMass = rigidbody.centerOfMass;
			mass = rigidbody.mass;
			drag = rigidbody.drag;
			angularDrag = rigidbody.angularDrag;
		}

		public void LoadProperty(Rigidbody rigidbody){
			rigidbody.useGravity = useGravity;
			rigidbody.isKinematic = isKinematic;
			rigidbody.maxAngularVelocity = maxAngelarVelicity;
			rigidbody.centerOfMass = centerOfMass;
			rigidbody.mass = mass;
			rigidbody.drag = drag;
			rigidbody.angularDrag = angularDrag;
		}
	}
	public SaveVariables saveVariables;
	// Use this for initialization
	void Start () {
//		print ((int)SteamVR_Input_Sources.LeftHand + " " + (int)SteamVR_Input_Sources.RightHand);
		if (GetComponent<Rigidbody> ()) {
			MyRigidbody = GetComponent<Rigidbody> ();
			saveVariables.SaveProperty (MyRigidbody);
		} 
		enabled = false;
	}


	public void GrabStart(CustomHand hand){
				
		Vector3 tempPosHandLocal=transform.InverseTransformPoint (hand.GrabPoint());
		tempPosHandLocal.x = 0;
		tempPosHandLocal.y = 0;
		MyRigidbody.useGravity = false;
		MyRigidbody.isKinematic = false;
		MyRigidbody.maxAngularVelocity = float.MaxValue;
		if (tempPosHandLocal.z > clampHandlePosZ.x && tempPosHandLocal.z < clampHandlePosZ.y) {
			if (hand.handType == SteamVR_Input_Sources.LeftHand) {
				SetInteractibleVariable (hand, handleObject [0]);
				handleObject [0].transform.localPosition = tempPosHandLocal;
			} else {
				if (hand.handType == SteamVR_Input_Sources.RightHand) {
					SetInteractibleVariable (hand, handleObject [1]);
					handleObject [1].transform.localPosition = tempPosHandLocal;
				}
			}
		} else {
			SetInteractibleVariable (hand);
		}

		if (leftHand && rightHand) {
			leftIsForvard = transform.InverseTransformPoint (leftMyGrabPoser.transform.position).z > transform.InverseTransformPoint (rightMyGrabPoser.transform.position).z;
			LocalDirectionWithPivotLeft = leftMyGrabPoser.transform.InverseTransformDirection (transform.up);
			LocalDirectionWithPivotRight = rightMyGrabPoser.transform.InverseTransformDirection (transform.up);
		}
		
	}

	public void GrabUpdate(CustomHand hand){
		if (rightHand && leftHand) {
			leftIsForvard = transform.InverseTransformPoint (leftMyGrabPoser.transform.position).z > transform.InverseTransformPoint (rightMyGrabPoser.transform.position).z;
			if (handleObject != null && handleObject.Count == 2) {
				if ((leftHand.Squeeze - rightHand.Squeeze) > SqueezeCheack&&rightMyGrabPoser==handleObject[1]) {
					handleObject [1].transform.localPosition = new Vector3 (0, 0, Mathf.Clamp (transform.InverseTransformPoint (rightHand.GrabPoint ()).z, clampHandlePosZ.x, clampHandlePosZ.y));
					if (leftIsForvard)
						leftIsForvard = !leftIsForvard;
				}
				if ((rightHand.Squeeze - leftHand.Squeeze) > SqueezeCheack&&leftMyGrabPoser==handleObject[0]) {
					handleObject [0].transform.localPosition = new Vector3 (0, 0, Mathf.Clamp (transform.InverseTransformPoint (leftHand.GrabPoint ()).z, clampHandlePosZ.x, clampHandlePosZ.y));
					if (!leftIsForvard)
						leftIsForvard = !leftIsForvard;
				}
			}

			if (leftIsForvard) {
				rightHand.ToolTransform.rotation = Quaternion.LookRotation (leftMyGrabPoser.transform.position- rightMyGrabPoser.transform.position,rightMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotRight));
				leftHand.ToolTransform.rotation= Quaternion.LookRotation (leftHand.PivotPoser.transform.position- rightHand.PivotPoser.transform.position,rightHand.PivotPoser.TransformDirection(LocalDirectionWithPivotRight));
				MyRigidbody.centerOfMass = transform.InverseTransformPoint(rightMyGrabPoser.transform.position);
				MyRigidbody.velocity = (rightHand.PivotPoser.position-rightMyGrabPoser.transform.position)/Time.fixedDeltaTime;
				MyRigidbody.angularVelocity = GetAngularVelocities (leftHand.ToolTransform.rotation, rightHand.ToolTransform.rotation);
			} else {
				rightHand.ToolTransform.rotation = Quaternion.LookRotation (rightMyGrabPoser.transform.position- leftMyGrabPoser.transform.position,leftMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotLeft));
				leftHand.ToolTransform.rotation= Quaternion.LookRotation (rightHand.PivotPoser.transform.position- leftHand.PivotPoser.transform.position,leftHand.PivotPoser.TransformDirection(LocalDirectionWithPivotLeft));
				MyRigidbody.centerOfMass = transform.InverseTransformPoint(leftMyGrabPoser.transform.position);
				MyRigidbody.velocity = (leftHand.PivotPoser.position-leftMyGrabPoser.transform.position)/Time.fixedDeltaTime;
				MyRigidbody.angularVelocity = GetAngularVelocities (leftHand.ToolTransform.rotation, rightHand.ToolTransform.rotation);
			}



		} else {
			MyRigidbody.centerOfMass = transform.InverseTransformPoint(GetMyGrabPoserTransform(hand).position);
			MyRigidbody.velocity = (hand.PivotPoser.position - GetMyGrabPoserTransform(hand).position)/Time.fixedDeltaTime;
			MyRigidbody.angularVelocity = GetAngularVelocities (hand.PivotPoser.rotation, GetMyGrabPoserTransform(hand).rotation);
		}	
	}

	public void GrabEnd(CustomHand hand){
		DettachHand (hand);

		if (!leftHand && !rightHand) {
			saveVariables.LoadProperty (MyRigidbody);
		}
	}

	public void Initialize(){
		if (GetComponent<Rigidbody> ()) {
			MyRigidbody = GetComponent<Rigidbody> ();
			saveVariables.SaveProperty (MyRigidbody);
		}
	}


	public void GrabStartCustom(CustomHand hand){

		Vector3 tempPosHandLocal=transform.InverseTransformPoint (hand.GrabPoint());
		tempPosHandLocal.x = 0;
		tempPosHandLocal.y = 0;
		MyRigidbody.useGravity = false;
		MyRigidbody.isKinematic = false;
		MyRigidbody.maxAngularVelocity = float.MaxValue;
		if (tempPosHandLocal.z > clampHandlePosZ.x && tempPosHandLocal.z < clampHandlePosZ.y) {
			if (hand.handType == SteamVR_Input_Sources.LeftHand) {
				SetInteractibleVariable (hand, handleObject [0]);
				handleObject [0].transform.localPosition = tempPosHandLocal;
			} else {
				if (hand.handType == SteamVR_Input_Sources.RightHand) {
					SetInteractibleVariable (hand, handleObject [1]);
					handleObject [1].transform.localPosition = tempPosHandLocal;
				}
			}
		} else {
			SetInteractibleVariable (hand);
		}

		if (leftHand && rightHand) {
			leftIsForvard = transform.InverseTransformPoint (leftMyGrabPoser.transform.position).z > transform.InverseTransformPoint (rightMyGrabPoser.transform.position).z;
			LocalDirectionWithPivotLeft = leftMyGrabPoser.transform.InverseTransformDirection (transform.up);
			LocalDirectionWithPivotRight = rightMyGrabPoser.transform.InverseTransformDirection (transform.up);
		}

	}

	public void GrabUpdateCustom(CustomHand hand){
		if (rightHand && leftHand) {
			leftIsForvard = transform.InverseTransformPoint (leftMyGrabPoser.transform.position).z > transform.InverseTransformPoint (rightMyGrabPoser.transform.position).z;
			if (handleObject != null && handleObject.Count == 2) {
				if ((leftHand.Squeeze - rightHand.Squeeze) > SqueezeCheack&&rightMyGrabPoser==handleObject[1]) {
					handleObject [1].transform.localPosition = new Vector3 (0, 0, Mathf.Clamp (transform.InverseTransformPoint (rightHand.GrabPoint ()).z, clampHandlePosZ.x, clampHandlePosZ.y));
					if (leftIsForvard)
						leftIsForvard = !leftIsForvard;
				}
				if ((rightHand.Squeeze - leftHand.Squeeze) > SqueezeCheack&&leftMyGrabPoser==handleObject[0]) {
					handleObject [0].transform.localPosition = new Vector3 (0, 0, Mathf.Clamp (transform.InverseTransformPoint (leftHand.GrabPoint ()).z, clampHandlePosZ.x, clampHandlePosZ.y));
					if (!leftIsForvard)
						leftIsForvard = !leftIsForvard;
				}
			}

			if (leftIsForvard) {
				rightHand.ToolTransform.rotation = Quaternion.LookRotation (leftMyGrabPoser.transform.position- rightMyGrabPoser.transform.position,rightMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotRight));
				leftHand.ToolTransform.rotation= Quaternion.LookRotation (leftHand.PivotPoser.transform.position- rightHand.PivotPoser.transform.position,rightHand.PivotPoser.TransformDirection(LocalDirectionWithPivotRight));
				MyRigidbody.centerOfMass = transform.InverseTransformPoint(rightMyGrabPoser.transform.position);
				MyRigidbody.velocity = (rightHand.PivotPoser.position-rightMyGrabPoser.transform.position)/Time.fixedDeltaTime;
				MyRigidbody.angularVelocity = GetAngularVelocities (leftHand.ToolTransform.rotation, rightHand.ToolTransform.rotation);
			} else {
				rightHand.ToolTransform.rotation = Quaternion.LookRotation (rightMyGrabPoser.transform.position- leftMyGrabPoser.transform.position,leftMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotLeft));
				leftHand.ToolTransform.rotation= Quaternion.LookRotation (rightHand.PivotPoser.transform.position- leftHand.PivotPoser.transform.position,leftHand.PivotPoser.TransformDirection(LocalDirectionWithPivotLeft));
				MyRigidbody.centerOfMass = transform.InverseTransformPoint(leftMyGrabPoser.transform.position);
				MyRigidbody.velocity = (leftHand.PivotPoser.position-leftMyGrabPoser.transform.position)/Time.fixedDeltaTime;
				MyRigidbody.angularVelocity = GetAngularVelocities (leftHand.ToolTransform.rotation, rightHand.ToolTransform.rotation);
			}



		} else {
			MyRigidbody.centerOfMass = transform.InverseTransformPoint(GetMyGrabPoserTransform(hand).position);
			MyRigidbody.velocity = (hand.PivotPoser.position - GetMyGrabPoserTransform(hand).position)/Time.fixedDeltaTime;
			MyRigidbody.angularVelocity = GetAngularVelocities (hand.PivotPoser.rotation, GetMyGrabPoserTransform(hand).rotation);
		}	
	}

	public void GrabEndCustom(CustomHand hand){
		DettachHand (hand);

		if (!leftHand && !rightHand) {
			saveVariables.LoadProperty (MyRigidbody);
		}
	}



	public static Vector3 GetAngularVelocities(Quaternion hand,Quaternion fake)
	{
		//		bool realNumbers = false;
		//
		//
		//		float velocityMagic = 50;
		//		float angularVelocityMagic = 50;
		//
		//		Vector3 targetItemPosition = TargetItemPosition(attachedObjectInfo);
		//		Vector3 positionDelta = (targetItemPosition - attachedObjectInfo.attachedRigidbody.position);
		//		velocityTarget = (positionDelta * velocityMagic * Time.deltaTime);
		//
		//		if (float.IsNaN(velocityTarget.x) == false && float.IsInfinity(velocityTarget.x) == false)
		//		{
		//			if (noSteamVRFallbackCamera)
		//				velocityTarget /= 10; //hacky fix for fallback
		//
		//			realNumbers = true;
		//		}
		//		else
		//			velocityTarget = Vector3.zero;
		//

		Quaternion rotationDelta = hand * Quaternion.Inverse(fake);
		Vector3 angularTarget=Vector3.zero;

		float angle;
		Vector3 axis;
		rotationDelta.ToAngleAxis(out angle, out axis);

		if (angle > 180)
			angle -= 360;

		if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
		{
			angularTarget = angle * axis *.95f;
		}
		else
			angularTarget = Vector3.zero;
		return angularTarget;
	}

	void OnDrawGizmosSelected(){
		if (GizmoVisible) {
//			Gizmos.DrawSphere (transform.TransformPoint(new Vector3 (0, 0, clampPosZ.x)), .05f);
//			Gizmos.DrawSphere (transform.TransformPoint(new Vector3 (0, 0, clampPosZ.y)), .05f);
			Gizmos.DrawLine(transform.TransformPoint(new Vector3 (0, 0, clampHandlePosZ.x)),transform.TransformPoint(new Vector3 (0, 0, clampHandlePosZ.y)));
		}
	}

}
