using UnityEngine;
using System;
using System.Collections;

// #if UNITY_EDITOR
// using UnityEditor;

[RequireComponent(typeof(Rigidbody))]
public class CarController : MonoBehaviour 
{
    public bool isNPV = false;
    // Input variables for steering the car
    public float throttlePos = 0.0f;
    public float steeringAngle = 0.0f;

    // Car physical constants
    public float cDrag;                             // Drag (air friction constant)
    public float cRoll;                             // Rolling resistance (wheel friction constant)
    public float carMass = 1200.0f;                 // Mass of the car in Kilograms
    public Transform centerOfMass;

    // Car handling variables
    public float brakingPower = 100f;               // Braking power of the car
    public AnimationCurve steeringSensitityCurve;   // How the steering sensitivity changes with the current car speed
    public float steeringSensitivity = 0.5f;        // Multiplier for the steering sensitivity curve

    // Engine parameters
    public float rpmMin = 1000.0f;                  // Minimum RPM of the engine
    public float rpmMax = 6000.0f;                  // Maximum RPM of the engine
    public float rpm;                               // Current RPM of the engine
    public float engineTorque;                      // Current torque delivered by the engine
    public float maxTorque;                         // Maximum value to clamp the torque
    public float peakTorque = 100f;                 // Value of the torque at the peak of the curve
    public AnimationCurve torqueRPMCurve;           // How the torque delivered changes with the RPM
    public float gearRatio = 2.66f;                 // First gear hardcoded
    public float differentialRatio = 3.42f;

    public float brakeTorque;

    public float maxSpeed;

    public WheelController[] wheels;                // References to the wheel scripts

    // Private variables
    private Rigidbody rigidBody;
    private Vector3 previousVelocity;
    private Vector3 totalAcceleration;

    // Car events
    public event Action<int> gearShiftEvent;
    public event Action<Collision> crashEvent;


    public int currentGear;
    public float[] gearsRatio = { -2.769f, 2.083f, 3.769f, 3.267f, 3.538f, 4.083f }; //Toyota Supra
    public int maxGears = 6;
    public float virtualRPM;


    private float timeAccelaration;

    public float currentSpeed = 1;
    public int maximumSpeed = 240; //KM/h

    private bool isGearShiftedDown = false;
    private float timeShift;

    //Damage - related - Added by Cockcrow
    [NonSerialized]
    public bool processContacts = false;        // This is set to True by the components that use contacts (Audio, Damage)
    [NonSerialized]
    public float impactThreeshold = 0.6f;       // 0.0 - 1.0. The DotNormal of the impact is calculated. Less than this value means drag, more means impact.
    [NonSerialized]
    public float impactInterval = 0.2f;         // Time interval between processing impacts for visual or sound effects.
    [NonSerialized]
    public float impactIntervalRandom = 0.4f;   // Random percentaje for the impact interval, avoiding regularities.
    [NonSerialized]
    public float impactMinSpeed = 2.0f;         // Minimum relative velocity at which conctacts may be consideered impacts.

    Transform m_transform;
//     Rigidbody m_rigidbody;

    public Transform cachedTransform { get { return m_transform; } }
    public Rigidbody cachedRigidbody { get { return rigidBody; } }

    public bool showContactGizmos = false;

    Vector3 m_localDragPosition = Vector3.zero;
    Vector3 m_localDragVelocity = Vector3.zero;
    int m_localDragHardness = 0;

    float m_lastStrongImpactTime = 0.0f;

    Vector3 m_sumImpactPosition = Vector3.zero;
    Vector3 m_sumImpactVelocity = Vector3.zero;

    public Vector3 localImpactPosition { get { return m_sumImpactPosition; } }
    public Vector3 localImpactVelocity { get { return m_sumImpactVelocity; } }
    public bool isHardDrag { get { return m_localDragHardness >= 0; } }

    int m_sumImpactCount = 0;
    int m_sumImpactHardness = 0;
    float m_lastImpactTime = 0.0f;


    public delegate void OnImpact();
    public OnImpact onImpact;

