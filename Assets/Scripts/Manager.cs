using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SimpleJSON;
using System.IO;







public class Manager : MonoBehaviour
{

    //Prefabs for game objects 
    public GameObject TankPrefab;
    public GameObject BUlletPrefab;
    

    //Data for training and tracking
    private bool m_Training = false;
    private int m_Population_Size = 150;
    private int m_Generation = 0;

    //Set neurel network paramaters
    private int[] m_Layers = new int[] { 18, 28, 28, 4 };
    private List<NeuralNetwork> m_Nets;
    //Nets on right side of screen
    private List<NeuralNetwork> m_Nets_R;

    public List<Tank> m_TankList = null;
    private int saves;

    

    //reset Training
    void Timer()
    {

        foreach (GameObject g in FindObjectsOfType(typeof(GameObject)))
        {
            if (g.layer == 10 || g.layer == 13)
            {
                Object.Destroy(g);
            }
        }
        m_Training = false;

        //Delete all bullets in scene (they tend to be a lot in training cycles)
        

        
    }
    private void Update()
    {
        //Save and load keys
        if (Input.GetKeyDown("s"))
        {
            Save();
        }

        if (Input.GetKeyDown("l"))
        {
            //Will automatically load any save file in database
            Load();
        }


        if (m_Training == false)
        {
            //only happens once
            if (m_Generation == 0)
            {
                InitTankNeuralNetworks();
            }

            //happens every generation
            else
            {
                //loop through nets in population
                


                m_Nets.Sort();
                m_Nets_R.Sort();
                
                for (int i = 0; i < m_Population_Size / 2; i++)
                {
                    m_Nets[i].best = false;
                    m_Nets_R[i].best = false;
                }
                
                
                for (int i = 0; i < m_Population_Size / 4; i++)
                {
                    
                    //kills half the population (lowest scores)
                    //when sorted better perfoming networks end at the lower end of the sort
                    m_Nets[i] = new NeuralNetwork(m_Nets[i + (m_Population_Size / 4)]);
                    m_Nets[i].Mutate();
                    

                    //Deepcopy instead of reset function
                    m_Nets[i + (m_Population_Size / 4)] = new NeuralNetwork(m_Nets[i + (m_Population_Size / 4)]);

                    m_Nets_R[i] = new NeuralNetwork(m_Nets_R[i + (m_Population_Size / 4)]);
                    m_Nets_R[i].Mutate();


                    //Deepcopy instead of reset function
                    m_Nets_R[i + (m_Population_Size / 4)] = new NeuralNetwork(m_Nets_R[i + (m_Population_Size / 4)]);
                    
                }
                int k = 1;
                for (int i = 38; i < 55; i++)
                {
                    
                        m_Nets[i] = new NeuralNetwork(m_Nets[m_Population_Size / 2 - k]);
                        m_Nets[i].Mutate();
                        m_Nets_R[i] = new NeuralNetwork(m_Nets_R[m_Population_Size / 2 - k]);
                        m_Nets_R[i].Mutate();
                    k++;
                    
                }

               
                for (int i = 0; i < m_Population_Size / 2; i++)
                {
                    
                    //reset fitness of all nets 
                    m_Nets[i].SetFitness(0f);
                    m_Nets_R[i].SetFitness(0f);
                    
                }
            }

            //new generation 
            m_Generation++;

            //Set timer to reset training
            m_Training = true;
            Invoke("Timer", 30f);
            CreateTanks();
            
        }




       //Autosaves every 100 generations

        if( m_Generation >= 100 + saves)
        {
                Save();
        }


    }

 
    private void CreateTanks()
    {
        //wipe tank list
        if (m_TankList != null)
        {
            for (int i = 0; i < m_TankList.Count; i++)
            {
                GameObject.Destroy(m_TankList[i].gameObject);
            }

        }

        //create new tank list
        m_TankList = new List<Tank>();


        m_Nets[m_Population_Size / 2 - 1].best = true;
        m_Nets_R[m_Population_Size / 2 - 1].best = true;
        //default settings
        Vector3 Pos;
        Pos = new Vector3(-10f, 0f);
        int Enemy = 1;
        int tracker = 1;
        //Create Number of tank objects based on population size
        for (int i = 0; i < m_Population_Size / 2; i++)
        {

            //Set starting position based on assigned Number
            
                Enemy = tracker;
                tracker += 2;
           

           Pos = new Vector3(-30f, -10f);
    

            //create tank gameobject
            Tank Tanky = ((GameObject)Instantiate(TankPrefab, Pos, TankPrefab.transform.rotation)).GetComponent<Tank>();
            Tanky.Init(m_Nets[i], Pos, Enemy);
            
            //Add to tank list
            m_TankList.Add(Tanky);
        }
        tracker = 0;

        for (int i = 0; i < m_Population_Size / 2; i++)
        {

            //Set starting position based on assigned Number
            Enemy = tracker;
            tracker += 2;

            Pos = new Vector3(30f, 10f);


            //create tank gameobject
            Tank Tanky = ((GameObject)Instantiate(TankPrefab, Pos, TankPrefab.transform.rotation)).GetComponent<Tank>();
            Tanky.Init(m_Nets_R[i], Pos, Enemy);

            //Add to tank list
            m_TankList.Add(Tanky);
        }

        //loop through all tanks
        for (int i = 0; i < m_Population_Size / 2; i++)
        {
          

            
                m_TankList[i].m_Enemy_tank = m_TankList[m_Population_Size / 2 + i];
                m_TankList[m_Population_Size / 2 + i].m_Enemy_tank = m_TankList[i];

        }

        }




