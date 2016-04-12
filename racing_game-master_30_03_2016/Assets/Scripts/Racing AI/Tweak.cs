using UnityEngine;
using System.Collections;

public class Tweak : MonoBehaviour {

    public int NumberOfCars = 8;
    public Transform CarPrefab;
    public Transform[] Cars;
    public RaceAI1[] AIScript;

    private float[,] Genome;
    public static int[] TriggerPoints;

    public float TimeKeeper;
    public Vector3[] InitialPosition;
    public Quaternion[] InitialOrient;

    public int Cycle = 0;
    public int Father;
    public bool FoundFather;
    public bool[] RandomPosition;
    public int PlaceHolder;

    public int count;
    public int swap;
    public float tempSort;
    public int tempFlo;
    public int[] FastestCar;

    void Start()
    {
        Cars = new Transform[NumberOfCars];
        AIScript = new RaceAI1[NumberOfCars];
        Genome = new float[NumberOfCars, 16];
        TriggerPoints = new int[NumberOfCars];
        InitialPosition = new Vector3[NumberOfCars];
        InitialOrient = new Quaternion[NumberOfCars];
        RandomPosition = new bool[NumberOfCars];
        FastestCar = new int[NumberOfCars];
        for (var i = 0; i < NumberOfCars; i++)
        {
            InitialPosition[i] = transform.position + (transform.forward * 8 * i);
            InitialOrient[i] = transform.rotation;
            Cars[i] = Instantiate(CarPrefab, InitialPosition[i] + ((i % 2) * transform.right * 4), transform.rotation) as Transform;

            AIScript[i] = Cars[i].GetComponent<RaceAI1>();
            AIScript[i].Mark = i;
            Genome[i, 0] = AIScript[i].tweakTurnMaxA;
            Genome[i, 1] = AIScript[i].tweakTurnMaxB;
            Genome[i, 2] = AIScript[i].tweakTurnA;
            Genome[i, 3] = AIScript[i].tweakThrottleMaxA;
            Genome[i, 4] = AIScript[i].tweakThrottleMaxB;
            Genome[i, 5] = AIScript[i].tweakThrottleMaxC;
            Genome[i, 6] = AIScript[i].tweakAccelerationMaxA;
            Genome[i, 7] = AIScript[i].tweakAccelerationMaxB;
            Genome[i, 8] = AIScript[i].tweakThrottleA;
            Genome[i, 9] = AIScript[i].tweakThrottleB;
            Genome[i, 10] = AIScript[i].tweakThrottleC;
            Genome[i, 11] = AIScript[i].tweakThrottleD;
            Genome[i, 12] = AIScript[i].tweakThrottleE;
            Genome[i, 13] = AIScript[i].tweakBreakA;
            Genome[i, 14] = AIScript[i].tweakBreakB;
            Genome[i, 15] = AIScript[i].tweakAvoidanceSteer;
        }

        for (int i = 0; i < NumberOfCars; i++)
        {
            for (var j = 0; j < 16; j++)
            {
                Genome[i, j] *= (Random.value * 2);
            }
        }

        SetGenome();

    }

    void Update()
    {
        TimeKeeper += Time.deltaTime;
        if (TimeKeeper > 20 + (Cycle * 5))
        {
            Cycle++;
            bubbleSort(TriggerPoints);
            TimeKeeper = 0;
            for (var i = 0; i < NumberOfCars; i++)
            {
                //print(TriggerPoints[i]+" "+i);
                while (RandomPosition[PlaceHolder] == true) PlaceHolder = Random.Range(0, NumberOfCars);
                RandomPosition[PlaceHolder] = true;
                Cars[i].position = InitialPosition[PlaceHolder] + ((PlaceHolder % 2) * transform.right * 4);
                Cars[i].GetComponent< Rigidbody > ().velocity = Vector3.zero;
                Cars[i].rotation = InitialOrient[i];
            }
            for (var j = 0; j < (NumberOfCars / 2); j++)
            {
                for (var k = 0; k < 16; k++)
                {
                    FoundFather = false;
                    for (var f = 0; f < (NumberOfCars / 2); f++)
                    {
                        if (Random.value < TriggerPoints[FastestCar[7 - f]] * 0.05) // finds the fastest cars then, then does random selection, but it is more likely that the fastest car will be selected as there is more of them 
                        {
                            Father = f;
                            FoundFather = true;
                            break;
                        }
                    }
                    if (!FoundFather) Father = Random.Range((NumberOfCars / 2), NumberOfCars); ;
                    Genome[FastestCar[j], k] = Genome[FastestCar[Father], k];
                }
            }

            SetGenome();

            for (int i = 0; i < NumberOfCars; i++)
            {
                RandomPosition[i] = false;
            }
        }

    }


    void SetGenome()
    {
        for (var i = 0; i < NumberOfCars; i++)
        {
            TriggerPoints[i] = 0;
            for (var j = 0; j < 16; j++)
            {
                if (Random.value < 0.01)
                    Genome[i, j] *= (Random.value * (1f + (1f / (Cycle + 1f)))) + (0.5f * (2f - (1f + (1f / (Cycle + 1f)))));//small mut revise
                if (Random.value < 0.001)
                    Genome[i, j] *= (Random.value * 10);//big mut

            }
            AIScript[i].tweakTurnMaxA = Genome[i, 0];
            AIScript[i].tweakTurnMaxB = Genome[i, 1];
            AIScript[i].tweakTurnA = Genome[i, 2];
            AIScript[i].tweakThrottleMaxA = Genome[i, 3];
            AIScript[i].tweakThrottleMaxB = Genome[i, 4];
            AIScript[i].tweakThrottleMaxC = Genome[i, 5];
            AIScript[i].tweakAccelerationMaxA = Genome[i, 6];
            AIScript[i].tweakAccelerationMaxB = Genome[i, 7];
            AIScript[i].tweakThrottleA = Genome[i, 8];
            AIScript[i].tweakThrottleB = Genome[i, 9];
            AIScript[i].tweakThrottleC = Genome[i, 10];
            AIScript[i].tweakThrottleD = Genome[i, 11];
            AIScript[i].tweakThrottleE = Genome[i, 12];
            AIScript[i].tweakBreakA = Genome[i, 13];
            AIScript[i].tweakBreakB = Genome[i, 14];
            AIScript[i].tweakAvoidanceSteer = Genome[i, 15];
        }

    }

    void bubbleSort(int[] sortThis)

    {
        for (count = 0; count < NumberOfCars; count++)
        {
            FastestCar[count] = count;
        }
        do
        {
            swap = 0;
            for (count = 0; count < NumberOfCars - 1; count++)
            {
                if (sortThis[count] > sortThis[count + 1])
                {
                    tempSort = sortThis[count];
                    sortThis[count] = sortThis[count + 1];
                    sortThis[count + 1] = (int) tempSort;
                    tempFlo = FastestCar[count];
                    FastestCar[count] = FastestCar[count + 1];
                    FastestCar[count + 1] = tempFlo;
                    swap = 1;
                }
            }
        } while (swap != 0);
        return;
    }

}
