using UnityEngine;
using System.Collections;

public class Teker : MonoBehaviour {

    public bool Cekis = true;
    public bool Direksiyon = true;
    public bool arka = false;
    public float gaz = 0.0f;

    private float don = 0.0f;
    private float torkEnfazla;
    private float tork;

    public float donusHiz;

    private float fren = 0.0f;

    public float dortTekerDireksiyon = 1.0f;//normalde 1 4x4 direksiyonda 2;
    public float icdis = 1;

    private float EnCokDon;
    private WheelHit hit;

    void Start()
    {
        torkEnfazla = 4500f;
        WheelCollider wheel = GetComponent<WheelCollider>();

        WheelFrictionCurve tmpForCurve = wheel.forwardFriction;
        WheelFrictionCurve tmpSidCurve = wheel.sidewaysFriction;
        JointSpring tmpSpr = wheel.suspensionSpring;

        tmpForCurve.stiffness = 0.04f;//0.040
        tmpForCurve.asymptoteValue = 5000f;//10000
        tmpSidCurve.stiffness = 0.02f;// change to 0.02
        tmpSidCurve.asymptoteValue = 5000f;//10000
        tmpSpr.spring = 15000f / icdis;
        tmpSpr.damper = 100f;

        wheel.forwardFriction = tmpForCurve;
        wheel.sidewaysFriction = tmpSidCurve;
        wheel.suspensionSpring = tmpSpr;


        if (Cekis)
        {
            tmpForCurve.stiffness = 0.04f;//0.040
            tmpForCurve.asymptoteValue = 5000f;//10000
            tmpSidCurve.stiffness = 0.06f;// change to 0.02
            tmpSidCurve.asymptoteValue = 5000f;//10000
            tmpSpr.spring = 15000f / icdis;
            tmpSpr.damper = 100f;

            wheel.forwardFriction = tmpForCurve;
            wheel.sidewaysFriction = tmpSidCurve;
            wheel.suspensionSpring = tmpSpr;
        }
    }

    void Update()
    {

        WheelCollider wheel = GetComponent<WheelCollider>();
        //Debug.Log(wheel.forwardSlip+"   "+wheel.sidewaysSlip );
        //if( hit.forwardSlip > 0.5 )

        //wheel.GetGroundHit( hit );
        donusHiz = transform.parent.GetComponent< Rigidbody > ().velocity.magnitude;//wheel.rpm;
        tork = torkEnfazla - (donusHiz * 35);
        EnCokDon = (62f - (donusHiz * 0.8f)) / dortTekerDireksiyon;//52//44  //f1 için 60 idi   /11.11
        if (EnCokDon < 5) EnCokDon = 5;
        if (torkEnfazla < 3000) torkEnfazla = 3000;
        //EnCokDon = (52-(donusHiz/10))/dortTekerDireksiyon;

        //Debug.Log("frenkayiii"+ hit.forwardSlip);
        //Debug.Log("yankayiii"+hit.sidewaysSlip);

        if (Input.GetKey(KeyCode.UpArrow) && Cekis && gaz < tork) gaz += tork * Time.deltaTime;
        else if (Cekis)
        {
            gaz = gaz - (gaz * Time.deltaTime);
        }
        if (Input.GetKey(KeyCode.DownArrow)) fren += 5000 * Time.deltaTime;
        else fren = fren - (fren * Time.deltaTime * 10);
        if (fren < 1) fren = 0;
        if (donusHiz < 1) donusHiz = 1;
        if (Input.GetKey(KeyCode.RightArrow) && Direksiyon && don < EnCokDon)
        {
            don += 100 * Time.deltaTime;
        }
        else if (Input.GetKey(KeyCode.LeftArrow) && Direksiyon && don > -EnCokDon)
        {
            don -= 100 * Time.deltaTime;
        }
        else if (Direksiyon)
        {
            don = don - (4 * don * Time.deltaTime);//Mathf.Sign(don)*30;
                                                   //Debug.Log(EnCokDon+"  "+don);
        }

        //if (357>transform.eulerAngles.x&&3<transform.eulerAngles.x&&Direksiyon) duzelt=-1;


        //Debug.Log(wheel.isGrounded );

        GetComponent< WheelCollider > ().motorTorque = gaz;
        //Debug.Log("gaz="+gaz);
        if (arka) GetComponent<WheelCollider> ().steerAngle = -don;
        else GetComponent<WheelCollider> ().steerAngle = don;
        GetComponent<WheelCollider> ().brakeTorque = fren;



    }

}
