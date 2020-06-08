using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Valve.VR;

public class WeaponCore : PhysicalObject
{

    [Header("Set Up Zone")]
    public Transform insideBullet;                      // ammo stiking inside
    public Transform casingExtractionPoint;             // casings extraction point
    public GameObject casingPrefab;                     // bullet casing to extract
    public GameObject ammoPrefab;                       // bullet casing to extract
    public ParticleSystem shootParticles;               // particles for shooting

    [Header("System")]
    public bool cocked = false;                         // if gun cocked
    public bool armed = false;                          // is has ammo inside
    public bool casingInside = false;                   // if there is a casing inside

    [System.Serializable]
    public struct MagConfig
    {
        public bool isDetachable;           // if magazine can detach
        public Transform attachPoint;    // magazne attach point
        public string type;              // magazine type
        public bool attached;
    }

    [Header("Magazine Configuration")]
    public MagConfig magConfig;

    [System.Serializable]
    public struct RecoilConfig
    {
        public Transform recoil;            //recoil calculation object
        public float angleOneShot;          // recoil angle for 1 shot
        public float angleReturnSpeed;      // angle return speed
        public float maxAngle;              // max angle
        public float distanceOneShot;       // recoil distance for 1 shot
        public float distanceReturnSpeed;   // distance return speed
        public float maxDistance;           // max distance
        public float currentAngle;          // current angle 
        public float doubleHandStabilizerFactor;
    }

    [Header("Recoil Configuration")]
    public RecoilConfig recoilConfig = new RecoilConfig
    {
        angleOneShot = 20f,
        angleReturnSpeed = 100f,
        maxAngle = 45f,
        distanceOneShot = 0.1f,
        distanceReturnSpeed = 1f,
        maxDistance = 0.2f,
        currentAngle = 0,
        doubleHandStabilizerFactor = 2f
    };

    [Header("Controllers")]
    [HideInInspector] public MagazineController attachedMag;                              //attached mag
    //public ReloadController reloadController;                           // reload handler ( script )
    public TriggerController trigger;                                   // trigger handler ( script )

    [Header("Events")]
    public UnityEvent onShoot;
    public UnityEvent onEmptyShot;

    // MAG EVENTS
    public UnityEvent onMagazineAttach;
    public UnityEvent onMagazineDetach;

    new public void GrabStart(CustomHand hand)
    {
        GrabStartCustom(hand);
    }

    new public void GrabUpdate(CustomHand hand)
    {
        GrabUpdateCustom(hand);

        if (trigger)
        {
            trigger.CustomUpdate(hand);
        }

        if (recoilConfig.recoil)
        {
            myRigidbody.velocity += transform.TransformDirection(recoilConfig.recoil.localPosition / Time.fixedDeltaTime);
            myRigidbody.angularVelocity += PhysicalObject.GetAngularVelocities(transform.rotation, recoilConfig.recoil.rotation, hand.GetBlendPose());
        }


        RecoilReturn();
    }

    new public void GrabEnd(CustomHand hand)
    {
        recoilConfig.currentAngle = 0;
        recoilConfig.recoil.localPosition = Vector3.zero;
        GrabEndCustom(hand);
    }

    public void Recoil()
    {
        float modifier = 1f;
        if(leftHand && rightHand)
        {
            modifier = recoilConfig.doubleHandStabilizerFactor / 10f;
        }
        recoilConfig.recoil.localPosition -= Vector3.forward * recoilConfig.distanceOneShot * modifier;
        recoilConfig.currentAngle -= recoilConfig.angleOneShot * modifier;
    }

    public void CasingExtractor(float thrust) // OPTIMAL THRUST 350f
    {
        GameObject casing = Instantiate(casingPrefab, casingExtractionPoint.position, transform.rotation);
        if (casing.GetComponent<Rigidbody>())
        {
            casing.GetComponent<Rigidbody>().AddForce(casingExtractionPoint.right * thrust);
        }
        Destroy(casing, 5f); // DESTROY CASING IN 5 SEC
    }

    public void AmmoExtractor(float thrust) // OPTIMAL THRUST 350f
    {
        GameObject casing = Instantiate(ammoPrefab, casingExtractionPoint.position, transform.rotation);
        if (casing.GetComponent<Rigidbody>())
        {
            casing.GetComponent<Rigidbody>().AddForce(casingExtractionPoint.right * thrust);
        }
    }


    void RecoilReturn()
    {
        if (recoilConfig.recoil)
        {
            recoilConfig.currentAngle = Mathf.Clamp(recoilConfig.currentAngle + recoilConfig.angleReturnSpeed * Time.deltaTime, -recoilConfig.maxAngle, 0);
            recoilConfig.recoil.localPosition = new Vector3(0, 0, Mathf.Clamp(recoilConfig.recoil.localPosition.z + recoilConfig.distanceReturnSpeed * Time.deltaTime, -recoilConfig.maxDistance, 0));
            recoilConfig.recoil.localEulerAngles = new Vector3(-recoilConfig.currentAngle, 0, 0);
        }
    }


    public IEnumerator DetachDelay(float time, bool state)
    {
        yield return new WaitForSeconds(time);
        magConfig.attached = state;
    }


    public void Shoot()
    {
        if (cocked)                         // CHECK IF WEAPON COCKED
        {
            if (armed)                      // CHECK IF ENOUGH AMMO
            {
                shootParticles.Play();      // SHOT PARTICLES
                Recoil();                   // ADD RECOIL
                onShoot.Invoke();           // SHOT EVENT
                armed = false;              // WEAPON DISARMED
                casingInside = true;
            }
            else
            {
                onEmptyShot.Invoke();       // EMPTY SHOT EVENT
            }

            cocked = false;                 // NO MORE COCKED
        }
    }
}
