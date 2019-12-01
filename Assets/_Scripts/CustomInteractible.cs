using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class CustomInteractible : MonoBehaviour {

    public bool isInteractible = true;

    public List<SteamVR_Skeleton_Poser> grabPoints;
    public CustomHand leftHand, rightHand;
    public SteamVR_Skeleton_Poser leftMyGrabPoser, rightMyGrabPoser;
    public bool TwoHanded, HideController;
	public CustomHand.GrabType grabType=CustomHand.GrabType.Grip;



    public Transform GetMyGrabPoserTransform() {
        if (leftMyGrabPoser)
            return leftMyGrabPoser.transform;
        if (rightMyGrabPoser)
            return rightMyGrabPoser.transform;
        return null;
    }

    public Transform GetMyGrabPoserTransform(CustomHand hand) {
        if (hand.handType == SteamVR_Input_Sources.LeftHand && leftMyGrabPoser)
            return leftMyGrabPoser.transform;
        if (hand.handType == SteamVR_Input_Sources.RightHand && rightMyGrabPoser)
            return rightMyGrabPoser.transform;
        return null;
    }

	public SteamVR_Skeleton_Poser GetMyGrabPoser(CustomHand hand) {
		if (hand.handType == SteamVR_Input_Sources.LeftHand && leftMyGrabPoser)
			return leftMyGrabPoser;
		if (hand.handType == SteamVR_Input_Sources.RightHand && rightMyGrabPoser)
			return rightMyGrabPoser;
		return null;
	}



    public Transform CloseObject(Vector3 tempPoint) {
        Transform TempClose = null;
        if (grabPoints != null) {
            float MinDistance = float.MaxValue;
            for (int i = 0; i < grabPoints.Count; i++) {
                if (Vector3.Distance(tempPoint, grabPoints[i].transform.position) < MinDistance) {
                    MinDistance = Vector3.Distance(tempPoint, grabPoints[i].transform.position);
                    TempClose = grabPoints[i].transform;
                }
            } 
        }
        return TempClose;
    }

    public SteamVR_Skeleton_Poser ClosePoser(Vector3 tempPoint) {
        SteamVR_Skeleton_Poser TempClose = null;
        if (grabPoints != null) {
            float MinDistance = float.MaxValue;
            for (int i = 0; i < grabPoints.Count; i++) {
                if (grabPoints[i] != leftMyGrabPoser && grabPoints[i] != rightMyGrabPoser) {
                    if (Vector3.Distance(tempPoint, grabPoints[i].transform.position) < MinDistance) {
                        MinDistance = Vector3.Distance(tempPoint, grabPoints[i].transform.position);
                        TempClose = grabPoints[i];
                    }
                }
            }
        }
        return TempClose;
    }

    public void SetInteractibleVariable(CustomHand hand) {
        if (hand.handType == SteamVR_Input_Sources.LeftHand) {
            if (leftHand)
                DettachHand(leftHand);
            if (!TwoHanded && rightHand)
                DettachHand(rightHand);
            leftMyGrabPoser = ClosePoser(hand.GrabPoint());
            if (leftMyGrabPoser) {
                hand.grabPoser = leftMyGrabPoser;
                leftHand = hand;
                leftHand.SkeletonUpdate();
            }
            //haptic
        }
        if (hand.handType == SteamVR_Input_Sources.RightHand) {
            if (rightHand)
                DettachHand(rightHand);
            if (!TwoHanded && leftHand)
                DettachHand(leftHand);
            rightMyGrabPoser = ClosePoser(hand.GrabPoint());
            if (rightMyGrabPoser) {
                hand.grabPoser = rightMyGrabPoser;
                rightHand = hand;
                rightHand.SkeletonUpdate();
            }
            //haptic
        }
    }

    public bool CanSelected(CustomHand hand) {
        if (!leftHand && !rightHand)
        {
            return true;
        } else {
            if ((leftHand && leftHand == hand) || (rightHand && rightHand == hand))
            {
                return true;
            }
            else
                return false;
        }
    }

	public void SetInteractibleVariable(CustomHand hand,SteamVR_Skeleton_Poser poser){
		if (hand.handType == SteamVR_Input_Sources.LeftHand) {
			if (leftHand)
				DettachHand (leftHand);
			if (!TwoHanded && rightHand)
				DettachHand (rightHand);
			leftMyGrabPoser = poser;
			if (leftMyGrabPoser) {
				hand.grabPoser = leftMyGrabPoser;
				leftHand = hand;
				leftHand.SkeletonUpdate ();
			}
			//haptic
		}
		if (hand.handType == SteamVR_Input_Sources.RightHand) {
			if (rightHand)
				DettachHand (rightHand);
			if (!TwoHanded && leftHand)
				DettachHand (leftHand);
			rightMyGrabPoser = poser;
			if (rightMyGrabPoser) {
				hand.grabPoser = rightMyGrabPoser;
				rightHand = hand;
				rightHand.SkeletonUpdate ();
			}
			//haptic
		}
	}



	public void DettachHand(CustomHand hand){
		hand.DetachHand ();
		if (hand.handType == SteamVR_Input_Sources.LeftHand) {
			leftMyGrabPoser = null;
			leftHand = null;
		}
		if (hand.handType == SteamVR_Input_Sources.RightHand) {
			rightMyGrabPoser = null;
			rightHand = null;
		}
	}

	public void DettachHands(){
		if (leftHand) {
			leftHand.DetachHand ();
			leftMyGrabPoser = null;
			leftHand = null;
		}
		if (rightHand) {
			rightHand.DetachHand ();
			rightMyGrabPoser = null;
			rightHand = null;
		}
	}

	public void SetInteractible(bool b){
		
	}

}
