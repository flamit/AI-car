using UnityEngine;
using System.Collections;

public class AIteker1 : MonoBehaviour {

	// Use this for initialization
	void Start () {
        WheelCollider wheel = GetComponent<WheelCollider>();

        JointSpring tmpSpr = wheel.suspensionSpring;

        tmpSpr.spring = 3000f;
        tmpSpr.damper = 20f;

        wheel.suspensionSpring = tmpSpr;

    }

}
