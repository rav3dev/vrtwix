using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RifleReload : MonoBehaviour {
	public Vector2 clampPos,clampRot;
	public bool canPos,canRot;
	public float f;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (canPos) {
			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, Mathf.Clamp (transform.localPosition.z, clampPos.x, clampPos.y));
		} else {
			transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, clampPos.y);
		}
		canRot = transform.localPosition.z == clampPos.y;

		float tempRotZ = Mathf.Clamp (-Vector3.SignedAngle (transform.up, transform.parent.up, transform.forward), clampRot.x, clampRot.y);
		if (canRot) {
			transform.localEulerAngles = new Vector3 (0, 0, tempRotZ);
		} else {
			transform.localEulerAngles = new Vector3 (0, 0, clampRot.y);
		}
		canPos = tempRotZ == clampRot.y;



	}
}
