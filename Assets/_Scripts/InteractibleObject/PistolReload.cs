using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PistolReload : MonoBehaviour {
	public Vector2 clampZ;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	public void Update () {
		transform.localPosition = new Vector3 (transform.localPosition.x, transform.localPosition.y, Mathf.Clamp (transform.localPosition.z, clampZ.x, clampZ.y));
	}
}
