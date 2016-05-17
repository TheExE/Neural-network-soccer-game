using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

public class TeamController : MonoBehaviour
{
    public tk2dTextMesh statText;
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
    private List<GoallyPlayer> goalyPlayers = new List<GoallyPlayer>();
    private List<AttackPlayer> attackPlayers = new List<AttackPlayer>();
    private List<Genome> attackPlayersPop = new List<Genome>();
    private List<Genome> defensePlayerPop = new List<Genome>();
    private List<Genome> goalyPlayerPop = new List<Genome>();
    private GeneticAlgorithm genAlgAttackPlayers;
    private GeneticAlgorithm genAlgDefensePlayers;
    private GeneticAlgorithm genAlgGoaly;
    private AttackPlayer attackExample;
    private DefensePlayer defenseExample;
    private GoallyPlayer goallyExample;

    private int generationCounterA = 0;
    private int generationCounterD = 0;
    private int generationCounterG = 0;

    private bool pauseGoallyEvo = false;
    private bool pauseDefenseEvo = false;
    private bool pauseAttackEvo = false;

    private List<Vector2> startPosition = new List<Vector2>();
    private int curTicks = 0;
    private string teamName;

    void Start()
    {
        if (gameObject.name.StartsWith("Red"))
        {
            teamName = "Red";
        }
        else
        {
            teamName = "Blue";
        }
        var att = GetComponentsInChildren<AttackPlayer>();
        foreach (AttackPlayer a in att)
        {
            if (a.NameType == GameConsts.ATTACK_PLAYER)
            {
                attackExample = a;
                break;
            }
        }
        defenseExample = GetComponentInChildren<DefensePlayer>();
        goallyExample = GetComponentInChildren<GoallyPlayer>();

        try
        {
            LoadStats();
        }
        catch (Exception e)
        {
            InitMainTeam();
            FillTeamWithDummyPlayers();
            InitGeneticAlgorithms();

        }
        InitStartingPositionForReset();
        WarmUp();
    }

    private void WarmUp()
    {
        int counter = 0;
        while (counter < 500)
        {
            /* DEFENSE PLAYERS */
            for (int i = 0; i < defensePlayers.Count; i++)
            {
                defensePlayers[i].UpdatePlayerBrains();
            }

            /* ATTACK PLAYERS */
            for (int i = 0; i < attackPlayers.Count; i++)
            {
                attackPlayers[i].UpdatePlayerBrains();
            }

            /* GOALY PLAYERS */
            for (int i = 0; i < goalyPlayers.Count; i++)
            {
                goalyPlayers[i].UpdatePlayerBrains();
            }
            counter++;
        }
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
        for (int i = 0; i < attackPlayers.Count; i++)
        {
            attackPlayers[i].PutWeights(genAlgAttackPlayers.Population[i].Weights);
        }
        for (int i = 0; i < goalyPlayers.Count; i++)
        {
            goalyPlayers[i].PutWeights(genAlgGoaly.Population[i].Weights);
        }
    }
    private void InitStartingPositionForReset()
    {
        for (int i = 0; i < defensePlayers.Count; i++)
        {
            startPosition.Add(new Vector2(defensePlayers[i].transform.position.x, defensePlayers[i].transform.position.y));
        }
        for (int i = 0; i < attackPlayers.Count; i++)
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
        statText.text = teamName + " gen A: " + generationCounterA + '\n'
                        + " " + teamName + " gen D: " + generationCounterD + '\n'
                        + "" + teamName + " gen G: " + generationCounterG;

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
                if (!pauseDefenseEvo)
                {
                    genAlgDefensePlayers.
                        Population[i].Fitness = defensePlayers[i].Fitness;
                }
            }

            /* ATTACK PLAYERS */
            for (int i = 0; i < attackPlayers.Count; i++)
            {
                attackPlayers[i].UpdatePlayerBrains();
                if (!pauseAttackEvo)
                {
                    genAlgAttackPlayers.
                        Population[i].Fitness = attackPlayers[i].Fitness;

                }
            }

