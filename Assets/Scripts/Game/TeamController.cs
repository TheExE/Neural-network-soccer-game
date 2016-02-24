using UnityEngine;
using System.Collections.Generic;
using System;

public class TeamController : MonoBehaviour
{
    public GameObject dummyAttacker;
    public GameObject dummyDefensePlayer;
    public GameObject dummyGoaly;

    private List<DefensePlayer> defensePlayers = new List<DefensePlayer>();
    private List<GoallyPlayer > goalyPlayers = new List<GoallyPlayer>();
    private List<AttackPlayer> attackPlayers = new List<AttackPlayer>();
    private List<Genome> attackPlayersPop = new List<Genome>();
    private List<Genome> defensePlayerPop = new List<Genome>();
    private List<Genome> goalyPlayerPop = new List<Genome>();
    private GeneticAlgorithm genAlgAttackPlayers;
    private GeneticAlgorithm genAlgDefensePlayers;
    private GeneticAlgorithm genAlgGoaly;
	private bool shouldSpeedUp = false;

   
    private int generationCounter;
    private int curTicks = 0;
    private tk2dTextMesh statText;
    private List<Vector2> startPosition = new List<Vector2>();

    void Start()
    {
        statText = GetComponentInChildren<tk2dTextMesh>();
        InitMainTeam();
        FillTeamWithDummyPlayers();
        InitGeneticAlgorithms();
        InitStartingPositionForReset();
    }

    private void InitGeneticAlgorithms()
    {
        /* INIT GENETIC ALGORITHM */
        genAlgAttackPlayers = new GeneticAlgorithm(GameConsts.ATTACK_PLAYER_COUNT, NeuralNetworkConst.MUTATION_RATE,
           NeuralNetworkConst.CROSSOVER_RATE, attackPlayers[0].NumberOfWeights);
        genAlgGoaly = new GeneticAlgorithm(GameConsts.GOALLY_PLAYER_COUNT, NeuralNetworkConst.MUTATION_RATE,
            NeuralNetworkConst.CROSSOVER_RATE, goalyPlayers[0].NumberOfWeights);
        genAlgDefensePlayers = new GeneticAlgorithm(GameConsts.DEFENSE_PLAYER_COUNT, NeuralNetworkConst.MUTATION_RATE,
            NeuralNetworkConst.CROSSOVER_RATE, defensePlayers[0].NumberOfWeights);

        for (int i = 0; i < defensePlayers.Count; i++)
        {
            defensePlayers[i].PutWeights(genAlgDefensePlayers.Population[i].Weights);
        }
        for(int i = 0; i < attackPlayers.Count; i++)
        {
            attackPlayers[i].PutWeights(genAlgAttackPlayers.Population[i].Weights);
        }
        for(int i = 0; i < goalyPlayers.Count; i++)
        {
            goalyPlayers[i].PutWeights(genAlgGoaly.Population[i].Weights);
        }
    }
    private void InitStartingPositionForReset()
    {
       for(int i = 0; i < defensePlayers.Count; i++)
       {
            startPosition.Add(new Vector2(defensePlayers[i].transform.position.x, defensePlayers[i].transform.position.y));
       }
       for(int i =0; i < attackPlayers.Count; i++)
       {
            startPosition.Add(new Vector2(attackPlayers[i].transform.position.x, attackPlayers[i].transform.position.y));
       }
       for (int i = 0; i < goalyPlayers.Count; i++)
       {
            startPosition.Add(new Vector2(goalyPlayers[i].transform.position.x, goalyPlayers[i].transform.position.y));
       }
    }

    void Update()
    {
        if(statText != null)
        {
            statText.text = "Cur gen: " + generationCounter +
       " BestFitness Def: " + Mathf.Round((float)(genAlgDefensePlayers.BestFitness));
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
			shouldSpeedUp = true;
        }
		else if(Input.GetKeyDown(KeyCode.F))
		{
			shouldSpeedUp = false;
		}
		
		if(shouldSpeedUp)
		{
			for (int i = 0; i < 200; i++ )
            {
                UpdateTeam();
            }
		}
       
        UpdateTeam();
    }
    private void UpdateTeam()
    {
        curTicks++;

        if (curTicks < NeuralNetworkConst.MAX_TICKS)
        {
            /* DEFENSE PLAYERS */
            for (int i = 0; i < defensePlayers.Count; i++)
            {
                defensePlayers[i].UpdatePlayerBrains();
                genAlgDefensePlayers.Population[i].Fitness = defensePlayers[i].Fitness;
            }

            /* ATTACK PLAYERS */
            for (int i = 0; i < attackPlayers.Count; i++)
            {
                attackPlayers[i].UpdatePlayerBrains();
                genAlgAttackPlayers.Population[i].Fitness = attackPlayers[i].Fitness;
            }

            /* GOALY PLAYERS */
            for (int i = 0; i < goalyPlayers.Count; i++)
            {
                goalyPlayers[i].UpdatePlayerBrains();
                genAlgGoaly.Population[i].Fitness = goalyPlayers[i].Fitness;
            }
        }
        //Generation passed create new population
        else
        {
            generationCounter++;
            curTicks = 0;

            genAlgDefensePlayers.Epoch();
            genAlgGoaly.Epoch();
            genAlgAttackPlayers.Epoch();

            /* DEFENSE PLAYERS */
            for (int i = 0; i < defensePlayers.Count; i++)
            {
                defensePlayers[i].PutWeights(genAlgDefensePlayers.Population[i].Weights);
                defensePlayers[i].Reset();
            }

            /* ATTACK PLAYER */
            for (int i = 0; i < attackPlayers.Count; i++)
            {
                attackPlayers[i].PutWeights(genAlgAttackPlayers.Population[i].Weights);
                attackPlayers[i].Reset();
            }
          
            /* GOALY PLAYERS */
            for (int i = 0; i < goalyPlayers.Count; i++)
            {
                goalyPlayers[i].PutWeights(genAlgGoaly.Population[i].Weights);
                goalyPlayers[i].Reset();
            }

        }
    }