    void InitTankNeuralNetworks()
    {

        
        
        //population must be even if for some reason it is not default will be 20
        if (m_Population_Size % 2 != 0)
        {
            m_Population_Size = 20;
        }

        m_Nets = new List<NeuralNetwork>();
        m_Nets_R = new List<NeuralNetwork>();

        //loop through popluation and create a nerual network and add to list
        for (int i = 0; i < m_Population_Size / 2; i++)
        {
            NeuralNetwork net = new NeuralNetwork(m_Layers);
            net.Mutate();
            m_Nets.Add(net);
        }

        for (int i = 0; i < m_Population_Size / 2; i++)
        {
            NeuralNetwork net = new NeuralNetwork(m_Layers);
            net.Mutate();
            m_Nets_R.Add(net);
        }



    }

    public void Save()
    {

        //Create a Jsonobject to store savedata inside of
        JSONObject m_SaveDataJson = new JSONObject();
        Debug.Log("save");
        saves += 100;

        //Add generation number to the information in Json
        m_SaveDataJson.Add("GenerationNumber", m_Generation);
        
      

       

        //loop through population and create a copy of every current neural network
        for (int i = 0; i < m_Population_Size / 2; i++) {
           
            //Cant just Copy the Class into json so need to manually copy data into Json arrays
            

            //Two arrays created since nueron information uses a Array inside of an array
            JSONArray NueronData = new JSONArray();
            JSONArray NueronData2 = new JSONArray();
            
            //m_nerons data sweep
            for (int j = 0; j < m_Nets[i].m_layers.Length; j++)
            {

                //iterate over everyone neruon in the layer and add information to jsonarray
                for (int k = 0; k < m_Nets[i].m_neurons[j].Length; k++) {
                    NueronData2.Add(m_Nets[i].m_neurons[j][k]);
                }
                
                //Add this array of neuron information to first array which references to the current layer
                NueronData.Add(NueronData2);
                NueronData2 = new JSONArray();
            }

            JSONArray NueronData_R = new JSONArray();
            JSONArray NueronData2_R = new JSONArray();

            //m_nerons data sweep
            for (int j = 0; j < m_Nets_R[i].m_layers.Length; j++)
            {

                //iterate over everyone neruon in the layer and add information to jsonarray
                for (int k = 0; k < m_Nets_R[i].m_neurons[j].Length; k++)
                {
                NueronData2_R.Add(m_Nets_R[i].m_neurons[j][k]);
                }

        //Add this array of neuron information to first array which references to the current layer
                NueronData_R.Add(NueronData2_R);
                NueronData2_R = new JSONArray();
            }





            //saving the data on weights to jsonarray so can be saved in file correctly


            JSONArray WeightData = new JSONArray();
            JSONArray WeightData2 = new JSONArray();
            JSONArray WeightData3 = new JSONArray();


            //Loop trhough all layers
            for (int j = 1; j < m_Nets[i].m_layers.Length; j++)
            {
                int neuronsInPreviousLayer = m_Nets[i].m_layers[j - 1];

                //iterate over everyone neruon in the layer
                for (int k = 0; k < m_Nets[i].m_neurons[j].Length; k++)
                {
                    //loop through previous layer since weights are stored for the weight values of the previous neurons to the current
                    for (int v = 0; v < neuronsInPreviousLayer; v++)
                    {
                        WeightData3.Add(m_Nets[i].m_weights[j - 1 ][k][v]);
                    }
                    WeightData2.Add(WeightData3);
                    WeightData3 = new JSONArray();
                }
                WeightData.Add(WeightData2);
                WeightData2 = new JSONArray();
            }

            JSONArray WeightData_R = new JSONArray();
            JSONArray WeightData2_R = new JSONArray();
            JSONArray WeightData3_R = new JSONArray();


            //Loop trhough all layers
            for (int j = 1; j < m_Nets_R[i].m_layers.Length; j++)
            {
                int neuronsInPreviousLayer = m_Nets_R[i].m_layers[j - 1];

                //iterate over everyone neruon in the layer
                for (int k = 0; k < m_Nets_R[i].m_neurons[j].Length; k++)
                {
                    //loop through previous layer since weights are stored for the weight values of the previous neurons to the current
                    for (int v = 0; v < neuronsInPreviousLayer; v++)
                    {
                        WeightData3_R.Add(m_Nets_R[i].m_weights[j - 1][k][v]);
                    }
                    WeightData2_R.Add(WeightData3_R);
                    WeightData3_R = new JSONArray();
                }
                WeightData_R.Add(WeightData2_R);
                WeightData2_R = new JSONArray();
            }




            //Add neuron and neuron weight data to Jsonfile in accasible format
            m_SaveDataJson.Add("Neuron" + i, NueronData);
            m_SaveDataJson.Add("Weight" + i, WeightData);
            m_SaveDataJson.Add("Neuron_R" + i, NueronData_R);
            m_SaveDataJson.Add("Weight_R"+ i, WeightData_R);


        }
        
        //write all of the information to a permanent file (name based on generation)
        File.WriteAllText(Application.persistentDataPath + "/SaveData" + m_Generation + ".json", m_SaveDataJson.ToString());
            

        }

