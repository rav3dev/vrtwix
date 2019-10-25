using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
using Valve.VR.InteractionSystem;
public class CustomHand : MonoBehaviour {
	public float gribRadius,indexRadius,pinchRadius;
	public LayerMask layerColliderChecker;
	public SteamVR_Action_Boolean grabButton,pinchButton;
    public SteamVR_Action_Single SqueezeButton;
	public SteamVR_Input_Sources handType;
	public GrabType grabType;
	public enum GrabType
	{
		None,
		Select,
		Grip,
		Pinch,
	}
	public SteamVR_RenderModel RenderModel;

	public int ind;
	public CustomHand otherCustomHand;
	public Collider[] SelectedGpibColliders,SelectedIndexColliders,SelectedPinchColliders;
	public CustomInteractible SelectedIndexInteractible,SelectedPinchInteractible,SelectedGpibInteractible,GrabInteractible;
	public SteamVR_Behaviour_Skeleton skeleton;
	public SteamVR_Skeleton_Poser grabPoser;
	public Vector3 posSavePoser,rotSavePoser,inverceLocalPosition;

	public Transform PivotPoser, ToolTransform;
	public bool HideController;
//	[HideInInspector]
	public float Squeeze;
	public SteamVR_Action_Boolean boola;
	public SteamVR_Action_Vibration hapticSignal=SteamVR_Input.GetAction<SteamVR_Action_Vibration>("Haptic");
	bool setHandTransform;
	// Use this for initialization
	void Start () {
		if (!PivotPoser)
            PivotPoser = new GameObject ().transform;
        PivotPoser.hideFlags = HideFlags.HideInHierarchy;
		if (!ToolTransform)
			ToolTransform = new GameObject ().transform;
		ToolTransform.hideFlags = HideFlags.HideInHierarchy;
		
		if (GetComponent<SteamVR_Behaviour_Pose> ()) {
			handType = GetComponent<SteamVR_Behaviour_Pose> ().inputSource;
		} else {
			Debug.LogError ("no SteamVR_Behaviour_Pose on this object");
		}
		if (GetComponentInChildren<SteamVR_Behaviour_Skeleton> ()) {
			skeleton = GetComponentInChildren<SteamVR_Behaviour_Skeleton> ();
		}
		if (GetComponentInChildren<SteamVR_RenderModel> ()) {
			RenderModel = GetComponentInChildren<SteamVR_RenderModel> ();
		}
		skeleton.BlendToSkeleton ();
//		skeleton.BlendToPoser(skeleton.fallbackPoser);
//		RenderModelVisible (!HideController);
//		SetRenderModels ();
	}
	void Update () {
//		HapticResponse (tv3.x, tv3.y, tv3.z); 	
		PivotUpdate ();
		GrabCheck ();
	}
	// Update is called once per frame
	void FixedUpdate () {
		Squeeze= SqueezeButton.GetAxis(handType);
		PivotUpdate ();
		if (grabPoser&&GrabInteractible) {
			GrabUpdate ();
			return;
		}
		SelectIndexObject ();
		SelectPinchObject ();
		SelectGribObject ();
		
	}

