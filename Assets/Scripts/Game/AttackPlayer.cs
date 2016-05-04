using UnityEngine;
using System.Collections.Generic;
using System;

public class AttackPlayer : MonoBehaviour
{
    public Goal oponentGoal;
    public GameObject oponentTeam;

    protected string nameType = GameConsts.ATTACK_PLAYER;
    protected NeuralNetwork brain;
    protected BallScript ballScript;
    protected static int id = 0;
    protected double fitness;
    protected DefensePlayer[] oponentDefense;

    protected bool colided = false;
    protected float curDistanceToBall;
	protected float bestDistanceToBall = float.MaxValue;
    protected float lastPositionX;
    protected float curBallHitDirectionError = 0;
    protected float bestBallHitDirectionError = float.MaxValue;
	protected bool isInited = false;
    protected Vector2 directionOfHitBall = Vector2.zero;
    protected float ballHitStrenght = 1;
    protected float scale = 1;
    protected float curTime = 0;
    protected int ballHitTimes = 0;
    protected bool isColided = false;

    private float bestDistanceToOponentGoal = float.MaxValue;
    private float curDistanceToOponentGoal;
    private Vector2 lastPosition = Vector2.zero;
	
    void Start()
    {
        if(!isInited)
        {
            InitPlayer();
        }
        lastPositionX = transform.position.x;
    }

    void Update()
    {
        curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
        curTime += Time.deltaTime;
        if(curTime > 2 && !isColided)
        {
            curTime = 0;
            curDistanceToOponentGoal = (oponentGoal.transform.position - transform.position).sqrMagnitude;

            /* Balls distance to oponents goal */
            if (curDistanceToOponentGoal < bestDistanceToOponentGoal)
            {
                bestDistanceToOponentGoal = curDistanceToOponentGoal;
                fitness ++;
            }

            /*if (curDistanceToBall < bestDistanceToBall)
            {
                bestDistanceToBall = curDistanceToBall;
                fitness ++;
            }

            /* REWARD FOR LESSER ERROR IN DIRECTION */
            /*if (curBallHitDirectionError < bestBallHitDirectionError)
            {
                bestBallHitDirectionError = curBallHitDirectionError;
                fitness ++;
            }*/

            if (isColided)
            {
                if (fitness > 0)
                {
                    Fitness--;
                }
            }
        }

        isColided = false;
        HandlePlayerRotation();
    }

    public virtual void InitPlayer()
    {
        if(!isInited)
        {
            id++;
            oponentDefense = oponentTeam.GetComponentsInChildren<DefensePlayer>();
            ballScript = FindObjectOfType<BallScript>();

            brain = new NeuralNetwork(NeuralNetworkConst.ATTACKER_INPUT_COUNT, NeuralNetworkConst.ATTACKER_OUTPUT_COUNT,
                NeuralNetworkConst.ATTACKER_HID_LAYER_COUNT, NeuralNetworkConst.ATTACKER_NEURONS_PER_HID_LAY);
            isInited = true;
        }
    }
    public virtual void UpdatePlayerBrains()
    {
        List<double> inputs = new List<double>();

        /* Add ball locations */

        /* Oponents goal */
        Vector2 toOponentGoal = (oponentGoal.transform.position - transform.position).normalized;
        /* inputs.Add(toOponentGoal.x);
         inputs.Add(toOponentGoal.y);*/
        inputs.Add(toOponentGoal.x);
        inputs.Add(toOponentGoal.y);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x + GetScaledOutput(output[0])*Time.deltaTime,
            transform.position.y + GetScaledOutput(output[1])* Time.deltaTime);

        /*directionOfHitBall = new Vector2((float)output[2], (float)output[3]);
        ballHitStrenght = (float)output[4];

        
        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (toOponentGoal - directionOfHitBall).sqrMagnitude;
        ballHitStrenght = 2f;
        directionOfHitBall = toOponentGoal;

        CheckCollision();
    }

    protected void CheckCollision()
    {
        float ditanceBetween = (ballScript.transform.position - transform.position).magnitude;


        if (ditanceBetween < 0.5f)
        {
            if (!gameObject.name.Contains("Dummy"))
            {
                ballScript.Shoot(directionOfHitBall, ballHitStrenght);
            }
            fitness++;
        }

        ClipPlayerToField();
    }

    protected void HandlePlayerRotation()
    {
        /* Switching player orientation */
        if (transform.position.x > lastPositionX + 0.1)
        {
            transform.rotation = Quaternion.Euler(0, 0, -180);
        }
        else if (transform.position.x < lastPositionX - 0.1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }


        lastPositionX = transform.position.x;
    }

    public double Fitness
    {
        get { return fitness; }
        set { fitness = value; }

    }
    public void PutWeights(List<double> weights)
    {
        brain.PutWeights(weights);
    }

    public int NumberOfWeights
    {
        get { return brain.GetNumberOfWeights(); }
    }

    public void Reset()
    {
        fitness = 0;
    }

    protected void ClipPlayerToField()
    {
        colided = false;
        if (transform.position.x > GameConsts.GAME_FIELD_RIGHT)
        {
            transform.position = new Vector2(GameConsts.GAME_FIELD_RIGHT, transform.position.y);
            colided = true;
			fitness -= 0.1f;
        }
        else if (transform.position.x < GameConsts.GAME_FIELD_LEFT)
        {
            transform.position = new Vector2(GameConsts.GAME_FIELD_LEFT, transform.position.y);
            colided = true;
			fitness -= 0.1f;
        }

        if (transform.position.y > GameConsts.GAME_FIELD_UP)
        {
            transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_UP);
            colided = true;
			fitness -= 0.1f;
        }
        else if (transform.position.y < GameConsts.GAME_FIELD_DOWN)
        {
            transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_DOWN);
            colided = true;
			fitness -= 0.1f;
        }
		
		if(fitness < 0)
		{
			fitness = 0;
		}
    }

    public int PlayerID
    {
        get { return id; }
    }

    private Vector3 GetClosestOponentPosition()
    {
        Vector3 closestOp = Vector3.zero;
        float bestDistance = float.MaxValue;
        foreach(DefensePlayer oponent in oponentDefense)
        {
            float curDistance = (oponent.transform.position - transform.position).sqrMagnitude;
            if(curDistance < bestDistance)
            {
                bestDistance = curDistance;
                closestOp = new Vector2(oponent.transform.position.x, oponent.transform.position.y);
            }
        }

        return closestOp;
    }

    public string NameType
    {
        get { return nameType; }
    }

    public List<int> SplitPoints
    {
        get { return brain.CauculateSplitPoints(); }
    }

    private int Vec2DSign(Vector2 v1, Vector2 v2)
    {
        if (v1.y * v2.x > v1.x * v2.y)
        {
            return 1;
        }
        else
        {
            return -1;
        }
    }

    public float CurDistanceToBall
    {
        get{ return curBallHitDirectionError; }
    }

    protected float GetScaledOutput(double output)
    {
        if (output <= 0.5)
        {
            output *= -1;
        }
        else
        {
            output -= 0.5f;
        }

        output *= 2;

        return (float)output;
    }

}
