using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Button : CustomInteractible
{
    public float distanseToPress; //button press reach distance
    [Range(.1f,1f)]
    public float DistanceMultiply=.1f; //button sensetivity slowdown
    public Transform MoveObject; //movable button object
    public UnityEvent ButtonDown, ButtonUp, ButtonUpdate; // events

    float StartButtonPosition; //tech variable, assigned at start of pressed button
    bool press; //button check, to ButtonDown call 1 time
    void Awake()
    {
        StartButtonPosition = MoveObject.localPosition.z;
    }
    

    void GrabStart(CustomHand hand)
    {
        SetInteractibleVariable(hand);
        hand.SkeletonUpdate();
        hand.grabType = CustomHand.GrabType.Select;
		Grab.Invoke ();
    }

    void GrabUpdate(CustomHand hand)
    {
        if ((rightHand || leftHand) && GetMyGrabPoserTransform(hand))
        {
            hand.SkeletonUpdate();
            GetComponentInChildren<MeshRenderer>().material.color = Color.grey;
            float tempDistance = Mathf.Clamp(StartButtonPosition-(StartButtonPosition-transform.InverseTransformPoint(hand.PivotPoser.position).z)*DistanceMultiply, StartButtonPosition, distanseToPress);
            if (tempDistance >= distanseToPress)
            {
                GetComponentInChildren<MeshRenderer>().material.color = Color.blue;
                if (!press)
                {
                    ButtonDown.Invoke();
                }
                press = true;
                ButtonUpdate.Invoke();
            }
            else
            {
                if (press)
                {
                    ButtonUp.Invoke();
                }
                press = false;
            }
            MoveObject.localPosition = new Vector3(0, 0, tempDistance);
            MoveObject.rotation = Quaternion.LookRotation(GetMyGrabPoserTransform(hand).forward, hand.PivotPoser.up);
            hand.GrabUpdateCustom();
        }
    }

    void GrabEnd(CustomHand hand)
    {
        //if ((rightHand || leftHand) && GetMyGrabPoserTransform(hand))
        //{
            MoveObject.localPosition = new Vector3(0, 0, StartButtonPosition);
            DettachHand(hand);

            GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        //}
		ReleaseHand.Invoke ();
    }
}
