using UnityEngine;
using System.Collections.Generic;
using System;

public class AttackPlayer : MonoBehaviour
{
    public Goal oponentGoal;
    public GameObject oponentTeam;

    protected string nameType = GameConsts.ATTACK_PLAYER;
    protected Rigidbody2D rgBody;
    protected NeuralNetwork brain;
    protected BallScript ballScript;
    protected static int id = 0;
    protected double fitness;
    protected DefensePlayer[] oponentDefense;
    protected bool colided = false;

    private float kickStrenght = 60f;
    private float speed = 0.02f;
    private float curDistanceBallToGoal;
    protected float curDistanceToBall;
    private float curDistaceToOpenent;
    private float bestDistanceToGoal = float.MaxValue;
    protected float bestDistanceToBall = float.MaxValue;
    private float bestDistanceToOponent = 0;
    private bool haveBall = false;
    private float lastPositionX;
    protected bool isInited = false;


    void Start()
    {
        if(!isInited)
        {
            InitPlayer();
        }
        curDistaceToOpenent = (GetClosestOponentPosition() - transform.position).sqrMagnitude;
        curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
        curDistanceBallToGoal = (oponentGoal.transform.position - ballScript.transform.position).sqrMagnitude;
        bestDistanceToBall = curDistanceToBall;
        bestDistanceToGoal = curDistanceBallToGoal;
        lastPositionX = transform.position.x;
    }

    void Update()
    {
        if(!colided)
        {
            curDistaceToOpenent = (GetClosestOponentPosition() - transform.position).sqrMagnitude;
            curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
            curDistanceBallToGoal = (oponentGoal.transform.position - 
                ballScript.transform.position).sqrMagnitude;

            /* Distance to goal and ball */
            if (haveBall)
            {
                if (curDistanceBallToGoal < bestDistanceToGoal && curDistanceToBall < bestDistanceToBall)
                {
                    bestDistanceToGoal = curDistanceBallToGoal;
                    bestDistanceToBall = curDistanceToBall;
                    fitness++;
                }

                /* Distance to oponent */
                if (curDistaceToOpenent > bestDistanceToOponent)
                {
                    bestDistanceToOponent = curDistaceToOpenent;
                    fitness++;
                }
            }
            else
            {
                if (curDistanceToBall < bestDistanceToBall)
                {
                    bestDistanceToBall = curDistanceToBall;
                    fitness++;
                }
            }
        }
        HandlePlayerRotation();
    }

    public virtual void InitPlayer()
    {
        if(!isInited)
        {
            id++;
            oponentDefense = oponentTeam.GetComponentsInChildren<DefensePlayer>();
            rgBody = GetComponent<Rigidbody2D>();
            ballScript = FindObjectOfType<BallScript>();

            brain = new NeuralNetwork(NeuralNetworkConst.ATTACKER_INPUT_COUNT, NeuralNetworkConst.ATTACKER_OUTPUT_COUNT,
                NeuralNetworkConst.ATTACKER_HID_LAYER_COUNT, NeuralNetworkConst.ATTACKER_NEURONS_PER_HID_LAY);
            isInited = true;
        }
    }


    public virtual void UpdatePlayerBrains()
    {
        //this will store all the inputs for the NN
        List<double> inputs = new List<double>();

        //ball distance to opponnents goal
        Vector2 ballToOponentGoal = (ballScript.transform.position - 
            oponentGoal.transform.position);
        inputs.Add(ballToOponentGoal.x);
        inputs.Add(ballToOponentGoal.y);

        //add ball locations
        Vector2 playerToBall = (ballScript.transform.position - transform.position);
        inputs.Add(playerToBall.x);
        inputs.Add(playerToBall.y);

        /*//add goal location
        Vector2 playerToOponentGoal = (oponentGoal.transform.position - transform.position).normalized;
        inputs.Add(playerToOponentGoal.x);
        inputs.Add(playerToOponentGoal.y);*/

        //add distance to cosest oponent defense
        Vector2 playerToClosesOponent = (GetClosestOponentPosition() - transform.position);
        inputs.Add(playerToClosesOponent.x);
        inputs.Add(playerToClosesOponent.y);

        //add info about having a ball or not
        inputs.Add(haveBall ? 1 : 0);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        transform.position = new Vector2(transform.position.x + (float)output[0] * Time.deltaTime,
            transform.position.y + (float)output[1] * Time.deltaTime);
    
       
        if (output[2] > 0)
        {
            //try to shoot or the goal
            ballScript.Shoot(this);
        }
        ClipPlayerToField();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            
            if (!ballScript.IsControlled)
            {
                ballScript.TakeControl(this);
                fitness++;
            }
            else
            {
                if (ballScript.TryToTake(this))
                {
                    fitness++;
                }
            }
            
        }
    }

    private void HandlePlayerRotation()
    {
        /* Switching player orientation */
        if (transform.position.x > lastPositionX + 0.1)
        {
            transform.rotation = Quaternion.Euler(0, 0, -180);
        }
        else if (transform.position.x < lastPositionX - 0.1)
        {
            transform.rotation = Quaternion.Euler(0, 0, 0);
        }


        lastPositionX = transform.position.x;
    }

    public float KickStrenght
    {
        get { return kickStrenght; }
        set { kickStrenght = value; }
    }

    public double Fitness
    {
        get { return fitness; }
        set { fitness = value; }

    }
    public void PutWeights(List<double> weights)
    {
        brain.PutWeights(weights);
    }

    public int NumberOfWeights
    {
        get { return brain.GetNumberOfWeights(); }
    }

    public void Reset()
    {
        fitness = 0;
        bestDistanceToBall = float.MaxValue;
        bestDistanceToGoal = float.MaxValue;
        curDistanceToBall = 0;
        curDistanceBallToGoal = 0;
    }

    public bool HaveBall
    {
        get { return haveBall; }
        set { haveBall = value; }
    }


    protected void ClipPlayerToField()
    {
        if (transform.position.x > GameConsts.GAME_FIELD_RIGHT)
        {
            transform.position = new Vector2(GameConsts.GAME_FIELD_RIGHT, transform.position.y);
            colided = true;
        }
        else if (transform.position.x < GameConsts.GAME_FIELD_LEFT)
        {
            transform.position = new Vector2(GameConsts.GAME_FIELD_LEFT, transform.position.y);
            colided = true;
        }
        else if (transform.position.y > GameConsts.GAME_FIELD_UP)
        {
            transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_UP);
            colided = true;
        }
        else if (transform.position.y < GameConsts.GAME_FIELD_DOWN)
        {
            transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_DOWN);
            colided = true;
        }
        else
        {
            colided = false;
        }
    }

    public Rigidbody2D PhysicsBody
    {
        get { return rgBody; }
    }

    public int PlayerID
    {
        get { return id; }
    }

    private Vector3 GetClosestOponentPosition()
    {
        Vector3 closestOp = Vector3.zero;
        float bestDistance = float.MaxValue;
        foreach(DefensePlayer oponent in oponentDefense)
        {
            float curDistance = (oponent.transform.position - transform.position).sqrMagnitude;
            if(curDistance < bestDistance)
            {
                bestDistance = curDistance;
                closestOp = new Vector2(oponent.transform.position.x, oponent.transform.position.y);
            }
        }

        return closestOp;
    }

    public string NameType
    {
        get { return nameType; }
    }
}
