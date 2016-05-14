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
    protected float fitness;
    protected DefensePlayer[] oponentDefense;

    protected bool colided = false;
    protected float curDistanceToBall;
    protected float bestDistanceToBall = float.MaxValue;
    protected float lastPositionX;
    protected bool isInited = false;
    protected Vector2 directionOfHitBall = Vector2.zero;
    protected float curBallHitError = float.MaxValue;
    protected float bestBallHitError = float.MaxValue;
    protected float ballHitStrenght = 1;
    protected float bestBallHitStrenght = 0;
    protected float scale = 1;
    protected float curTime = 0;
    protected int ballHitTimes = 0;
    protected bool isColided = false;
    protected Vector3 lastPosition = Vector3.zero;
    protected float curColideTimer = 0f;
    private bool isBallKicked = false;

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
        curDistanceToBall = (ballScript.transform.position - transform.position).sqrMagnitude;
        curColideTimer += Time.deltaTime;
        curTime += Time.deltaTime;
        
        if(curTime > 0.15f)
        {
            curTime = 0;
            if (curDistanceToBall < bestDistanceToBall || curDistanceToBall < 0.2f)
            {
                bestDistanceToBall = curDistanceToBall;
                fitness++;
            }

            if (colided)
            {
                fitness--;
                isBallKicked = false;
            }
            /* REWARD FOR HITING BALL IN RIGHT DIRECTION */
            if (!IsBallGoingToBeOutBoundAfterKick())
            {
                if (curBallHitError < bestBallHitError || curBallHitError < 0.1f)
                {
                    curBallHitError = bestBallHitError;
                    fitness += 5;
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

        if(curColideTimer > 1)
        {
            curColideTimer = 0;
            isBallKicked = false;
        }
        HandlePlayerRotation();
    }

    public virtual void InitPlayer()
    {
        if (!isInited)
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
        List<double> inputs = new List<double>();

        /* Add ball locations */
        Vector2 toBall = (ballScript.transform.position - transform.position).normalized;
        inputs.Add(toBall.x);
        inputs.Add(toBall.y);

        /* Oponents goal */
        Vector3 oponentGoalPos = new Vector3(oponentGoal.transform.position.x, oponentGoal.transform.position.y, 0);
        if(oponentGoalPos.x < 0)
        {
            oponentGoalPos.x += 0.5f;
        }
        else
        {
            oponentGoalPos.x -= 0.5f;
        }
        Vector2 toOponentGoal = (oponentGoalPos - transform.position).normalized;
        inputs.Add(toOponentGoal.x);
        inputs.Add(toOponentGoal.y);

        //update the brain and get feedback
        List<double> output = brain.Update(inputs);
        rgBody.AddForce(new Vector2((float)output[0], (float)output[1]), ForceMode2D.Impulse);

        directionOfHitBall = new Vector2((float)output[2], (float)output[3]);
        ballHitStrenght = (float)output[4];
        curBallHitError = (directionOfHitBall - toOponentGoal).sqrMagnitude;
        ClipPlayerToField();
    }

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            ballScript.Shoot(directionOfHitBall, ballHitStrenght);
            if (ballHitTimes < 50)
            {
                fitness++;
                ballHitTimes++;
            }

            if (gameObject.name.Contains("Att"))
            {
                isBallKicked = true;
            }
        }
    }

    public void OnTriggerStay2D(Collider2D collision)
    {
        OnTriggerEnter2D(collision);
    }

    protected void HandlePlayerRotation()
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

    public void PunishCampers()
    {
        Vector2 dif = transform.position - lastPosition;
        if(dif.magnitude < 0.01f)
        {
            fitness--;
        }
    }
    public float Fitness
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
    public void Reset(bool isBallInNet)
    {
        if(!isBallInNet)
        {
            fitness = 0;
            curDistanceToBall = float.MaxValue;
            bestDistanceToBall = float.MaxValue;
            curBallHitError = float.MaxValue;
            bestBallHitError = float.MaxValue;
        }
        /* RESET FORCE */
        rgBody.velocity = Vector2.zero;
        rgBody.angularVelocity = 0f;
        rgBody.angularDrag = 0f;
    }
    
    protected void ClipPlayerToField()
    {
        colided = false;
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

        if (transform.position.y > GameConsts.GAME_FIELD_UP)
        {
            transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_UP);
            colided = true;
        }
        else if (transform.position.y < GameConsts.GAME_FIELD_DOWN)
        {
            transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_DOWN);
            colided = true;
        }
    }
    protected bool IsBallGoingToBeOutBoundAfterKick()
    {
        bool hitInvalid = false;
        Vector2 ballPosition = ballScript.transform.position;
        if(ballPosition.x + directionOfHitBall.x < GameConsts.GAME_FIELD_LEFT)
        {
            hitInvalid = true;
        }
        else if(ballPosition.x + directionOfHitBall.x > GameConsts.GAME_FIELD_RIGHT)
        {
            hitInvalid = true;
        }
        else if(ballPosition.y + directionOfHitBall.y < GameConsts.GAME_FIELD_DOWN)
        {
            hitInvalid = true;
        }
        else if(ballPosition.y + directionOfHitBall.y > GameConsts.GAME_FIELD_UP)
        {
            hitInvalid = true;
        }
        return hitInvalid;
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
        foreach (DefensePlayer oponent in oponentDefense)
        {
            float curDistance = (oponent.transform.position - transform.position).sqrMagnitude;
            if (curDistance < bestDistance)
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

    public List<int> SplitPoints
    {
        get { return brain.CauculateSplitPoints(); }
    }

    public float CurDistanceToBall
    {
        get { return curDistanceToBall; }
    }

    public bool IsBallKicked
    {
        get { return isBallKicked; }
    }
    
}
