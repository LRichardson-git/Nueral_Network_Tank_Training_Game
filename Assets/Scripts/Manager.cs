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
                for (int i = 0; i < m_Population_Size / 2; i++)
                {

                    //kills half the population (lowest scores)
                    //when sorted better perfoming networks end at the lower end of the sort
                    m_Nets[i] = new NeuralNetwork(m_Nets[i + (m_Population_Size / 2)]);
                    m_Nets[i].Mutate();

                    //Deepcopy instead of reset function
                    m_Nets[i + (m_Population_Size / 2)] = new NeuralNetwork(m_Nets[i + (m_Population_Size / 2)]); 
                }

                for (int i = 0; i < m_Population_Size; i++)
                {
                    //reset fitness of all nets 
                    m_Nets[i].SetFitness(0f);
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
        
 
   
        //Create Number of tank objects based on population size
        for (int i = 0; i < m_Population_Size; i++)
        {
            
            int Enemy = 1;

            //Default starting position (always changed)
            Vector3 Pos;
            Pos = new Vector3(-10f, 0f);
            //set start position and enemy value (odd and even tanks vs each other)

            
            //Set starting position based on assigned Number
            if (i % 2 == 0)
            {
                Enemy = i + 1;
                Pos = new Vector3(-30f, -10f);
            }
            else
            {
                Enemy = i - 1;
                Pos = new Vector3(30f, 10f);
            }
            

            //create tank gameobject
            Tank Tanky = ((GameObject)Instantiate(TankPrefab, Pos, TankPrefab.transform.rotation)).GetComponent<Tank>();
            Tanky.Init(m_Nets[i], Pos, Enemy);
            
            //Add to tank list
            m_TankList.Add(Tanky);

        }

        //loop through all tanks
        for (int i = 0; i < m_Population_Size; i++)
        {
            //Give tank a refernce to their assigned enemy in training
            if (i % 2 == 0)
            {
                m_TankList[i].m_Enemy_tank = m_TankList[i + 1];
            }
            else
            {
                m_TankList[i].m_Enemy_tank = m_TankList[i - 1];
            }
            
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

        //loop through popluation and create a nerual network and add to list
        for (int i = 0; i < m_Population_Size; i++)
        {
            NeuralNetwork net = new NeuralNetwork(m_Layers);
            net.Mutate();
            m_Nets.Add(net);
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
        for (int i = 0; i < m_Population_Size; i++) {
           
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
           
            
            //Add neuron and neuron weight data to Jsonfile in accasible format
            m_SaveDataJson.Add("Neuron" + i, NueronData);
            m_SaveDataJson.Add("Weight" + i, WeightData);
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
        for (int i = 0; i < m_Population_Size; i++)
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