    public static CarController current = null;

    void Start () 
    {
        rigidBody = GetComponent<Rigidbody>();
        currentGear = 1; //starting gear, in future we can put a starter
    }

    void OnEnable()
    {
        m_transform = GetComponent<Transform>();
    }

    void Update()
    {
        timeAccelaration = Mathf.Clamp(timeAccelaration, 0f, timeAccelaration);
        currentSpeed = rigidBody.velocity.magnitude * 3.6f;

        Vector3 localVelocity = transform.InverseTransformDirection(rigidBody.velocity);
        
        /*if ( throttlePos < 0.0f)
        {
            if (localVelocity.z > 0.0f)
            {
                Debug.Log("Brakes");
                wheels[0].brakeTorque = brakingPower;
                wheels[1].brakeTorque = brakingPower;
                wheels[2].brakeTorque = brakingPower;
                wheels[3].brakeTorque = brakingPower;
                throttlePos = 0.0f;
            }
            else
            {
                wheels[0].brakeTorque = 0.0f;
                wheels[1].brakeTorque = 0.0f;
                wheels[2].brakeTorque = 0.0f;
                wheels[3].brakeTorque = 0.0f;
            }
        }*/
            /*
        else
        {
            throttlePos = Input.GetAxis(playerPrefix + "Vertical");

            wheels[0].brakeTorque = 0.0f;
            wheels[1].brakeTorque = 0.0f;
            wheels[2].brakeTorque = 0.0f;
            wheels[3].brakeTorque = 0.0f;
        }
             * */
                /*
        if (Input.GetAxis(playerPrefix + "Vertical") > -1.0f)
        {
            throttlePos = Input.GetAxis(playerPrefix + "Vertical");
        }
        */
        // Get the wheel average rotation rate from the front wheels
        float wheelRotRate = 0.5f * (wheels[0].rpm + wheels[1].rpm);

        // Update the engine RPM from the wheel rotation rate
        rpm = wheelRotRate * gearsRatio[currentGear] * differentialRatio;
        rpm = Mathf.Clamp(rpm, rpmMin, rpmMax);

        // Get the maximum torque the engine can deliver for the current RPM
        maxTorque = GetMaxTorque(rpm);
        // Get the final delivered torque from the throttle position

        if (!isNPV)
        {
            engineTorque = maxTorque * throttlePos;
            if (throttlePos == 0.0f)
            {
                engineTorque = 0.0f;
            }

          
            brakeTorque = 0;
            if (throttlePos < 0.0f)
            {
                brakeTorque = Mathf.Abs(throttlePos) * brakingPower;
            }
        }

        // Apply the torque to the wheels
        //wheels[1].driveTorque = engineTorque;
        //wheels[0].driveTorque = engineTorque;
        wheels[2].driveTorque = engineTorque;
        wheels[3].driveTorque = engineTorque;

    
        wheels[0].brakeTorque = brakeTorque;
        wheels[1].brakeTorque = brakeTorque;
        wheels[2].brakeTorque = brakeTorque;
        wheels[3].brakeTorque = brakeTorque;

        GearsShift();

       
        if (processContacts)
        {
            UpdateDragState(Vector3.zero, Vector3.zero, m_localDragHardness);
            // debugText = string.Format("Drag Pos: {0}  Drag Velocity: {1,5:0.00}  Drag Friction: {2,4:0.00}", localDragPosition, localDragVelocity.magnitude, localDragFriction);
        }

    }



    /// <summary>
    /// Sample the RPM vs torque engine curve to get the torque for the current RPM value
    /// </summary>
    /// <param name="currentRPM">The current engine RPM</param>
    float GetMaxTorque(float currentRPM)
    {
        float normalizedRPM = (currentRPM - rpmMin) / (rpmMax - rpmMin);
        float val = torqueRPMCurve.Evaluate(Mathf.Abs(normalizedRPM)) * Mathf.Sign(normalizedRPM);
        return val * peakTorque;
    }


