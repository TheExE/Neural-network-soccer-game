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
    public tk2dTextMesh fpstxt;
    public AudioClip clip;
    public TeamController blueTeam;
    public TeamController redTeam;

    private tk2dTextMesh redTeamTxt;
    private tk2dTextMesh blueTeamTxt;
    private static int redTeamScore;
    private static int blueTeamScore;
    private BallScript ball;
    private bool shouldRenderDummys = false;

	void Start () 
    {
        Application.runInBackground = true;
        Application.targetFrameRate = 500;
        redTeamTxt = redTeamScoreObj.GetComponent<tk2dTextMesh>();
        blueTeamTxt = blueTeamScoreObj.GetComponent<tk2dTextMesh>();
        ball = GameObject.FindObjectOfType<BallScript>();
	}
	
	void Update () 
    {
        fpstxt.text = "fps:" + Mathf.RoundToInt((1 / Time.deltaTime));
        redTeamTxt.text = "Score: " + redTeamScore;
        blueTeamTxt.text = "Score: " + blueTeamScore;
        blueTeamFitnes.text = "A Fit: " + blueTeam.BestAttacker + '\n'
            + " D Fit: " + blueTeam.BestDefense + '\n'
            + " G Fit: " + blueTeam.BestGoally;
        redTeamFitness.text = "A Fit: " + redTeam.BestAttacker + '\n'
            + " D Fit: " + redTeam.BestDefense + '\n'
            + " G Fit: " + redTeam.BestGoally;

        EvolutionPauseChecker();
        CheckSaveStats();
        CheckDrawDummys();
    }

    private void EvolutionPauseChecker()
    {
        /* PAUSE ATTACKER EVO */
        if (Input.GetKeyDown(KeyCode.Alpha1) && redTeam.PauseAttackEvo)
        {
            redTeam.PauseAttackEvo = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha1) && !redTeam.PauseAttackEvo)
        {
            redTeam.PauseAttackEvo = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha4) && blueTeam.PauseAttackEvo)
        {
            blueTeam.PauseAttackEvo = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha4) && !blueTeam.PauseAttackEvo)
        {
            blueTeam.PauseAttackEvo = true;
        }

        /* PAUSE DEFENSE PLAYER EVO */
        if (Input.GetKeyDown(KeyCode.Alpha2) && redTeam.PauseDefenseEvo)
        {
            redTeam.PauseDefenseEvo = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2) && !redTeam.PauseDefenseEvo)
        {
            redTeam.PauseDefenseEvo = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha5) && blueTeam.PauseDefenseEvo)
        {
            blueTeam.PauseDefenseEvo = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha5) && !blueTeam.PauseDefenseEvo)
        {
            blueTeam.PauseDefenseEvo = true;
        }

        /* PAUSE GOALLY PALYER EVO */
        if (Input.GetKeyDown(KeyCode.Alpha3) && redTeam.PauseGoallyEvo)
        {
            redTeam.PauseGoallyEvo = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3) && !redTeam.PauseGoallyEvo)
        {
            redTeam.PauseGoallyEvo = true;
        }
        if (Input.GetKeyDown(KeyCode.Alpha6) && blueTeam.PauseGoallyEvo)
        {
            blueTeam.PauseGoallyEvo = false;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha6) && !blueTeam.PauseGoallyEvo)
        {
            blueTeam.PauseGoallyEvo = true;
        }
    }
    private void CheckSaveStats()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            redTeam.SaveState();
            blueTeam.SaveState();
        }
    }
    private void CheckDrawDummys()
    {
        if (Input.GetKeyDown(KeyCode.W))
        {
            if (!shouldRenderDummys)
            {
                redTeam.TurnOnDummySprites();
                blueTeam.TurnOnDummySprites();
                shouldRenderDummys = true;
            }
            else
            {
                redTeam.TurnOffDummySprites();
                blueTeam.TurnOffDummySprites();
                shouldRenderDummys = false;
            }
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
        blueTeam.IncreaseTeamsFitness(1);
    }

    public void IncreaseFitnessRedTeam()
    {
        redTeam.IncreaseTeamsFitness(1);
    }
     
    public void RestartGame()
    {
        ball.Reset();
        blueTeam.Reset();
        redTeam.Reset();
    }
}
