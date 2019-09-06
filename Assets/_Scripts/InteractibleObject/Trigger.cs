using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class Trigger : MonoBehaviour
{
	public Vector2 angle;
	public SteamVR_Action_Single triggerAxis=SteamVR_Input.GetAction<SteamVR_Action_Single>("Trigger");
	public SteamVR_Action_Boolean triggerClick=SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerClick");
	public PrimitiveWeapon primitiveWeapon;
	public ManualReload manualReload;
	public bool isClick;
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
		if (!isClick) {
			if (triggerClick.GetState (hand.handType)) {
				if (!isClick) {
					if (manualReload.reloadFinish&&primitiveWeapon.Shoot ()) {
						if (manualReload.typeReload == ManualReload.TypeReload.Slider) {
							manualReload.enabled = true;
						}
					}
				}
				isClick = true;
			
			}
		} else {
			if (!triggerClick.GetState (hand.handType)) {
				isClick = false;
			}
		}

		transform.localEulerAngles = new Vector3 (Mathf.Lerp (angle.x, angle.y, triggerAxis.GetAxis (hand.handType)), 0);

    }

}