	void FixedUpdate ()
    {
//         Debug.Log("steer: " + steeringAngle);
        float currentSteeringAngle = steeringAngle * steeringSensitivity * steeringSensitityCurve.Evaluate(transform.InverseTransformDirection(rigidBody.velocity).z / maxSpeed) * 45.0f;
        currentSteeringAngle = Mathf.Clamp(currentSteeringAngle, -45.0f, 45.0f);

        // Turn the front wheels according to the input
        wheels[0].steeringAngle = currentSteeringAngle;
        wheels[1].steeringAngle = currentSteeringAngle;

        //wheels[0].overrideSlipRatio = true;
        //wheels[0].overridenSlipRatio = wheels[1].slipRatio;

        //wheels[3].overrideSlipRatio = true;
        //wheels[3].overridenSlipRatio = wheels[2].slipRatio;

        // Calculate and apply the longitudinal force (comes from air drag and rolling friction)
        Vector3 velocity = rigidBody.transform.InverseTransformDirection(rigidBody.velocity);
        Vector3 fDrag = -cDrag * velocity.z * velocity.z * transform.forward;
        Vector3 fRoll = -cRoll * velocity.z * transform.forward;
        Vector3 fLong = fDrag + fRoll;  
        Vector3 acceleration = fLong / carMass;
        rigidBody.velocity += acceleration * Time.deltaTime;
        //Debug.DrawLine(transform.position, transform.position + totalAcceleration, Color.magenta);

        // Calculate the total acceleration of the car and use it to displace the center of mass.
        // This way we get different weight transfer to each wheel
        rigidBody.centerOfMass = centerOfMass.localPosition;
        totalAcceleration = (rigidBody.velocity - previousVelocity) / Time.deltaTime;
        totalAcceleration = totalAcceleration.magnitude > 15.0f ? totalAcceleration.normalized : totalAcceleration;
        rigidBody.centerOfMass -= transform.InverseTransformDirection(Vector3.Scale(totalAcceleration, Vector3.forward + Vector3.right) * 0.01f);
        previousVelocity = rigidBody.velocity;

        
        if (Mathf.Abs(rigidBody.angularVelocity.y) > 5.0f)
        {
            rigidBody.angularDrag = 3.0f;
        }
        else
        {
            rigidBody.angularDrag = 0.2f;
        }

   
        if (processContacts)
            HandleImpacts();


    }


    /// <summary>
    /// Shifts gear up or down according to speed.
    /// </summary>
    void GearsShift()
    {
        if (currentSpeed > maximumSpeed / maxGears * (currentGear - 1) && currentSpeed < maximumSpeed / maxGears * (currentGear))
        {
            virtualRPM = (currentSpeed / (maximumSpeed / maxGears * currentGear)) * rpmMax / 10;
        }
        if (currentGear < maxGears && currentSpeed > maximumSpeed / maxGears * (currentGear))
        {
            currentGear++;
            // Fire the gear shift event
            if(gearShiftEvent != null)
            {
                gearShiftEvent(currentGear);
            }
        }
        else if (currentGear > 1 && currentSpeed < maximumSpeed / maxGears * (currentGear - 1) && !isGearShiftedDown)
        {
            currentGear--;
            // Fire the gear shift event
            if (gearShiftEvent != null)
            {
                gearShiftEvent(currentGear);
            }

            timeShift = Time.timeSinceLevelLoad + 1.0f;
            isGearShiftedDown = true;
        }
        virtualRPM = (currentSpeed / (maximumSpeed / maxGears * currentGear)) * rpmMax / 1;
        
        if (Time.timeSinceLevelLoad >= timeShift && isGearShiftedDown)
        {
            isGearShiftedDown = false;
        }
    }


    /// <summary>
    /// Collission handler
    /// </summary>
    /// <param name="other"></param>
    void OnCollisionEnter(Collision other)
    {
        
        // Prevent the wheels to sleep for some time if a strong impact occurs

        if (other.relativeVelocity.magnitude > 4.0f)
            m_lastStrongImpactTime = Time.time;

        if (processContacts)
            ProcessContacts(other, true);

        if (crashEvent != null)
        {
            crashEvent(other);
        }


    }

