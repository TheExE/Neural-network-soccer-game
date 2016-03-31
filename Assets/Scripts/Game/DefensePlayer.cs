using UnityEngine;
using System.Collections.Generic;

public class DefensePlayer : AttackPlayer 
{
    public GameObject homeGoal;
    public AttackPlayer attackerPlayer;
    private float curDistanceToHomeGoal;
    private float curDistanceToOponentAttacker;
    private float bestDistanceToHomeGoal = float.MaxValue;
    private float bestDistToOponentAttacker = float.MaxValue;
    private AttackPlayer oponentAttacker;
    private bool passBall = false;

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
        curDistanceToOponentAttacker = (oponentAttacker.transform.position - transform.position).sqrMagnitude;
        curDistanceToHomeGoal = (homeGoal.transform.position - transform.position).sqrMagnitude;
        curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
	}

	void Update ()
    {
        curDistanceToOponentAttacker = (oponentAttacker.transform.position - transform.position).sqrMagnitude;
        curDistanceToHomeGoal = (homeGoal.transform.position - transform.position).sqrMagnitude;
        curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;


		/* DISTANCE TO BALL */
		if (curDistanceToBall < bestDistanceToBall)
		{
			bestDistanceToBall = curDistanceToBall;
			fitness += 0.2f;
		}

		/*DISTANCE TO HOME GOAL*/
		if (curDistanceToHomeGoal < bestDistanceToHomeGoal)
		{
			bestDistanceToHomeGoal = curDistanceToHomeGoal;
			fitness += 0.8f;
		}
	}

    public override void InitPlayer()
    {
        if(!isInited)
        {
            id++;
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
        Vector2 toAttacker = (attackerPlayer.transform.position - ballScript.transform.position).normalized;
        inputs.Add(toAttacker.x);
        inputs.Add(toAttacker.y);

        /* Update the brain and get feedback */
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x + (float)output[0] * Time.deltaTime,
            transform.position.y + (float)output[1] * Time.deltaTime);

        directionOfHitBall = new Vector2((float)output[2], (float)output[3]);

        ClipPlayerToField();
		GivePenaltieToCampers();
    }
    new public void Reset()
    {
        base.Reset();
        curDistanceToHomeGoal = 0;
        curDistanceToOponentAttacker = 0;
        bestDistanceToHomeGoal = float.MaxValue;
        bestDistToOponentAttacker = float.MaxValue;
    }
}
