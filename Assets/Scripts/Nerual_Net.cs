using System;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;
public class NeuralNetwork : IComparable<NeuralNetwork>
{

    //dataStructures / arrays

    public int[] m_layers; //layers
    public float[][] m_neurons; //neuron matix
    public float[][][] m_weights; //weight matrix
    public float[][][] m_weightsDelta;
    public float m_fitness; //fitness of the network
    public bool best = false;
    float[] gamma;
    float[] m_inputs;
    float[] m_outputs;


    //initialize neural netowrk
    public NeuralNetwork(int[] layers)
    {
        //deep copy of layers of this network 
        this.m_layers = new int[layers.Length];

        //copy the reference layers information into m_layers (so when is constructed we decided how many neurons etc it has)
        for (int i = 0; i < layers.Length; i++)
        {
            this.m_layers[i] = layers[i];
        }


        //generate matrix
        InitNeurons();
        InitWeights();


       
    }


   

    //create neuron matrix
    private void InitNeurons()
    {
        //Neuron Initilization
        //Create a list to convert into jagged array
        //CREATE a list of FLOAT ARRAYS each layer has its own float array
        List<float[]> neuronsList = new List<float[]>();


        for (int i = 0; i < m_layers.Length; i++) //run through all layers to add to list
        {
            //add new float array to list based on the number of neurons in that layer
            neuronsList.Add(new float[m_layers[i]]); //add layer to neuron list
        }

        m_neurons = neuronsList.ToArray(); //convert list to array


    }

    //create weights matrix
    private void InitWeights()
    {
        //create a list of 3d float jagged arrays
        List<float[][]> weightsList = new List<float[][]>();

        List<float[][]> DeltaList = new List<float[][]>();
        // we will skip bias neruons


        //itterate over all neurons that have a weight connection
        // we start with the hidden layer (index 1) 
        for (int i = 1; i < m_layers.Length; i++)
        {

            //each layer will need its own weight matrix for its neuron
            //neurons connect to many neurons in the previous layer so we need a weight list for the previous layer and the current one since we start with the hidden layer
            //so we use the float array in the list for the previous connections between the current neuron and the previous ones
            //and they are many neurons in any layer so we use a list (to contain the neurons) and a float array to contain connection weights to the different neurons

            List<float[]> layerWeightsList = new List<float[]>(); //layer weight list for this current layer (will be converted to 2D array)
            List<float[]> layerDetlaList = new List<float[]>();
            //get number of neurons in previous layer
            int neuronsInPreviousLayer = m_layers[i - 1];

            //itterate over all neurons in this current layer
            for (int j = 0; j < m_neurons[i].Length; j++)
            {
                //an array the size of the amount of neurons in the previous layer
                float[] neuronWeights = new float[neuronsInPreviousLayer]; //neruons weights
                float[] DeltaWeights = new float[neuronsInPreviousLayer];
                //itterate over all neurons in the previous layer and set the weights randomly between 0.5f and -0.5
                for (int k = 0; k < neuronsInPreviousLayer; k++)
                {
                    //give random weights to neuron weights
                    neuronWeights[k] = UnityEngine.Random.Range(-0.5f, 0.5f);
                    DeltaWeights[k] = 0;
                }

                layerWeightsList.Add(neuronWeights); //add neuron weights of this current layer to layer weights
                layerDetlaList.Add(DeltaWeights);
            }

            //add to weight list and will have all of the weights between the first layer and the hidden layer in a list of float arrays
            weightsList.Add(layerWeightsList.ToArray()); //add this layers weights converted into 2D array into weights list
            DeltaList.Add(layerDetlaList.ToArray());
        }
        //add to m_weights e.g. m_weights [1][1][1] = 1st neuron hidden layer, 1 weight connection, value of weight connection
        m_weights = weightsList.ToArray(); //convert to 3D array
        m_weightsDelta = DeltaList.ToArray();

        
    }

 


    

