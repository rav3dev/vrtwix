using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
public class CustomHand : MonoBehaviour
{
    [Header("Hand Interaction Settings")]
    public float gripRadius;
    public float indexRadius;
    public float pinchRadius;

    public Vector3 gripPoint = new Vector3(0, 0, -.1f); // local interaction point positions
    public Vector3 indexPoint = new Vector3(-0.04f, -0.055f, -0.005f);
    public Vector3 pinchPoint = new Vector3(0, 0, -.05f);

    public LayerMask layerColliderChecker;//Layer to interact & grab with

    [Header("Inputs And Actions")]
    public SteamVR_Action_Boolean grabButton;//grab inputs
    public SteamVR_Action_Boolean pinchButton;
    public SteamVR_Action_Vibration hapticSignal = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");//Output of haptic ramble
    public SteamVR_Action_Single squeezeButton;//squeeze input he he
    public SteamVR_Input_Sources handType;//hand type, is it right or left
    public GrabType grabType;// current grab type
    public enum GrabType
    {
        None,
        Select,
        Grip,
        Pinch,
    }
    public SteamVR_renderModel renderModel;// controller model

    [Header("Blend speed settings")]
    [Range(0.001f, 1f)] public float blend = .1f; // hand blend state transition speed
    [Range(0.001f, 1f)] public float blendPosition = .1f; // hand blend position transition speed

    //SYSTEM VARIABLES
    [HideInInspector] public bool smoothBlendPhysicsObject;// smooth pickup of physical object
    [HideInInspector] public Collider[] selectedGripColliders, selectedIndexColliders, selectedPinchColliders;//colliders in a grab radius
     public CustomInteractible selectedIndexInteractible, selectedPinchInteractible, selectedGripInteractible, grabInteractible;// nearest interaction objects and object is currently interacting with
    [HideInInspector] public SteamVR_Behaviour_Skeleton skeleton;// current hand's skeleton
    [HideInInspector] public SteamVR_Skeleton_Poser grabPoser;// poser of object currently interacting with
    [HideInInspector] public Vector3 posSavePoser, rotSavePoser, inverceLocalPosition;//magic variables, which are need to calculate something ( need to know )
    [HideInInspector] public Transform pivotPoser, toolTransform;//Pivot from hands poser, hidden instrument to simplify some calculations
    [HideInInspector] public bool hideController, alwayshideController;//hide controller
    [HideInInspector] public float squeeze;//squeeze strength 

    bool setHandTransform;//Assing position, to pass of the 1st frame, used to be a bug ( maybe remove, need to check if this bug still here )
    float blendToAnimation = 1, blendToPose = 1, blendToPoseMoveObject = 1;//smooth transition for animation and pose

    //STORAGE
    Vector3 endFramePos, oldInterpolatePos;
    Quaternion endFrameRot, oldInterpolateRot;

    void Start()
    {
        if (!pivotPoser)
            pivotPoser = new GameObject().transform;
        pivotPoser.hideFlags = HideFlags.HideInHierarchy;
        if (!toolTransform)
            toolTransform = new GameObject().transform;
        toolTransform.hideFlags = HideFlags.HideInHierarchy;

        if (GetComponent<SteamVR_Behaviour_Pose>())
        {
            handType = GetComponent<SteamVR_Behaviour_Pose>().inputSource;
        }
        else
        {
            Debug.LogError("no SteamVR_Behaviour_Pose on this object");
        }
        if (GetComponentInChildren<SteamVR_Behaviour_Skeleton>())
        {
            skeleton = GetComponentInChildren<SteamVR_Behaviour_Skeleton>();
        }
        if (GetComponentInChildren<SteamVR_renderModel>())
        {
            renderModel = GetComponentInChildren<SteamVR_renderModel>();
            StartCoroutine(hideControllerCoroutine());
        }
        skeleton.BlendToSkeleton();
        
    }

    void FixedUpdate()
    {
        SelectObject(PointByPoint(indexPoint), GrabType.Select, selectedIndexColliders, ref selectedIndexInteractible);
        squeeze = squeezeButton.GetAxis(handType);
        PivotUpdate();
        GrabCheck();

        if (grabPoser && grabInteractible)
        {
            GrabUpdate();
            return;
        }
        
        SelectObject(PointByPoint(pinchPoint), GrabType.Pinch, selectedPinchColliders, ref selectedPinchInteractible);
        SelectObject(PointByPoint(gripPoint), GrabType.Grip, selectedGripColliders, ref selectedGripInteractible);

    }
    IEnumerator hideControllerCoroutine() {
        while (true)
        {
            if (renderModel.transform.childCount > 0)
            {
                renderModelVisible(hideController);
                break;
            }
            yield return 0;
        }
    }

