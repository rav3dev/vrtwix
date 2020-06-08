using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class MagazineController : MonoBehaviour
{
    public string ammoType;
    public int maxCapacity = 15;
    public int ammo;
    public int defaultLayer = 0;
    public GameObject stikingAmmo;
    public string magType;
    public UnityEvent onBulletAdd;
     public WeaponCore weaponController;

    private void Update()
    {
        if(ammo < 1)
        {
            stikingAmmo.SetActive(false);
        } else
        {
            stikingAmmo.SetActive(true);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.tag == ammoType && ammo < maxCapacity)
        {
            if (other.GetComponent<PhysicalObject>())
            {
                other.GetComponent<PhysicalObject>().DettachHands();
                ammo++;
                onBulletAdd.Invoke();
                Destroy(other.gameObject);
            } else
            {
                Debug.Log("NO PHYS COMPONENT");
            }
        }
    }

    public void GrabStart(CustomHand hand)
    {
        if (weaponController)
        {
            IEnumerator coroutine = weaponController.DetachDelay(.25f, false);
            weaponController.StartCoroutine(coroutine);
            transform.parent = null;
            weaponController.attachedMag = null;
            weaponController = null;
            gameObject.layer = defaultLayer;
            stikingAmmo.layer = defaultLayer;
        }
    }
}
