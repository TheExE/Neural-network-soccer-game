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
        curTime += Time.deltaTime;
        if (curTime > 2 && !isColided)
        {
            curTime = 0;

            curDistanceToHomeGoal = (homeGoal.transform.position - transform.position).sqrMagnitude;
            curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
            curDistToOponentAttacker = (oponentAttacker.transform.position - transform.position).sqrMagnitude;
            curDistToGoaly = (teamGoally.transform.position - transform.position).sqrMagnitude;
            curDistanceToAttacker = (attackerPlayer.transform.position - transform.position).sqrMagnitude;


            /* DISTANCE TO GOALY PLAYER */
            /*if (curDistToGoaly > bestDistToGoly && curDistToGoaly < 2f)
            {
                bestDistToGoly = curDistToGoaly;
                fitness++;
            }*/


            /* DISTANCE TO BALL */
            if (curDistanceToBall < bestDistanceToBall || curDistanceToBall < 0.1f)
            {
                bestDistanceToBall = curDistanceToBall;
                fitness++;
            }

            /* DISTANCE TO HOME GOAL */
            if (curDistanceToHomeGoal < bestDistanceToHomeGoal || curDistanceToHomeGoal < 1f)
            {
                bestDistanceToHomeGoal = curDistanceToHomeGoal;
                fitness++;
            }


            /* REWARD FOR LESSER ERROR IN DIRECTION */
            /*if (curBallHitDirectionError < bestBallHitDirectionError || curBallHitDirectionError < 0.1f)
            {
                bestBallHitDirectionError = curBallHitDirectionError;
                fitness++;
            }*/

            /* REWARD FOR GOING CLOSER TO OPPONENT ATTACKR */
            /*if (curDistToOponentAttacker < bestDistanceToOponentAttacker)
            {
                bestDistanceToOponentAttacker = curDistToOponentAttacker;
                fitness++;
            }*/

            /* REWARD FOR NOT BEING IN THE WAY OF ATTACKER */
            /*if (curDistanceToAttacker > bestDistanceToAttacker && curDistanceToAttacker < 4f)
            {
                bestDistanceToAttacker = curDistanceToAttacker;
                fitness++;
            }*/
        }

        isColided = false;
        HandlePlayerRotation();
    }

    public override void InitPlayer()
    {
        if (!isInited)
        {
            id++;
            teamGoally = transform.parent.gameObject.GetComponentInChildren<GoallyPlayer>();
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
        Vector2 toBall = (ballScript.transform.position - transform.position).normalized;
        inputs.Add(toBall.x);
        inputs.Add(toBall.y);

        /* Add move to home goal */
        Vector2 toHomeGoal = (homeGoal.transform.position - transform.position).normalized;
        inputs.Add(toHomeGoal.x);
        inputs.Add(toHomeGoal.y);

        /* Add ball hit direction */
        Vector2 ballToGoal = (oponentGoal.transform.position - ballScript.transform.position).normalized;
        /*inputs.Add(ballToGoal.x);
        inputs.Add(ballToGoal.y);*/

        /* Update the brain and get feedback */
        List<double> output = brain.Update(inputs);
        /*transform.position = new Vector2(transform.position.x + (float)output[0] * Time.deltaTime,
            transform.position.y + (float)output[1] * Time.deltaTime);*/

        rgBody.AddForce(new Vector2(((float)output[0]), ((float)output[1])), ForceMode2D.Impulse);
        directionOfHitBall = ballToGoal;

        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (ballToGoal - directionOfHitBall).sqrMagnitude;
        ballHitStrenght = 0.6f;

        ClipPlayerToField();
        PunishCampers();
        lastPosition = new Vector3(transform.position.x, transform.position.y);
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

        if(!isBallInNet)
        {
            curDistanceToHomeGoal = float.MaxValue;
            bestDistanceToHomeGoal = float.MaxValue;
        }
    }

}
