using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine;

public class ReloadSlider : CustomInteractible
{
    public WeaponCore weaponController;
    public bool slideCatch = false;
    public float reloadDistance = 0.15f;
    public float returnSpeed = 2f;
    public float slideCatchDistance = 0.035f;
    public float sliderRecoilSpeed = 0.05f;
    public int state = 0;                       //0 - default, 1 - back position, 2 - ammo received from magazine, 3 - slide catch
    public bool isGrabbed = false;
    public bool isMovedByRecoil = false;
    public Vector3 defaultPostion;

    [Header("Events")]
    public UnityEvent onBackPosition;
    public UnityEvent onFrontPosition;
    public UnityEvent onBulletLoaded;
    public UnityEvent onBulletExtracted;

    private void Start()
    {
        defaultPostion = transform.localPosition;
    }

    public void GrabStart(CustomHand hand)
    {
        SetInteractibleVariable(hand);
        isGrabbed = true;
        //weaponController.onShoot.AddListener(SliderRecoilMovement);
    }

    public void SliderRecoilMovement()
    {
        IEnumerator sliderRecoilRoutine = SliderRecoilMoveBack(sliderRecoilSpeed);
        isMovedByRecoil = true;
        StartCoroutine(sliderRecoilRoutine);
    }

    public IEnumerator SliderRecoilMoveBack(float timeToMove)
    {
        float t = 0f;
        Vector3 localPos = transform.localPosition;
        while (t < 1)
        {
            if (isGrabbed)
            {
                StopAllCoroutines();
                isMovedByRecoil = false;
            }
            t += Time.deltaTime / timeToMove;
            transform.localPosition = Vector3.Lerp(localPos, new Vector3(defaultPostion.x, defaultPostion.y, defaultPostion.z - reloadDistance), t);
            yield return null;
        }
        Debug.Log("End of coroutine");
    }

    private void FixedUpdate()
    {
        if (!isGrabbed && !isMovedByRecoil)
        {
            if (state == 3)
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, new Vector3(defaultPostion.x, defaultPostion.y, -slideCatchDistance), Time.deltaTime * returnSpeed);
            }
            else
            {
                transform.localPosition = Vector3.MoveTowards(transform.localPosition, defaultPostion, Time.deltaTime * returnSpeed);
            }
        }

        if (transform.localPosition.z <= defaultPostion.z - reloadDistance)                      // 5% margin (reloadDistance * 0.05)
        {
            BackPositionHandler();
        }
        else if (transform.localPosition.z == defaultPostion.z)
        {
            FrontPositionHandler();
        }
    }

    void BackPositionHandler()
    {
        if (state == 0 || state == 3)                           // check if it's suits for reloading
        {
            state = 1;
            onBackPosition.Invoke();

            if (weaponController.armed)                         // check if weapon was armed, to extract bullet which was inside
            {
                weaponController.AmmoExtractor(200f);
                weaponController.armed = false;
            }
            else if (weaponController.casingInside)           // check if casing left inside
            {
                weaponController.CasingExtractor(350f);
                weaponController.casingInside = false;
            }

            if (weaponController.magConfig.attached && weaponController.attachedMag)
            {
                if (weaponController.attachedMag.ammo > 0)
                {
                    weaponController.attachedMag.ammo--;

                    state = 2;
                }
                else if (weaponController.attachedMag.ammo == 0 && isMovedByRecoil && slideCatch)
                {
                    state = 3;
                }
            }
            else if (isMovedByRecoil && slideCatch)
            {
                state = 3;
            }

            if (weaponController.trigger.state == 1 && slideCatch && !isMovedByRecoil)
            {
                state = 3;
            }

            isMovedByRecoil = false;
        }
    }

    void FrontPositionHandler()
    {
        if (state != 0)                                      // check if not default state
        {
            weaponController.cocked = true;
            if (state == 2)
            {
                weaponController.armed = true;              // ammo was received, then gun is now armed
            }

            state = 0;                                      // return to default state
        }
    }

    public void GrabUpdate(CustomHand hand)
    {
        transform.position = hand.pivotPoser.position;
        transform.localPosition = new Vector3(defaultPostion.x, defaultPostion.y, Mathf.Clamp(transform.localPosition.z, defaultPostion.z - reloadDistance, defaultPostion.z));
    }

    public void GrabEnd(CustomHand hand)
    {
        isGrabbed = false;
        DettachHand(hand);
    }
}