    //Feedforward network with given input array
    public float[] FeedForward(float[] inputs)
    {
        m_inputs = inputs;
        ////here we will iterate through OUR imputs and put them into the neruon matrix
        for (int i = 0; i < inputs.Length; i++)
        {
            //0 = first layer , i = current neuron
            

            m_neurons[0][i] = inputs[i];
        }

        //itterate over all neurons and compute feedforward values STARTING FROM THE SECOND LAYER
        for (int i = 1; i < m_layers.Length; i++)
        {
            //iterate over everyone neruon in the layer
            for (int j = 0; j < m_neurons[i].Length; j++)
            {
                //summation of weights from previous layer
                float value = 0f;
                //now iterate over all the neurons in the previous layer 
                //(since the current target neuron in the current layer has connections to all of these previous neruson)

                for (int k = 0; k < m_neurons[i - 1].Length; k++)
                {
                    //weight matrix one layer shorter since we start from second layer
                    //j is the current neuron that we are one
                    //k is the neruon on the previous layer we then multiply by the value of the previous neuron

                    value += m_weights[i - 1][j][k] * m_neurons[i - 1][k]; //sum off all weights connections of this neuron weight their values in previous layer
                }
                //layer i  and neuron
                m_neurons[i][j] = (float)Math.Tanh(value); //Hyperbolic tangent activation (will conver this value between -1 and 1)
            }
        }
        //returning last layer aka outputlayer

       
        m_outputs = m_neurons[m_neurons.Length - 1];
        return m_neurons[m_neurons.Length - 1]; //return output layer
        
    }


    //mutation for weights based on chance

    //loop through all layers in wieght matrix and change it based on chance
    public void Mutate()
    {
        for (int i = 0; i < m_weights.Length; i++)
        {
            for (int j = 0; j < m_weights[i].Length; j++)
            {
                for (int k = 0; k < m_weights[i][j].Length; k++)
                {
                    float weight = m_weights[i][j][k];

                    //mutate weight value 
                    float randomNumber = UnityEngine.Random.Range(0f, 100f);

                    //4 diffrent types of mutations

                    //0.2% change mutation will occur etc
                    if (randomNumber <= 2f)
                    { //if 1
                      //flip sign of weight
                        weight *= -1f;
                    }
                    else if (randomNumber <= 4f)
                    { //if 2
                      //pick random weight between -1 and 1
                        weight = UnityEngine.Random.Range(-0.5f, 0.5f);
                    }
                    else if (randomNumber <= 6f)
                    { //if 3
                      //randomly increase by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f) + 1f;
                        weight *= factor;
                    }
                    else if (randomNumber <= 8f)
                    { //if 4
                      //randomly decrease by 0% to 100%
                        float factor = UnityEngine.Random.Range(0f, 1f);
                        weight *= factor;
                    }

                    m_weights[i][j][k] = weight;
                }
            }
        }
    }


    //fitness (how well the neural net is doing
    public void AddFitness(float fit)
    {
        m_fitness += fit;
    }

    public void SetFitness(float fit)
    {
        m_fitness = fit;
    }

    public float GetFitness()
    {
        return m_fitness;
    }

 
  //comapre two neural nets fitness
  //will be sorted
  //nerual net with highest fitness will be at the last index
    public int CompareTo(NeuralNetwork other)
    {
        if (other == null) return 1;

        if (m_fitness > other.m_fitness)
            return 1;
        else if (m_fitness < other.m_fitness)
            return -1;
        else
            return 0;
    }







    //deep copy neural netowrk

    public NeuralNetwork(NeuralNetwork copyNetwork)
    {
        //created copy of layers from copyNetwork
        this.m_layers = new int[copyNetwork.m_layers.Length];
        for (int i = 0; i < copyNetwork.m_layers.Length; i++)
        {
            this.m_layers[i] = copyNetwork.m_layers[i];
        }

        InitNeurons();
        InitWeights();
        //copy the weights pass in from neurla network
        CopyWeights(copyNetwork.m_weights, copyNetwork.m_weightsDelta);
        
    }

    private void CopyWeights(float[][][] copyWeights, float[][][]  DeltaW)
    {
        //iterate trhough all weights and copy weights in
        for (int i = 0; i < m_weights.Length; i++)
        {
            for (int j = 0; j < m_weights[i].Length; j++)
            {
                for (int k = 0; k < m_weights[i][j].Length; k++)
                {
                    m_weights[i][j][k] = copyWeights[i][j][k];
                }
            }
        }

               for (int i = 0; i < m_weights.Length; i++)
               {
                    for (int j = 0; j < m_weights[i].Length; j++)
                {
                            for (int k = 0; k < m_weights[i][j].Length; k++)
                        {
                         m_weightsDelta[i][j][k] = DeltaW[i][j][k];
                    }
               }
              }





     }


