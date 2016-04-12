using UnityEngine;
using System.Collections;

public class CG : MonoBehaviour {

	// Use this for initialization
	void Start () {
        GetComponent< Rigidbody > ().centerOfMass = new Vector3(0, 0.1f, 0.65f);//z 0.65
    }

}
