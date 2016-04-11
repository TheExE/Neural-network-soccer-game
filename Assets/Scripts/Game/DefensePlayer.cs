using UnityEngine;
using System.Collections.Generic;

public class DefensePlayer : AttackPlayer 
{
    public GameObject homeGoal;
    public AttackPlayer attackerPlayer;

    private float curDistanceToHomeGoal;
    private float bestDistanceToHomeGoal = float.MaxValue;
    private AttackPlayer oponentAttacker;
    private GoallyPlayer teamGoally;
    private float curDistanceToAttacker = 0;
    private float bestDistanceToAttacker = 0;
    private float curDistToOponentAttacker = float.MaxValue;
    private float bestDistanceToOponentAttacker = float.MaxValue;
    private float curDistToGoaly = 0f;
    private float bestDistToGoly = 0f;

    public DefensePlayer()
    {
        nameType = GameConsts.DEFENSE_PLAYER;
    }

	void Start () 
    {
        if(!isInited)
        {
            InitPlayer();
        }
        lastPositionX = transform.position.x;
    }

	void Update ()
    {
        curDistanceToHomeGoal = (homeGoal.transform.position - transform.position).sqrMagnitude;
        curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
        curDistToOponentAttacker = (oponentAttacker.transform.position - transform.position).sqrMagnitude;
        curDistToGoaly = (teamGoally.transform.position - transform.position).sqrMagnitude;
        curDistanceToAttacker = (attackerPlayer.transform.position - transform.position).sqrMagnitude;


        /* DISTANCE TO GOALY PLAYER */
        if (curDistToGoaly > bestDistToGoly && curDistToGoaly < 2f)
        {
            bestDistToGoly = curDistToGoaly;
            fitness += 0.75f;            
        }


		/* DISTANCE TO BALL */
		if (curDistanceToBall < bestDistanceToBall)
		{
			bestDistanceToBall = curDistanceToBall;
			fitness += 0.45f;
		}

		/* DISTANCE TO HOME GOAL */
		if (curDistanceToHomeGoal < bestDistanceToHomeGoal)
		{
			bestDistanceToHomeGoal = curDistanceToHomeGoal;
			fitness += 0.6f;
		}


        /* REWARD FOR LESSER ERROR IN DIRECTION */
        if (curBallHitDirectionError < bestBallHitDirectionError)
        {
            bestBallHitDirectionError = curBallHitDirectionError;
            fitness += 0.9f;
        }

        /* REWARD FOR GOING CLOSER TO OPPONENT ATTACKR */
        if(curDistToOponentAttacker < bestDistanceToOponentAttacker)
        {
            bestDistanceToOponentAttacker = curDistToOponentAttacker;
            fitness += 0.8f;
        }

        /* REWARD FOR NOT BEING IN THE WAY OF ATTACKER */
        if(curDistanceToAttacker > bestDistanceToAttacker && curDistanceToAttacker < 4f)
        {
            bestDistanceToAttacker = curDistanceToAttacker;
            fitness += 0.7f;
        }

        HandlePlayerRotation();
    }

    public override void InitPlayer()
    {
        if(!isInited)
        {
            id++;
            teamGoally = transform.parent.GetComponentInChildren<GoallyPlayer>();
            oponentAttacker = oponentTeam.GetComponentInChildren<AttackPlayer>();
            rgBody = GetComponent<Rigidbody2D>();
            ballScript = FindObjectOfType<BallScript>();
            brain = new NeuralNetwork(NeuralNetworkConst.DEFENSE_INPUT_COUNT, NeuralNetworkConst.DEFENSE_OUTPUT_COUNT,
                NeuralNetworkConst.DEFENSE_HID_LAYER_COUNT, NeuralNetworkConst.DEFENSE_NEURONS_PER_HID_LAY);
            isInited = true;
        }
    }
    public override void UpdatePlayerBrains()
    {
        List<double> inputs = new List<double>();

        /* Add move to ball */
        Vector2 toBall = (ballScript.transform.position - transform.position).normalized;
        inputs.Add(toBall.x);
        inputs.Add(toBall.y);

        /* Add move to home goal */
        Vector2 toHomeGoal = (homeGoal.transform.position - transform.position).normalized;
        inputs.Add(toHomeGoal.x);
        inputs.Add(toHomeGoal.y);

        /* Add ball hit direction */
        Vector2 ballToGoal = (oponentGoal.transform.position - ballScript.transform.position).normalized;
        inputs.Add(ballToGoal.x);
        inputs.Add(ballToGoal.y);

        /* Update the brain and get feedback */
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x + (float)output[0] * Time.deltaTime,
            transform.position.y + (float)output[1] * Time.deltaTime);

        directionOfHitBall = new Vector2((float)output[2], (float)output[3]);
       
        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (ballToGoal - directionOfHitBall).sqrMagnitude;
        ballHitStrenght = (float)output[4];

        ClipPlayerToField();
		GivePenaltieToCampers();
    }

    new void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            ballScript.Shoot(directionOfHitBall, ballHitStrenght);
            fitness += 0.8f;
        }
    }

    new public void Reset()
    {
        base.Reset();
        curDistanceToHomeGoal = 0;
        bestDistanceToHomeGoal = float.MaxValue;
        curDistToGoaly = 0;
        bestDistToGoly = 0;
        curDistanceToAttacker = 0;
        bestDistanceToAttacker = 0;
    }
}
