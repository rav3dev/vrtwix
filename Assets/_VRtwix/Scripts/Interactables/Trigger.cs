using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Valve.VR;
public class Trigger : MonoBehaviour
{
	public float Axis;//% нажатия кнопки
	public Vector2 angle; //ограничения поворота
	public SteamVR_Action_Single triggerAxis=SteamVR_Input.GetAction<SteamVR_Action_Single>("Trigger"); //инпут
	public SteamVR_Action_Boolean triggerClick=SteamVR_Input.GetAction<SteamVR_Action_Boolean>("TriggerClick"); //инпут
    public PrimitiveWeapon primitiveWeapon; //оружие к которому приклеплен курок
	public ManualReload manualReload; //скрипт перезарядки который на этом оружеи
	public bool isClick; //нажат ли курок
	public enum TypeShoot 
	{
		Safety,
		Semi,
		Automatic,
	}
	;public TypeShoot typeShoot;//предохранитель

    void Start()
    {
		primitiveWeapon = GetComponentInParent<PrimitiveWeapon> ();
		manualReload = primitiveWeapon.GetComponentInChildren<ManualReload> ();
    }
    
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
