using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviour
{
	public GameObject spawnObject;
    public float force;
    public void Spawn()
    {
        if (spawnObject)
        {
            GameObject InstanseObject = Instantiate(spawnObject, transform.position, transform.rotation) as GameObject;
            if (InstanseObject.GetComponent<Rigidbody>()) {
                InstanseObject.GetComponent<Rigidbody>().AddRelativeForce(Vector3.forward * force, ForceMode.VelocityChange);
            }
        }
    }
}
