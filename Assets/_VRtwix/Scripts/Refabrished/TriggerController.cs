using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class TriggerController : MonoBehaviour
{
    public float triggerAxis;                                                                                       // trigger press state %
    public Transform triggerDestination;                                                                            // trigger final position when max pressed
    public float wallSpace = 0.2f;                                                                                  // DISTANCE BETWEEN "WALL" AND TRIGGER CLICK
    public SteamVR_Action_Single triggerState = SteamVR_Input.GetAction<SteamVR_Action_Single>("Trigger");          //input
    public SteamVR_Action_Boolean triggerClick = SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerClick");   //input
    public WeaponCore weaponController;                                                                             // controller to handle shot event etc.
    public int state = 0; // 0 - default, 1 - clicked, 2 - unlcicked
    Vector3 triggerDefaultPosition;
    Vector3 triggerDefaultRotation;
    private void Start()
    {
        SaveDefaults();
    }

    void SaveDefaults()
    {
        triggerDefaultPosition = transform.localPosition;
        triggerDefaultRotation = transform.localEulerAngles;
    }

    public void CustomUpdate(CustomHand hand)
    {
        if (weaponController.GetMyGrabPoser(hand) == weaponController.grabPoints[0]) // check if hand holding a handle with trigger
        {
            triggerAxis = triggerState.GetAxis(hand.handType);

            if (triggerClick.GetStateDown(hand.handType) && state == 0)
            {
                state = 1;
                //weaponController.Shoot();
            }
            else if (triggerClick.GetStateUp(hand.handType))
            {
                state = 2;
            }
            else if (state == 2 && triggerAxis < 0.5f)
            {
                state = 0;
            }

            TriggerTransformMovement(triggerAxis);
        } else
        {
            return;
        }
        
    }

    void TriggerTransformMovement(float axis)
    {
        if (state != 1)
        {
            axis = axis * (1f - wallSpace); // MOFIDY AXIS IF IT'S NOT CLICKED
        }

        transform.localPosition = Vector3.Lerp(triggerDefaultPosition, triggerDestination.localPosition, axis);
        transform.localEulerAngles = Vector3.Lerp(triggerDefaultRotation, triggerDestination.localEulerAngles, axis);
    }
}
