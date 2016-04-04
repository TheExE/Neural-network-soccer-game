using UnityEngine;
using System.Collections.Generic;

public class GoallyPlayer : AttackPlayer
{
    public GameObject goalToSave;
    private float curYDiffWithBall = 0;
    private float bestYDiffWithBall = float.MaxValue;
    private float curYDiffWithGoalCenter = 0;
    private float bestYDiffWithGoalCenter = float.MaxValue;
    private bool shouldPassBall = false;
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
        curYDiffWithBall = ballScript.transform.position.y - transform.position.y;
        curYDiffWithGoalCenter = goalToSave.transform.position.y - transform.position.y;

        if(curYDiffWithBall < bestYDiffWithBall)
        {
            bestYDiffWithBall = curYDiffWithBall;
            fitness++;
        }

        if(curYDiffWithGoalCenter < bestYDiffWithGoalCenter)
        {
            bestYDiffWithGoalCenter = curYDiffWithGoalCenter;
            fitness += 0.5f;
        }

        /* REWARD FOR LESSER ERROR IN DIRECTION */
        if (curBallHitDirectionError < bestBallHitDirectionError)
        {
            bestBallHitDirectionError = curBallHitDirectionError;
            fitness += 0.8f;
        }
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
        Vector2 toDefensePlayer = (teamDefense[Random.Range(0,1)].transform.position - ballScript.transform.position).normalized;
        inputs.Add(toDefensePlayer.x);
        inputs.Add(toDefensePlayer.y);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x, transform.position.y + 
		(float)output[0] * Time.deltaTime);
        directionOfHitBall = new Vector2((float)output[1], (float)output[2]);

        /* RECORD MISTAKE IN DIRECTION */
        curBallHitDirectionError = (toDefensePlayer - directionOfHitBall).sqrMagnitude;

        ClipPlayerToField();
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

    new public void Reset()
    {
        base.Reset();
        curYDiffWithBall = 0;
        bestYDiffWithBall = float.MaxValue;
    }
}
