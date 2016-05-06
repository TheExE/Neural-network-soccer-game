using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {

    public GameObject redTeamScoreObj;
    public GameObject blueTeamScoreObj;
    public tk2dTextMesh redTeamFitness;
    public tk2dTextMesh blueTeamFitnes;
    public tk2dTextMesh fpsText;

    private tk2dTextMesh redTeamTxt;
    private tk2dTextMesh blueTeamTxt;
    private static int redTeamScore;
    private static int blueTeamScore;
    private BallScript ball;
    private TeamController teamController;

    private float timer = 0f;
    private static bool shouldPauseEvolution = true;

	void Start () 
    {
        Application.runInBackground = true;
        redTeamTxt = redTeamScoreObj.GetComponent<tk2dTextMesh>();
        blueTeamTxt = blueTeamScoreObj.GetComponent<tk2dTextMesh>();
        ball = GameObject.FindObjectOfType<BallScript>();
        teamController = GetComponent<TeamController>();
	}
	
	void Update () 
    {
        redTeamTxt.text = "Score: " + redTeamScore;
        blueTeamTxt.text = "Score: " + blueTeamScore;
        fpsText.text = "Fps:" + Mathf.RoundToInt(1 / Time.deltaTime);
        blueTeamFitnes.text = "Att Fit: " + teamController.BestBlueAttacker;
        redTeamFitness.text = "Att Fit: " + teamController.BestRedAttacker;

        timer += Time.deltaTime;
        if(timer > 2)
        {

            int bestIdx = teamController.GetClosestToBallAttackPlayer().index;
            if (bestIdx != -1)
            {
                teamController.Attacker[bestIdx].Fitness++;
            }
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
        teamController.IncreaseTeamsFitness(1,true);
    
    }

    public void IncreaseFitnessRedTeam()
    {
        teamController.IncreaseTeamsFitness(1, false);
    }
     
    public void RestartGame()
    {
        ball.Reset();
        teamController.Reset();
    }

    public static bool ShouldContinueEvolution
    {
        get { return shouldPauseEvolution; }

    }

}
