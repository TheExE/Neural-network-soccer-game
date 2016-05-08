using UnityEngine;
using System.Collections.Generic;
using System;

public class TeamController : MonoBehaviour
{
    public tk2dTextMesh statText;
    public GameObject redTeam;
    public GameObject blueTeam;
    public struct DistanceAndIndex
    {
        public float distance;
        public int index;

        public DistanceAndIndex(float distance, int index)
        {
            this.distance = distance;
            this.index = index;
        }
    }
    public GameObject dummyAttacker;
    public GameObject dummyDefensePlayer;
    public GameObject dummyGoaly;

    private List<DefensePlayer> defensePlayers = new List<DefensePlayer>();
    private List<GoallyPlayer > goalyPlayers = new List<GoallyPlayer>();
    private List<AttackPlayer> attackPlayers = new List<AttackPlayer>();
    private List<AttackPlayer> redTeamAttackers = new List<AttackPlayer>();
    private List<AttackPlayer> blueTeamAttackers = new List<AttackPlayer>();
    private List<Genome> attackPlayersPop = new List<Genome>();
    private List<Genome> defensePlayerPop = new List<Genome>();
    private List<Genome> goalyPlayerPop = new List<Genome>();
    private GeneticAlgorithm genAlgAttackPlayers;
    private GeneticAlgorithm genAlgDefensePlayers;
    private GeneticAlgorithm genAlgGoaly;
   
    private int generationCounter = 0;
    private List<Vector2> startPosition = new List<Vector2>();
    private int curTicks = 0;

    private AttackPlayer redTeamAttackerExample;
    private DefensePlayer redTeamDefenseExample;
    private GoallyPlayer redTeamGoallyExample;
    private AttackPlayer blueTeamAttackerExmaple;
    private DefensePlayer blueTeamDefenseExmaple;
    private GoallyPlayer blueTeamGoallyExample;

    void Start()
    {
        var att = redTeam.GetComponentsInChildren<AttackPlayer>();
        foreach(AttackPlayer a in att)
        {
            if(a.NameType == GameConsts.ATTACK_PLAYER)
            {
                redTeamAttackerExample = a;
                break;
            }
        }
        redTeamDefenseExample = redTeam.GetComponentInChildren<DefensePlayer>();
        redTeamGoallyExample = redTeam.GetComponentInChildren<GoallyPlayer>();

        var attb = blueTeam.GetComponentsInChildren<AttackPlayer>();
        foreach(AttackPlayer a in attb)
        {
            if(a.NameType == GameConsts.ATTACK_PLAYER)
            {
                blueTeamAttackerExmaple = a;
                break;
            }
        }
        blueTeamDefenseExmaple = blueTeam.GetComponentInChildren<DefensePlayer>();
        blueTeamGoallyExample = blueTeam.GetComponentInChildren<GoallyPlayer>();

        InitMainTeam();
        FillTeamWithDummyPlayers();
        InitGeneticAlgorithms();
        InitStartingPositionForReset();
    }

    private void InitGeneticAlgorithms()
    {
        /* INIT GENETIC ALGORITHM */
        genAlgAttackPlayers = new GeneticAlgorithm(GameConsts.ATTACK_PLAYER_COUNT, NeuralNetworkConst.MUTATION_RATE,
           NeuralNetworkConst.CROSSOVER_RATE, attackPlayers[0].NumberOfWeights, attackPlayers[0].SplitPoints);
        genAlgGoaly = new GeneticAlgorithm(GameConsts.GOALLY_PLAYER_COUNT, NeuralNetworkConst.MUTATION_RATE,
            NeuralNetworkConst.CROSSOVER_RATE, goalyPlayers[0].NumberOfWeights, goalyPlayers[0].SplitPoints);
        genAlgDefensePlayers = new GeneticAlgorithm(GameConsts.DEFENSE_PLAYER_COUNT, NeuralNetworkConst.MUTATION_RATE,
            NeuralNetworkConst.CROSSOVER_RATE, defensePlayers[0].NumberOfWeights, defensePlayers[0].SplitPoints);

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
        statText.text = "Cur gen: " + generationCounter;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Application.targetFrameRate = -1;
        }
		else if(Input.GetKeyDown(KeyCode.F))
		{
            Application.targetFrameRate = 60;
        }

