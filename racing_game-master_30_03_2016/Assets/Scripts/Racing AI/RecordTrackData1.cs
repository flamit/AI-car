using UnityEngine;
using System.Collections;
using System.IO;

// 

public class RecordTrackData1 : MonoBehaviour {

    public GameObject Tile;                 // Tile Gameobject: Tile is one kind of mark to display recording/recorded track data. (CBC)
    public float Tilesize = 10f;            // adjust this to match the length of your car. If your car is 4 units long, this can be 5 units.
    public float tileBelowCar = 1.0f;       // When you record track data, the bellow offset position of tiles paved (CBC)

    private float throttle;                 // throttle value(engine torque) of car to record
    private float turn;                     // steering value of car to record
    private float brake;                    // brake value to record
    private float direction;                // rotation angle around Y axis of car to record
    private float speed;                    // current speed to record 
    private float diff;                     // increment/decrement of direction

    public string filePath = "yol.txt";     // the filename to record track data

    public CarController controller;        // CarController for Amit Car

    private StreamWriter sw;                // stream writer object to write track data file
    private float x;                        // current x position of car
    private float z;                        // current y position of car

    private Vector2[,] oTurn = new Vector2[1000, 1000];         // array of steering value 
    private Vector2[,] oThrottle = new Vector2[1000, 1000];     // array of Throttle value
    private Vector2[,] oBrake = new Vector2[1000, 1000];        // array of Brake value
    private Vector2[,] counter = new Vector2[1000, 1000];       // array of counter value
    private Vector2[,] oDirection = new Vector2[1000, 1000];    // array of direction value
    private Vector2[,] oSpeed = new Vector2[1000, 1000];        // array of speed value

    public int i;                           // index of 2-dimensional tile array
    public int j;                           // index of 2-dimensional tile array

    private int iRegister = 0;                      // index of 2-dimensional tile array recorded
    private int jRegister = 0;                      // index of 2-dimensional tile array recorded
    private float turnRegister = 0.0f;              // steering value recorded
    private float throttleRegister = 0.0f;          // throttle value recorded
    private float brakeRegister = 0;                // brake value recorded
    private int count = 0;                          // count value recorded
    private float directionRegister = 0;            // direction value recorded
    private float speedRegister = 0;                // speed value recored
    private bool Record = true;                     // boolean flag for recording

    void Start()
    {
        controller = GetComponent<CarController>();
//         sw = new StreamWriter("yol.txt");
        sw = new StreamWriter(Application.dataPath + "/Scripts/Racing AI/" + filePath);
        sw.WriteLine("a");

        // initialize counter array
        for (int q = 0; q < 1000; q++)
        {
            for (var p = 0; p < 1000; p++)
            {
                counter[q, p] = Vector2.zero;
            }
        }
    }

    void Update()
    {
        // exit from recording state
        if (Input.GetKey("escape"))
        {
            sw.Flush();
            sw.Close();
            oTurn = null;
            oThrottle = null;
            oBrake = null;
            counter = null;
            direction = float.NaN;
            speed = float.NaN;
        }

        // pause/resume recording
        if (Input.GetKeyDown("space"))
            Record = !Record;
        
        if (Record)
        {
            z = transform.position.z;
            x = transform.position.x;
            //             throttle = wheel2.motorTorque;
            //             turn = wheel1.steerAngle;

            throttle = controller.engineTorque;
            turn = controller.steeringAngle;
            brake = controller.brakeTorque;

//             throttle = wheel2.driveTorque;
//             turn = wheel1.steeringAngle;
//             brake = wheel2.brakeTorque;

            direction = transform.eulerAngles.y;
            speed = GetComponent< Rigidbody > ().velocity.magnitude;

            //get the index of the tile matching the current position of car
            i = (int)((x - (x % Tilesize)) / Tilesize);
            j = (int)((z - (z % Tilesize)) / Tilesize);

            if (iRegister != i || jRegister != j)       // if new index : if the car moves new position by tile size,,
            {
                oTurn[iRegister, jRegister] = new Vector2(turnRegister, 0);
                oThrottle[iRegister, jRegister] = new Vector2(throttleRegister, 0);
                oBrake[iRegister, jRegister] = new Vector2(brakeRegister, 0);
                counter[iRegister, jRegister] = new Vector2(count, 0);
                oDirection[iRegister, jRegister] = new Vector2(directionRegister, 0);
                oSpeed[iRegister, jRegister] = new Vector2(speedRegister, 0);

                if (direction > 180)
                    direction = direction - 360;

//              Debug.Log("i: " + i +"  j:" + j );
                count = (int)counter[i, j].x;
                turnRegister = oTurn[i, j].x;
                throttleRegister = oThrottle[i, j].x;
                brakeRegister = oBrake[i, j].x;
                directionRegister = oDirection[i, j].x;
                speedRegister = oSpeed[i, j].x;
                sw.WriteLine(iRegister);
                sw.WriteLine(jRegister);
                sw.WriteLine(oTurn[iRegister, jRegister].x);
                sw.WriteLine(oThrottle[iRegister, jRegister].x);
                sw.WriteLine(oBrake[iRegister, jRegister].x);
                sw.WriteLine(counter[iRegister, jRegister].x);
                sw.WriteLine(oDirection[iRegister, jRegister].x);
                sw.WriteLine(oSpeed[iRegister, jRegister].x);
                sw.WriteLine("----");
                iRegister = i;
                jRegister = j;

            }
            if (count == 0)
            {
                // locate the tile clone object on the track
                Instantiate(Tile, new Vector3(((i * Tilesize) + (Tilesize / 2)), transform.position.y - (tileBelowCar * Tilesize), (j * Tilesize) + (Tilesize / 2)), Quaternion.identity);

                turnRegister = turn;
                throttleRegister = throttle;
                brakeRegister = brake;
                directionRegister = direction;
                speedRegister = speed;
            }
            else
            {
                turnRegister += turn;
                throttleRegister += throttle;
                brakeRegister += brake;
                diff = ((direction - directionRegister + 180 + 360) % 360) - 180;
                directionRegister = (360 + directionRegister + (diff / 2)) % 360;
                speedRegister += speed;
            }
            count++;
        }


//         if (wheel2.GetGroundHit(out hit))
//         {
// 
//             //print ("forwardSlip "+hit.forwardSlip+" sidewaysSlip "+hit.sidewaysSlip);
// 
//         }

    }

    void WriteFile(string filepathIncludingFileName)
    {
        StreamWriter sw = new StreamWriter(filepathIncludingFileName);

        sw.WriteLine(turn);
        sw.WriteLine(throttle);
        sw.WriteLine(brake);
        sw.Flush();
        sw.Close();
    }

    void ReadFile(string filepathIncludingFileName)
    {
        string[] sr;

        sr = System.IO.File.ReadAllLines(filepathIncludingFileName);

        int f = 0;
        string input = "";
        while (true)
        {
            input = sr[f];
            f++;

            if (input == null) { break; }
            Debug.Log("line=" + input);
        }

    }

}
