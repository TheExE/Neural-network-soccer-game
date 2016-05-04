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
        Vector2 toDefensePlayer = (teamDefense[GetClosestDefensePlayerIdx()]
            .transform.position - ballScript.transform.position).normalized;
        inputs.Add(toDefensePlayer.x);
        inputs.Add(toDefensePlayer.y);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);

        transform.position = new Vector2(transform.position.x,
            transform.position.y + (GetScaledOutput(output[0]) * Time.deltaTime));
        directionOfHitBall = new Vector2(GetScaledOutput(output[1]) * 2, GetScaledOutput(output[2]) * 2);

        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (toDefensePlayer - directionOfHitBall).sqrMagnitude;
        ballHitStrenght = GetScaledOutput(output[3]);

        CheckCollision();
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

    private int GetClosestDefensePlayerIdx()
    {
        float bestDistance = float.MaxValue;
        int keyIdx = -1;
        int index = 0;
        foreach (DefensePlayer def in teamDefense)
        {
            float curDistance = (def.transform.position - transform.position).sqrMagnitude;
            if (curDistance < bestDistance)
            {
                bestDistance = curDistance;
                keyIdx = index;
            }
            index++;
        }

        return keyIdx;
    }
}
