using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour
{
    private Rigidbody2D rgBody;
    private Vector2 startPosition;
    private AttackPlayer curPlayer = null;
    private bool isTakenControlOf = false;

    void Start()
    {
        rgBody = GetComponent<Rigidbody2D>();
        startPosition = new Vector2(transform.position.x, transform.position.y);
    }


    void Update()
    {
        if (curPlayer == null)
        {

            if (transform.position.x > GameConsts.GAME_FIELD_RIGHT && (transform.position.y < GameConsts.GOAL_DOWN
                || transform.position.y > GameConsts.GOAL_UP))
            {
                transform.position = new Vector2(GameConsts.GAME_FIELD_RIGHT, transform.position.y);
            }
            else if (transform.position.x < GameConsts.GAME_FIELD_LEFT && (transform.position.y > GameConsts.GOAL_UP
                || transform.position.y < GameConsts.GOAL_DOWN))
            {
                transform.position = new Vector2(GameConsts.GAME_FIELD_LEFT, transform.position.y);
            }

            if (transform.position.y > GameConsts.GAME_FIELD_UP)
            {
                transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_UP);
            }
            else if (transform.position.y < GameConsts.GAME_FIELD_DOWN)
            {
                transform.position = new Vector2(transform.position.x, GameConsts.GAME_FIELD_DOWN);
            }
        }
        else
        {
            if (curPlayer.transform.rotation.eulerAngles.z == 0)
            {

                transform.position = new Vector2(curPlayer.transform.position.x - 0.5f, curPlayer.transform.position.y);
            }
            else
            {

                transform.position = new Vector2(curPlayer.transform.position.x + 0.5f, curPlayer.transform.position.y);
            }
        }
    }

    public void Shoot(AttackPlayer player)
    {
        if (curPlayer != null && player.PlayerID == curPlayer.PlayerID)
        {
            rgBody.AddForce((transform.position - curPlayer.transform.position) * 10);
            curPlayer.HaveBall = false;
            isTakenControlOf = false;
            curPlayer = null;
        }
    }

    public void Pass(AttackPlayer from, AttackPlayer to)
    {
        Vector2 toAttacker = to.transform.position - from.transform.position;
        rgBody.AddForce(toAttacker.normalized * toAttacker.magnitude);
    }

    public void TakeControl(AttackPlayer player)
    {
        curPlayer = player;
        isTakenControlOf = true;
        curPlayer.HaveBall = true;
    }

    public void Reset()
    {
        transform.position = new Vector2(startPosition.x, startPosition.y);
        rgBody.velocity = Vector2.zero;
    }

    public bool IsControlled
    {
        get { return isTakenControlOf; }
    }

    public bool TryToTake(AttackPlayer player)
    {
        bool succeeded = false;
        float chance = Random.Range(0, 1);
        if (chance < 0.01f)
        {
            curPlayer.HaveBall = false;
            player.HaveBall = true;
            curPlayer = player;
            succeeded = true;
        }

        return succeeded;
    }

}