            /* GOALY PLAYERS */
            for (int i = 0; i < goalyPlayers.Count; i++)
            {
                goalyPlayers[i].UpdatePlayerBrains();

                if (!pauseGoallyEvo)
                {
                    genAlgGoaly.
                        Population[i].Fitness = goalyPlayers[i].Fitness;

                }
            }
        }
        //Generation passed create new population
        else
        {
            curTicks = 0;

            if (!pauseDefenseEvo)
            {
                generationCounterD++;
                genAlgDefensePlayers.Epoch();
                /* DEFENSE PLAYERS */
                for (int i = 0; i < defensePlayers.Count; i++)
                {
                    defensePlayers[i].
                        PutWeights(genAlgDefensePlayers.
                        Population[i].Weights);
                    defensePlayers[i].Reset(false);
                }
            }

            if (!pauseAttackEvo)
            {
                generationCounterA++;
                genAlgAttackPlayers.Epoch();
                /* ATTACK PLAYER */
                for (int i = 0; i < attackPlayers.Count; i++)
                {
                    attackPlayers[i].
                        PutWeights(genAlgAttackPlayers.
                        Population[i].Weights);
                    attackPlayers[i].Reset(false);
                }
            }

            if (!pauseGoallyEvo)
            {
                generationCounterG++;
                genAlgGoaly.Epoch();
                /* GOALY PLAYERS */
                for (int i = 0; i < goalyPlayers.Count; i++)
                {
                    goalyPlayers[i].
                        PutWeights(genAlgGoaly.
                        Population[i].Weights);
                    goalyPlayers[i].Reset(false);
                }
            }
        }
    }
    public void Reset()
    {
        /* DEFENSE PLAYERS */
        for (int i = 0; i < defensePlayers.Count; i++)
        {
            defensePlayers[i].transform.position = new Vector2(startPosition[i].x, startPosition[i].y);
            defensePlayers[i].Reset(true);
        }
        /* ATTACK PLAYERS */
        for (int i = 0; i < attackPlayers.Count; i++)
        {
            attackPlayers[i].transform.position = new Vector2(startPosition[defensePlayers.Count + i].x,
                startPosition[defensePlayers.Count + i].y);
            attackPlayers[i].Reset(true);
        }

        /* GOALY PLAYERS */
        for (int i = 0; i < goalyPlayers.Count; i++)
        {
            goalyPlayers[i].transform.position =
                new Vector2(startPosition[defensePlayers.Count + attackPlayers.Count + i].x,
                startPosition[defensePlayers.Count + attackPlayers.Count + i].y);
            goalyPlayers[i].Reset(true);
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
                    attackPlayers[attackPlayers.Count - 1].InitPlayer();
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
            gO.transform.position = new Vector2(defenseExample.transform.position.x, defenseExample.transform.position.y);
            DefensePlayer defPlayer = gO.GetComponent<DefensePlayer>();
            defPlayer.oponentTeam = defenseExample.oponentTeam;
            defPlayer.oponentGoal = defenseExample.oponentGoal;
            defPlayer.homeGoal = defenseExample.homeGoal;
            defPlayer.attackerPlayer = attackExample;
            defPlayer.InitPlayer();
            defPlayer.TeamGoally = goallyExample;
            defPlayer.OponentsAttacker = defenseExample.OponentsAttacker;
            defensePlayers.Add(defPlayer);
        }

        /* ATTACK PLAYERS */
        while (attackPlayers.Count < GameConsts.ATTACK_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyAttacker) as GameObject;
            gO.transform.parent = transform;
            gO.transform.position = new Vector2(attackExample.transform.position.x, attackExample.transform.position.x);
            AttackPlayer attPlayer = gO.GetComponent<AttackPlayer>();
            attPlayer.oponentGoal = attackExample.oponentGoal;
            attPlayer.oponentTeam = attackExample.oponentTeam;
            attPlayer.InitPlayer();
            attackPlayers.Add(attPlayer);
        }

        /* GOALY PLAYERS */
        while (goalyPlayers.Count < GameConsts.GOALLY_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyGoaly) as GameObject;
            gO.transform.parent = transform;
            gO.transform.position = new Vector2(goallyExample.transform.position.x, goallyExample.transform.position.y);
            GoallyPlayer goaly = gO.GetComponent<GoallyPlayer>();
            goaly.oponentGoal = goallyExample.oponentGoal;
            goaly.oponentTeam = goallyExample.oponentTeam;
            goaly.goalToSave = goallyExample.goalToSave;
            goaly.InitPlayer();
            goalyPlayers.Add(goaly);
        }
    }
    public List<DefensePlayer> DefensePlayers
    {
        get { return defensePlayers; }
    }
    public List<AttackPlayer> Attackers
    {
        get { return attackPlayers; }
    }

    public float BestAttacker
    {
        get
        {
            float bestFitness = 0;
            foreach (AttackPlayer a in attackPlayers)
            {
                if (a.Fitness > bestFitness)
                {
                    bestFitness = a.Fitness;
                }
            }
            return bestFitness;
        }

    }
    public float BestGoally
    {
        get
        {
            float bestFitness = 0;
            foreach (GoallyPlayer g in goalyPlayers)
            {
                if (g.Fitness > bestFitness)
                {
                    bestFitness = g.Fitness;
                }
            }
            return bestFitness;
        }
    }
    public float BestDefense
    {
        get
        {
            float bestFitness = 0;
            foreach (DefensePlayer d in defensePlayers)
            {
                if (d.Fitness > bestFitness)
                {
                    bestFitness = d.Fitness;
                }
            }
            return bestFitness;
        }
    }

    public void IncreaseTeamsFitness(float amount)
    {
        AttackPlayer[] allplayers = GetComponentsInChildren<AttackPlayer>();
        foreach (AttackPlayer a in allplayers)
        {
            a.Fitness += amount;
        }
    }

    public AttackPlayer Attacker
    {
        get { return attackExample; }
    }

    public GoallyPlayer GoallyEx
    {
        get { return goallyExample; }
    }

    public void SaveState()
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream mAttackers = new MemoryStream();
        MemoryStream mDefense = new MemoryStream();
        MemoryStream mGoally = new MemoryStream();
        MemoryStream mAttackerGen = new MemoryStream();
        MemoryStream mDefenseGen = new MemoryStream();
        MemoryStream mGoallyGen = new MemoryStream();

        bf.Serialize(mAttackers, attackPlayers);
        bf.Serialize(mDefense, defensePlayers);
        bf.Serialize(mGoally, goalyPlayers);
        bf.Serialize(mAttackerGen, genAlgAttackPlayers);
        bf.Serialize(mDefenseGen, genAlgDefensePlayers);
        bf.Serialize(mGoallyGen, genAlgGoaly);

        mAttackers.WriteTo(File.Create(GameConsts.SAVE_A));
        mAttackerGen.WriteTo(File.Create(GameConsts.SAVE_AG));
        mDefense.WriteTo(File.Create(GameConsts.SAVE_D));
        mDefenseGen.WriteTo(File.Create(GameConsts.SAVE_DG));
        mGoally.WriteTo(File.Create(GameConsts.SAVE_G));
        mGoallyGen.WriteTo(File.Create(GameConsts.SAVE_GG));
    }
    private void LoadStats()
    {
        BinaryFormatter bf = new BinaryFormatter();
        MemoryStream mAttackers = new MemoryStream(File.ReadAllBytes(GameConsts.SAVE_A));
        MemoryStream mDefense = new MemoryStream(File.ReadAllBytes(GameConsts.SAVE_D));
        MemoryStream mGoally = new MemoryStream(File.ReadAllBytes(GameConsts.SAVE_G));
        MemoryStream mAttackerGen = new MemoryStream(File.ReadAllBytes(GameConsts.SAVE_AG));
        MemoryStream mDefenseGen = new MemoryStream(File.ReadAllBytes(GameConsts.SAVE_DG));
        MemoryStream mGoallyGen = new MemoryStream(File.ReadAllBytes(GameConsts.SAVE_GG));

        attackPlayers = (List<AttackPlayer>)bf.Deserialize(mAttackers);
        defensePlayers = (List<DefensePlayer>)bf.Deserialize(mDefense);
        goalyPlayers = (List<GoallyPlayer>)bf.Deserialize(mGoally);
        genAlgAttackPlayers = (GeneticAlgorithm)bf.Deserialize(mAttackerGen);
        genAlgDefensePlayers = (GeneticAlgorithm)bf.Deserialize(mDefenseGen);
        genAlgGoaly = (GeneticAlgorithm)bf.Deserialize(mGoallyGen);

    }
    public void TurnOnDummySprites()
    {
        foreach (AttackPlayer a in attackPlayers)
        {
            if (a.gameObject.name.StartsWith("Dum"))
            {
                a.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        foreach (DefensePlayer a in defensePlayers)
        {
            if (a.gameObject.name.StartsWith("Dum"))
            {
                a.GetComponent<MeshRenderer>().enabled = true;
            }
        }
        foreach (GoallyPlayer a in goalyPlayers)
        {
            if (a.gameObject.name.StartsWith("Dum"))
            {
                a.GetComponent<MeshRenderer>().enabled = true;
            }
        }
    }
    public void TurnOffDummySprites()
    {
        foreach (AttackPlayer a in attackPlayers)
        {
            if (a.gameObject.name.StartsWith("Dum"))
            {
                a.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        foreach (DefensePlayer a in defensePlayers)
        {
            if (a.gameObject.name.StartsWith("Dum"))
            {
                a.GetComponent<MeshRenderer>().enabled = false;
            }
        }
        foreach (GoallyPlayer a in goalyPlayers)
        {
            if (a.gameObject.name.StartsWith("Dum"))
            {
                a.GetComponent<MeshRenderer>().enabled = false;
            }
        }
    }

    public bool PauseGoallyEvo
    {
        get { return pauseGoallyEvo; }
        set { pauseGoallyEvo = value; }
    }

    public bool PauseDefenseEvo
    {
        get { return pauseDefenseEvo; }
        set { pauseDefenseEvo = value; }
    }

    public bool PauseAttackEvo
    {
        get { return pauseAttackEvo; }
        set { pauseAttackEvo = value; }
    }

}

