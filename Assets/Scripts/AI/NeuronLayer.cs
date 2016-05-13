using UnityEngine;
using System.Collections.Generic;

[SerializeField]
public class NeuronLayer
{
    private int neuronCount;
    private List<Neuron> layerOfNeurons = new List<Neuron>();


    public NeuronLayer(int neuronCount, int neuronInputCount)
    {
        this.neuronCount = neuronCount;

        for (int i = 0; i < neuronCount; i++)
        {
            layerOfNeurons.Add(new Neuron(neuronInputCount));
        }
    }

    public int NeuronCount
    {
        get { return neuronCount; }
    }

    public List<Neuron> LayerNeurons
    {
        get { return layerOfNeurons; }
    }
}