    void GrabCheck()
    {
        if (grabType != GrabType.None && grabInteractible)
        {
            if (grabType == GrabType.Pinch && pinchButton.GetStateUp(handType))
            {
                grabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                GrabEnd();
            }
            if (grabType == GrabType.Grip && grabButton.GetStateUp(handType))
            {
                grabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                GrabEnd();
            }
        }

        if (!grabPoser)
        {
            BlendControll(true);

            CustomInteractible oldgrabInteractible = grabInteractible;
            if (selectedIndexInteractible)
            {
                grabInteractible = selectedIndexInteractible;
                InteractionProcessor(oldgrabInteractible, grabInteractible, GrabType.Select);
            }
            else if (selectedPinchInteractible && pinchButton.GetStateDown(handType))
            {
                grabInteractible = selectedPinchInteractible;
                InteractionProcessor(oldgrabInteractible, grabInteractible, GrabType.Pinch);
            }
            else if (selectedGripInteractible && grabButton.GetStateDown(handType))
            {
                grabInteractible = selectedGripInteractible;
                InteractionProcessor(oldgrabInteractible, grabInteractible, GrabType.Grip);
            }
        }
    }

    private void InteractionProcessor(CustomInteractible oldgrabInteractible, CustomInteractible grabInteractible, GrabType procGrabType)
    {
        if (grabInteractible != oldgrabInteractible)
        {
            if (oldgrabInteractible)
                oldgrabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
            if (grabInteractible)
            {
                grabInteractible.SendMessage("GrabStart", this, SendMessageOptions.DontRequireReceiver);
                setHandTransform = false;
                grabType = procGrabType;
                renderModelVisible(!grabInteractible.hideController);
                SkeletonUpdate();
                blendToPose = 1;
                blendToPoseMoveObject = 1;
                endFramePos = transform.parent.InverseTransformPoint(skeleton.transform.position);
                endFrameRot = skeleton.transform.rotation;
            }
        }
    }

    public void GrabUpdateCustom()
    {
        if (grabPoser)
        {
            skeleton.BlendToPoser(grabPoser, 0);

            posSavePoser = grabPoser.transform.localPosition;
            rotSavePoser = grabPoser.transform.localEulerAngles;

            grabPoser.transform.rotation = transform.rotation * grabPoser.GetBlendedPose(skeleton).rotation;
            grabPoser.transform.position = transform.TransformPoint(grabPoser.GetBlendedPose(skeleton).position);

            PivotUpdate();

            inverceLocalPosition = grabPoser.transform.InverseTransformPoint(transform.position);

            grabPoser.transform.localPosition = posSavePoser;
            grabPoser.transform.localEulerAngles = rotSavePoser;

            skeleton.transform.position = grabPoser.transform.TransformPoint(inverceLocalPosition);
            skeleton.transform.rotation = grabPoser.transform.rotation * Quaternion.Inverse(grabPoser.GetBlendedPose(skeleton).rotation);
            BlendControll(false);
            skeleton.skeletonBlend = blendToAnimation;
        }
    }

    void BlendControll(bool positive)
    {
        if (positive)
        {
            if (blend > 0)
            {
                blendToAnimation += 1f / blend * Time.deltaTime;
                blendToAnimation = Mathf.Clamp01(blendToAnimation);
                blendToPose += 1f / blendPosition * Time.deltaTime;
                blendToPose = Mathf.Clamp01(blendToPose);
                blendToPoseMoveObject += 1f / blendPosition * Time.deltaTime;
                blendToPoseMoveObject = Mathf.Clamp01(blendToPoseMoveObject);
            }
            else
            {
                blendToAnimation = 1;
            }
        }
        else
        {
            if (blend > 0)
            {
                blendToAnimation -= 1f / blend * Time.deltaTime;
                blendToAnimation = Mathf.Clamp01(blendToAnimation);
                blendToPose -= 1f / blendPosition * Time.deltaTime;
                blendToPose = Mathf.Clamp01(blendToPose);
                blendToPoseMoveObject -= 1f / blendPosition * Time.deltaTime;
                blendToPoseMoveObject = Mathf.Clamp01(blendToPoseMoveObject);
            }
            else
            {
                blendToAnimation = 0;
            }
        }
    }

    void GrabUpdate()
    {

        if (grabPoser)
        {
            skeleton.BlendToPoser(grabPoser, 0);

            posSavePoser = grabPoser.transform.localPosition;
            rotSavePoser = grabPoser.transform.localEulerAngles;

            grabPoser.transform.rotation = transform.rotation * grabPoser.GetBlendedPose(skeleton).rotation;
            grabPoser.transform.position = transform.TransformPoint(grabPoser.GetBlendedPose(skeleton).position);

            PivotUpdate();

            inverceLocalPosition = grabPoser.transform.InverseTransformPoint(transform.position);

            grabPoser.transform.localPosition = posSavePoser;
            grabPoser.transform.localEulerAngles = rotSavePoser;

            grabInteractible.SendMessage("GrabUpdate", this, SendMessageOptions.DontRequireReceiver);
            BlendControll(false);
            skeleton.skeletonBlend = blendToAnimation;
        }
    }

    public void HapticResponse(float hlength, float hfreq, float hpower)
    {
        hapticSignal.Execute(0, hlength, hfreq, hpower, handType);

    }

