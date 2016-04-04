using UnityEngine;
using System.Collections.Generic;

public class Neuron
{
    List<double> neuronWeights = new List<double>();


    public Neuron(int inputCount)
    {
        //+1 because of bias 
        for (int i = 0; i < inputCount+1; i++)
        {
            neuronWeights.Add(Random.Range(-1f, 1f));
        }
    }

    public List<double> NeuronWeights
    {
        get { return neuronWeights; }
    }
}
