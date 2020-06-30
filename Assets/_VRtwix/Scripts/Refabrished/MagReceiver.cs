using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MagReceiver : MonoBehaviour
{
    public WeaponCore weaponController;
    public int magAttachedLayer = 25;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<MagazineController>() && other.GetComponent<PhysicalObject>())
        {
            PhysicalObject interactionController = other.GetComponent<PhysicalObject>();
            MagazineController magController = other.GetComponent<MagazineController>();

            if (magController.magType == weaponController.magConfig.type && !weaponController.magConfig.attached && weaponController.magConfig.isDetachable)
            {
                interactionController.DettachHands();
                other.transform.position = weaponController.magConfig.attachPoint.position;
                other.transform.rotation = weaponController.magConfig.attachPoint.rotation;

                other.GetComponent<Rigidbody>().isKinematic = true;
                other.transform.parent = weaponController.transform;
                magController.weaponController = weaponController;
                weaponController.attachedMag = magController;
                weaponController.magConfig.attached = true;
                weaponController.onMagazineAttach.Invoke();
                other.gameObject.layer = magAttachedLayer;
                magController.stikingAmmo.layer = magAttachedLayer;
            }
        }
    }
}
