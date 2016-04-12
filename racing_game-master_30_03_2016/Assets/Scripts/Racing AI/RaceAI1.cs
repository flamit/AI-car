using UnityEngine;
using System.Collections;
using System.IO;

public class RaceAI1 : MonoBehaviour {

    public float Tilesize = 10.0f;
    private float throttle;
    private float turn;
    private float brake;
    private float direction;
    private float speed = 0.0f;
    private float dragR;
    public GameObject Usercar;

    //those variables named "tweak" are to be tuned with respect to the dynamics of your car & track
    public float tweakTurnMaxA = 50;                //maximum steering angle at standstill
    public float tweakTurnMaxB = 11.11f;            //decreases the decrease in max steering angle with speed
    public float tweakTurnA = 0.8f;                 //Sharpens steering 
    public float tweakThrottleMaxA = 25f;           //increases maximum throttle with speed
    public float tweakThrottleMaxB = 1050f;         //increases maximum throttle independent of speed
    public float tweakThrottleMaxC = 3.0f;          //decreases maximum throttle with deviation from recoded direction
    public float tweakAccelerationMaxA = 0.3f;      //increases maximum allowed acceleration in straight
    public float tweakAccelerationMaxB = 1000f;     //increases maximum allowed lateral acceleration 
    public float tweakThrottleA = 1.0f;             // Adjusts throttle with the difference in recorded speed and actual speed
    public float tweakThrottleB = 400f;             // resitricts the throttle to this value if the car is very slow
    public float tweakThrottleC = 2000f;            //throttle value at the first two seconds
    public float tweakThrottleD = 3f;               //Rate of slowdown when a car is ahead
    public float tweakThrottleE = 1.0f;             //Sharpens throttle response
    public float tweakBreakA = 1.0f;                //increases the break force proprotinal to the difference in recorded speed and actual speed
    public float tweakBreakB = 1.4f;                //recorded brake force times this is the brake force applied
    public float tweakAvoidanceSteer = 7f;          // Increases car avoidance steering magnitude
    public float tweakWaitforUser = 0.1f;           //Increases breaking if the car is forward the user car
    public bool TweakToggle = true;                 //Turn this on to let GA tweaking

    public int layerMask = 12;                      //set this to the layer of your cars

    public bool PoliceMode = false;                 //urn this on for Cop Mode
    //var EscapeMode : boolean = false;
    public float timeToReset = 5; //++++++++++++++++++++++++++

    public int Mark;//+++++++++++++++++++++++++++++++

    public CarController controller;                // CarController for Amit Car

    private float x;                                // current x position of car
    private float z;                                // current z position of car

    public int i;                                   // index of 2-dimensional tile array
    public int j;                                   // index of 2-dimensional tile array

    private int iregister = 0;                      // index of 2-dimensional tile array recorded
    private int jregister = 0;                      // index of 2-dimensional tile array recorded
    private float turnregister = 0.0f;              // steering value recorded
    private float throttleregister = 0.0f;          // throttle value recorded
    private float brakeregister = 0.0f;             // brake value recorded
    private int count = 0;                          // count value recorded
    private float directionregister = 0.0f;         // direction value recorded
    private float speedregister = 0.0f;             // speed value recored

    private float TurningSpeed;                     // current rpm of car engine
    private float TurnMax;                          // maximum rpm of car engine
    private float ThrottleMax;                      // maximum throttle of car engine - torque
    private float DeltaDirection;                   // delta value between current direction and recorded value

    public float TileToTileTime = 0.0f;
    public int[] iPre = new int [10];
    public int[] jPre = new int [10];
    public int[] directionPre = new int[10];
    
    private float rthrottle;
    private float rturn;
    private float rbrake;
    private float speedz = 0.0f;
    private float speedx = 0.0f;
    private float accelerationdirection;
    private float acceleration;
    private float accelerationx;
    private float accelerationz;

    public float z0 = 0.0f;
    public float y0 = 0.0f;

