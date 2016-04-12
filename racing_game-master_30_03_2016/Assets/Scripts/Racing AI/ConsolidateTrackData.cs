using UnityEngine;
using System.Collections;
using System.IO;

public class ConsolidateTrackData : MonoBehaviour {

    public string filePathAl1 = "b1.txt";
    public string filePathAl2 = "b2.txt";
    public string filePathTo2 = "s.txt";

    private StreamWriter sw;
    private float diff;
    private float directionSon;
    private float x;
    private float z;
    private Vector2[,] oTurn = new Vector2[1000, 1000];
    private Vector2[,] oThrottle = new Vector2[1000, 1000];
    private Vector2[,] oBrake = new Vector2[1000, 1000];
    private Vector2[,] counter = new Vector2[1000, 1000];
    private Vector2[,] oDirection = new Vector2[1000, 1000];
    private Vector2[,] oSpeed = new Vector2[1000, 1000];

    private Vector2[,] oTurn1 = new Vector2[1000, 1000];
    private Vector2[,] oThrottle1 = new Vector2[1000, 1000];
    private Vector2[,] oBrake1 = new Vector2[1000, 1000];
    private Vector2[,] counter1 = new Vector2[1000, 1000];
    private Vector2[,] oDirection1 = new Vector2[1000, 1000];
    private Vector2[,] oSpeed1 = new Vector2[1000, 1000];

    private Vector2[,] oTurn2 = new Vector2[1000, 1000];
    private Vector2[,] oThrottle2 = new Vector2[1000, 1000];
    private Vector2[,] oBrake2 = new Vector2[1000, 1000];
    private Vector2[,] counter2 = new Vector2[1000, 1000];
    private Vector2[,] oDirection2 = new Vector2[1000, 1000];
    private Vector2[,] oSpeed2 = new Vector2[1000, 1000];

    public int i;
    public int j;

    private int iRegister = 0;
    private int jRegister = 0;
    private float turnRegister = 0.0f;
    private float throttleRegister = 0.0f;
    private float brakeRegister = 0.0f;
    private int count = 0;
    private float directionRegister = 0.0f;
    private float speedRegister = 0.0f;
    private string temp;
    private float temp2;


    void Start()
    {

    }

