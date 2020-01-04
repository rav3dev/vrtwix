using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
public class Button : CustomInteractible
{
    public float distanseToPress; //дистанция по достижении которой кнопка нажмется
    [Range(.1f,1f)]
    public float DistanceMultiply=.1f; //замедление чуствительности кнопки.
    public Transform MoveObject; //сама кнопка которая движется
    public UnityEvent ButtonDown, ButtonUp, ButtonUpdate; // ивенты

    float StartButtonPosition; //Техническая переменная на старте присваивается позиция кнопки отжатой
    bool press; //проверка нажатия кнопки чтобы ButtonDown вызвать 1 раз
    void Start()
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
        if ((rightHand || leftHand) && GetMyGrabPoserTransform(hand))
        {
            MoveObject.localPosition = new Vector3(0, 0, StartButtonPosition);
            DettachHand(hand);

            GetComponentInChildren<MeshRenderer>().material.color = Color.green;
        }
		ReleaseHand.Invoke ();
    }
}
