﻿using UnityEngine;
using System.Collections.Generic;

public class DefensePlayer : AttackPlayer 
{
    public GameObject homeGoal;
    private float curDistanceToHomeGoal;
    private float curDistanceToOponentAttacker;
    private float bestDistanceToHomeGoal = float.MaxValue;
    private float bestDistToOponentAttacker = float.MaxValue;
    private AttackPlayer oponentAttacker;
    private AttackPlayer attackerPlayer;
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

        if(!HaveBall)
        {
            /* DISTANCE TO BALL */
            if (curDistanceToBall < bestDistanceToBall)
            {
                bestDistanceToBall = curDistanceToBall;
                fitness++;
            }

            /* DISTANCE TO OPPONENT */
            if (curDistanceToOponentAttacker < bestDistToOponentAttacker)
            {
                bestDistToOponentAttacker = curDistanceToOponentAttacker;
                fitness++;
            }

            /*DISTANCE TO HOME GOAL*/
            if (curDistanceToHomeGoal < bestDistanceToHomeGoal)
            {
                bestDistanceToHomeGoal = curDistanceToHomeGoal;
                fitness++;
            }
        }
        else
        {
            /* WHEN WE HAVE BALL WE DONT WANT TO RUN TO ENEMIES */
            if (curDistanceToOponentAttacker > bestDistToOponentAttacker)
            {
                bestDistToOponentAttacker = curDistanceToOponentAttacker;
                fitness++;
            }
        }

        if(HaveBall && passBall)
        {
            ballScript.Pass(this, attackerPlayer);
        }
	}

    public override void InitPlayer()
    {
        if(!isInited)
        {
            id++;
            attackerPlayer = transform.parent.gameObject.GetComponentInChildren<AttackPlayer>();
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
        //this will store all the inputs for the NN
        List<double> inputs = new List<double>();

        //add ball locations
        inputs.Add(ballScript.transform.position.x - transform.position.x);
        inputs.Add(ballScript.transform.position.y - transform.position.y);

        //add oponent Attacker
        inputs.Add(oponentAttacker.transform.position.x - transform.position.x);
        inputs.Add(oponentAttacker.transform.position.y - transform.position.y);

        //add distance to home goal
        inputs.Add(homeGoal.transform.position.x - transform.position.x);
        inputs.Add(homeGoal.transform.position.y - transform.position.y);

        //add does have ball
        inputs.Add(HaveBall ? 0 : 1);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x + (float)output[0] * 2* Time.deltaTime,
            transform.position.y + (float)output[1] * 2 * Time.deltaTime);
        if(output[2] > 0)
        {
            passBall = true;
        }


        ClipPlayerToField();
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
