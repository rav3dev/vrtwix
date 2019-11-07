using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class Trigger : MonoBehaviour
{
	public float Axis;
	public Vector2 angle;
	public SteamVR_Action_Single triggerAxis=SteamVR_Input.GetAction<SteamVR_Action_Single>("Trigger");
	public SteamVR_Action_Boolean triggerClick=SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerClick");
	public PrimitiveWeapon primitiveWeapon;
	public ManualReload manualReload;
	public bool isClick;
	public enum TypeShoot
	{
		Safety,
		Semi,
		Automatic,
	}
	;public TypeShoot typeShoot;
//	public enum 
    // Start is called before the first frame update
    void Start()
    {
		primitiveWeapon = GetComponentInParent<PrimitiveWeapon> ();
		manualReload = primitiveWeapon.GetComponentInChildren<ManualReload> ();
    }

    // Update is called once per frame
	public void customUpdate(CustomHand hand)
    {
		if (triggerClick.GetStateUp (hand.handType)) {
			isClick = false;
		}
		switch (typeShoot) {
		case TypeShoot.Semi:
			if (!isClick && triggerClick.GetStateDown (hand.handType)) {
				if (manualReload.typeReload == ManualReload.TypeReload.Revolver) {
					manualReload.RevolverNextBullet ();
				}
				if (manualReload.reloadFinish && primitiveWeapon.Shoot ()) {
					isClick = true;
					if (manualReload.typeReload == ManualReload.TypeReload.Slider) {
						manualReload.enabled = true;
					}
				}
			}
		break;

		case TypeShoot.Automatic:
			if (triggerClick.GetState (hand.handType) && manualReload.reloadFinish && primitiveWeapon.Shoot ()) {
				if (manualReload.typeReload == ManualReload.TypeReload.Slider) {
					manualReload.enabled = true;
				}
			}
		break;
		default:
		break;
		}
		Axis=triggerAxis.GetAxis (hand.handType);
		transform.localEulerAngles = new Vector3 (Mathf.Lerp (angle.x, angle.y, Axis), 0);


		if (manualReload.typeReload == ManualReload.TypeReload.Revolver) {
			manualReload.CustomRevolverUpdate ();
		}
    }

}
