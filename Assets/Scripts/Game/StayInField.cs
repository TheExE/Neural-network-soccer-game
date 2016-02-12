using UnityEngine;
using System.Collections;

public class StayInField : MonoBehaviour {


	void Start ()
    {
	
	}
	
	
	void Update () 
    {
        ClipPlayerToGoalLine();
	}

    private void ClipPlayerToGoalLine()
    {
        if (transform.position.x > 2.95f)
        {
            transform.position = new Vector2(2.95f, transform.position.y);
        }
        else if (transform.position.x < -2.95f)
        {
            transform.position = new Vector2(-2.95f, transform.position.y);
        }

        if(transform.position.y > 2.6f)
        {
            transform.position = new Vector2(transform.position.x, 2.6f);
        }
        else if(transform.position.y < -2.5f)
        {
            transform.position = new Vector2(transform.position.x, -2.5f);
        }
    }
}
