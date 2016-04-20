using UnityEngine;
using System.Collections.Generic;

public class Neuron
{
    private int inputCount;
    private List<double> neuronWeights = new List<double>();

    public Neuron(int inputCount)
    {
        this.inputCount = inputCount;

        //+1 because of bias 
        for (int i = 0; i < inputCount+1; i++)
        {
            neuronWeights.Add(Random.Range(-1f, 1f));
        }
    }

    public int InputCount
    {
        get { return inputCount; }
    }


    public List<double> NeuronWeights
    {
        get { return neuronWeights; }
    }
}
