using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Button : CustomInteractible
{
    public float distanseToPress;
    public float DistanceDettach = .05f;
    [Range(.1f,1f)]
    public float DistanceMultiply=.1f;
    public Transform MoveObject;
    public UnityEvent ButtonDown, ButtonUp, ButtonUpdate;
    float startZCoordinate,StartButtonPosition;
    bool press;
    // Start is called before the first frame update
    void Start()
    {
        StartButtonPosition = MoveObject.localPosition.z;
    }
    

    void GrabStart(CustomHand hand)
    {
        SetInteractibleVariable(hand);
        hand.SkeletonUpdate();
        startZCoordinate = transform.InverseTransformPoint(hand.PivotPoser.position).z;
        hand.grabType = CustomHand.GrabType.Select;
		Grab.Invoke ();
    }

    void GrabUpdate(CustomHand hand)
    {
        if ((rightHand || leftHand) && GetMyGrabPoserTransform(hand))
        {
            hand.SkeletonUpdate();
            GetComponentInChildren<MeshRenderer>().material.color = new Color(Random.value, Random.value, Random.value);// Color.grey;
            float percentHandPose = Mathf.InverseLerp(StartButtonPosition, distanseToPress, transform.InverseTransformPoint(hand.PivotPoser.position).z);
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
        if ((rightHand || leftHand) && GetMyGrabPoserTransform(hand))
        {
            MoveObject.localPosition = new Vector3(0, 0, StartButtonPosition);
            DettachHand(hand);

            GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        }
		ReleaseHand.Invoke ();
    }
}
