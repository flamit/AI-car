using UnityEngine;
using System.Collections;
using System.IO;

public class GetData : MonoBehaviour {

    public string filePath = "yol2.txt";                                    // filename recorded track data already
    public GameObject Tile;                                                 // Tile game object
    public int Tilesize = 10;
    public static Vector2[,] oturn = new Vector2[1000, 1000];               // array of steering value 
    public static Vector2[,] othrottle = new Vector2[1000, 1000];           // array of Throttle value
    public static Vector2[,] obrake = new Vector2[1000, 1000];              // array of Brake value
    public static Vector2[,] counter = new Vector2[1000, 1000];             // array of counter value
    public static Vector2[,] odirection = new Vector2[1000, 1000];          // array of direction value
    public static Vector2[,] ospeed = new Vector2[1000, 1000];              // array of speed value
    string temp;
    float temp2;
    public bool tiles;                                                      // boolean flag for displaying recorded tile


	// Use this for initialization
	void Start () {

        string[] sr;

        if (File.Exists(Application.dataPath + "/" + filePath))
            sr = System.IO.File.ReadAllLines(Application.dataPath + "/" + filePath);
        else
            sr = System.IO.File.ReadAllLines(Application.dataPath + "/Scripts/Racing AI/" + filePath);

        if (sr[0] == "a")
        {
            int f = 0;
            do 
            {
                f++;
                if (sr[f] == null) { break; }
                int i = int.Parse(sr[f]);
                f++;
                int j = int.Parse(sr[f]);
                f++;
                temp2 = float.Parse(sr[f]);
                f++;
                oturn[i, j] = new Vector2(temp2, 0.0f);

                temp = sr[f];
                f++;
                temp2 = float.Parse(temp);
                othrottle[i, j] = new Vector2(temp2, 0);

                temp = sr[f];
                f++;
                temp2 = float.Parse(temp);
                obrake[i, j] = new Vector2(temp2, 0);

                temp = sr[f];
                f++;
                temp2 = float.Parse(temp);
                counter[i, j] = new Vector2(temp2, 0);

                temp = sr[f];
                f++;
                temp2 = float.Parse(temp);
                odirection[i, j] = new Vector2(temp2, 0);

                temp = sr[f];
                f++;
                temp2 = float.Parse(temp);
                ospeed[i, j] = new Vector2(temp2, 0);

                if (tiles)
                    Instantiate(Tile, new Vector3(((i * Tilesize) + (Tilesize / 2)), 200, (j * Tilesize) + (Tilesize / 2)), Quaternion.identity);
//                 Debug.Log("f: " + f);
            } while (/*sr[f] != null ||*/ f < sr.Length - 1);
//             }
        }

    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKey("escape"))
        {
            oturn = null;
            othrottle = null;
            obrake = null;
            counter = null;
            odirection = null;
            ospeed = null;
        }
    }
}
