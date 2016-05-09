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
    private static bool shouldPauseEvolutionA = false;
    private static bool shouldPauseEvolutionD = false;
    private static bool shouldPauseEvolutionG = false;

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
        blueTeamFitnes.text = "A Fit: " + teamController.BestBlueAttacker + '\n'
            + " D Fit: " + teamController.BestBlueDefense + '\n'
            + " G Fit: " + teamController.BestBlueGoally;
        redTeamFitness.text = "A Fit: " + teamController.BestRedAttacker + '\n'
            + " D Fit: " + teamController.BestRedDefense + '\n'
            + " G Fit: " + teamController.BestRedGoally;

        timer += Time.deltaTime;
        if(timer > 2)
        {

            int bestIdx = teamController.GetClosestToBallAttackPlayer().index;
            if (bestIdx != -1)
            {
                teamController.Attacker[bestIdx].Fitness++;
            }
        }

        /* PAUSE ATTACKER EVO */
        if(Input.GetKeyDown(KeyCode.A) && shouldPauseEvolutionA)
        {
            shouldPauseEvolutionA = false;
        }
        else if(Input.GetKeyDown(KeyCode.A) && !shouldPauseEvolutionA)
        {
            shouldPauseEvolutionA = true;
        }

        /* PAUSE DEFENSE PLAYER EVO */
        if (Input.GetKeyDown(KeyCode.D) && shouldPauseEvolutionD)
        {
            shouldPauseEvolutionD = false;
        }
        else if (Input.GetKeyDown(KeyCode.D) && !shouldPauseEvolutionD)
        {
            shouldPauseEvolutionD = true;
        }

        /* PAUSE GOALLY PALYER EVO */
        if (Input.GetKeyDown(KeyCode.G) && shouldPauseEvolutionG)
        {
            shouldPauseEvolutionG = false;
        }
        else if (Input.GetKeyDown(KeyCode.G) && !shouldPauseEvolutionG)
        {
            shouldPauseEvolutionG = true;
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

    public static bool ShouldPauseEvoA
    {
        get { return shouldPauseEvolutionA; }
    }
    public static bool ShouldPauseEvoD
    {
        get { return shouldPauseEvolutionD; }
    }
    public static bool ShouldPauseEvoG
    {
        get { return shouldPauseEvolutionG; }
    }

}