	void GrabCheck(){
		if (grabType != GrabType.None&&GrabInteractible) {
			if (grabType == GrabType.Pinch&&pinchButton.GetStateUp(handType)) {
				GrabInteractible.SendMessage ("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
				GrabEnd ();
			}
			if (grabType == GrabType.Grip&&grabButton.GetStateUp(handType)) {
				GrabInteractible.SendMessage ("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
				GrabEnd ();
			}
		}

		if (!grabPoser){
			CustomInteractible OldGrabInteractible=GrabInteractible;
			if (SelectedIndexInteractible) {
				GrabInteractible = SelectedIndexInteractible;
				if (GrabInteractible != OldGrabInteractible) {
					if (OldGrabInteractible)
						OldGrabInteractible.SendMessage ("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
					if (GrabInteractible) {
						GrabInteractible.SendMessage ("GrabStart", this, SendMessageOptions.DontRequireReceiver);
						setHandTransform = false;
						grabType = GrabType.Select;
						RenderModelVisible (!GrabInteractible.HideController);
						SkeletonUpdate ();
					}
				}
			} else {
				if (SelectedPinchInteractible&&pinchButton.GetStateDown(handType)) {
					GrabInteractible = SelectedPinchInteractible;
					if (GrabInteractible != OldGrabInteractible) {
						if (OldGrabInteractible)
							OldGrabInteractible.SendMessage ("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
						if (GrabInteractible) {
							GrabInteractible.SendMessage ("GrabStart", this, SendMessageOptions.DontRequireReceiver);
							setHandTransform = false;
							grabType = GrabType.Pinch;
							RenderModelVisible (!GrabInteractible.HideController);
							SkeletonUpdate ();
						}
					}
				} else {
					if (SelectedGpibInteractible&&grabButton.GetStateDown(handType)) {
						GrabInteractible = SelectedGpibInteractible;
						if (GrabInteractible != OldGrabInteractible) {
							if (OldGrabInteractible)
								OldGrabInteractible.SendMessage ("GrabEnd", this, SendMessageOptions.DontRequireReceiver);
							if (GrabInteractible) {
								GrabInteractible.SendMessage ("GrabStart", this, SendMessageOptions.DontRequireReceiver);
								setHandTransform = false;
								grabType = GrabType.Grip;
								RenderModelVisible (!GrabInteractible.HideController);
								SkeletonUpdate ();
							}
						}
					}
				}
			}
		}
	}


	public void GrabUpdateCustom(){
		if (grabPoser) {
			skeleton.BlendToPoser (grabPoser,0);

			posSavePoser = grabPoser.transform.localPosition;
			rotSavePoser = grabPoser.transform.localEulerAngles;

			grabPoser.transform.rotation = transform.rotation * grabPoser.GetBlendedPose (skeleton).rotation;
			grabPoser.transform.position = transform.TransformPoint (grabPoser.GetBlendedPose (skeleton).position);

			PivotUpdate ();

			inverceLocalPosition = grabPoser.transform.InverseTransformPoint (transform.position);	

			grabPoser.transform.localPosition = posSavePoser;
			grabPoser.transform.localEulerAngles = rotSavePoser;

			skeleton.transform.position = grabPoser.transform.TransformPoint (inverceLocalPosition);
			skeleton.transform.rotation = grabPoser.transform.rotation * Quaternion.Inverse (grabPoser.GetBlendedPose (skeleton).rotation);
		}
//		} else {
//			skeleton.BlendToSkeleton ();
//		}
	}

	void GrabUpdate(){
		
		if (grabPoser) {
			skeleton.BlendToPoser (grabPoser,0);

			posSavePoser = grabPoser.transform.localPosition;
			rotSavePoser = grabPoser.transform.localEulerAngles;

			grabPoser.transform.rotation=transform.rotation*grabPoser.GetBlendedPose(skeleton).rotation;
			grabPoser.transform.position = transform.TransformPoint(grabPoser.GetBlendedPose (skeleton).position);

			PivotUpdate ();

			inverceLocalPosition=grabPoser.transform.InverseTransformPoint (transform.position);	

			grabPoser.transform.localPosition = posSavePoser;
			grabPoser.transform.localEulerAngles = rotSavePoser;

			GrabInteractible.SendMessage ("GrabUpdate",this,SendMessageOptions.DontRequireReceiver);

//
//			skeleton.transform.position= grabPoser.transform.TransformPoint(inverceLocalPosition);
//			skeleton.transform.rotation= grabPoser.transform.rotation*Quaternion.Inverse(grabPoser.GetBlendedPose(skeleton).rotation);
		}
//		} else {
//			skeleton.BlendToSkeleton ();
//		}


	}

	public void HapticResponse(float hlength,float hfreq,float hpower){
		hapticSignal.Execute (0, hlength , hfreq , hpower , handType);

	}

	void LateUpdate(){
		if (grabPoser&&setHandTransform) {
			skeleton.transform.position = grabPoser.transform.TransformPoint (inverceLocalPosition);
			skeleton.transform.rotation = grabPoser.transform.rotation * Quaternion.Inverse (grabPoser.GetBlendedPose (skeleton).rotation);
		}
		setHandTransform = true;
	}

	public void RenderModelVisible(bool visible){
		if (RenderModel){
			MeshRenderer[] tempMeshRenderer = RenderModel.GetComponentsInChildren<MeshRenderer> ();
			for (int i = 0; i < tempMeshRenderer.Length; i++) {
				tempMeshRenderer [i].enabled = visible;
			}
		}
	}

	public void SetRenderModels(){
//		mrcontroller=RenderModel.GetComponentsInChildren<MeshRenderer> ();
		StartCoroutine (startHide ());
	}

	IEnumerator startHide(){
		while (true) {
			if (RenderModel.meshRenderers.Count > 0) {
				RenderModel.SetMeshRendererState (!HideController);
				break;
			}
			yield return new WaitForSeconds (0);
		}


	}

	void GrabEnd(){
		skeleton.transform.localPosition = Vector3.zero;
		skeleton.transform.localEulerAngles = Vector3.zero; ///save coord
		skeleton.BlendToSkeleton (0);
//		skeleton.BlendToPoser(skeleton.fallbackPoser,0);
		RenderModelVisible (!HideController);

		grabPoser = null;
		GrabInteractible = null;
		grabType = GrabType.None;
	}

	public void DetachHand(){
		skeleton.transform.localPosition = Vector3.zero;
		skeleton.transform.localEulerAngles = Vector3.zero; ///save coord

//		skeleton.BlendToPoser(skeleton.fallbackPoser,0);
		skeleton.BlendToSkeleton (0);
//		skeleton.skeletonBlend=1;
		grabPoser = null;
		GrabInteractible=null;
		grabType = GrabType.None;
	}

	void SelectIndexObject(){
		if (!grabPoser) {
//		CustomInteractible SelectedInteractibleOld = SelectedIndexInteractible;
		SelectedIndexColliders = Physics.OverlapSphere (IndexPoint(), indexRadius, layerColliderChecker);
		SelectedIndexInteractible = null;
		for (int i = 0; i < SelectedIndexColliders.Length; i++) {
			CustomInteractible tempCustomInteractible = SelectedIndexColliders [i].GetComponentInParent<CustomInteractible> ();
			if (tempCustomInteractible != null && tempCustomInteractible.isInteractible&&tempCustomInteractible.grabType==GrabType.Select) {

				SelectedIndexInteractible = tempCustomInteractible;
			}
		}

//		if (SelectedInteractibleOld != SelectedIndexInteractible) {
//			if (SelectedInteractibleOld) {
//				if (grabType==GrabType.None||grabType==GrabType.Select)
//					SelectedInteractibleOld.gameObject.SendMessage ("SelectIndexEnd", this, SendMessageOptions.DontRequireReceiver);
//			}
//		if (SelectedIndexInteractible&&!grabPoser)
//				SelectedIndexInteractible.gameObject.SendMessage ("SelectIndexStart", this, SendMessageOptions.DontRequireReceiver);
//		} else {
//			if (SelectedIndexInteractible)
//				if (grabType==GrabType.None||grabType==GrabType.Select)
//					SelectedIndexInteractible.gameObject.SendMessage ("SelectIndexUpdate", this, SendMessageOptions.DontRequireReceiver);
//		}
		}
	}

	void SelectPinchObject(){
		if (!grabPoser) {
	//	CustomInteractible SelectedInteractibleOld = SelectedIndexInteractible;
		SelectedPinchColliders = Physics.OverlapSphere (PinchPoint(), pinchRadius, layerColliderChecker);
		SelectedPinchInteractible = null;
		for (int i = 0; i < SelectedPinchColliders.Length; i++) {
			CustomInteractible tempCustomInteractible = SelectedPinchColliders [i].GetComponentInParent<CustomInteractible> ();
			if (tempCustomInteractible != null && tempCustomInteractible.isInteractible&&tempCustomInteractible.grabType==GrabType.Pinch) {
				SelectedPinchInteractible = tempCustomInteractible;
			}
		}

//		if (SelectedInteractibleOld != SelectedIndexInteractible) {
//			if (SelectedInteractibleOld) {
//				if (grabType==GrabType.None||grabType==GrabType.Select)
//					SelectedInteractibleOld.gameObject.SendMessage ("SelectIndexEnd", this, SendMessageOptions.DontRequireReceiver);
//			}
//			if (SelectedIndexInteractible&&!grabPoser)
//				SelectedIndexInteractible.gameObject.SendMessage ("SelectIndexStart", this, SendMessageOptions.DontRequireReceiver);
//		} else {
//			if (SelectedIndexInteractible)
//			if (grabType==GrabType.None||grabType==GrabType.Select)
//				SelectedIndexInteractible.gameObject.SendMessage ("SelectIndexUpdate", this, SendMessageOptions.DontRequireReceiver);
//		}
		}
	}

	void SelectGribObject(){
		if (!grabPoser) {
	//	CustomInteractible SelectedInteractibleOld = SelectedGpibInteractible;
		SelectedGpibColliders = Physics.OverlapSphere (transform.TransformPoint (new Vector3 (0, 0, -.1f)), gribRadius, layerColliderChecker);
		SelectedGpibInteractible = null;
		for (int i = 0; i < SelectedGpibColliders.Length; i++) {
			CustomInteractible tempCustomInteractible = SelectedGpibColliders [i].GetComponentInParent<CustomInteractible> ();
			if (tempCustomInteractible != null && tempCustomInteractible.isInteractible&&tempCustomInteractible.grabType==GrabType.Grip) {
				SelectedGpibInteractible = tempCustomInteractible;
			}
		}
//		if (SelectedInteractibleOld != SelectedInteractible) {
//			if (SelectedInteractibleOld) {
//				if (grabType==GrabType.None||grabType==GrabType.Select)
//					SelectedInteractibleOld.gameObject.SendMessage ("SelectEnd", this, SendMessageOptions.DontRequireReceiver);
//			}
//		if (SelectedInteractible&&!grabPoser)
//				SelectedInteractible.gameObject.SendMessage ("SelectStart", this, SendMessageOptions.DontRequireReceiver);
//		} else {
//			if (SelectedInteractible) {
//				if (grabType==GrabType.None||grabType==GrabType.Select)
//				SelectedInteractible.gameObject.SendMessage ("SelectUpdate", this, SendMessageOptions.DontRequireReceiver);
//			}
//		}
		}
	}
	public void SkeletonUpdate(){
		if (skeleton){
			if (grabPoser) {
				skeleton.BlendToPoser (grabPoser);
				PivotUpdate ();
			}
//			}else{
//				skeleton.BlendToSkeleton ();
//			}
		}
	}

	public void PivotUpdate(){
		if (grabPoser) {
			PivotPoser.rotation = transform.rotation * grabPoser.GetBlendedPose (skeleton).rotation;
			PivotPoser.position = transform.TransformPoint (grabPoser.GetBlendedPose (skeleton).position);
		}
	}

	public Vector3 GrabPoint(){
		return transform.TransformPoint (new Vector3 (0, 0, -.1f));
	}

	public Vector3 PinchPoint(){
		return transform.TransformPoint (new Vector3 (0, 0, -.05f));
	}

	public Vector3 IndexPoint(){
		if (handType == SteamVR_Input_Sources.RightHand)
			return transform.TransformPoint( new Vector3 (0.03009196f, -0.07610637f, -0.004979379f));
		if (handType == SteamVR_Input_Sources.LeftHand)
			return transform.TransformPoint(new Vector3 (-0.03009196f, -0.07610637f, -0.004979379f));
		return Vector3.zero;
	}

	void OnDrawGizmosSelected(){
		Gizmos.DrawWireSphere (transform.TransformPoint (new Vector3 (0, 0, -.1f)), gribRadius);
		Gizmos.DrawWireSphere (transform.TransformPoint (new Vector3 (0, 0, -.05f)), pinchRadius);
		if (handType==SteamVR_Input_Sources.RightHand)
			Gizmos.DrawWireSphere (transform.TransformPoint (new Vector3 (0.03009196f, -0.07610637f, -0.004979379f)), indexRadius);
		if (handType==SteamVR_Input_Sources.LeftHand)
			Gizmos.DrawWireSphere (transform.TransformPoint (new Vector3 (-0.03009196f, -0.07610637f, -0.004979379f)), indexRadius);
    }


}