    public void Load()
    {

        //TODO - ADD some UI or something to make loading easier
        //Currently have to manually type name of data here if want to load
        Debug.Log("load");
       
      
        //Cancel invokcation of timer function
        CancelInvoke();

       

        //Loadfile from path
        string jsonString = File.ReadAllText(Application.persistentDataPath + "/SaveData1.json");

        //Parse the data into an object
        JSONObject Data = (JSONObject)JSON.Parse(jsonString);

        Debug.Log("test Data:");
        Debug.Log(Data["Weight48"].AsArray[0][0][15]);


        //Transfer Data from file into the nueral netowrk list
        for (int i = 0; i < m_Population_Size /2; i++)
        {

            for (int j = 0; j < m_Nets[i].m_layers.Length; j++)
            {
                for (int k = 0; k < m_Nets[i].m_neurons[j].Length; k++)
                {
                    //Copy neuron data
                    m_Nets[i].m_neurons[j][k] = Data["Neuron" + i].AsArray[j][k];
                }
            }

            //Transer Weigt Data into neural networks
            for (int j = 1; j < m_Nets[i].m_layers.Length; j++)
            {
                int neuronsInPreviousLayer = m_Nets[i].m_layers[j - 1];

                //iterate over everyone neruon in the layer
                for (int k = 0; k < m_Nets[i].m_neurons[j].Length; k++)
                {
                    //loop through previous layer since weights are stored for the weight values of the previous neurons to the current
                    for (int v = 0; v < neuronsInPreviousLayer; v++)
                    {
                        m_Nets[i].m_weights[j - 1][k][v] = Data["Weight" + i].AsArray[j - 1][k][v];
                    }

                }

            }

        }

        for (int i = 0; i < m_Population_Size / 2; i++)
        {

            for (int j = 0; j < m_Nets_R[i].m_layers.Length; j++)
            {
                for (int k = 0; k < m_Nets_R[i].m_neurons[j].Length; k++)
                {
                    //Copy neuron data
                    m_Nets_R[i].m_neurons[j][k] = Data["Neuron_R" + i].AsArray[j][k];
                }
            }

            //Transer Weigt Data into neural networks
            for (int j = 1; j < m_Nets_R[i].m_layers.Length; j++)
            {
                int neuronsInPreviousLayer = m_Nets_R[i].m_layers[j - 1];

                //iterate over everyone neruon in the layer
                for (int k = 0; k < m_Nets_R[i].m_neurons[j].Length; k++)
                {
                    //loop through previous layer since weights are stored for the weight values of the previous neurons to the current
                    for (int v = 0; v < neuronsInPreviousLayer; v++)
                    {
                        m_Nets_R[i].m_weights[j - 1][k][v] = Data["Weight" + i].AsArray[j - 1][k][v];
                    }

                }

            }

        }







        Debug.Log(m_Nets[1].m_weights[1][1][1]);
        Debug.Log(Data["Weight" + 1].AsArray[1][1][1]);

        //Reset scene
        Timer();



    }


}


  



// TODO -- comment everthing
//run testing
//setup playing vs best perfoming AI
//more fitneess incentives


