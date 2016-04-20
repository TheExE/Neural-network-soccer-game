using UnityEngine;
using System.Collections.Generic;

public class GoallyPlayer : AttackPlayer
{
    public GameObject goalToSave;

    private float curYDiffWithBall = 0;
    private float bestYDiffWithBall = float.MaxValue;
    private float curYDiffWithGoalCenter = 0;
    private float bestYDiffWithGoalCenter = float.MaxValue;
    private int chosenDefensePlayer = 0;
    private DefensePlayer[] teamDefense;

    public GoallyPlayer()
    {
        nameType = GameConsts.GOALLY_PLAYER;
    }
    
    void Start()
    {
        if(!isInited)
        {
            InitPlayer();
        }
    }

    void Update()
    {
        curTime += Time.deltaTime;
        if(curTime > 2f && !isColided)
        {
            curTime = 0;
            curYDiffWithBall = ballScript.transform.position.y - transform.position.y;
            curYDiffWithGoalCenter = goalToSave.transform.position.y - transform.position.y;

            if (curYDiffWithBall < bestYDiffWithBall)
            {
                bestYDiffWithBall = curYDiffWithBall;
                fitness++;
            }

            if (curYDiffWithGoalCenter < bestYDiffWithGoalCenter)
            {
                bestYDiffWithGoalCenter = curYDiffWithGoalCenter;
                fitness++;
            }

            /* REWARD FOR LESSER ERROR IN DIRECTION */
            if (curBallHitDirectionError < bestBallHitDirectionError)
            {
                bestBallHitDirectionError = curBallHitDirectionError;
                fitness++;
            }
        }

        isColided = false;
    }

    public override void InitPlayer()
    {
        if(!isInited)
        {
            id++;
            teamDefense = transform.parent.gameObject.GetComponentsInChildren<DefensePlayer>();
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

        /* Add y distance from goal middle */
         Vector2 toGoalCenter = (goalToSave.transform.position - transform.position).normalized;
         inputs.Add(toGoalCenter.y);


        /* Add ball hit direction */
        Vector2 toOponentGoal = (oponentGoal.transform.position - ballScript.transform.position).normalized;
        inputs.Add(toOponentGoal.x);
        inputs.Add(toOponentGoal.y);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);

        rgBody.AddForce(new Vector2(0f, ((float)output[0])), ForceMode2D.Impulse);
        directionOfHitBall = new Vector2((float)output[1], (float)output[2]);

        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (toOponentGoal - directionOfHitBall).sqrMagnitude;
        ballHitStrenght = (float)output[3];

      //  ClipPlayerToField();
    }
    private float GetDistanceToClosesDefensePlayer()
    {
        float bestDistance = float.MaxValue;
        foreach(DefensePlayer def in teamDefense)
        {
            float curDistance = (def.transform.position - transform.position).sqrMagnitude;
            if (curDistance < bestDistance)
            {
                bestDistance = curDistance;
            }
        }

        return bestDistance;
    }

    new public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            ballScript.Shoot(directionOfHitBall, ballHitStrenght);

            if(ballHitTimes < 10)
            {
                fitness += 1f;
                ballHitTimes++;
            }
        }
        isColided = true;
    }
}
