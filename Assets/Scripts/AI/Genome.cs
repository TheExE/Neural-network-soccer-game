using UnityEngine;
using System.Collections.Generic;

public class Genome
{
    private List<double> weights;
	private double fitness;

	public Genome()
    {
        fitness = 0;
        weights = new List<double>();
    }

	public Genome(List<double> weights, double fitness)
    {
        this.weights = weights;
        this.fitness = fitness;
    }

   
	public double Fitness
    {
        get { return fitness; }
        set { fitness = value; }
    }

    public List<double> Weights
    {
        get { return weights; }
    }


    public static bool operator <(Genome g1, Genome g2)
    {
        return g1.fitness < g2.fitness;
    }
    public static bool operator >(Genome g1, Genome g2)
    {
        return g2.fitness < g1.fitness;
    }
    public static int Comparison(Genome g1, Genome g2)
    {
        if (g1.Fitness < g2.Fitness)
        {
            return -1;
        }
        else if (g1.Fitness == g2.Fitness)
        {
            return 0;
        }
        else if (g1.Fitness > g2.Fitness)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }

}
