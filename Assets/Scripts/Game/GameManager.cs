using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class GameManager : MonoBehaviour
{
    public GameObject redTeamScoreObj;
    public GameObject blueTeamScoreObj;
    public tk2dTextMesh redTeamFitness;
    public tk2dTextMesh blueTeamFitnes;
    public tk2dTextMesh fpsText;
    public AudioClip clip;

    private tk2dTextMesh redTeamTxt;
    private tk2dTextMesh blueTeamTxt;
    private static int redTeamScore;
    private static int blueTeamScore;
    private BallScript ball;
    private TeamController teamController;
    private AudioSource source;
    private bool isFirstRun = true;

    private static bool shouldPauseEvolutionA = false;
    private static bool shouldPauseEvolutionD = false;
    private static bool shouldPauseEvolutionG = false;

	void Start () 
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 500;
        redTeamTxt = redTeamScoreObj.GetComponent<tk2dTextMesh>();
        blueTeamTxt = blueTeamScoreObj.GetComponent<tk2dTextMesh>();
        ball = GameObject.FindObjectOfType<BallScript>();
        teamController = GetComponent<TeamController>();
        source = GetComponent<AudioSource>();
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

        /* STOP EVOLVING ATTACKER */
        if(teamController.RedTeamAttacker.IsBallKicked && teamController.BlueTeamAttacker.IsBallKicked)
        {
            if(!shouldPauseEvolutionA)
            {
                shouldPauseEvolutionA = true;
                source.PlayOneShot(clip);
            }
        }

        /* STOP EVOLVING GOALLY */
        if(teamController.RedTeamGoally.IsTrained && teamController.BlueTeamGoally.IsTrained)
        {
            shouldPauseEvolutionG = true;
        }
        else
        {
            shouldPauseEvolutionG = false;
        }


        if(Input.GetKeyDown(KeyCode.S))
        {
            teamController.SaveState();
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
