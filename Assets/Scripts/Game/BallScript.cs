using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour
{
    private Rigidbody2D rgBody;
    private Vector2 startPosition;
    private float cullDown = 0.2f;
    private float curCullDown = 0f;
    private bool ballIsFlying = false;
    private float ballFieldUp = GameConsts.GAME_FIELD_UP - 0.08f;
    private float ballFieldDown = GameConsts.GAME_FIELD_DOWN + 0.08f;

    void Start()
    {
        rgBody = GetComponent<Rigidbody2D>();
        startPosition = new Vector2(transform.position.x, transform.position.y);
    }


    void Update()
    {
        if(ballIsFlying)
        {
            curCullDown += Time.deltaTime;
            if(curCullDown > cullDown)
            {
                ballIsFlying = false;
                curCullDown = 0;
            }
        }

        /*KeepBallInField();*/
    }
    
    public void KeepBallInField()
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

        if (transform.position.y > ballFieldUp)
        {
            transform.position = new Vector2(transform.position.x, ballFieldUp);
        }
        else if (transform.position.y < ballFieldDown)
        {
            transform.position = new Vector2(transform.position.x, ballFieldDown);
        }
    }

    public void Shoot(Vector2 direction, float hitStrenght)
    {
        float resultingStrenght = 0f;

        if(hitStrenght < 0)
        {
            hitStrenght *= -1f;
            hitStrenght = 1 - hitStrenght;
        }
        else
        {
            resultingStrenght = (GameConsts.BALL_HIT_STRENGHT_SCALE / 2f);
        }

        resultingStrenght += (GameConsts.BALL_HIT_STRENGHT_SCALE / 2f) * hitStrenght;

        if (!ballIsFlying)
        {
            rgBody.AddForce(direction * resultingStrenght);
            ballIsFlying = true;
        }
    }
    public void Reset()
    {
        transform.position = new Vector2(startPosition.x, startPosition.y);
        rgBody.velocity = Vector2.zero;
    }

}
