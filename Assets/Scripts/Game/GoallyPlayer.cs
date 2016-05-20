using UnityEngine;
using System.Collections.Generic;

public class GoallyPlayer : AttackPlayer
{
    public GameObject goalToSave;

    private float curYDiffWithBall = 0;
    private float bestYDiffWithBall = float.MaxValue;
    private AttackPlayer teamAttacker;

    public GoallyPlayer()
    {
        nameType = GameConsts.GOALLY_PLAYER;
    }

    void Start()
    {
        if (!isInited)
        {
            InitPlayer();
        }
    }

    void Update()
    {
        curYDiffWithBall = ballScript.transform.position.y - transform.position.y;

        if (transform.position.y > GameConsts.GOALLY_LINE_UP ||
            transform.position.y < GameConsts.GOALLY_LINE_DOWN)
        {
            fitness--;
        }

        if (!isColided)
        {
            /* REWARD FOR HIT DIRECTION */
            if (!IsBallGoingToBeOutBoundAfterKick())
            {
                if ((curBallHitError < bestBallHitError && curYDiffWithBall < bestYDiffWithBall) ||
                    (curYDiffWithBall < 0.2f && curBallHitError < 0.1f))
                {
                    bestBallHitError = curBallHitError;
                    bestYDiffWithBall = curYDiffWithBall;
                    fitness++;
                }
            }
            else
            {
                fitness--;
            }
            if (curYDiffWithBall < bestYDiffWithBall || curYDiffWithBall < 0.1f)
            {
                bestYDiffWithBall = curYDiffWithBall;
                fitness++;
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



    }

    public override void InitPlayer()
    {
        if (!isInited)
        {
            id++;
            var attackers = transform.parent.gameObject.GetComponentsInChildren<AttackPlayer>();
            foreach(AttackPlayer a in attackers)
            {
                if(a.NameType == GameConsts.ATTACK_PLAYER)
                {
                    teamAttacker = a;
                    break;
                }
            }
            rgBody = GetComponent<Rigidbody2D>();
            ballScript = FindObjectOfType<BallScript>();
            brain = new NeuralNetwork(NeuralNetworkConst.GOLY_INPUT_COUNT, NeuralNetworkConst.GOLY_OUTPUT_COUNT,
                NeuralNetworkConst.GOLY_HID_LAYER_COUNT, NeuralNetworkConst.GOLY_NEURONS_PER_HID_LAY);

            curYDiffWithBall = ballScript.transform.position.y - transform.position.y;
            isInited = true;
        }

    }

    public override void UpdatePlayerBrains()
    {
        List<double> inputs = new List<double>();

        /* Add ball locations */
        Vector2 toBall = (ballScript.transform.position - transform.position).normalized;
        inputs.Add(toBall.y);

        /* Add ball hit direction */
        Vector2 toAttacker = (teamAttacker.transform.position - ballScript.transform.position).normalized;
        inputs.Add(toAttacker.x);
        inputs.Add(toAttacker.y);

        /* Update ANN and get Output */
        List<double> output = brain.Update(inputs);

        rgBody.AddForce(new Vector2(0f, ((float)output[0])), ForceMode2D.Impulse);
        directionOfHitBall = new Vector2((float)output[1], (float)output[2]);
        ballHitStrenght = (float)output[3];
        curBallHitError = (directionOfHitBall - toAttacker).sqrMagnitude;
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
    public AttackPlayer TeamAttacker
    {
        get { return teamAttacker; }
        set { teamAttacker = value; }
    }
    new public void Reset(bool isBallInNet)
    {
        base.Reset(isBallInNet);
        if (!isBallInNet)
        {
            curYDiffWithBall = float.MaxValue;
            bestYDiffWithBall = float.MaxValue;
        }
    }
    public bool IsColided
    {
        get { return isColided; }
    }

}
