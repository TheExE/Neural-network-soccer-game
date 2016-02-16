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

        if(HaveBall)
        {
            ballScript.Pass(this, teamDefense[chosenDefensePlayer]);
            fitness++;
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
        //this will store all the inputs for the NN
        List<double> inputs = new List<double>();

        //add ball locations
        inputs.Add(ballScript.transform.position.y);

        //add y distance from goal middle
        inputs.Add(goalToSave.transform.position.y);

        //add info about closes defense player
       // inputs.Add(GetDistanceToClosesDefensePlayer());


        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x, transform.position.y + 
		(float)output[0] * 2 * Time.deltaTime);


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
