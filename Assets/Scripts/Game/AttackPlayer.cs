using UnityEngine;
using System.Collections.Generic;
using System;

public class AttackPlayer : MonoBehaviour
{
    public Goal oponentGoal;
    public GameObject oponentTeam;

    protected string nameType = GameConsts.ATTACK_PLAYER;
    protected Rigidbody2D rgBody;
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


 
    private float bestDistanceToOponentGoal = float.MaxValue;
    private float curDistanceToOponentGoal;
	private float campTimer = 0;
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
        curDistanceToOponentGoal = (oponentGoal.transform.position - transform.position).sqrMagnitude;

        /* Balls distance to oponents goal */
        if (curDistanceToOponentGoal < bestDistanceToOponentGoal)
        {
            bestDistanceToOponentGoal = curDistanceToOponentGoal;
            fitness += 0.4;
        }

        if (curDistanceToBall < bestDistanceToBall)
        {
            bestDistanceToBall = curDistanceToBall;
            fitness += 0.9f;
        }

        /* REWARD FOR LESSER ERROR IN DIRECTION */
        if(curBallHitDirectionError < bestBallHitDirectionError)
        {
            bestBallHitDirectionError = curBallHitDirectionError;
            fitness += 0.8f;
        }
        
        HandlePlayerRotation();
    }

    public virtual void InitPlayer()
    {
        if(!isInited)
        {
            id++;
            oponentDefense = oponentTeam.GetComponentsInChildren<DefensePlayer>();
            rgBody = GetComponent<Rigidbody2D>();
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
        Vector2 toBall = (ballScript.transform.position - transform.position).normalized;
        inputs.Add(toBall.x);
        inputs.Add(toBall.y);

        /* Oponents goal */
        Vector2 toOponentGoal = (oponentGoal.transform.position - transform.position).normalized;
        inputs.Add(toOponentGoal.x);
        inputs.Add(toOponentGoal.y);

        /* Ball hit direction */
        Vector2 ballToGoal = (oponentGoal.transform.parent.transform.position
            - ballScript.transform.position).normalized;
        inputs.Add(ballToGoal.x);
        inputs.Add(ballToGoal.y);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x + (float)output[1] * Time.deltaTime,
            transform.position.y + (float)output[0] * Time.deltaTime);

        directionOfHitBall = new Vector2((float)output[2], (float)output[3]);
        ballHitStrenght = (float)output[4];

        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (ballToGoal - directionOfHitBall).sqrMagnitude;

        ClipPlayerToField();
		GivePenaltieToCampers();
    }

	protected void GivePenaltieToCampers()
	{
		campTimer += Time.deltaTime;
		bool isPositionChanged = true;
		if(transform.position.x <= lastPosition.x + 0.1f && transform.position.x >= lastPosition.x - 0.1f &&
			transform.position.y <= lastPosition.y + 0.1f &&
			transform.position.y >= lastPosition.y - 0.1f)
		{
			isPositionChanged = false;
		}
		
		if(campTimer > 3 && !isPositionChanged)
		{
			campTimer = 0;
			fitness -= 0.1f;
			if(fitness < 0)
			{
				fitness = 0;
			}
		}
		else if(isPositionChanged)
		{
			campTimer = 0;
		}
		
		lastPosition = new Vector2(transform.position.x, transform.position.y);
	}
    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            ballScript.Shoot(directionOfHitBall, ballHitStrenght);
            fitness += 0.8f;
        }
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
        bestDistanceToBall = float.MaxValue;
        curDistanceToBall = float.MaxValue;
        bestBallHitDirectionError = float.MaxValue;
        curBallHitDirectionError = float.MaxValue;
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

    public Rigidbody2D PhysicsBody
    {
        get { return rgBody; }
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

}
