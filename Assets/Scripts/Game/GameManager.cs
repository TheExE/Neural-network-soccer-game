using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public GameObject redTeamScoreObj;
    public GameObject blueTeamScoreObj;

    private tk2dTextMesh redTeamTxt;
    private tk2dTextMesh blueTeamTxt;
    private static int redTeamScore;
    private static int blueTeamScore;
    private BallScript ball;
    private TeamController[] teamControllers;
    private tk2dTextMesh fpsText;
    private float timer = 0f;
    private static bool shouldPauseEvolution = true;

	void Start () 
    {
        fpsText = GetComponentInChildren<tk2dTextMesh>();
        Application.runInBackground = true;
        redTeamTxt = redTeamScoreObj.GetComponent<tk2dTextMesh>();
        blueTeamTxt = blueTeamScoreObj.GetComponent<tk2dTextMesh>();
        ball = GameObject.FindObjectOfType<BallScript>();
        teamControllers = GameObject.FindObjectsOfType<TeamController>();
	}
	
	void Update () 
    {
        redTeamTxt.text = "Score: " + redTeamScore;
        blueTeamTxt.text = "Score: " + blueTeamScore;
        fpsText.text = "Fps:" + Mathf.RoundToInt(1 / Time.deltaTime);

        timer += Time.deltaTime;
        if(timer > 2)
        {

            TeamController.DistanceAndIndex blueAttack = teamControllers[0].GetClosestToBallAttackPlayer();
            TeamController.DistanceAndIndex redAttack = teamControllers[1].GetClosestToBallAttackPlayer();

            if (blueAttack.distance < redAttack.distance)
            {
                teamControllers[0].Attacker[blueAttack.index].Fitness++;
            }
            else if(redAttack.distance < blueAttack.distance)
            {
                teamControllers[1].Attacker[redAttack.index].Fitness++;
            }

            timer = 0;
        }

        if(Input.GetKeyDown(KeyCode.P))
        {
            shouldPauseEvolution = false;
        }
        else if(Input.GetKeyDown(KeyCode.U))
        {
            shouldPauseEvolution = true;
        }
	}

    public static int RedTeamScore
    {
        get { return redTeamScore; }
        set { redTeamScore = value; }
    }

    public static int BlueTeamScore
    {
        get { return blueTeamScore; }
        set { blueTeamScore = value; }
    }


    public void IncreaseFitnessBlueTeam()
    {
        IncreaseTeamsFitness(teamControllers[0]);
    
    }

    public void IncreaseFitnessRedTeam()
    {
        IncreaseTeamsFitness(teamControllers[1]);
    }
     
    public void RestartGame()
    {
        ball.Reset();
        for(int i = 0; i < teamControllers.Length; i++)
        {
            teamControllers[i].Reset();
        }
    }

    private void IncreaseTeamsFitness(TeamController teamCont)
    {
        teamCont.IncreaseTeamsFitness(NeuralNetworkConst.FITNESS_FOR_GOAL);
    }

    public static bool ShouldContinueEvolution
    {
        get { return shouldPauseEvolution; }

    }

}
