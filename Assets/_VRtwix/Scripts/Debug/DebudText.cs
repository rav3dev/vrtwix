using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class DebudText : MonoBehaviour {
	public Text text;
	public Joystick joystick;
	public SteeringWheel steeringWheel;
	// Use this for initialization
	public void Start(){
		text = GetComponent<Text> ();
	}
	public void Update(){
		if (joystick)
		text.text = joystick.value.ToString();
		if (steeringWheel)
			text.text = ((int)steeringWheel.angle).ToString();
	}

}
