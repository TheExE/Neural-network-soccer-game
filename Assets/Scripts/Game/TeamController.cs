using UnityEngine;
using System.Collections.Generic;
using System;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

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
    public GameObject leftUpCorner;
    public GameObject leftDownCorner;
    public GameObject rightUpCorner;
    public GameObject rightDownCorner;

    private List<DefensePlayer> defensePlayers = new List<DefensePlayer>();
    private List<GoallyPlayer > goalyPlayers = new List<GoallyPlayer>();
    private List<AttackPlayer> attackPlayers = new List<AttackPlayer>();
    private List<AttackPlayer> redTeamAttackers = new List<AttackPlayer>();
    private List<DefensePlayer> redTeamDefense = new List<DefensePlayer>();
    private List<GoallyPlayer> redTeamGoally = new List<GoallyPlayer>();
    private List<AttackPlayer> blueTeamAttackers = new List<AttackPlayer>();
    private List<DefensePlayer> blueTeamDefense = new List<DefensePlayer>();
    private List<GoallyPlayer> blueTeamGoally = new List<GoallyPlayer>();
    private List<Genome> attackPlayersPop = new List<Genome>();
    private List<Genome> defensePlayerPop = new List<Genome>();
    private List<Genome> goalyPlayerPop = new List<Genome>();
    private GeneticAlgorithm genAlgAttackPlayers;
    private GeneticAlgorithm genAlgDefensePlayers;
    private GeneticAlgorithm genAlgGoaly;
   
    private int generationCounterA = 0;
    private int generationCounterD = 0;
    private int generationCounterG = 0;

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

        try
        {
            LoadStats();
        }
        catch(Exception e)
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
       for(int i = 0; i < attackPlayers.Count; i++)
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
        statText.text = "Cur gen A: " + generationCounterA + '\n'
                        + " Cur gen D: " + generationCounterD + '\n'
                        + " Cur gen G: " + generationCounterG;
        if (Input.GetKeyDown(KeyCode.Space))
        {
            Application.targetFrameRate = -1;
        }
		else if(Input.GetKeyDown(KeyCode.F))
		{
            Application.targetFrameRate = 60;
        }
        PunishForCorners();
        UpdateTeam();
    }

    private void PunishForCorners()
    {
        foreach (AttackPlayer a in attackPlayers)
        {
            if ((a.transform.position - leftDownCorner.transform.position).sqrMagnitude <= 0.25)
            {
                a.Fitness--;
            }
            else if ((a.transform.position - leftUpCorner.transform.position).sqrMagnitude <= 0.25)
            {
                a.Fitness--;
            }
            else if ((a.transform.position - rightUpCorner.transform.position).sqrMagnitude <= 0.25)
            {
                a.Fitness--;
            }
            else if ((a.transform.position - rightDownCorner.transform.position).sqrMagnitude <= 0.25)
            {
                a.Fitness--;
            }
        }
        foreach (DefensePlayer d in defensePlayers)
        {
            if ((d.transform.position - leftDownCorner.transform.position).sqrMagnitude <= 0.25)
            {
                d.Fitness--;
            }
            else if ((d.transform.position - leftUpCorner.transform.position).sqrMagnitude <= 0.25)
            {
                d.Fitness--;
            }
            else if ((d.transform.position - rightUpCorner.transform.position).sqrMagnitude <= 0.25)
            {
                d.Fitness--;
            }
            else if ((d.transform.position - rightDownCorner.transform.position).sqrMagnitude <= 0.25)
            {
                d.Fitness--;
            }
        }

        foreach (GoallyPlayer g in goalyPlayers)
        {
            if ((g.transform.position - leftDownCorner.transform.position).sqrMagnitude <= 0.25)
            {
                g.Fitness--;
            }
            else if ((g.transform.position - leftUpCorner.transform.position).sqrMagnitude <= 0.25)
            {
                g.Fitness--;
            }
            else if ((g.transform.position - rightUpCorner.transform.position).sqrMagnitude <= 0.25)
            {
                g.Fitness--;
            }
            else if ((g.transform.position - rightDownCorner.transform.position).sqrMagnitude <= 0.25)
            {
                g.Fitness--;
            }
        }
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
            curTicks = 0;
           
            if(!GameManager.ShouldPauseEvoD)
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
           
            if(!GameManager.ShouldPauseEvoA)
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
            
            if(!GameManager.ShouldPauseEvoG)
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
        for(int i = 0; i < defensePlayers.Count; i++)
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
                new Vector2(startPosition[defensePlayers.Count+attackPlayers.Count+i].x,
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
                    if (att.gameObject.name.StartsWith("B"))
                    {
                        blueTeamDefense.Add(att as DefensePlayer);
                    }
                    else
                    {
                        redTeamDefense.Add(att as DefensePlayer);
                    }
                    defensePlayers.Add(att as DefensePlayer);
                    defensePlayers[defensePlayers.Count - 1].InitPlayer();
                    break;

                case GameConsts.GOALLY_PLAYER:
                    if (att.gameObject.name.StartsWith("B"))
                    {
                        blueTeamGoally.Add(att as GoallyPlayer);
                    }
                    else
                    {
                        redTeamGoally.Add(att as GoallyPlayer);
                    }
                    goalyPlayers.Add(att as GoallyPlayer);
                    goalyPlayers[goalyPlayers.Count - 1].InitPlayer();
                    break;
            }

        }
    }
    private void FillTeamWithDummyPlayers()
    {
        Vector2 redDefensePos = redTeam.GetComponentInChildren<DefensePlayer>().transform.position;
        Vector2 blueDefensePos = blueTeam.GetComponentInChildren<DefensePlayer>().transform.position;
        /* DEFENSE PLAYERS */
        int counter = 0;
        while (defensePlayers.Count < GameConsts.DEFENSE_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyDefensePlayer) as GameObject;

            if (counter % 2 == 0)
            {
                gO.transform.parent = redTeam.transform;
                gO.transform.position = new Vector2(redDefensePos.x, redDefensePos.y);
                DefensePlayer defPlayer = gO.GetComponent<DefensePlayer>();
                defPlayer.oponentTeam = redTeamDefenseExample.oponentTeam;
                defPlayer.oponentGoal = redTeamDefenseExample.oponentGoal;
                defPlayer.homeGoal = redTeamDefenseExample.homeGoal;
                defPlayer.attackerPlayer = redTeamDefenseExample.attackerPlayer;
                defPlayer.InitPlayer();
                defPlayer.TeamGoally = redTeamDefenseExample.TeamGoally;
                defPlayer.OponentsAttacker = redTeamDefenseExample.OponentsAttacker;
                defensePlayers.Add(defPlayer);

                redTeamDefense.Add(defPlayer);
            }
            else
            {
                gO.transform.parent = blueTeam.transform;
                gO.transform.position = new Vector2(blueDefensePos.x, blueDefensePos.y);
                DefensePlayer defPlayer = gO.GetComponent<DefensePlayer>();
                defPlayer.oponentTeam = blueTeamDefenseExmaple.oponentTeam;
                defPlayer.oponentGoal = blueTeamDefenseExmaple.oponentGoal;
                defPlayer.homeGoal = blueTeamDefenseExmaple.homeGoal;
                defPlayer.attackerPlayer = blueTeamDefenseExmaple.attackerPlayer;
                defPlayer.InitPlayer();
                defPlayer.TeamGoally = blueTeamDefenseExmaple.TeamGoally;
                defPlayer.OponentsAttacker = blueTeamDefenseExmaple.OponentsAttacker;
                defensePlayers.Add(defPlayer);

                blueTeamDefense.Add(defPlayer);
            }
            counter++;
        }

        Vector2 redAttackerPos = redTeam.GetComponentInChildren<AttackPlayer>().transform.position;
        Vector2 blueAttackerPos = blueTeam.GetComponentInChildren<AttackPlayer>().transform.position;

        counter = 0;
        /* ATTACK PLAYERS */
        while (attackPlayers.Count < GameConsts.ATTACK_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyAttacker) as GameObject;
            if (counter % 2 == 0)
            {
                gO.transform.parent = redTeam.transform;
                gO.transform.position = new Vector2(redAttackerPos.x, redAttackerPos.y);
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
                gO.transform.position = new Vector2(blueAttackerPos.x, blueAttackerPos.y);
                AttackPlayer attPlayer = gO.GetComponent<AttackPlayer>();
                attPlayer.oponentGoal = blueTeamAttackerExmaple.oponentGoal;
                attPlayer.oponentTeam = blueTeamAttackerExmaple.oponentTeam;
                attPlayer.InitPlayer();
                attackPlayers.Add(attPlayer);

                blueTeamAttackers.Add(attPlayer);
            }

            counter++;
        }


        Vector2 redGoallyPos = redTeam.GetComponentInChildren<GoallyPlayer>().transform.position;
        Vector2 blueGoallyPos = blueTeam.GetComponentInChildren<GoallyPlayer>().transform.position;
        counter = 0;
        /* GOALY PLAYERS */
        while (goalyPlayers.Count < GameConsts.GOALLY_PLAYER_COUNT)
        {
            var gO = Instantiate(dummyGoaly) as GameObject;

            if(counter % 2 == 0)
            {
                gO.transform.parent = redTeam.transform;
                gO.transform.position = new Vector2(redGoallyPos.x, redGoallyPos.y);
                GoallyPlayer goaly = gO.GetComponent<GoallyPlayer>();
                goaly.oponentGoal = redTeamGoallyExample.oponentGoal;
                goaly.oponentTeam = redTeamGoallyExample.oponentTeam;
                goaly.goalToSave = redTeamGoallyExample.goalToSave;
                goaly.InitPlayer();
                goalyPlayers.Add(goaly);

                redTeamGoally.Add(goaly);
            }
            else
            {
                gO.transform.parent = blueTeam.transform;
                gO.transform.position = new Vector2(blueGoallyPos.x, blueGoallyPos.y);
                GoallyPlayer goaly = gO.GetComponent<GoallyPlayer>();
                goaly.oponentGoal = blueTeamGoallyExample.oponentGoal;
                goaly.oponentTeam = blueTeamGoallyExample.oponentTeam;
                goaly.goalToSave = blueTeamGoallyExample.goalToSave;
                goaly.InitPlayer();
                goalyPlayers.Add(goaly);

                blueTeamGoally.Add(goaly);
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
                if (a.Fitness > bestFitness)
                {
                    bestFitness = a.Fitness;
                }
            }
            return bestFitness;
        }

    }
    public float BestBlueGoally
    {
        get
        {
            float bestFitness = 0;
            foreach (GoallyPlayer g in blueTeamGoally)
            {
                if (g.Fitness > bestFitness)
                {
                    bestFitness = g.Fitness;
                }
            }
            return bestFitness;
        }
    }
    public float BestBlueDefense
    {
        get
        {
            float bestFitness = 0;
            foreach (DefensePlayer d in blueTeamDefense)
            {
                if (d.Fitness > bestFitness)
                {
                    bestFitness = d.Fitness;
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
    public float BestRedGoally
    {
        get
        {
            float bestFitness = 0;
            foreach (GoallyPlayer g in redTeamGoally)
            {
                if (g.Fitness > bestFitness)
                {
                    bestFitness = g.Fitness;
                }
            }
            return bestFitness;
        }
    }
    public float BestRedDefense
    {
        get
        {
            float bestFitness = 0;
            foreach (DefensePlayer d in redTeamDefense)
            {
                if (d.Fitness > bestFitness)
                {
                    bestFitness = d.Fitness;
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
    public AttackPlayer BlueTeamAttacker
    {
        get { return blueTeamAttackerExmaple; }
    }
    public AttackPlayer RedTeamAttacker
    {
        get { return redTeamAttackerExample; }
    }
    public GoallyPlayer RedTeamGoally
    {
        get { return redTeamGoallyExample; }
    }
    public GoallyPlayer BlueTeamGoally
    {
        get { return blueTeamGoallyExample; }
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

}

