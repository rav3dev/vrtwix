using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FullAutoWeapon : SemiAutoWeapon
{
    public bool fullAuto = true;
    // Start is called before the first frame update
    void Start()
    {
        Initialize();
        onShoot.AddListener(reloader.SliderRecoilMovement);
    }

    public override void ShotHandler()
    {
        if (fullAuto)
        {
            if (trigger.state == 1 && reloader.state == 0)
            {
                Shoot();
            }
        }
        else
        {
            base.ShotHandler();
        }
    }

}
