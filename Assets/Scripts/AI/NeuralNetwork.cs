using UnityEngine;
using System.Collections.Generic;
using System;

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
		    /* FIRST HIDDEN LAYER */
	        nnLayers.Add(new NeuronLayer(neuronsPerHiddenLayer, networkInputCount));
    
            for (int i = 0; i < hiddenLayerCount-1; i++)
            {

			   nnLayers.Add(new NeuronLayer(neuronsPerHiddenLayer, neuronsPerHiddenLayer));
            }

             /* OUTPUT LAYER */
	         nnLayers.Add(new NeuronLayer(networkOutputCount, neuronsPerHiddenLayer));
	    }

      else
      {
	      /* THERE IS NO HIDDEN LAYERS SO CREATE OUTPUT LAYER */
	      nnLayers.Add(new NeuronLayer(networkOutputCount, networkInputCount));
      }
    }


    public List<double> GetWeights()
    {
        /* GETS ALL WEIGHTS IN NETWORK */

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
	
	    /* +1 Because we also want to incoporate output layer*/
	    for (int i = 0; i < hiddenLayerCount + 1; i++)
	    {

		    /* For each neuron */ 
		    for (int j = 0; j < nnLayers[i].NeuronCount; j++)
		    {
			    /* For each weight */
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

                splitPoints.Add(weightCounter);
            }
        }
        return splitPoints;
    }


    public List<double> Update(List<double> inputs)
    {
        /* Cauculate the output from a set of inputs */

	    List<double> outputs = new List<double>();
	    int weightIdx = 0;
	
	    //Check to see if correct amount of inputs is passed
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

			    int	numInputs = nnLayers[i].LayerNeurons[j].InputCount;
			
			    //sum all weights * inputs	
			    for (int k = 0; k < numInputs-1; k++)
			    {
				    netinput += nnLayers[i].LayerNeurons[j].NeuronWeights[k] * 
                        inputs[weightIdx];
                    weightIdx++;
			    }

			    //last weight is bias
			    netinput += nnLayers[i].LayerNeurons[j].
                    NeuronWeights[numInputs] * NeuralNetworkConst.BIAS;

                //get cur neuron output and store it 
			    outputs.Add(ActivationFunction(netinput, NeuralNetworkConst.ACTIVATION_TRESHOLD));
                weightIdx = 0;
		    }
	    }

	    return outputs;
    }

	private double ActivationFunction(double netinput, double response)
    {
       return  (1 / (1 + Math.Exp(-netinput / response)));
    }
}
