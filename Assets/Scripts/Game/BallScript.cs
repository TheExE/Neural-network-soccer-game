using UnityEngine;
using System.Collections;

public class BallScript : MonoBehaviour
{
    private Rigidbody2D rgBody;
    private Vector2 startPosition;
    private float cullDown = 1.5f;
    private float curCullDown = 0f;
    private bool ballIsFlying = false;

    void Start()
    {
        rgBody = GetComponent<Rigidbody2D>();
        startPosition = new Vector2(transform.position.x, transform.position.y);
    }


    void Update()
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

        if(ballIsFlying)
        {
            curCullDown += Time.deltaTime;
            if(curCullDown > cullDown)
            {
                ballIsFlying = false;
            }
        }
    }
    public void Shoot(Vector2 direction)
    {
        if(!ballIsFlying)
        {
            rgBody.AddForce(direction * -10);
            ballIsFlying = true;
        }
    }
    public void Reset()
    {
        transform.position = new Vector2(startPosition.x, startPosition.y);
        rgBody.velocity = Vector2.zero;
    }

}
