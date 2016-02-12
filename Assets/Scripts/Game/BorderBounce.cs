using UnityEngine;
using System.Collections;

public class BorderBounce : MonoBehaviour {

    
	void Start () 
    {
	
	}
	

	void Update ()
    {
	
	}

    public void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.gameObject.tag == "Ball")
        {
            var ballBody = collision.gameObject.GetComponent<Rigidbody2D>();
            ballBody.AddForce((ballBody.gameObject.transform.position - transform.position)* ballBody.angularVelocity*0.8f);
        }
    }
}
