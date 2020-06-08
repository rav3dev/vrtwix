using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SemiAutoWeapon : WeaponCore
{
    public ReloadSlider reloader;
    bool isCycled = false;
    public bool slideCatch = false;
    public float fireRate = 500;

    void Start()
    {
        Initialize();
        onShoot.AddListener(reloader.SliderRecoilMovement);
    }

    private void SetRPM()
    {
        reloader.returnSpeed = fireRate / 60f / 2f / 2f;
        reloader.sliderRecoilSpeed = fireRate / (fireRate  * 100 );
    }

    private void SetSlideCatch()
    {
        reloader.slideCatch = slideCatch;
    }

    public virtual void ShotHandler()
    {
        if (trigger.state == 1 && reloader.state == 0 && !isCycled)
        {
            Shoot();
            isCycled = true;
        }
        else if (trigger.state == 2)
        {
            isCycled = false;
        }
    }

    void FixedUpdate()
    {
        ShotHandler();
        SetRPM();
        SetSlideCatch();
    }
}
