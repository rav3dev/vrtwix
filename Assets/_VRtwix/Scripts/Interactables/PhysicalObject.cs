using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
[RequireComponent(typeof(Rigidbody))]
public class PhysicalObject : CustomInteractible {
    public bool twoHandTypeOnlyBackHandRotation; //rotate only right hand
    public bool twoHandTypeMiddleRotation; //mean rotation for 2 hands
    public List<SteamVR_Skeleton_Poser> handleObject;//count=2
	public Rigidbody MyRigidbody; 
	public bool GizmoVisible; //display line of hand swing on long grip
	public Vector2 clampHandlePosZ; // grip limit
	[Range(0,1)]
	public float squeezeCheack; // squeeze death zone difference
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

	void Start () {
		if (GetComponent<Rigidbody> ()) {
			MyRigidbody = GetComponent<Rigidbody> ();
			saveVariables.SaveProperty (MyRigidbody);
		} 
		enabled = false;
	}

	public void GrabStart(CustomHand hand){
		Vector3 tempPosHandLocal=transform.InverseTransformPoint (hand.PointByPoint(hand.gripPoint));
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
		if (pickReleaseOnce){
			if (!leftHand||!rightHand){
				Grab.Invoke ();//sound
			}
		}else{
			Grab.Invoke ();
		}
	}

