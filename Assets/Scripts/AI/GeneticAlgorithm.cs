using UnityEngine;
using System.Collections.Generic;

[SerializeField]
public class GeneticAlgorithm
{
    private List<Genome> population = new List<Genome>();
    private List<int> splitPoints = new List<int>();
    private int populationSize;
    private int chromosomeLenght;
    private double totalFitness;
    private double bestFitness = 0;
    private double worstFitness;

    private int fittestGenome;
    private double mutationRate;
    private double crossOverRate;


    public GeneticAlgorithm(int populationSize, double mutationRate, double crossOverRate, int weightCount, List<int> splitPoints)
    {
        this.populationSize = populationSize;
        this.mutationRate = mutationRate;
        this.crossOverRate = crossOverRate;
        this.chromosomeLenght = weightCount;
        this.totalFitness = 0;
        this.fittestGenome = 0;
        this.bestFitness = 0;
        this.worstFitness = int.MaxValue;
        this.splitPoints.AddRange(splitPoints);


        /* INITIALIZE POPULATION WITH RANDOM WEIGHTS */
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

            /* Determine two crossover points */
            int index1 = Random.Range(0, splitPoints.Count - 2);
            int index2 = Random.Range(index1 + 1, splitPoints.Count - 1);

            int crossPoint1 = splitPoints[index1];
            int crossPoint2 = splitPoints[index2];

            /* Create the offspring */
            for (int i = 0; i < chromosomeLenght; i++)
            {
                if (i < crossPoint1 || i >= crossPoint2)
                {
                    /* Keep the same genes if outside of crossover points */
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
        /* Traverse the chromosome and mutate each weight dependent
           on the mutation rate */
        for (int i = 0; i < chromosomeLenght; i++)
        {
            /* Does this weight gets changed */
            if (Random.value < mutationRate)
            {
                /* Add or subtract a small value to the weight */
                chromo[i] += (Random.Range(-1f, 1f) * NeuralNetworkConst.MAX_PERTURBATION);
            }
        }
    }
    private void GrabNBest(int bestCount, int copyCount, List<Genome> pop)
    {
        //Add n best players to the list
        while (bestCount > 0)
        {
            for (int i = 0; i < copyCount; i++)
            {
                pop.Add(population[populationSize - bestCount]);
            }

            bestCount--;
        }
    }
    private void CalculateBestWorstAverageTotal()
    {
        totalFitness = 0;

        double highestSoFar = 0;
        double lowestSoFar = double.MaxValue;

        for (int i = 0; i < population.Count; i++)
        {
            //find best fitness
            if (population[i].Fitness > highestSoFar)
            {
                highestSoFar = population[i].Fitness;
                fittestGenome = i;
                bestFitness = highestSoFar;
            }

            //find worst fitness
            if (population[i].Fitness < lowestSoFar)
            {
                lowestSoFar = population[i].Fitness;

                worstFitness = lowestSoFar;
            }

            totalFitness += population[i].Fitness;
        }
    }
    private void Reset()
    {
        totalFitness = 0;
        bestFitness = 0;
        worstFitness = double.MaxValue;
    }
    public void Epoch()
    {
        //reset the appropriate variables
        Reset();

        //sort the population (for scaling and elitism)
        population.Sort(Genome.Comparison);

        //calculate best, worst, average and total fitness
        CalculateBestWorstAverageTotal();

        //create a temporary vector to store new chromosones
        List<Genome> newPopulation = new List<Genome>();

        //Now to add a little elitism we shall add in some copies of the
        //fittest genomes. Make sure we add an EVEN number or the roulette
        //wheel sampling will crash
        if ((NeuralNetworkConst.NUMBER_OF_ELITE_COPYS * NeuralNetworkConst.NUMBER_OF_ELITE % 2) == 0)
        {
            GrabNBest(NeuralNetworkConst.NUMBER_OF_ELITE, NeuralNetworkConst.NUMBER_OF_ELITE_COPYS, newPopulation);
        }


        //now we enter the GA loop
        //repeat until a new population is generated
        while (newPopulation.Count < populationSize)
        {
            //grab two chromosones
            Genome mum = TournamentSelection(NeuralNetworkConst.TOURNAMENT_COMPETITIORS);
            Genome dad = TournamentSelection(NeuralNetworkConst.TOURNAMENT_COMPETITIORS);

            //create some offspring via crossover
            List<double> baby1 = new List<double>();
            List<double> baby2 = new List<double>();

            CrossoverAtSplitPoints(mum.Weights, dad.Weights, baby1, baby2);

            //now we mutate
            Mutate(baby1);
            Mutate(baby2);

            //now copy into vecNewPop population
            newPopulation.Add(new Genome(baby1, 0));
            newPopulation.Add(new Genome(baby2, 0));
        }

        //finished so assign new pop back into m_vecPop
        population = newPopulation;
    }
    Genome TournamentSelection(int n)
    {
        double bestFitnessSoFar = -999999;

        int chosenOne = 0;

        //Select N members from the population at random testing against 
        //the best found so far
        for (int i = 0; i < n; i++)
        {
            int thisTry = Random.Range(0, populationSize-1);

            if (population[thisTry].Fitness > bestFitnessSoFar)
            {
                chosenOne = thisTry;
                bestFitnessSoFar = population[thisTry].Fitness;
            }
        }

        //return the champion
        return population[chosenOne];
    }
    public List<Genome> Population
    {
        get { return population; }
        set { population = value; }
    }
    public double BestFitness
    {
        get { return bestFitness; }
    }
}