    // BACKPROPERGATION
    // NOT USED KINDA ADDED IT AFTER I PANICKED BUT DOSENT HELP WITH THIS TYPE OF TRAINING
    // BUT NOW I KNOW HOW TO USE IT


    public void BackProp(float[] expected, int outp)
    {
        // run over all layers backwards

        for (int i = m_layers.Length - 1; i >= 0; i--)
        {
            if (i == m_layers.Length - 1)
            {
                BackPropOutput(expected, i, outp); //back prop output
            }

            else
            {
                if (outp != 2)
                    BackPropHidden(gamma, i, outp); //back prop hidden
            }
        }

        //Update weights
        UpdateWeights();

    }

    public void UpdateWeights()
    {

        for (int i = 0; i < m_weights.Length; i++)
        {
            for (int j = 0; j < m_weights[i].Length; j++)
            {
                for (int k = 0; k < m_weights[i][j].Length; k++)
                {
                    m_weights[i][j][k] -= m_weightsDelta[i][j][k] * 0.033f;
                }
            }
        }


    }
    public void BackPropHidden(float[] gammaForward, int l, int outp)
    {

        float[] gammaF = gammaForward;
        gamma = new float[m_layers[l]];

        //UnityEngine.Debug.Log(gamma.Length);
        // UnityEngine.Debug.Log(gammaF.Length);
        // UnityEngine.Debug.Log(m_weights[l][gammaF.Length - 1][m_layers[l] - 1]);
        //  UnityEngine.Debug.Break();

        //Caluclate new gamma using gamma sums of the forward layer
        for (int i = 0; i < m_layers[l]; i++)
        {
            gamma[i] = 0;

            for (int j = 0; j < gammaF.Length; j++)
            {
                gamma[i] += gammaF[j] * m_weights[l][j][i];
            }

            gamma[i] *= TanHDer(m_neurons[l][i]);
        }

        //Caluclating detla weights

        if (l > 0)
        {
            for (int i = 0; i < m_layers[l]; i++)
            {
                for (int j = 0; j < m_layers[l - 1]; j++)
                {
                    m_weightsDelta[l - 1][i][j] = gamma[i] * m_neurons[l - 1][j];
                }

            }

        }

        else
        {
            // if (best == true)
            //    UnityEngine.Debug.Log(gamma);
            // UnityEngine.Debug.Log(gamma[1]);
            int k = 0;
            for (int i = 0; i < m_layers[l]; i++)
            {
                for (int j = 0; j < m_inputs.Length; j++)
                {
                    //UnityEngine.Debug.Log(m_weightsDelta[l][i][j]);
                    m_weightsDelta[l][i][j] = gamma[i] * m_neurons[0][j];

                }

            }
            // UnityEngine.Debug.Log(gamma);
            //UnityEngine.Debug.Log(k);
            // UnityEngine.Debug.Break();

        }

        //UnityEngine.Debug.Log("ran");
    }



   



    public void BackPropOutput(float[] expected, int l, int outp)
    {
        //Error dervative of the cost function

        float[] error;



        error = new float[m_layers[l]];
        gamma = new float[m_layers[l]];



        // UnityEngine.Debug.Log(0 + outp);
        // UnityEngine.Debug.Log(m_layers[l]);
        for (int i = 0 + outp; i < m_layers[l]; i++)
            error[i] = m_outputs[i] - expected[i];

        //Gamma calculation
        for (int i = 0 + outp; i < m_layers[l]; i++)
            gamma[i] = error[i] * TanHDer(m_outputs[i]);

        //Caluclating detla weights
        for (int i = 0 + outp; i < m_layers[l]; i++)
        {
            for (int j = 0; j < m_layers[l - 1]; j++)
            {
                if (m_weightsDelta == null)
                    UnityEngine.Debug.Log("lol");


                m_weightsDelta[l - 1][i][j] = gamma[i] * m_neurons[l - 1][j];
            }

        }
    }

    public float TanHDer(float value)
    {
        return 1 - (value * value);
    }


}