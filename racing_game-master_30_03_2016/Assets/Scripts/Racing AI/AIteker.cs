using UnityEngine;
using System.Collections;

public class AIteker : MonoBehaviour {

	// Use this for initialization
	void Start () {
        WheelCollider wheel = GetComponent<WheelCollider>();

        JointSpring tmpSpr = wheel.suspensionSpring;
        tmpSpr.spring = 3000;
        tmpSpr.damper = 20;
        
        wheel.suspensionSpring = tmpSpr;
    }

}