    private float accelerationMax;

    public Vector3 temp3;

    private Vector3 rayDirection;
    private RaycastHit hitR;
    private RaycastHit hitL;
    private float hitMin;
    private float leux;
    private float leuz;

    private int sweepRay = 5;
    private int sweepcount = 0;

    public bool UserIsNear = false;
    public bool UserIsVeryNear = false;
    public bool UserIsFront = false;
    public Vector3 targetDir;

    private float AngleBetweenCars;
    private float EscapeMultiplier;


    void Start()
    {
        layerMask = ~layerMask;

        controller = GetComponent<CarController>();

        //randomize driving properties of cars
        rthrottle = Random.Range(1f, ((transform.position.x / 3700f) + 1.18f));
        rturn = Random.Range(1f, 1.1f);
        rbrake = Random.Range(1f, 1.1f);
        dragR = GetComponent< Rigidbody > ().drag;
        for (var k = 0; k < 10; k++)
        {
            iPre[k] = 0;
            jPre[k] = 0;
            directionPre[k] = 0;
        }
    }


    void Update()
    {

        //         TurningSpeed = wheelFront1.rpm;
        TurningSpeed = controller.rpm;

        z = transform.position.z;
        x = transform.position.x;
        leux = transform.localEulerAngles.x;
        leuz = transform.localEulerAngles.z;
        if (leux > 180) leux = 360 - leux;
        if (leuz > 180) leuz = 360 - leuz;
        //if (EscapeMode) EscapeMultiplier=-1;
        //else EscapeMultiplier=1;


        accelerationx = GetComponent< Rigidbody > ().velocity.x - speedx;
        accelerationz = GetComponent< Rigidbody > ().velocity.z - speedz;
        speedz = GetComponent< Rigidbody > ().velocity.z;
        speedx = GetComponent< Rigidbody > ().velocity.x;
        direction = transform.eulerAngles.y % 360;
        speed = GetComponent< Rigidbody > ().velocity.magnitude;
        accelerationdirection = -getAngleOfPointOnCircle(-accelerationx, -accelerationz) - 90f;         // the direction to accelerate
        acceleration = Mathf.Sqrt((accelerationx * accelerationx) + (accelerationz * accelerationz));

        i = (int)((x - (x % Tilesize)) / Tilesize);
        j = (int)((z - (z % Tilesize)) / Tilesize);

        CheckTime();

        if (iregister != i || jregister != j)
        {
            TileToTileTime = Time.timeSinceLevelLoad;
            if (GetData.counter[i, j].x == float.NaN)
                count = 0;
            else
                count = (int) GetData.counter[i, j].x;

            // buffer for previous position of car. it's necessary for reset position when the car is in stuck
            if (count != 0)
            {
                directionPre[9] = directionPre[8];
                directionPre[8] = directionPre[7];
                directionPre[7] = directionPre[6];
                directionPre[6] = directionPre[5];
                directionPre[5] = directionPre[4];
                directionPre[4] = directionPre[3];
                directionPre[3] = directionPre[2];
                directionPre[2] = directionPre[1];
                directionPre[1] = (int)directionregister;
                iPre[9] = iPre[8];
                jPre[9] = jPre[8];
                iPre[8] = iPre[7];
                jPre[8] = jPre[7];
                iPre[7] = iPre[6];
                jPre[7] = jPre[6];
                iPre[6] = iPre[5];
                jPre[6] = jPre[5];
                iPre[5] = iPre[4];
                jPre[5] = jPre[4];
                iPre[4] = iPre[3];
                jPre[4] = jPre[3];
                iPre[3] = iPre[2];
                jPre[3] = jPre[2];
                iPre[2] = iPre[1];
                jPre[2] = jPre[1];
                iPre[1] = iregister;
                jPre[1] = jregister;
            }
            iregister = i;
            jregister = j;

            if (count == null || count == 0) count = 1;

            throttleregister = (int) GetData.othrottle[i, j].x / count;
            turnregister = GetData.oturn[i, j].x / count;
            brakeregister = GetData.obrake[i, j].x / count;
            directionregister = GetData.odirection[i, j].x;
            speedregister = GetData.ospeed[i, j].x / count;


            if (throttleregister == 0 && directionregister == 0 && turnregister == 0)
            {
                throttleregister = tweakThrottleC;
                if (GetData.odirection[i - 1, j - 1] != null)
                    directionregister = direction + direction - GetData.odirection[i - 1, j - 1].x;
                else if (GetData.odirection[i, j - 1] != null)
                    directionregister = direction + direction - GetData.odirection[i, j - 1].x;
                else if (GetData.odirection[i + 1, j - 1] != null)
                    directionregister = direction + direction - GetData.odirection[i + 1, j - 1].x;
                else if (GetData.odirection[i - 1, j] != null)
                    directionregister = direction + direction - GetData.odirection[i - 1, j].x;
                else if (GetData.odirection[i + 1, j] != null)
                    directionregister = direction + direction - GetData.odirection[i + 1, j].x;
                else if (GetData.odirection[i - 1, j + 1] != null)
                    directionregister = direction + direction - GetData.odirection[i - 1, j + 1].x;
                else if (GetData.odirection[i, j + 1] != null)
                    directionregister = direction + direction - GetData.odirection[i, j + 1].x;
                else if (GetData.odirection[i + 1, j + 1] != null)
                    directionregister = direction + direction - GetData.odirection[i + 1, j + 1].x;
                brakeregister = 0;
                speedregister = GetData.ospeed[iPre[1], jPre[1]].x;
                turnregister = 0;
            }
        }

        // calculate the each element value of engine based on current car state, recorded track data and tweak parameters

        // delta angle value between current direction and recorded value
        DeltaDirection = (directionregister - direction);

        //maximum value of rpm and throttle of engine
        TurnMax = tweakTurnMaxA - (TurningSpeed / tweakTurnMaxB);
        ThrottleMax = (speed * tweakThrottleMaxA * rthrottle) + tweakThrottleMaxB - (Mathf.Abs(DeltaDirection) * tweakThrottleMaxC);

        // calculate acceleration max limit
        if ((direction - accelerationdirection) > 180)
            accelerationMax = tweakAccelerationMaxA - (turnregister / tweakAccelerationMaxB) - (Mathf.Abs((direction - accelerationdirection) - 360) / tweakAccelerationMaxB);

        if ((direction - accelerationdirection) <= 180 && (direction - accelerationdirection) > -180)
            accelerationMax = tweakAccelerationMaxA - (turnregister / tweakAccelerationMaxB) - (Mathf.Abs(direction - accelerationdirection) / tweakAccelerationMaxB);

        if ((direction - accelerationdirection) <= -180)
            accelerationMax = tweakAccelerationMaxA - (turnregister / tweakAccelerationMaxB) - (Mathf.Abs((direction - accelerationdirection) + 360) / tweakAccelerationMaxB);

        accelerationMax -= Mathf.Max(0.0003f * Mathf.Abs(leux), 0.0005f * Mathf.Abs(leuz));

        throttle = (tweakThrottleE * throttleregister * rthrottle) - ((speed - (speedregister * rthrottle)) * tweakThrottleA);
        if (brake != 0 && ThrottleMax < throttle && (acceleration > accelerationMax || turnregister > (TurnMax / 3))) throttle = ThrottleMax;
        if (speed < 2 || throttle <= 0) throttle = tweakThrottleB;
        if (Time.timeSinceLevelLoad < 2) throttle = tweakThrottleC * rthrottle;

        // Draw gizmo line for debug visualization rendering
        temp3 = transform.TransformPoint(new Vector3(Tilesize * 0.4f + ((2f - sweepcount) * Tilesize * 0.15f), 2f, Tilesize * 8f));
        Debug.DrawLine(transform.position + new Vector3(0, Tilesize * 0.1f, 0), temp3, Color.red);
        temp3 = transform.TransformPoint(new Vector3(-Tilesize * 0.4f + ((2f - sweepcount) * Tilesize * 0.15f), 2f, Tilesize * 8f));
        Debug.DrawLine(transform.position + new Vector3(0, Tilesize * 0.1f, 0), temp3, Color.red);

        if (speed < speedregister) brake = 0;
        else if (speed > speedregister * rthrottle) brake = (speed - speedregister) * tweakBreakA;
        else brake = brakeregister / tweakBreakB;

        if (DeltaDirection > 180) turn = (turnregister + (DeltaDirection) - 360);
        if (DeltaDirection <= 180 && DeltaDirection > -180) turn = (turnregister + (DeltaDirection));
        if (DeltaDirection <= -180) turn = (turnregister + (DeltaDirection) + 360);

        temp3 = transform.TransformPoint(new Vector3((2f - sweepcount) * Tilesize * 0.15f, Tilesize * 0.25f, Tilesize * 8f));
        rayDirection = temp3 - transform.position;
        temp3 = transform.TransformPoint(new Vector3(Tilesize * 0.4f, Tilesize * 0.15f, Tilesize * 0.6f));

        // interaction with user car
        if (Vector3.Distance(Usercar.transform.position, transform.position) < Tilesize * 16 && Time.timeSinceLevelLoad > 4 && PoliceMode) UserIsNear = true;
        else UserIsNear = false;
        if (Vector3.Distance(Usercar.transform.position, transform.position) < Tilesize * 4 && Time.timeSinceLevelLoad > 4 && PoliceMode) UserIsVeryNear = true;
        else UserIsVeryNear = false;

        targetDir = Usercar.transform.position - transform.position;
        AngleBetweenCars = (Vector3.Angle(transform.forward, targetDir));

        if ((AngleBetweenCars) < 20 && UserIsNear) UserIsFront = true;
        else UserIsFront = false;

        // adjust throttle, brake, steering according the distance to obstacle or curve
        if (Physics.Raycast(temp3, rayDirection, out hitR, Tilesize * 8, layerMask) && !UserIsVeryNear && !UserIsFront)
        {
            if (hitR.distance >= Tilesize * 4) throttle /= 2;
            if (hitR.distance <= Tilesize * 4) throttle /= 4;
            //else brake=(tweakThrottleD*(Tilesize+1))/(0.2*(hitR.distance+1));
            brake -= 0.1f * leux;
            turn -= tweakAvoidanceSteer * (Tilesize + 1) / (hitR.distance + 1);
        }
        temp3 = transform.TransformPoint(new Vector3(-Tilesize * 0.4f, Tilesize * 0.1f, Tilesize * 0.6f));

        if (Physics.Raycast(temp3, rayDirection, out hitL, Tilesize * 8, layerMask) && !UserIsVeryNear && !UserIsFront)
        {
            if (hitL.distance >= Tilesize * 4) throttle /= 2;
            if (hitL.distance <= Tilesize * 4) throttle /= 4;
            //if (hit.distance<=Tilesize*3)brake=4;
            //else brake=(tweakThrottleD*(Tilesize+1))/(0.2*(hitL.distance+1));
            brake -= 0.2f * leux;
            turn += tweakAvoidanceSteer * (Tilesize + 1) / (hitL.distance + 1);
        }
        if (hitL.distance != 0 && hitR.distance != 0 && !UserIsVeryNear && !UserIsFront)
        {
            hitMin = Mathf.Min(hitL.distance, hitR.distance);
            brake = (tweakThrottleD * (Tilesize + 1f)) / (0.4f * (hitMin + 1f));
            brake -= 0.4f * leux;
        }
        if (hitL.distance == 0 && hitR.distance != 0 && !UserIsVeryNear && !UserIsFront)
        {
            brake = (tweakThrottleD * (Tilesize + 1f)) / (0.4f * (hitR.distance + 1f));
            brake -= 0.5f * leux;
        }
        if (hitL.distance != 0 && hitR.distance == 0 && !UserIsVeryNear && !UserIsFront)
        {
            brake = (tweakThrottleD * (Tilesize + 1f)) / (0.4f * (hitL.distance + 1f));
            brake -= 0.5f * leux;
        }
        sweepcount++;
        if (sweepcount == sweepRay) sweepcount = 0;
        if (speed < 2) brake = 0;
//         if (!wheelBack1.isGrounded)
//         {
//             throttle = 0;
//             GetComponent< Rigidbody > ().drag = 1;
//         }//the physics engine makes crashes look very unrealistic (the cars fly away at rocket speeds) this is to prevent it.
//         else GetComponent< Rigidbody > ().drag = dragR;
        turn *= tweakTurnA;
        if (turn > TurnMax) turn = TurnMax;
        if (turn < -TurnMax) turn = -TurnMax;
        turn *= rturn;

        if (Mathf.Abs(Vector3.Angle(targetDir, Usercar.transform.forward)) < 60)
        { //check this again
            brake += Vector3.Distance(Usercar.transform.position, transform.position) * (tweakWaitforUser);
        }

        if (PoliceMode && UserIsNear)
        {
            /*if(160<(AngleBetweenCars)&&(AngleBetweenCars)<200) {
            throttle*=1.05;
            }*/
            /*
            if(AngleBetweenCars>180&&AngleBetweenCars<300) {
            turn+=((AngleBetweenCars-180)/3);
            if(turn>20)turn=20;
            }
            if(AngleBetweenCars>60&&AngleBetweenCars<180){
            turn-=(-0.3333*AngleBetweenCars)+60;
            if(turn<-20)turn=-20;
            }
            */
            //turn=(-0.0028*(AngleBetweenCars*AngleBetweenCars))+(0.5*AngleBetweenCars);
            if (Vector3.Angle(transform.right, targetDir) < 90)
                turn += ((-0.0028f * (AngleBetweenCars * AngleBetweenCars)) + (0.5f * AngleBetweenCars)) / 3f;
            else turn -= ((-0.0028f * (AngleBetweenCars * AngleBetweenCars)) + (0.5f * AngleBetweenCars)) / 3f;
            if (turn > TurnMax) turn = TurnMax;
            if (turn < -TurnMax) turn = -TurnMax;
            if (AngleBetweenCars > 120)
            {
                if (GetComponent< Rigidbody > ().velocity.magnitude > Usercar.transform.GetComponent< Rigidbody > ().velocity.magnitude * 0.8f)
                    brake = 1.5f * (brake + (tweakThrottleMaxB / 5f));
            }
        }

        controller.engineTorque = throttle;
        controller.steeringAngle = turn;
//         Debug.Log("turn: " + turn);
        if (brake < 0) brake = 0;
        controller.brakeTorque = brake;

    }

    // check if the car is in stuck. calculate the time staying on same tile
    void CheckTime()
    {
        if (Time.timeSinceLevelLoad - TileToTileTime > timeToReset)
        {
            transform.position = new Vector3((Tilesize / 2f) + iPre[9] * Tilesize, transform.position.y + 10.0f, (Tilesize / 2f) + jPre[9] * Tilesize);
            transform.localEulerAngles = new Vector3(0, directionPre[9], 0);
            throttle = tweakThrottleB;
        }
    }

    float getAngleOfPointOnCircle(float x, float y)
    {
        var r = Mathf.Sqrt(Mathf.Abs(x - z0) * Mathf.Abs(x - z0) + Mathf.Abs(y - y0) * Mathf.Abs(y - y0));
        var p0x = z0;
        var p0y = y0 + r;
        return ((2 * Mathf.Atan2(y - p0y, x - p0x)) * 180 / Mathf.PI);
    }

    void OnTriggerEnter(Collider other)
    {
        if (TweakToggle)
            Tweak.TriggerPoints[Mark]++;
    }

}
