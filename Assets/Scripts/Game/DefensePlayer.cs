﻿using UnityEngine;
using System.Collections.Generic;

public class DefensePlayer : AttackPlayer
{
    public GameObject homeGoal;
    public AttackPlayer attackerPlayer;

    private float curDistanceToHomeGoal;
    private float bestDistanceToHomeGoal = float.MaxValue;
    private AttackPlayer oponentAttacker;
    private GoallyPlayer teamGoally;
    private float curDistToOponentAttacker = float.MaxValue;
    private float bestDistanceToOponentAttacker = float.MaxValue;
    private bool isRedTeam = false;
    private bool isCloseEnoughToAttacker = false;


    public DefensePlayer()
    {
        nameType = GameConsts.DEFENSE_PLAYER;
    }
    void Start()
    {
        if (!isInited)
        {
            InitPlayer();
        }
        lastPositionX = transform.position.x;
    }
    void Update()
    {
        curDistToOponentAttacker = (oponentAttacker.transform.position - transform.position).sqrMagnitude;
  
        if (!isColided)
        {
            /* REWARD FOR HIT DIRECTION */
            if (!IsBallGoingToBeOutBoundAfterKick())
            {
                if ((curBallHitError < bestBallHitError &&
                    curDistToOponentAttacker < bestDistanceToOponentAttacker ) ||
                    (curBallHitError < 0.1f &&
                    curDistToOponentAttacker < 0.3f))
                {
                    bestBallHitError = curBallHitError;
                    fitness++;
                }
            }
            else
            {
                fitness--;
            }

            /* REWARD FOR BALL HIT STRENGHT */
            if (ballHitStrenght > bestBallHitStrenght && ballHitStrenght < 1)
            {
                bestBallHitStrenght = ballHitStrenght;
                fitness++;
            }
        }
        else
        {
            fitness--;
        }

        HandlePlayerRotation();
    }
    public override void InitPlayer()
    {
        if (!isInited)
        {
            id++;
            teamGoally = transform.parent.gameObject.GetComponentInChildren<GoallyPlayer>();
            if (teamGoally.transform.position.x < 0)
            {
                isRedTeam = false;
            }
            else
            {
                isRedTeam = true;
            }

            AttackPlayer[] list = oponentTeam.GetComponentsInChildren<AttackPlayer>();
            foreach (AttackPlayer a in list)
            {
                if (a.NameType == GameConsts.ATTACK_PLAYER)
                {
                    oponentAttacker = a;
                    break;
                }
            }
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

        /* Add move to oponent Attacker */
        Vector2 toOponentAttacker = (oponentAttacker.transform.position - transform.position).normalized;
        inputs.Add(toOponentAttacker.x);
        inputs.Add(toOponentAttacker.y);

        /* Add ball hit direction */
        Vector3 oponentGoalPos = new Vector3(oponentGoal.transform.position.x, oponentGoal.transform.position.y, 0);
        if (oponentGoalPos.x < 0)
        {
            oponentGoalPos.x += 0.3f;
        }
        else
        {
            oponentGoalPos.x -= 0.3f;
        }
        Vector2 toOponentGoal = (oponentGoalPos - transform.position).normalized;
        inputs.Add(toOponentGoal.x);
        inputs.Add(toOponentGoal.y);

        /* Update ANN and get Output */
        List<double> output = brain.Update(inputs);

        rgBody.AddForce(new Vector2(((float)output[0]), ((float)output[1])), ForceMode2D.Impulse);
        directionOfHitBall = new Vector2((float)output[2], (float)output[3]);

        /* RECORD MISTAKE IN DIRECTION */
        ballHitStrenght = (float)output[4];
        curBallHitError = (directionOfHitBall - toOponentGoal).sqrMagnitude;
        ClipPlayerToField();
    }

    new public void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    new public void OnTriggerStay2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
    }

    public AttackPlayer OponentsAttacker
    {
        get { return oponentAttacker; }
        set { oponentAttacker = value; }
    }

    public GoallyPlayer TeamGoally
    {
        get { return teamGoally; }
        set { teamGoally = value; }
    }

    new public void Reset(bool isBallInNet)
    {
        base.Reset(isBallInNet);

        if (!isBallInNet)
        {
            curDistToOponentAttacker = float.MaxValue;
            bestDistanceToOponentAttacker = float.MaxValue;
        }
    }

    public float CurDistToOponentAttacker
    {
        get { return curDistToOponentAttacker; }
    }

}