    public void Reset()
    {
        /* DEFENSE PLAYERS */
        for(int i = 0; i < defensePlayers.Count; i++)
        {
            defensePlayers[i].transform.position = new Vector2(startPosition[i].x, startPosition[i].y);
            defensePlayers[i].Reset();
        }
        /* ATTACK PLAYERS */
        for (int i = 0; i < attackPlayers.Count; i++)
        {
            attackPlayers[i].transform.position = new Vector2(startPosition[defensePlayers.Count + i].x,
                startPosition[defensePlayers.Count + i].y);
            attackPlayers[i].Reset();
        }

        /* GOALY PLAYERS */
        for (int i = 0; i < goalyPlayers.Count; i++)
        {
            goalyPlayers[i].transform.position = 
                new Vector2(startPosition[defensePlayers.Count+attackPlayers.Count+i].x,
                startPosition[defensePlayers.Count + attackPlayers.Count + i].y);
            goalyPlayers[i].Reset();
        }
    }
    public List<GoallyPlayer> Goally
    {
        get { return goalyPlayers; }
    }
    private void InitMainTeam()
    {
        /* INIT TEAM */
        var a = GetComponentsInChildren<AttackPlayer>();
        foreach (AttackPlayer att in a)
        {
            switch (att.NameType)
            {
                case GameConsts.ATTACK_PLAYER:
                    attackPlayers.Add(att);
                    attackPlayers[attackPlayers.Count-1].InitPlayer();
                    break;

                case GameConsts.DEFENSE_PLAYER:
                    defensePlayers.Add(att as DefensePlayer);
                    defensePlayers[defensePlayers.Count - 1].InitPlayer();
                    break;

                case GameConsts.GOALLY_PLAYER:
                    goalyPlayers.Add(att as GoallyPlayer);
                    goalyPlayers[goalyPlayers.Count - 1].InitPlayer();
                    break;
            }

        }
    }
    private void FillTeamWithDummyPlayers()
    {

        /* DEFENSE PLAYERS */
        while (defensePlayers.Count < GameConsts.DEFENSE_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyDefensePlayer) as GameObject;
            gO.transform.parent = transform;
            DefensePlayer defPlayer = gO.GetComponent<DefensePlayer>();
            defPlayer.oponentTeam = defensePlayers[0].oponentTeam;
            defPlayer.oponentGoal = defensePlayers[0].oponentGoal;
            defPlayer.homeGoal = defensePlayers[0].homeGoal;
            defPlayer.attackerPlayer = defensePlayers[0].attackerPlayer;
            defPlayer.InitPlayer();
            defensePlayers.Add(defPlayer);
        }

        /* ATTACK PLAYERS */
        while (attackPlayers.Count < GameConsts.ATTACK_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyAttacker) as GameObject;
            gO.transform.parent = transform;
            AttackPlayer attPlayer = gO.GetComponent<AttackPlayer>();
            attPlayer.oponentGoal = attackPlayers[0].oponentGoal;
            attPlayer.oponentTeam = attackPlayers[0].oponentTeam;
            attPlayer.InitPlayer();
            attackPlayers.Add(attPlayer);
        }
        
        /* GOALY PLAYERS */
        while (goalyPlayers.Count < GameConsts.GOALLY_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyGoaly) as GameObject;
            gO.transform.parent = transform;
            GoallyPlayer goaly = gO.GetComponent<GoallyPlayer>();
            goaly.oponentGoal = goalyPlayers[0].oponentGoal;
            goaly.oponentTeam = goalyPlayers[0].oponentTeam;
            goaly.goalToSave = goalyPlayers[0].goalToSave;
            goaly.InitPlayer();
            goalyPlayers.Add(goaly);
        }
    }
    public List<DefensePlayer> DefensePlayers
    {
        get { return defensePlayers; }
    }
    public List<AttackPlayer> Attacker
    {
        get { return attackPlayers; }
    }
    public void IncreaseTeamsFitness(float amount)
    {
        /* DEFENSE PLAYERS */
        for (int i = 0; i < defensePlayers.Count; i++)
        {
            defensePlayers[i].Fitness += amount;
        }
        /* ATTACK PLAYERS */
        for (int i = 0; i < attackPlayers.Count; i++)
        {
            attackPlayers[i].Fitness += amount;
        }

        /* GOALY PLAYERS */
        for (int i = 0; i < goalyPlayers.Count; i++)
        {
            goalyPlayers[i].Fitness += amount;
        }
    }
}

