using UnityEngine;
using System.Collections;

public class TileAlign : MonoBehaviour {

    public float i = 0;
    public float j = 0;
    public float Tilesize = 4;

    void Start()
    {
        transform.localEulerAngles = new Vector3(0, 0, 0);

        i = (transform.position.x - (transform.position.x % Tilesize)) / Tilesize;
        j = (transform.position.z - (transform.position.z % Tilesize)) / Tilesize;
    }

}
