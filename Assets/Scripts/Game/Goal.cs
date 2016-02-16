using UnityEngine;
using System.Collections;
public class Goal : MonoBehaviour 
{
	public GameObject goalpopUp;
	private GameObject curGoalAnim;
    private GameManager gm;
	void Start () 
    {
        gm = GameObject.FindObjectOfType<GameManager>();
	}
	
	void Update ()
    {
	}
	
    public void OnTriggerEnter2D(Collider2D other)
    {
		if(other.tag == "Ball")
		{    
			curGoalAnim = Instantiate(goalpopUp) as GameObject;
			Invoke("AfterGoalAction", 0.6f);
			if (gameObject.name.StartsWith("R"))
			{
				GameManager.BlueTeamScore++;
				gm.IncreaseFitnessBlueTeam();
            }
            else   
			{
                GameManager.RedTeamScore++;
                gm.IncreaseFitnessRedTeam();
			
			}
			
		}
	}
    private void AfterGoalAction()
	{
		Destroy(curGoalAnim);
        gm.RestartGame();
	}

}