    Rect areagui = new Rect(0f, 20f, 500f, 300f);
    bool showDebug;
    void OnGUI()
    {
        GUI.contentColor = Color.black;

        if (GUILayout.Button("Toggle Debug"))
            showDebug = !showDebug;
        if (!showDebug)
            return;
//         GUILayout.BeginArea(areagui, EditorStyles.helpBox);
        GUILayout.BeginHorizontal();

        GUILayout.BeginVertical();
        GUILayout.Label("Wheel");
        GUILayout.Label("RPM");
        GUILayout.Label("FroceFWD");
        GUILayout.Label("ForceSide");
        GUILayout.Label("SlipRatio");
        GUILayout.Label("SlipAngle");
        GUILayout.Label("LinearVel");

        GUILayout.EndVertical();

        foreach (WheelController w in wheels)
        {
            GUILayout.BeginVertical();
            GUILayout.Label(w.name);
            GUILayout.Label(w.rpm.ToString("0.0"));
            GUILayout.Label(w.fwdForce.ToString("0.0"));
            GUILayout.Label(w.sideForce.ToString("0.0"));
            GUILayout.Label(w.slipRatio.ToString("0.000"));
            GUILayout.Label((w.slipAngle).ToString("0.0"));
            GUILayout.Label((w.linearVel).ToString("0.00"));

            GUILayout.EndVertical();
        }
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.green;

        Gizmos.DrawSphere(GetComponent<Rigidbody>().worldCenterOfMass, 0.1f);
    }

    //added by cockcrow
//     void OnCollisionEnter(Collision collision)
//     {
//         // Prevent the wheels to sleep for some time if a strong impact occurs
// 
//         if (collision.relativeVelocity.magnitude > 4.0f)
//             m_lastStrongImpactTime = Time.time;
// 
//         if (processContacts)
//             ProcessContacts(collision, true);
//     }


    void OnCollisionStay(Collision collision)
    {
        if (processContacts)
            ProcessContacts(collision, false);
    }

    void ProcessContacts(Collision col, bool forceImpact)
    {
        int impactCount = 0;                        // All impacts
        Vector3 impactPosition = Vector3.zero;
        Vector3 impactVelocity = Vector3.zero;
        int impactHardness = 0;

        int dragCount = 0;
        Vector3 dragPosition = Vector3.zero;
        Vector3 dragVelocity = Vector3.zero;
        int dragHardness = 0;

        float sqrImpactSpeed = impactMinSpeed * impactMinSpeed;

        // We process all contacts individually and get an impact and/or drag amount out of each one.

        foreach (ContactPoint contact in col.contacts)
        {
            Collider collider = contact.otherCollider;

            // Get the type of the impacted material: hard +1, soft -1

            int hardness = 0;
            //			UpdateGroundMaterialCached(collider.sharedMaterial, ref m_lastImpactedMaterial, ref m_impactedGroundMaterial);

            //			if (m_impactedGroundMaterial != null)
            //				hardness = m_impactedGroundMaterial.surfaceType == GroundMaterial.SurfaceType.Hard? +1 : -1;

            // Calculate the velocity of the body in the contact point with respect to the colliding object

            Vector3 v = rigidBody.GetPointVelocity(contact.point);
            if (collider.attachedRigidbody != null)
                v -= collider.attachedRigidbody.GetPointVelocity(contact.point);

            float dragRatio = Vector3.Dot(v, contact.normal);

            // Determine whether this contact is an impact or a drag

            if (dragRatio < -impactThreeshold || forceImpact && col.relativeVelocity.sqrMagnitude > sqrImpactSpeed)
            {
                // Impact

                impactCount++;
                impactPosition += contact.point;
                impactVelocity += col.relativeVelocity;
                impactHardness += hardness;

                //				if (showContactGizmos)
                //					Debug.DrawLine(contact.point, contact.point + CommonTools.Lin2Log(v), Color.red);
            }
            else if (dragRatio < impactThreeshold)
            {
                // Drag

                dragCount++;
                dragPosition += contact.point;
                dragVelocity += v;
                dragHardness += hardness;

                //				if (showContactGizmos)
                //					Debug.DrawLine(contact.point, contact.point + CommonTools.Lin2Log(v), Color.cyan);
            }

            // Debug.DrawLine(contact.point, contact.point + CommonTools.Lin2Log(v), Color.Lerp(Color.cyan, Color.red, Mathf.Abs(dragRatio)));
            if (showContactGizmos)
                Debug.DrawLine(contact.point, contact.point + contact.normal * 0.25f, Color.yellow);
        }

        // Accumulate impact values received.

        if (impactCount > 0)
        {
            float invCount = 1.0f / impactCount;
            impactPosition *= invCount;
            impactVelocity *= invCount;

            m_sumImpactCount++;
            m_sumImpactPosition += m_transform.InverseTransformPoint(impactPosition);
            m_sumImpactVelocity += m_transform.InverseTransformDirection(impactVelocity);
            m_sumImpactHardness += impactHardness;
        }

        // Update the current drag value

        if (dragCount > 0)
        {
            float invCount = 1.0f / dragCount;
            dragPosition *= invCount;
            dragVelocity *= invCount;

            UpdateDragState(m_transform.InverseTransformPoint(dragPosition), m_transform.InverseTransformDirection(dragVelocity), dragHardness);
        }
    }

