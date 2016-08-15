using UnityEngine;
using System.Collections.Generic;
using System;

[SerializeField]
public class NeuralNetwork
{
    private int networkInputCount;
    private int networkOutputCount;
    private int hiddenLayerCount;
    private int neuronsPerHiddenLayer;
    private List<NeuronLayer> nnLayers = new List<NeuronLayer>();

    public NeuralNetwork(int networkInputCount, int networkOutputCount, int hiddenLayerCount, int neuronsPerHiddenLayer) 
    {
        this.networkInputCount = networkInputCount;
        this.networkOutputCount = networkOutputCount;
        this.hiddenLayerCount = hiddenLayerCount;
        this.neuronsPerHiddenLayer = neuronsPerHiddenLayer;
        
        CreateNetwork();
    }

    private void CreateNetwork()
    {
	    if (hiddenLayerCount > 0)
	    {
		    /* First hidLayer */
	        nnLayers.Add(new NeuronLayer(neuronsPerHiddenLayer, networkInputCount));
    
            for (int i = 0; i < hiddenLayerCount-1; i++)
            {

			   nnLayers.Add(new NeuronLayer(neuronsPerHiddenLayer, neuronsPerHiddenLayer));
            }

             /* output layer */
	         nnLayers.Add(new NeuronLayer(networkOutputCount, neuronsPerHiddenLayer));
	    }

      else
      {
           /* Just oputput layer */
	      nnLayers.Add(new NeuronLayer(networkOutputCount, networkInputCount));
      }
    }


    public List<double> GetWeights()
    { 
	    List<double> weights = new List<double>();

	    for (int i = 0; i < hiddenLayerCount + 1; i++)
	    {
		    for (int j = 0; j < nnLayers[i].NeuronCount; j++)
		    {
                for (int k = 0; k < nnLayers[i].LayerNeurons[j].InputCount; k++)
			    {
				    weights.Add(nnLayers[i].
                        LayerNeurons[j].NeuronWeights[k]);
			    }
		    }
	    }

	    return weights;
    }

    public int GetNumberOfWeights()
    {
        return GetWeights().Count;
    }

    public void PutWeights(List<double> weights)
    {
        int index = 0;

	    for (int i = 0; i < hiddenLayerCount + 1; i++)
	    {
		    for (int j = 0; j < nnLayers[i].NeuronCount; j++)
		    {
			    for (int k = 0; k < nnLayers[i].LayerNeurons[j].InputCount; k++)
			    {
				    nnLayers[i].LayerNeurons[j].NeuronWeights[k] = weights[index];
                    index++;
			    }
		    }
	    }
    }
    public List<int> CauculateSplitPoints()
    {
        List<int> splitPoints = new List<int>();
        int weightCounter = 0;

        for (int i = 0; i < hiddenLayerCount+1; i++)
        {
            for(int j = 0; j < nnLayers[i].NeuronCount; j++)
            {
                for(int k = 0; k < nnLayers[i].LayerNeurons[j].InputCount; k++)
                {
                    weightCounter++;
                }
                splitPoints.Add(weightCounter-1);
            }
        }
        return splitPoints;
    }
    public List<double> Update(List<double> inputs)
    {
	    List<double> outputs = new List<double>();
	    int weightIdx = 0;
	
	    /* Check for amount of inputs passed*/
	    if (inputs.Count != networkInputCount)
        {
		    return outputs;
        }

        for (int i = 0; i < hiddenLayerCount + 1; i++)
	    {		
		    if ( i > 0 )
            {
                inputs.Clear();
			    inputs.AddRange(outputs);
            }

		    outputs.Clear();

		   
		    for (int j = 0; j < nnLayers[i].NeuronCount; j++)
		    {
			    double netinput = 0;

			    int	numInputs = nnLayers[i].LayerNeurons[j].InputCount-1;
			
			    /* weights * inputs */
			    for (int k = 0; k < numInputs; k++)
			    {
				    netinput += nnLayers[i].LayerNeurons[j].NeuronWeights[k] * 
                        inputs[weightIdx];
                    weightIdx++;
			    }

			    /* Bias */
			    netinput += nnLayers[i].LayerNeurons[j].
                    NeuronWeights[numInputs] * NeuralNetworkConst.BIAS;

                /* Add the output from ActivationFunction to this layers output */
			    outputs.Add(ActivationFunction(netinput, NeuralNetworkConst.ACTIVATION_TRESHOLD));
                weightIdx = 0;
		    }
	    }

	    return outputs;
    }
	private double ActivationFunction(double netinput, double response)
    {
        double result = (1 / (1 + Math.Exp(-netinput / response)));
        if(result <= 0.5)
        {
            result *= -1;
        }
        else
        {
            result -= 0.5;
        }

        result *= 2;

        return result;
    }
}
