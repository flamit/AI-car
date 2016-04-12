using UnityEngine;
using System.Collections;

public class ChangeColor : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Color tmpClr = GetComponent<Renderer>().material.color;
        tmpClr.r -= Random.value * 4;
        tmpClr.g -= Random.value * 4;
        tmpClr.b -= Random.value * 4;

        GetComponent<Renderer>().material.color = tmpClr;
    }

}
