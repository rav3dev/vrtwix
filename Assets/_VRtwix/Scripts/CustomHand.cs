using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
public class CustomHand : MonoBehaviour
{
    public float gripRadius, indexRadius, pinchRadius;//different grabs radiuses
    public Vector3 grabPoint = new Vector3(0, 0, -.1f), indexPoint = new Vector3(-0.04f, -0.055f, -0.005f), pinchPoint = new Vector3(0, 0, -.05f);//локальная позиция точки захвата левой руки

    public LayerMask layerColliderChecker;//Layer to interact & grab with
    public SteamVR_Action_Boolean grabButton, pinchButton;//grab inputs
    public SteamVR_Action_Single SqueezeButton;//squeeze input he he
    public SteamVR_Input_Sources handType;//hand type, is it right or left
    public GrabType grabType;// current grab type
    public enum GrabType
    {
        None,
        Select,
        Grip,
        Pinch,
    }
    public SteamVR_RenderModel RenderModel;// controller model
    [Range(0.001f, 1f)]
    public float blend = .1f, blendPosition = .1f;// hand blend transition speed
    public bool smoothBlendPhysicsObject;// smooth pickup of physical object
    public Collider[] SelectedGpibColliders, SelectedIndexColliders, SelectedPinchColliders;//colliders in a grab radius
    public CustomInteractible SelectedIndexInteractible, SelectedPinchInteractible, SelectedGpibInteractible, GrabInteractible;// nearest interaction objects and object is currently interacting with
    public SteamVR_Behaviour_Skeleton skeleton;// current hand's skeleton
    public SteamVR_Skeleton_Poser grabPoser;// poser of object currently interacting with
    public Vector3 posSavePoser, rotSavePoser, inverceLocalPosition;//magic variables, which are need to calculate something ( need to know )
    public Transform PivotPoser, ToolTransform;//Pivot from hands poser, hidden instrument to simplify some calculations
    public bool HideController, alwaysHideController;//hide controller
    public float Squeeze;//squeeze strength 
    public SteamVR_Action_Vibration hapticSignal = SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");//Output of haptic ramble
    bool setHandTransform;//Assing position, to pass of the 1st frame, used to be a bug ( maybe remove, need to check if this bug still here )
    float blendToAnimation = 1, blendToPose = 1, blendToPoseMoveObject = 1;//smooth transition for animation and pose
    
    Vector3 endFramePos, oldInterpolatePos;
    Quaternion endFrameRot, oldInterpolateRot;

    //protected SteamVR_Events.Action renderModelLoadedAction;

    //protected void Awake()
    //{
    //    renderModelLoadedAction = SteamVR_Events.RenderModelLoadedAction(OnRenderModelLoaded);
    //}

    //private void OnRenderModelLoaded(SteamVR_RenderModel loadedRenderModel, bool success)
    //{
    //    print(1);
    //}

    void Start()
    {
        if (!PivotPoser)
            PivotPoser = new GameObject().transform;
        PivotPoser.hideFlags = HideFlags.HideInHierarchy;
        if (!ToolTransform)
            ToolTransform = new GameObject().transform;
        ToolTransform.hideFlags = HideFlags.HideInHierarchy;

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
        if (GetComponentInChildren<SteamVR_RenderModel>())
        {
            RenderModel = GetComponentInChildren<SteamVR_RenderModel>();
            StartCoroutine(HideControllerCoroutine());
        }
        skeleton.BlendToSkeleton();
        
    }

    void FixedUpdate()
    {
        SelectIndexObject();
        Squeeze = SqueezeButton.GetAxis(handType);
        PivotUpdate();
        GrabCheck();

        if (grabPoser && GrabInteractible)
        {
            GrabUpdate();
            return;
        }

        SelectPinchObject();
        SelectGribObject();

    }
    IEnumerator HideControllerCoroutine() {
        while (true)
        {
            if (RenderModel.transform.childCount > 0)
            {
                RenderModelVisible(HideController);
                break;
            }
            yield return 0;
        }
    }

