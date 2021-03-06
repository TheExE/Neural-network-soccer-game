﻿using UnityEngine;
using System.Collections.Generic;

[SerializeField]
public class GeneticAlgorithm
{
    private List<Genome> population = new List<Genome>();
    private List<int> splitPoints = new List<int>();
    private int populationSize;
    private int chromosomeLenght;
    private double mutationRate;
    private double crossOverRate;


    public GeneticAlgorithm(int populationSize, double mutationRate, double crossOverRate, int weightCount, List<int> splitPoints)
    {
        this.populationSize = populationSize;
        this.mutationRate = mutationRate;
        this.crossOverRate = crossOverRate;
        this.chromosomeLenght = weightCount;
        this.splitPoints.AddRange(splitPoints);


        /* Create random pop */
        for (int i = 0; i < populationSize; i++)
        {
            population.Add(new Genome());

            for (int j = 0; j < chromosomeLenght; j++)
            {
                population[i].Weights.Add(Random.Range(-1f, 1f));
            }
        }
    }

    private void CrossoverAtSplitPoints(List<double> individ1, List<double> individ2, List<double> offspr1, List<double> offspr2)
    {
        if ((Random.value) > crossOverRate || individ1 == individ2)
        {
            offspr1.AddRange(individ1);
            offspr2.AddRange(individ2);
        }
        else
        {

            /* Find crossover points */
            int index1 = Random.Range(0, splitPoints.Count - 2);
            int index2 = Random.Range(index1 + 1, splitPoints.Count - 1);

            int crossPoint1 = splitPoints[index1];
            int crossPoint2 = splitPoints[index2];

            for (int i = 0; i < chromosomeLenght; i++)
            {
                if (i < crossPoint1 || i >= crossPoint2)
                {
                    offspr1.Add(individ1[i]);
                    offspr2.Add(individ2[i]);
                }
                else
                {
                    /* CROSSOVER */
                    offspr1.Add(individ2[i]);
                    offspr2.Add(individ1[i]);
                }
            }
        }
    }

    private void Mutate(List<double> chromo)
    {
        for (int i = 0; i < chromosomeLenght; i++)
        {
            if (Random.value < mutationRate)
            {
                /* Substract or add some small value */
                chromo[i] += (Random.Range(-1f, 1f) * NeuralNetworkConst.MAX_PERTURBATION);
            }
        }
    }
    private void GrabNBest(int bestCount, int copyCount, List<Genome> pop)
    {
        int counter = 1;
        while (counter >= bestCount)
        {
            for (int i = 0; i < copyCount; i++)
            {
                pop.Add(population[populationSize - counter]);
            }

            counter++;
        }
    }
    public void Epoch()
    {
        population.Sort(Genome.Comparison);

        /* TEMP list for new pop */
        List<Genome> newPopulation = new List<Genome>();

        if ((NeuralNetworkConst.NUMBER_OF_ELITE_COPYS * NeuralNetworkConst.NUMBER_OF_ELITE % 2) == 0)
        {
            GrabNBest(NeuralNetworkConst.NUMBER_OF_ELITE, NeuralNetworkConst.NUMBER_OF_ELITE_COPYS, newPopulation);
        }
        
        /* Fill the population */
        while (newPopulation.Count < populationSize)
        {
            /* Get winners */
            Genome individ1 = TournamentSelection(NeuralNetworkConst
                .TOURNAMENT_COMPETITIORS);
            Genome individ2 = TournamentSelection(NeuralNetworkConst
                .TOURNAMENT_COMPETITIORS);

            /* Crossover */
            List<double> offspr1 = new List<double>();
            List<double> offspr2 = new List<double>();
            CrossoverAtSplitPoints(individ1.Weights, individ2.Weights, offspr1, offspr2);

            /* Mutate */
            Mutate(offspr1);
            Mutate(offspr2);

            /* Add to new pop */
            newPopulation.Add(new Genome(offspr1, 0));
            newPopulation.Add(new Genome(offspr2, 0));
        }


        /* Clear old Population and add the new one */
        population.Clear();
        population.AddRange(newPopulation);
    }
    Genome TournamentSelection(int n)
    {
        double bestfitness = double.MaxValue *(-1);
        int chosenOne = 0;

        for (int i = 0; i < n; i++)
        {
            int thisTry = Random.Range(0, populationSize-1);

            if (population[thisTry].Fitness > bestfitness)
            {
                chosenOne = thisTry;
                bestfitness = population[thisTry].Fitness;
            }
        }

        return population[chosenOne];
    }
    public List<Genome> Population
    {
        get { return population; }
        set { population = value; }
    }
}
