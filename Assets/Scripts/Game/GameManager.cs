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
    private float timer = 0;
    private static bool shouldSpeedUp = false;

	void Start () 
    {
        Application.runInBackground = true;
        redTeamTxt = redTeamScoreObj.GetComponent<tk2dTextMesh>();
        blueTeamTxt = blueTeamScoreObj.GetComponent<tk2dTextMesh>();
        ball = GameObject.FindObjectOfType<BallScript>();
        teamControllers = GameObject.FindObjectsOfType<TeamController>();
        fpsText = GetComponentInChildren<tk2dTextMesh>();
	}
	
	void Update () 
    {
        fpsText.text = "Fps:" + Mathf.RoundToInt(1 / Time.deltaTime);
        redTeamTxt.text = "Score: " + redTeamScore;
        blueTeamTxt.text = "Score: " + blueTeamScore;

        timer += Time.deltaTime;
        if(timer >= 4)
        {
            timer = 0;
            TeamController teamOne = teamControllers[0];
            TeamController teamTwo = teamControllers[1];
            TeamController.ValueAndIndex attacker1Score = teamOne.BestAttackerSqrtMagnitudeToBall();
            TeamController.ValueAndIndex attacker2Score = teamTwo.BestAttackerSqrtMagnitudeToBall();

            if( attacker1Score.value < attacker2Score.value)
            {
                teamOne.Attacker[attacker1Score.index].Fitness++;
            }
            else
            {
                teamTwo.Attacker[attacker2Score.index].Fitness++;
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            shouldSpeedUp = true;
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            shouldSpeedUp = false;
        }

        if (shouldSpeedUp)
        {
            for (int i = 0; i < NeuralNetworkConst.MAX_TICKS; i++)
            {
                teamControllers[0].UpdateTeam();
                teamControllers[1].UpdateTeam();
                ball.UpdateBall();
            }
        }
        else
        {
            teamControllers[0].UpdateTeam();
            teamControllers[1].UpdateTeam();
            ball.UpdateBall();
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
}