    void GrabCheck()
    {
        if (grabType != GrabType.None && GrabInteractible)
        {
            if (grabType == GrabType.Pinch && pinchButton.GetStateUp(handType))
            {
                GrabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                GrabEnd();
            }
            if (grabType == GrabType.Grip && grabButton.GetStateUp(handType))
            {
                GrabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                GrabEnd();
            }
        }

        if (!grabPoser)
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

            CustomInteractible OldGrabInteractible = GrabInteractible;
            if (SelectedIndexInteractible)
            {
                GrabInteractible = SelectedIndexInteractible;
                if (GrabInteractible != OldGrabInteractible)
                {
                    if (OldGrabInteractible)
                        OldGrabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                    if (GrabInteractible)
                    {
                        GrabInteractible.SendMessage("GrabStart", this, SendMessageOptions.DontRequireReceiver);
                        setHandTransform = false;
                        grabType = GrabType.Select;
                        RenderModelVisible(!GrabInteractible.HideController);
                        SkeletonUpdate();
                        blendToPose = 1;
                        blendToPoseMoveObject = 1;
                        endFramePos = transform.parent.InverseTransformPoint(skeleton.transform.position);
                        endFrameRot = skeleton.transform.rotation;
                    }
                }
            }
            else
            {
                if (SelectedPinchInteractible && pinchButton.GetStateDown(handType))
                {
                    GrabInteractible = SelectedPinchInteractible;
                    if (GrabInteractible != OldGrabInteractible)
                    {
                        if (OldGrabInteractible)
                            OldGrabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                        if (GrabInteractible)
                        {
                            GrabInteractible.SendMessage("GrabStart", this, SendMessageOptions.DontRequireReceiver);
                            setHandTransform = false;
                            grabType = GrabType.Pinch;
                            RenderModelVisible(!GrabInteractible.HideController);
                            SkeletonUpdate();
                            blendToPose = 1;
                            blendToPoseMoveObject = 1;
                            endFramePos = transform.parent.InverseTransformPoint(skeleton.transform.position);
                            endFrameRot = skeleton.transform.rotation;
                        }
                    }
                }
                else
                {
                    if (SelectedGpibInteractible && grabButton.GetStateDown(handType))
                    {
                        GrabInteractible = SelectedGpibInteractible;
                        if (GrabInteractible != OldGrabInteractible)
                        {
                            if (OldGrabInteractible)
                                OldGrabInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                            if (GrabInteractible)
                            {
                                GrabInteractible.SendMessage("GrabStart", this, SendMessageOptions.DontRequireReceiver);
                                setHandTransform = false;
                                grabType = GrabType.Grip;
                                RenderModelVisible(!GrabInteractible.HideController);
                                SkeletonUpdate();
                                blendToPose = 1;
                                blendToPoseMoveObject = 1;
                                endFramePos = transform.parent.InverseTransformPoint(skeleton.transform.position);
                                endFrameRot = skeleton.transform.rotation;
                            }
                        }
                    }
                }
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
            skeleton.skeletonBlend = blendToAnimation;
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

            GrabInteractible.SendMessage("GrabUpdate", this, SendMessageOptions.DontRequireReceiver);
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

    public void RenderModelVisible(bool visible)
    {
        if (RenderModel)
        {
            if (alwaysHideController)
                RenderModel.SetMeshRendererState(false);
            else
                RenderModel.SetMeshRendererState(visible);
        }
    }

    void GrabEnd()
    {
        endFramePos = transform.parent.InverseTransformPoint(oldInterpolatePos);
        endFrameRot = oldInterpolateRot;

        skeleton.transform.localPosition = Vector3.zero;
        skeleton.transform.localEulerAngles = Vector3.zero; ///save coord
		skeleton.BlendToSkeleton(blend);

        RenderModelVisible(!HideController);
        blendToPose = 0;
        blendToPoseMoveObject = 0;
        grabPoser = null;
        GrabInteractible = null;
        grabType = GrabType.None;
    }

    public void DetachHand()
    {
        GrabEnd();
    }

    void SelectIndexObject()
    {
        if (!grabPoser)
        {
            SelectedIndexColliders = Physics.OverlapSphere(IndexPoint(), indexRadius, layerColliderChecker);
            SelectedIndexInteractible = null;
            float tempCloseDistance = float.MaxValue;
            for (int i = 0; i < SelectedIndexColliders.Length; i++)
            {
                CustomInteractible tempCustomInteractible = SelectedIndexColliders[i].GetComponentInParent<CustomInteractible>();
                if (tempCustomInteractible != null && tempCustomInteractible.isInteractible && tempCustomInteractible.grabType == GrabType.Select)
                {
                    if (Vector3.Distance(tempCustomInteractible.transform.position, IndexPoint()) < tempCloseDistance)
                    {
                        tempCloseDistance = Vector3.Distance(tempCustomInteractible.transform.position, IndexPoint());
                        SelectedIndexInteractible = tempCustomInteractible;
                    }
                }
            }
        }
        else
        {
            if (SelectedIndexInteractible)
            {
                SelectedIndexColliders = Physics.OverlapSphere(IndexPoint(), indexRadius * 2f, layerColliderChecker);
                if (SelectedIndexColliders == null || SelectedIndexColliders.Length == 0)
                {
                    SelectedIndexInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                    GrabEnd();
                    SelectedIndexInteractible = null;
                    return;
                }
                for (int i = 0; i < SelectedIndexColliders.Length; i++)
                {
                    CustomInteractible tempCustomInteractible = SelectedIndexColliders[i].GetComponentInParent<CustomInteractible>();
                    if (tempCustomInteractible && tempCustomInteractible == SelectedIndexInteractible)
                    {
                        return;
                    }
                }
                SelectedIndexInteractible.SendMessage("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
                GrabEnd();
                SelectedIndexInteractible = null;
            }
        }
    }

    void SelectPinchObject()
    {
        if (!grabPoser)
        {
            SelectedPinchColliders = Physics.OverlapSphere(PinchPoint(), pinchRadius, layerColliderChecker);
            SelectedPinchInteractible = null;
            float tempCloseDistance = float.MaxValue;
            for (int i = 0; i < SelectedPinchColliders.Length; i++)
            {
                CustomInteractible tempCustomInteractible = SelectedPinchColliders[i].GetComponentInParent<CustomInteractible>();
                if (tempCustomInteractible != null && tempCustomInteractible.isInteractible && tempCustomInteractible.grabType == GrabType.Pinch)
                {
                    if (Vector3.Distance(tempCustomInteractible.transform.position, PinchPoint()) < tempCloseDistance)
                    {
                        tempCloseDistance = Vector3.Distance(tempCustomInteractible.transform.position, PinchPoint());
                        SelectedPinchInteractible = tempCustomInteractible;
                    }
                }
            }
        }
    }

    void SelectGribObject()
    {
        if (!grabPoser)
        {
            SelectedGpibColliders = Physics.OverlapSphere(GrabPoint(), gripRadius, layerColliderChecker);
            SelectedGpibInteractible = null;
            float tempCloseDistance = float.MaxValue;
            for (int i = 0; i < SelectedGpibColliders.Length; i++)
            {
                CustomInteractible tempCustomInteractible = SelectedGpibColliders[i].GetComponentInParent<CustomInteractible>();
                if (tempCustomInteractible != null && tempCustomInteractible.isInteractible && tempCustomInteractible.grabType == GrabType.Grip)
                {
                    if (Vector3.Distance(tempCustomInteractible.transform.position, GrabPoint()) < tempCloseDistance)
                    {
                        tempCloseDistance = Vector3.Distance(tempCustomInteractible.transform.position, GrabPoint());
                        SelectedGpibInteractible = tempCustomInteractible;
                    }
                }
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
            PivotPoser.rotation = transform.rotation * grabPoser.GetBlendedPose(skeleton).rotation;
            PivotPoser.position = transform.TransformPoint(grabPoser.GetBlendedPose(skeleton).position);
        }
    }

    public Vector3 GrabPoint()
    {
        if (handType == SteamVR_Input_Sources.RightHand)
            return transform.TransformPoint(Vector3.Scale(new Vector3(-1, 1, 1), grabPoint));
        if (handType == SteamVR_Input_Sources.LeftHand)
            return transform.TransformPoint(grabPoint);
        return Vector3.zero;
    }

    public Vector3 PinchPoint()
    {
        if (handType == SteamVR_Input_Sources.RightHand)
            return transform.TransformPoint(Vector3.Scale(new Vector3(-1, 1, 1), pinchPoint));
        if (handType == SteamVR_Input_Sources.LeftHand)
            return transform.TransformPoint(pinchPoint);
        return Vector3.zero;
    }

    public Vector3 IndexPoint()
    {
        if (handType == SteamVR_Input_Sources.RightHand)
            return transform.TransformPoint(Vector3.Scale(new Vector3(-1, 1, 1), indexPoint));
        if (handType == SteamVR_Input_Sources.LeftHand)
            return transform.TransformPoint(indexPoint);
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
        Gizmos.DrawWireSphere(PinchPoint(), gripRadius);
        Gizmos.DrawWireSphere(GrabPoint(), pinchRadius);
        Gizmos.DrawWireSphere(IndexPoint(), indexRadius);
    }


}