    void LateUpdate()
    {
        if (grabPoser)
        {

            if (setHandTransform)
            {

                skeleton.transform.position = grabPoser.transform.TransformPoint(inverceLocalPosition);
                skeleton.transform.rotation = grabPoser.transform.rotation * Quaternion.Inverse(grabPoser.GetBlendedPose(skeleton).rotation);

                skeleton.transform.position = Vector3.Lerp(skeleton.transform.position, transform.parent.TransformPoint(endFramePos), blendToPose);
                skeleton.transform.rotation = Quaternion.Lerp(skeleton.transform.rotation, endFrameRot, blendToPose);

                oldInterpolatePos = skeleton.transform.position;
                oldInterpolateRot = skeleton.transform.rotation;
            }
            else
            {
                setHandTransform = true;
            }
        }
        else
        {
            skeleton.transform.position = Vector3.Lerp(transform.parent.TransformPoint(endFramePos), skeleton.transform.parent.position, blendToPose);
            skeleton.transform.rotation = Quaternion.Lerp(endFrameRot, skeleton.transform.parent.rotation, blendToPose);
        }


    }

    public void renderModelVisible(bool visible)
    {
        if (renderModel)
        {
            if (alwayshideController)
                renderModel.SetMeshRendererState(false);
            else
                renderModel.SetMeshRendererState(visible);
        }
    }

    void GrabEnd()
    {
        endFramePos = transform.parent.InverseTransformPoint(oldInterpolatePos);
        endFrameRot = oldInterpolateRot;

        skeleton.transform.localPosition = Vector3.zero;
        skeleton.transform.localEulerAngles = Vector3.zero; ///save coord
		skeleton.BlendToSkeleton(blend);

        renderModelVisible(!hideController);
        blendToPose = 0;
        blendToPoseMoveObject = 0;
        grabPoser = null;
        grabInteractible = null;
        grabType = GrabType.None;
    }

    public void DetachHand()
    {
        GrabEnd();
    }

    void SelectObject(Vector3 selectPoint, GrabType grabType, Collider[] colliders, ref CustomInteractible interactible)
    {
        if (!grabPoser)
        {
            colliders = Physics.OverlapSphere(selectPoint, gripRadius, layerColliderChecker);
            interactible = null;
            float tempCloseDistance = float.MaxValue;
            for (int i = 0; i < colliders.Length; i++)
            {
                CustomInteractible tempCustomInteractible = colliders[i].GetComponentInParent<CustomInteractible>();
                if (tempCustomInteractible != null && tempCustomInteractible.isInteractible && tempCustomInteractible.grabType == grabType)
                {
                    if (Vector3.Distance(tempCustomInteractible.transform.position, selectPoint) < tempCloseDistance)
                    {
                        tempCloseDistance = Vector3.Distance(tempCustomInteractible.transform.position, selectPoint);
                        interactible = tempCustomInteractible;
                    }
                }
            }
        }
        else if(grabType == GrabType.Select) // HANDLE SELECT TYPE
        {
            if (interactible)
            {
                colliders = Physics.OverlapSphere(selectPoint, indexRadius * 2f, layerColliderChecker);
                if (colliders == null || colliders.Length == 0)
                {
                    interactible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                    GrabEnd();
                    interactible = null;
                    return;
                }
                for (int i = 0; i < colliders.Length; i++)
                {
                    CustomInteractible tempCustomInteractible = colliders[i].GetComponentInParent<CustomInteractible>();
                    if (tempCustomInteractible && tempCustomInteractible == interactible)
                    {
                        return;
                    }
                }
                interactible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                GrabEnd();
                interactible = null;
            }
        }
    }

    public void SkeletonUpdate()
    {
        if (skeleton)
        {
            if (grabPoser)
            {
                skeleton.BlendToPoser(grabPoser);
                PivotUpdate();
            }
        }
    }

    public void PivotUpdate()
    {
        if (grabPoser)
        {
            pivotPoser.rotation = transform.rotation * grabPoser.GetBlendedPose(skeleton).rotation;
            pivotPoser.position = transform.TransformPoint(grabPoser.GetBlendedPose(skeleton).position);
        }
    }

    public Vector3 PointByPoint(Vector3 point)
    {
        if (handType == SteamVR_Input_Sources.RightHand)
            return transform.TransformPoint(Vector3.Scale(new Vector3(-1, 1, 1), point));
        if (handType == SteamVR_Input_Sources.LeftHand)
            return transform.TransformPoint(point);
        return Vector3.zero;
    }

    public void SetEndFramePos() {
        endFramePos = transform.parent.InverseTransformPoint(skeleton.transform.position);
    }

    public void SetBlendPose(float setBlend) {
        blendToPoseMoveObject = setBlend;
    }

    public float GetBlendPose()
    {
        if (smoothBlendPhysicsObject)
            return 1 - blendToPoseMoveObject;
        else
            return 1;

    }

    void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(PointByPoint(pinchPoint), gripRadius);
        Gizmos.DrawWireSphere(PointByPoint(gripPoint), pinchRadius);
        Gizmos.DrawWireSphere(PointByPoint(indexPoint), indexRadius);
    }


}
