using UnityEngine;
using System.Collections.Generic;

public class TeamController : MonoBehaviour
{
    private List<DefensePlayer> defensePlayers = new List<DefensePlayer>();
    private GoallyPlayer goallyPlayer;
    private AttackPlayer attackPlayer;
    private List<Genome> populationAttackPlayers = new List<Genome>();
    private List<Genome> populationGolyPlayer = new List<Genome>();
    private List<Genome> populationDefensePLayers = new List<Genome>();
    private GeneticAlgorithm genAlgAttackPlayers;
    private GeneticAlgorithm genAlgDefensePlayers;
    private GeneticAlgorithm genAlgGoaly;

    private int generationCounter;
    private int curTicks = 0;
    private tk2dTextMesh statText;
    private List<Vector2> startPosition = new List<Vector2>();

    void Start()
    {
        statText = GetComponentInChildren<tk2dTextMesh>();

        /* INIT TEAM */
        var a = GetComponentsInChildren<AttackPlayer>();
        foreach(AttackPlayer att in a)
        {
            switch (att.NameType)
            {
                case GameConsts.ATTACK_PLAYER:
                    attackPlayer = att;
                    attackPlayer.InitPlayer();
                    break;

                case GameConsts.DEFENSE_PLAYER:
                    defensePlayers.Add(att as DefensePlayer);
                    defensePlayers[defensePlayers.Count-1].InitPlayer();
                    break;

                case GameConsts.GOALLY_PLAYER:
                    goallyPlayer = att as GoallyPlayer;
                    goallyPlayer.InitPlayer();
                    break;
            }

        }

        /* INIT GENETIC ALGORITHM */
        genAlgAttackPlayers = new GeneticAlgorithm(2, NeuralNetworkConst.MUTATION_RATE,
            NeuralNetworkConst.CROSSOVER_RATE, attackPlayer.NumberOfWeights);
        genAlgGoaly = new GeneticAlgorithm(2, NeuralNetworkConst.MUTATION_RATE, 
            NeuralNetworkConst.CROSSOVER_RATE, goallyPlayer.NumberOfWeights);
        genAlgDefensePlayers = new GeneticAlgorithm(3, NeuralNetworkConst.MUTATION_RATE,
            NeuralNetworkConst.CROSSOVER_RATE, defensePlayers[0].NumberOfWeights);

        for (int i = 0; i < defensePlayers.Count; i++)
        {
            defensePlayers[i].PutWeights(genAlgDefensePlayers.Population[i].Weights);
            startPosition.Add(new Vector2(defensePlayers[i].transform.position.x, defensePlayers[i].transform.position.y));
        }
        startPosition.Add(new Vector2(goallyPlayer.transform.position.x, goallyPlayer.transform.position.y));
        goallyPlayer.PutWeights(genAlgGoaly.Population[0].Weights);
        startPosition.Add(new Vector2(attackPlayer.transform.position.x, attackPlayer.transform.position.y));
        attackPlayer.PutWeights(genAlgAttackPlayers.Population[0].Weights);
    }

    void Update()
    {
        if(Input.GetKey(KeyCode.Space))
        {
            for (int i = 0; i < 100; i++ )
            {
                UpdateTeam();
            }
        }
       
        UpdateTeam();
    }

    private void UpdateTeam()
    {
        statText.text = "Cur gen: " + generationCounter + " BestFitness Def:" + genAlgDefensePlayers.BestFitness;
        curTicks++;

        if (curTicks < NeuralNetworkConst.MAX_TICKS)
        {
            for (int i = 0; i < defensePlayers.Count; i++)
            {
                defensePlayers[i].UpdatePlayerBrains();
                genAlgDefensePlayers.Population[i].Fitness = defensePlayers[i].Fitness;
            }
            goallyPlayer.UpdatePlayerBrains();
            genAlgGoaly.Population[0].Fitness = goallyPlayer.Fitness;
            attackPlayer.UpdatePlayerBrains();
            genAlgAttackPlayers.Population[0].Fitness = attackPlayer.Fitness;
        }
        //Another generation has been completed.
        //Time to run the GA and update the sweepers with their new NNs
        else
        {
            generationCounter++;
            curTicks = 0;

            //run the GA to create a new population
            genAlgDefensePlayers.Population = genAlgDefensePlayers.Epoch(genAlgDefensePlayers.Population);
            genAlgGoaly.Population = genAlgGoaly.Epoch(genAlgGoaly.Population);
            genAlgAttackPlayers.Population = genAlgAttackPlayers.Epoch(genAlgAttackPlayers.Population);

            //put new brains into players
            for (int i = 0; i < defensePlayers.Count; i++)
            {
                defensePlayers[i].PutWeights(genAlgDefensePlayers.Population[i].Weights);
                defensePlayers[i].Reset();
            }
            goallyPlayer.PutWeights(genAlgGoaly.Population[0].Weights);
            goallyPlayer.Reset();
            attackPlayer.PutWeights(genAlgAttackPlayers.Population[0].Weights);
            attackPlayer.Reset();
           
        }
    }

    public void Reset()
    {
        for(int i = 0; i < defensePlayers.Count; i++)
        {
            defensePlayers[i].transform.position = new Vector2(startPosition[i].x, startPosition[i].y);
            defensePlayers[i].Reset();
        }
        goallyPlayer.transform.position = new Vector2(startPosition[defensePlayers.Count].x, startPosition[defensePlayers.Count].y);
        goallyPlayer.Reset();
        attackPlayer.transform.position = new Vector2(startPosition[defensePlayers.Count + 1].x, startPosition[defensePlayers.Count + 1].y);
        attackPlayer.Reset();
    }

    public GoallyPlayer Goally
    {
        get { return goallyPlayer; }
    }

    public List<DefensePlayer> DefensePlayers
    {
        get { return defensePlayers; }
    }

    public AttackPlayer Attacker
    {
        get { return attackPlayer; }
    }
    
}
