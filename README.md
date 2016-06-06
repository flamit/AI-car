# AI-car
Genetic algorithm

Introduction

The AI cars in the game should have real life characteristics, they should essentially mimic another player. A common way to do the AI car is to use a waypoint method, by which the cars would follow lines along the track, and the AI cars would occasionally over take each other, the movements are the same from lap to lap and thus predictability of the movements would be picked up by the player and this would be of detriment to the gameplay. Using a method based on genetic algorithm it is possible to create an organic movement of the car where movements would be different from lap to lap. This type of behaviour would give the impression that the car is being controlled by another player and thus increase gameplay fun. 


A generic genetic algorithm

These genetic algorithms are based on Darwin's evolution theory, where the solution to a problem evolves after each generation. The Algorithm starts with a set of parameters which define the current population  (in our case table 1). Based on survival rules the parameters (or genes) will form the new population, which should have better rates of survival. This is repeated until the values converge to a final solution.



[Start] Generate random population of n chromosomes (suitable solutions for the problem)
1. [Fitness] Evaluate the fitness f(x) of each chromosome x in the population
2. [New population] Create a new population by repeating following steps until the new population is complete
1. [Selection] Select two parent chromosomes from a population according to their fitness (the better fitness, the bigger chance to be selected)
2. [Crossover] With a crossover probability cross over the parents to form a new offspring (children). If no crossover was performed, offspring is an exact copy of parents.
3. [Mutation] With a mutation probability mutate new offspring at each locus (position in chromosome).
4. [Accepting] Place new offspring in a new population
3. [Replace] Use new generated population for a further run of algorithm
4. [Test] If the end condition is satisfied, stop, and return the best solution in current population
5. [Loop] Go to step 2






High level description of AI 



Creating example data for car

The first thing is to record data from the players car, we create tiles that record the data of the following parameters and save into a text file. We take multiple recordings and combine with each previous recording.  


    private Vector2[,] oTurn = new Vector2[1000, 1000];         // array of steering value 
    private Vector2[,] oThrottle = new Vector2[1000, 1000];     // array of Throttle value
    private Vector2[,] oBrake = new Vector2[1000, 1000];        // array of Brake value
    private Vector2[,] counter = new Vector2[1000, 1000];       // array of counter value
    private Vector2[,] oDirection = new Vector2[1000, 1000];    // array of direction value
    private Vector2[,] oSpeed = new Vector2[1000, 1000];        // array of speed value


