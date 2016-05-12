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
    private float curDistToOponentAttacker = float.MaxValue;
    private float bestDistanceToOponentAttacker = float.MaxValue;

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
        lastPosition = new Vector3(transform.position.x, transform.position.y);
    }

    void Update()
    {
        curTime += Time.deltaTime;
        if (curTime > 1)
        {
            curTime = 0;

            curDistanceToHomeGoal = (homeGoal.transform.position - transform.position).sqrMagnitude;
            curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
            curDistToOponentAttacker = (oponentAttacker.transform.position - transform.position).sqrMagnitude;


            /* DISTANCE TO BALL */
            if (curDistanceToBall < bestDistanceToBall || curDistanceToBall < 0.1f)
            {
                bestDistanceToBall = curDistanceToBall;
                fitness++;
            }

            /* DISTANCE TO HOME GOAL */
            if (curDistanceToHomeGoal < bestDistanceToHomeGoal || curDistanceToHomeGoal < 0.78f)
            {
                bestDistanceToHomeGoal = curDistanceToHomeGoal;
                fitness++;
            }


            /* REWARD FOR GOING CLOSER TO OPPONENT ATTACKR */
            if (curDistToOponentAttacker < bestDistanceToOponentAttacker || curDistToOponentAttacker < 0.1f)
            {
                bestDistanceToOponentAttacker = curDistToOponentAttacker;
                fitness++;
            }
        }
        curColideTimer += Time.deltaTime;
        if (curColideTimer > 4)
        {
            if (colided)
            {
                curColideTimer = 0;
                fitness--;
            }
        }

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
        Vector2 toOponentAttacker = (oponentAttacker.transform.position - transform.position).normalized;
        inputs.Add(toOponentAttacker.x);
        inputs.Add(toOponentAttacker.y);

        /* Add move to home goal */
        Vector2 toHomeGoal = (homeGoal.transform.position - transform.position).normalized;
        inputs.Add(toHomeGoal.x);
        inputs.Add(toHomeGoal.y);

        /* Update the brain and get feedback */
        List<double> output = brain.Update(inputs);

        rgBody.AddForce(new Vector2(((float)output[0]), ((float)output[1])), ForceMode2D.Impulse);
        directionOfHitBall = (oponentGoal.transform.position - ballScript.transform.position).normalized;

        /* RECORD MISTAKE IN DIRECTION */
        ballHitStrenght = 0.6f;

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

        if(!isBallInNet)
        {
            curDistanceToHomeGoal = float.MaxValue;
            bestDistanceToHomeGoal = float.MaxValue;
        }
    }

}