    void UpdateDragState(Vector3 dragPosition, Vector3 dragVelocity, int dragHardness)
    {
        if (dragVelocity.sqrMagnitude > 0.001f)
        {
            m_localDragPosition = Vector3.Lerp(m_localDragPosition, dragPosition, 10.0f * Time.deltaTime);
            m_localDragVelocity = Vector3.Lerp(m_localDragVelocity, dragVelocity, 20.0f * Time.deltaTime);
            m_localDragHardness = dragHardness;
        }
        else
        {
            m_localDragVelocity = Vector3.Lerp(m_localDragVelocity, Vector3.zero, 10.0f * Time.deltaTime);
        }

        //		if (showContactGizmos && localDragVelocity.sqrMagnitude > 0.001f)
        //			Debug.DrawLine(transform.TransformPoint(localDragPosition), transform.TransformPoint(localDragPosition) + CommonTools.Lin2Log(transform.TransformDirection(localDragVelocity)), Color.cyan, 0.05f, false);
    }

    void HandleImpacts()
    {
        // Multiple impacts within an impact interval are accumulated and averaged later.

        if (Time.time - m_lastImpactTime >= impactInterval && m_sumImpactCount > 0)
        {
            // Prepare the impact parameters

            float invCount = 1.0f / m_sumImpactCount;

            m_sumImpactPosition *= invCount;
            m_sumImpactVelocity *= invCount;

            // Notify the listeners on the impact

            if (onImpact != null)
            {
                current = this;
                onImpact();
                current = null;
            }

            // debugText = string.Format("Count: {4}  Impact Pos: {0}  Impact Velocity: {1} ({2,5:0.00})  Impact Friction: {3,4:0.00}", localImpactPosition, localImpactVelocity, localImpactVelocity.magnitude, localImpactFriction, m_sumImpactCount);
            //			if (showContactGizmos && localImpactVelocity.sqrMagnitude > 0.001f)
            //				Debug.DrawLine(transform.TransformPoint(localImpactPosition), transform.TransformPoint(localImpactPosition) + CommonTools.Lin2Log(transform.TransformDirection(localImpactVelocity)), Color.red, 0.2f, false);

            // Reset impact data

            m_sumImpactCount = 0;
            m_sumImpactPosition = Vector3.zero;
            m_sumImpactVelocity = Vector3.zero;
            m_sumImpactHardness = 0;

            m_lastImpactTime = Time.time + impactInterval * UnityEngine.Random.Range(-impactIntervalRandom, impactIntervalRandom);  // Add a random variation for avoiding regularities
        }
    }
    //--abcc

}

/*#endif*/