        UpdateTeam();
    }
    private void UpdateTeam()
    {
        curTicks++;
        if (curTicks < NeuralNetworkConst.MAX_TICKS || !GameManager.ShouldContinueEvolution)
        {
            /* DEFENSE PLAYERS */
            for (int i = 0; i < defensePlayers.Count; i++)
            {
                defensePlayers[i].UpdatePlayerBrains();
                genAlgDefensePlayers.
                    Population[i].Fitness = defensePlayers[i].Fitness;
            }

            /* ATTACK PLAYERS */
            for (int i = 0; i < attackPlayers.Count; i++)
            {
                attackPlayers[i].UpdatePlayerBrains();
                genAlgAttackPlayers.
                    Population[i].Fitness = attackPlayers[i].Fitness;
            }

            /* GOALY PLAYERS */
            for (int i = 0; i < goalyPlayers.Count; i++)
            {
                goalyPlayers[i].UpdatePlayerBrains();
                genAlgGoaly.
                    Population[i].Fitness = goalyPlayers[i].Fitness;
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
                defensePlayers[i].
                    PutWeights(genAlgDefensePlayers.
                    Population[i].Weights);
                defensePlayers[i].Reset();
            }

            /* ATTACK PLAYER */
            for (int i = 0; i < attackPlayers.Count; i++)
            {
                attackPlayers[i].
                    PutWeights(genAlgAttackPlayers.
                    Population[i].Weights);
                attackPlayers[i].Reset();
            }
          
            /* GOALY PLAYERS */
            for (int i = 0; i < goalyPlayers.Count; i++)
            {
                goalyPlayers[i].
                    PutWeights(genAlgGoaly.
                    Population[i].Weights);
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
        var a = GameObject.FindObjectsOfType<AttackPlayer>();
        foreach (AttackPlayer att in a)
        {
            switch (att.NameType)
            {
                case GameConsts.ATTACK_PLAYER:
                    if(att.gameObject.name.StartsWith("B"))
                    {
                        blueTeamAttackers.Add(att);
                    }
                    else
                    {
                        redTeamAttackers.Add(att);
                    }
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
        int counter = 0;
        while (defensePlayers.Count < GameConsts.DEFENSE_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyDefensePlayer) as GameObject;

            if (counter % 2 == 0)
            {
                gO.transform.parent = redTeam.transform;
                DefensePlayer defPlayer = gO.GetComponent<DefensePlayer>();
                defPlayer.oponentTeam = redTeamDefenseExample.oponentTeam;
                defPlayer.oponentGoal = redTeamDefenseExample.oponentGoal;
                defPlayer.homeGoal = redTeamDefenseExample.homeGoal;
                defPlayer.attackerPlayer = redTeamDefenseExample.attackerPlayer;
                defPlayer.InitPlayer();
                defPlayer.TeamGoally = redTeamDefenseExample.TeamGoally;
                defPlayer.OponentsAttacker = redTeamDefenseExample.OponentsAttacker;
                defensePlayers.Add(defPlayer);
            }
            else
            {
                gO.transform.parent = blueTeam.transform;
                DefensePlayer defPlayer = gO.GetComponent<DefensePlayer>();
                defPlayer.oponentTeam = blueTeamDefenseExmaple.oponentTeam;
                defPlayer.oponentGoal = blueTeamDefenseExmaple.oponentGoal;
                defPlayer.homeGoal = blueTeamDefenseExmaple.homeGoal;
                defPlayer.attackerPlayer = blueTeamDefenseExmaple.attackerPlayer;
                defPlayer.InitPlayer();
                defPlayer.TeamGoally = blueTeamDefenseExmaple.TeamGoally;
                defPlayer.OponentsAttacker = blueTeamDefenseExmaple.OponentsAttacker;
                defensePlayers.Add(defPlayer);
            }
            counter++;
        }

        counter = 0;
        /* ATTACK PLAYERS */
        while (attackPlayers.Count < GameConsts.ATTACK_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyAttacker) as GameObject;
            if (counter % 2 == 0)
            {
                gO.transform.parent = redTeam.transform;
                AttackPlayer attPlayer = gO.GetComponent<AttackPlayer>();
                attPlayer.oponentGoal = redTeamAttackerExample.oponentGoal;
                attPlayer.oponentTeam = redTeamAttackerExample.oponentTeam;
                attPlayer.InitPlayer();
                attackPlayers.Add(attPlayer);

                redTeamAttackers.Add(attPlayer);
            }
            else
            {
                gO.transform.parent = blueTeam.transform;
                AttackPlayer attPlayer = gO.GetComponent<AttackPlayer>();
                attPlayer.oponentGoal = blueTeamAttackerExmaple.oponentGoal;
                attPlayer.oponentTeam = blueTeamAttackerExmaple.oponentTeam;
                attPlayer.InitPlayer();
                attackPlayers.Add(attPlayer);

                blueTeamAttackers.Add(attPlayer);
            }

            counter++;
        }


        counter = 0;
        /* GOALY PLAYERS */
        while (goalyPlayers.Count < GameConsts.GOALLY_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyGoaly) as GameObject;

            if(counter % 2 == 0)
            {
                gO.transform.parent = redTeam.transform;
                GoallyPlayer goaly = gO.GetComponent<GoallyPlayer>();
                goaly.oponentGoal = redTeamGoallyExample.oponentGoal;
                goaly.oponentTeam = redTeamGoallyExample.oponentTeam;
                goaly.goalToSave = redTeamGoallyExample.goalToSave;
                goaly.InitPlayer();
                goalyPlayers.Add(goaly);
            }
            else
            {
                gO.transform.parent = blueTeam.transform;
                GoallyPlayer goaly = gO.GetComponent<GoallyPlayer>();
                goaly.oponentGoal = blueTeamGoallyExample.oponentGoal;
                goaly.oponentTeam = blueTeamGoallyExample.oponentTeam;
                goaly.goalToSave = blueTeamGoallyExample.goalToSave;
                goaly.InitPlayer();
                goalyPlayers.Add(goaly);
            }

            counter++;
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

    public List<AttackPlayer> BlueAttacker
    {
        get { return blueTeamAttackers; }
    }
    public List<AttackPlayer> RedAttacker
    {
        get { return redTeamAttackers; }
    }

    public float BestBlueAttacker
    {
        get
        {
            float bestFitness = 0;
            foreach (AttackPlayer a in blueTeamAttackers)
            {
                if(a.Fitness > bestFitness)
                {
                    bestFitness = a.Fitness;
                }
            }
            return bestFitness;
        }
        
    }

    public float BestRedAttacker
    {
        get
        {
            float bestFitness = 0;
            foreach (AttackPlayer a in redTeamAttackers)
            {
                if (a.Fitness > bestFitness)
                {
                    bestFitness = a.Fitness;
                }
            }
            return bestFitness;
        }

    }


    public void IncreaseTeamsFitness(float amount, bool inscreaseBlueTeam)
    {
        AttackPlayer[] team;
        if (inscreaseBlueTeam)
        {
            team = blueTeam.GetComponentsInChildren<AttackPlayer>();
        }
        else
        {
            team = redTeam.GetComponentsInChildren<AttackPlayer>();
        }
        foreach (AttackPlayer a in team)
        {
            a.Fitness += amount;
        }
    }

    public DistanceAndIndex GetClosestToBallAttackPlayer()
    {
        int index = -1;
        float bestDistance = float.MaxValue;

        for(int i = 0; i < attackPlayers.Count; i++)
        {
            if(attackPlayers[i].CurDistanceToBall > bestDistance)
            {
                bestDistance = attackPlayers[i].CurDistanceToBall;
                index = i;
            }
        }

        return new DistanceAndIndex(bestDistance, index);
    }
}

