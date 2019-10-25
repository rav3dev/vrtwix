using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VelocityFix : MonoBehaviour
{
	public Rigidbody r;
	public int posFrames=10, rotFrames=10;
	public float positionMultiply=1.1f,rotationMultiply=1.1f;
	public Vector3[] posSave, rotSave;

	int currentframe;
    // Start is called before the first frame update
    void Start()
    {
		r=GetComponent<Rigidbody> ();

    }

    // Update is called once per frame

	void GrabStart(CustomHand hand)
	{
		posSave = new Vector3[posFrames];
		rotSave = new Vector3[rotFrames];
		currentframe = 0;
	}

	void GrabUpdate(CustomHand hand){
		currentframe++;
		posSave [currentframe % posFrames] = r.velocity;
		rotSave [currentframe % rotFrames] = r.angularVelocity;
	}

	void GrabEnd(CustomHand hand)
    {
		Vector3 tempPos = Vector3.zero;
		for (int i = 0; i < posSave.Length; i++) {
			tempPos += posSave [i];
		}

		r.velocity = tempPos/posFrames*positionMultiply;

		Vector3 tempRot = Vector3.zero;
		for (int i = 0; i < rotSave.Length; i++) {
			tempRot += rotSave [i];
		}

		r.angularVelocity = tempRot/rotFrames*rotationMultiply;
    }
}