Genetic Algorithm for physics Car

 For the AI cars the purpose was to create the car and have the AI move the internal car physics mechanisms. This meant move the steering, Throttle, Brake direction and speed. The aim was to make the race AI cars move realistically and have random subtle organic behaviours.  I felt that a genetic algorithm is a better method than other methods such as waypoint, the movements of the cars can look artificial, as they follow a path, they are also more likely to follow an order in which the cars go through each lap. As genetic algorithms have at the heart randomness to perpetuate the internal mechanisms, movements of each car will be different lap by lap, this will poise problems for the players that are  driving up against the AI cars and improve gameplay. The idea with respect to the genetic algorithms if we break down each part of the laps, and have the AI cars race, the cars that are most successful in each part of the race will go on to spawn the next generation and also carry the tweak parameters that will alter the car physics for the next generation.  I used bubblesort to organise the cars in terms position on the track,  cars that are towards the front are randomly select a car that would spawn the next generation of cars, we also take the tweak parameters and create slight random permutations. The purpose of the randomness is to increase diversity and inhibit premature convergence. 

 {
                if (Random.value < 0.01)
                    Genome[i, j] *= (Random.value * (1f + (1f / (Cycle + 1f)))) + (0.5f * (2f - (1f + (1f / (Cycle + 1f)))));//small mutation 
                if (Random.value < 0.001)
                    Genome[i, j] *= (Random.value * 10);//large mutation

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


From the recorded data we make the adjustments of the tweaks parameters by making comparisons between them. The AI cars are there to provide real competition for the player, so the parameters are going to be different from the usual players car. For example there is limit that the AI car can no longer decrease in speed, and has a slow down parameter if the car gets ahead of the user. 


if (Mathf.Abs(Vector3.Angle(targetDir, Usercar.transform.forward)) < 60)
        { //check this again
            brake += Vector3.Distance(Usercar.transform.position, transform.position) * (tweakWaitforUser);
        }

 The purpose of the tweaking parameters are to optimise the parameters that the cars are controlled by,  such asTurningSpeed( current rpm of car engine), TurnMax maximum rpm of car engine,
  ThrottleMax maximum throttle of car engine. 

For example if we find that for an epoch that having a very high TurnMax is detrimental to the movement of the car because it may cause more collision, then for those particular cars are unlikely to make the next generation of cars. In this case cars with large turnMax would also have large tweakTurnMaxB, as cars with a tweakTurnMaxB will have a large rpm. This means that the next generation cars will have a lower tweakTurnMaxB in order to lower the maximum RPM.     


 TurnMax = tweakTurnMaxA - (TurningSpeed / tweakTurnMaxB);
  ThrottleMax = (speed * tweakThrottleMaxA * rthrottle) + tweakThrottleMaxB - (Mathf.Abs(DeltaDirection) * tweakThrottleMaxC);    

Here the optimisation rule is heavily influenced by the speed, to put quite simply the greater the speed of the car the greater the maximum Throttle allowed for that car. The car that attains the greatest max throttle is more likely to be the front runner, however to distinguish between these fastest cars, the cars that take the a path that is similar to the example car that was driven initially will be rewarded extra throttle. And so this means after each evolution you should find the car at the front has likely reached the highest speeds but also has the taken the path that is nearest to the path of the recorded car. The DeltaDirection is value between current direction and recorded value.    

Fig1:Table showing the different tweaking parameters of the  
public float tweakTurnMaxA = 50;  
maximum steering angle at standstill
public float tweakTurnMaxB = 11.11f; 
decreases the decrease in max steering angle with speed
public float tweakTurnA = 0.8f; 
Sharpens steering
public float tweakThrottleMaxA = 25f;
increases maximum throttle with speed
public float tweakThrottleMaxB = 1050f; 
increases maximum throttle independent of speed
public float tweakThrottleMaxC = 3.0f;
decreases maximum throttle with deviation from recorded direction
public float tweakAccelerationMaxA = 0.3f;
increases maximum allowed acceleration in straight
public float tweakAccelerationMaxB = 1000f;
increases maximum allowed lateral acceleration
public float tweakThrottleA = 1.0f;
 Adjusts throttle with the difference in recorded speed and actual speed
public float tweakThrottleB = 400f;
restricts the throttle to this value if the car is very slow
public float tweakThrottleC = 2000f;
throttle value at the first two seconds
public float tweakThrottleD = 3f; 
Rate of slowdown when a car is ahead
 public float tweakThrottleE = 1.0f;
Sharpens throttle response
public float tweakBreakA = 1.0f;
increases the break force proportional to the difference in recorded speed and actual speed
public float tweakBreakB = 1.4f;
recorded brake force times this is the brake force applied
public float tweakAvoidanceSteer = 7f;
Increases car avoidance steering magnitude
 public float tweakWaitforUser = 0.1f;
Increases breaking if the car is ahead of the user car
P  

Link below shows the cars learning to drive around the track, here the cars are seeking optimised parameters, for each of the tweak parameters in Figure1. The movements of the car are controlled by the raycasting as can be seen in the video.
https://www.youtube.com/watch?v=miYO-7KOjf4

Driving against the cars, around 2.30 minutes it is showing AI cars colliding with each other unexpectedly, the behaviours are different from lap to lap, you can't always be sure when you will be overtaken by another car or when   

https://www.youtube.com/watch?v=iY4-SQvuGx4

Playing it myself the behaviours are different from lap to lap.