    void Awake()
    {
        string[] sr1;
        string[] sr2;

        sr1 = System.IO.File.ReadAllLines(Application.dataPath + "/Scripts/Racing AI/" + filePathAl1);
        sr2 = System.IO.File.ReadAllLines(Application.dataPath + "/Scripts/Racing AI/" + filePathAl2);
        //         temp = sr1.ReadLine();
        //         temp = sr2.ReadLine();
        sw = new StreamWriter(Application.dataPath + "/Scripts/Racing AI/" + filePathTo2);
        sw.WriteLine("a");

        int f = 0;
        do 
        {
            f++;
            temp = sr1[f];
            f++;
            if (temp == null) break;
            i = int.Parse(temp);
            temp = sr1[f];
            f++;
            j = int.Parse(temp);

            temp = sr1[f];
            f++;
            temp2 = float.Parse(temp);
            oTurn1[i, j] = new Vector2(temp2, 0);

            temp = sr1[f];
            f++;
            temp2 = float.Parse(temp);
            oThrottle1[i, j] = new Vector2(temp2, 0);

            temp = sr1[f];
            f++;
            temp2 = float.Parse(temp);
            oBrake1[i, j] = new Vector2(temp2, 0);

            temp = sr1[f];
            f++;
            temp2 = float.Parse(temp);
            counter1[i, j] = new Vector2(temp2, 0);

            temp = sr1[f];
            f++;
            temp2 = float.Parse(temp);
            oDirection1[i, j] = new Vector2(temp2, 0);

            temp = sr1[f];
            f++;
            temp2 = float.Parse(temp);
            oSpeed1[i, j] = new Vector2(temp2, 0);

        } while (/*sr1[f] != null*/f < sr1.Length - 1);

        f = 0;
        do 
        {
            f++;
            temp = sr2[f];
            f++;
            if (temp == null) break;
            i = int.Parse(temp);
            temp = sr2[f];
            f++;
            j = int.Parse(temp);

            temp = sr2[f];
            f++;
            temp2 = float.Parse(temp);
            oTurn2[i, j] = new Vector2(temp2, 0);

            temp = sr2[f];
            f++;
            temp2 = float.Parse(temp);
            oThrottle2[i, j] = new Vector2(temp2, 0);

            temp = sr2[f];
            f++;
            temp2 = float.Parse(temp);
            oBrake2[i, j] = new Vector2(temp2, 0);

            temp = sr2[f];
            f++;
            temp2 = float.Parse(temp);
            counter2[i, j] = new Vector2(temp2, 0);

            temp = sr2[f];
            f++;
            temp2 = float.Parse(temp);
            oDirection2[i, j] = new Vector2(temp2, 0);

            temp = sr2[f];
            f++;
            temp2 = float.Parse(temp);
            oSpeed2[i, j] = new Vector2(temp2, 0);

        } while (/*sr2[f] != null*/f < sr2.Length - 1);

        for (i = 0; i < 1000; i++)
        {
            for (j = 0; j < 1000; j++)
            {
                if (oThrottle1[i, j].x == float.NaN && oThrottle2[i, j].x != float.NaN)
                    oThrottle[i, j] = new Vector2(oThrottle2[i, j].x, 0);

                if (oThrottle1[i, j].x != float.NaN && oThrottle2[i, j].x == float.NaN)
                    oThrottle[i, j] = new Vector2(oThrottle1[i, j].x, 0);

                if (oThrottle1[i, j].x != float.NaN && oThrottle2[i, j].x != float.NaN)
                    oThrottle[i, j] = new Vector2(oThrottle1[i, j].x + oThrottle2[i, j].x, 0);

                if (oTurn1[i, j].x == float.NaN && oThrottle2[i, j].x != float.NaN)
                    oTurn[i, j] = new Vector2(oTurn2[i, j].x, 0);
                if (oTurn1[i, j].x != float.NaN && oThrottle2[i, j].x == float.NaN)
                    oTurn[i, j] = new Vector2(oTurn1[i, j].x, 0);
                if (oTurn1[i, j].x != float.NaN && oThrottle2[i, j].x != float.NaN)
                    oTurn[i, j] = new Vector2(oTurn1[i, j].x + oTurn2[i, j].x, 0);

                if (oBrake1[i, j].x == float.NaN && oBrake2[i, j].x != float.NaN)
                    oBrake[i, j] = new Vector2(oBrake2[i, j].x, 0);
                if (oBrake1[i, j].x != float.NaN && oBrake2[i, j].x == float.NaN)
                    oBrake[i, j] = new Vector2(oBrake1[i, j].x, 0);
                if (oBrake1[i, j].x != float.NaN && oBrake2[i, j].x != float.NaN)
                    oBrake[i, j] = new Vector2(oBrake1[i, j].x + oBrake2[i, j].x, 0);

                if (counter1[i, j].x == float.NaN && counter2[i, j].x != float.NaN)
                    counter[i, j] = new Vector2(counter2[i, j].x, 0);
                if (counter1[i, j].x != float.NaN && counter2[i, j].x == float.NaN)
                    counter[i, j] = new Vector2(counter1[i, j].x, 0);
                if (counter1[i, j].x != float.NaN && counter2[i, j].x != float.NaN)
                    counter[i, j] = new Vector2(counter1[i, j].x + counter2[i, j].x, 0);

                if ((oDirection1[i, j].x == float.NaN || oDirection1[i, j].x == 0) && (oDirection2[i, j].x != float.NaN || oDirection2[i, j].x != 0))
                    oDirection[i, j] = new Vector2(oDirection2[i, j].x, 0);
                if ((oDirection1[i, j].x != float.NaN || oDirection1[i, j].x != 0) && (oDirection2[i, j].x == float.NaN || oDirection2[i, j].x == 0))
                    oDirection[i, j] = new Vector2(oDirection1[i, j].x, 0);
                if ((oDirection1[i, j].x != float.NaN && oDirection2[i, j].x != float.NaN) && (oDirection1[i, j].x != 0 && oDirection2[i, j].x != 0))
                {

                    diff = ((oDirection1[i, j].x - oDirection2[i, j].x + 180 + 360) % 360) - 180;
                    directionSon = (((360 + oDirection2[i, j].x + (diff / 2)) % 360));
                    oDirection[i, j] = new Vector2(directionSon, 0);
                }

                if (oSpeed1[i, j].x == float.NaN && oSpeed2[i, j].x != float.NaN)
                    oSpeed[i, j] = new Vector2(oSpeed2[i, j].x, 0);
                if (oSpeed1[i, j].x != float.NaN && oSpeed2[i, j].x == float.NaN)
                    oSpeed[i, j] = new Vector2(oSpeed1[i, j].x, 0);
                if (oSpeed1[i, j].x != float.NaN && oSpeed2[i, j].x != float.NaN)
                    oSpeed[i, j] = new Vector2(oSpeed1[i, j].x + oSpeed2[i, j].x, 0);

                if (oTurn[i, j].x != 0 || oThrottle[i, j].x != 0 || oBrake[i, j].x != 0 || oDirection[i, j].x != 0 || oSpeed[i, j].x != 0)
                {
                    sw.WriteLine(i);
                    sw.WriteLine(j);
                    sw.WriteLine(oTurn[i, j].x);
                    sw.WriteLine(oThrottle[i, j].x);
                    sw.WriteLine(oBrake[i, j].x);
                    sw.WriteLine(counter[i, j].x);
                    sw.WriteLine(oDirection[i, j].x);
                    sw.WriteLine(oSpeed[i, j].x);
                    sw.WriteLine("----");
                }
            }
        }
        sw.Flush();
        sw.Close();
//         sr1.Close();
//         sr2.Close();
        oTurn = null;
        oThrottle = null;
        oBrake = null;
        counter = null;
        oDirection = null;
        oSpeed = null;

        oTurn1 = null;
        oThrottle1 = null;
        oBrake1 = null;
        counter1 = null;
        oDirection1 = null;
        oSpeed1 = null;

        oTurn2 = null;
        oThrottle2 = null;
        oBrake2 = null;
        counter2 = null;
        oDirection2 = null;
        oSpeed2 = null;
        Destroy(gameObject);
    }

}