	public void GrabUpdate(CustomHand hand){

		if (rightHand && leftHand) {
			leftIsForvard = transform.InverseTransformPoint (leftMyGrabPoser.transform.position).z > transform.InverseTransformPoint (rightMyGrabPoser.transform.position).z;
            bool leftIsForvardTemp = leftIsForvard;

            if (handleObject != null && handleObject.Count == 2) {
                if ((leftHand.squeeze - rightHand.squeeze) > squeezeCheack && rightMyGrabPoser == handleObject[1])
                {
                    handleObject[1].transform.localPosition = new Vector3(0, 0, Mathf.Clamp(transform.InverseTransformPoint(rightHand.PointByPoint(hand.gripPoint)).z, clampHandlePosZ.x, clampHandlePosZ.y));
                    if (leftIsForvard)
                        leftIsForvard = !leftIsForvard;
                }
                if ((rightHand.squeeze - leftHand.squeeze) > squeezeCheack && leftMyGrabPoser == handleObject[0])
                {
                    handleObject[0].transform.localPosition = new Vector3(0, 0, Mathf.Clamp(transform.InverseTransformPoint(leftHand.PointByPoint(hand.gripPoint)).z, clampHandlePosZ.x, clampHandlePosZ.y));
                    if (!leftIsForvard)
                        leftIsForvard = !leftIsForvard;
                }
			}
			if (useSecondPose&&ifUseSecondPose()) {
				if (secondPoses.Contains (leftHand.grabPoser)) {
                    MyRigidbody.centerOfMass = transform.InverseTransformPoint (GetMyGrabPoserTransform (rightHand).position);
					MyRigidbody.velocity = (rightHand.pivotPoser.position - GetMyGrabPoserTransform (rightHand).position) / Time.fixedDeltaTime* hand.GetBlendPose();
					MyRigidbody.angularVelocity = GetAngularVelocities (rightHand.pivotPoser.rotation, GetMyGrabPoserTransform (rightHand).rotation, hand.GetBlendPose());
				} else {
					MyRigidbody.centerOfMass = transform.InverseTransformPoint (GetMyGrabPoserTransform (leftHand).position);
					MyRigidbody.velocity = (leftHand.pivotPoser.position - GetMyGrabPoserTransform (leftHand).position) / Time.fixedDeltaTime* hand.GetBlendPose();
					MyRigidbody.angularVelocity = GetAngularVelocities (leftHand.pivotPoser.rotation, GetMyGrabPoserTransform (leftHand).rotation, hand.GetBlendPose());
				}
			} else {
                if (!twoHandTypeOnlyBackHandRotation)
                    leftIsForvardTemp = leftIsForvard;
                if (twoHandTypeMiddleRotation)
                {
                    if (leftIsForvardTemp)
                    {
                        rightHand.toolTransform.rotation = Quaternion.LookRotation(leftMyGrabPoser.transform.position - rightMyGrabPoser.transform.position, rightMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotRight) + leftMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotLeft));
                        leftHand.toolTransform.rotation = Quaternion.LookRotation(leftHand.pivotPoser.transform.position - rightHand.pivotPoser.transform.position, rightHand.pivotPoser.TransformDirection(LocalDirectionWithPivotRight) + leftHand.pivotPoser.TransformDirection(LocalDirectionWithPivotLeft));
                    }
                    else
                    {
                        rightHand.toolTransform.rotation = Quaternion.LookRotation(rightMyGrabPoser.transform.position - leftMyGrabPoser.transform.position, leftMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotLeft) + rightMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotRight));
                        leftHand.toolTransform.rotation = Quaternion.LookRotation(rightHand.pivotPoser.transform.position - leftHand.pivotPoser.transform.position, leftHand.pivotPoser.TransformDirection(LocalDirectionWithPivotLeft) + rightHand.pivotPoser.TransformDirection(LocalDirectionWithPivotRight));
                    }
                }
                else
                {
                    if (leftIsForvardTemp)
                    {
                        rightHand.toolTransform.rotation = Quaternion.LookRotation(leftMyGrabPoser.transform.position - rightMyGrabPoser.transform.position, rightMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotRight));
                        leftHand.toolTransform.rotation = Quaternion.LookRotation(leftHand.pivotPoser.transform.position - rightHand.pivotPoser.transform.position, rightHand.pivotPoser.TransformDirection(LocalDirectionWithPivotRight));
                    }
                    else
                    {
                        rightHand.toolTransform.rotation = Quaternion.LookRotation(rightMyGrabPoser.transform.position - leftMyGrabPoser.transform.position, leftMyGrabPoser.transform.TransformDirection(LocalDirectionWithPivotLeft));
                        leftHand.toolTransform.rotation = Quaternion.LookRotation(rightHand.pivotPoser.transform.position - leftHand.pivotPoser.transform.position, leftHand.pivotPoser.TransformDirection(LocalDirectionWithPivotLeft));
                    }
                }

                if (leftIsForvard)
                {
                    
                    MyRigidbody.centerOfMass = transform.InverseTransformPoint(rightMyGrabPoser.transform.position);
                    MyRigidbody.velocity = (rightHand.pivotPoser.position - rightMyGrabPoser.transform.position) / Time.fixedDeltaTime* rightHand.GetBlendPose();
                    MyRigidbody.angularVelocity = GetAngularVelocities(leftHand.toolTransform.rotation, rightHand.toolTransform.rotation, rightHand.GetBlendPose());
                }
                else
                {
                    
                    MyRigidbody.centerOfMass = transform.InverseTransformPoint(leftMyGrabPoser.transform.position);
                    MyRigidbody.velocity = (leftHand.pivotPoser.position - leftMyGrabPoser.transform.position) / Time.fixedDeltaTime* leftHand.GetBlendPose();
                    MyRigidbody.angularVelocity = GetAngularVelocities(leftHand.toolTransform.rotation, rightHand.toolTransform.rotation, leftHand.GetBlendPose());
                }
			}
		} else {//one hand
            MyRigidbody.centerOfMass = transform.InverseTransformPoint(GetMyGrabPoserTransform(hand).position);
			MyRigidbody.velocity = (hand.pivotPoser.position - GetMyGrabPoserTransform(hand).position)/Time.fixedDeltaTime* hand.GetBlendPose();
			MyRigidbody.angularVelocity = GetAngularVelocities (hand.pivotPoser.rotation, GetMyGrabPoserTransform(hand).rotation, hand.GetBlendPose());
		}	
	}

	public void GrabEnd(CustomHand hand){
		DettachHand (hand);
		if (!leftHand && !rightHand) {
			saveVariables.LoadProperty (MyRigidbody);
		}
		if (pickReleaseOnce){
			if (!rightHand&&!leftHand){
				ReleaseHand.Invoke ();//sound
			}
		}else{
			ReleaseHand.Invoke ();
		}
        if (leftHand)
        {
            leftHand.SetBlendPose(1);
            leftHand.SetEndFramePos();
        }
        if (rightHand)
        {
            rightHand.SetBlendPose(1);
            rightHand.SetEndFramePos();
        }
    }

	public void Initialize(){
		if (GetComponent<Rigidbody> ()) {
			MyRigidbody = GetComponent<Rigidbody> ();
			saveVariables.SaveProperty (MyRigidbody);
		}
	}


	public void GrabStartCustom(CustomHand hand){
		Vector3 tempPosHandLocal=transform.InverseTransformPoint (hand.PointByPoint(hand.gripPoint));
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
		if (pickReleaseOnce){
			if (!leftHand||!rightHand){
				Grab.Invoke ();//sound
			}
		}else{
			Grab.Invoke ();
		}
        
	}

	public void GrabUpdateCustom(CustomHand hand){
        GrabUpdate(hand);
	}

	public void GrabEndCustom(CustomHand hand){
        GrabEnd(hand);
	}

	public static Vector3 GetAngularVelocities(Quaternion hand,Quaternion fake,float blend)
	{
		Quaternion rotationDelta = hand * Quaternion.Inverse(fake);
		Vector3 angularTarget=Vector3.zero;

		float angle;
		Vector3 axis;
		rotationDelta.ToAngleAxis(out angle, out axis);

		if (angle > 180)
			angle -= 360;

		if (angle != 0 && float.IsNaN(axis.x) == false && float.IsInfinity(axis.x) == false)
		{
			angularTarget = angle * axis *0.95f*blend;
		}
		else
			angularTarget = Vector3.zero;
		return angularTarget;
	}

	void OnDrawGizmosSelected(){
		if (GizmoVisible) {
			Gizmos.color=Color.red;
			Gizmos.DrawLine(transform.TransformPoint(new Vector3 (0, 0, clampHandlePosZ.x)),transform.TransformPoint(new Vector3 (0, 0, clampHandlePosZ.y)));
		}
	}

}
