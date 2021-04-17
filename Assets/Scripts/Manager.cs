using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//saving data
[System.Serializable]
public struct SaveData
{
    public int GenerationNumber;

    
    [SerializeField]
    public List<NetData> NetDataList;

}
[System.Serializable]
public struct NetData
{
    [SerializeField]
    public int[] m_layers; //layers
    public float[][] m_neurons; //neuron matix
    public float[][][] m_weights; //weight matrix
    public float m_fitnesss;
}





public class Manager : MonoBehaviour
{

    public SaveData m_SaveData;

    //variables like training or population size
    public GameObject TankPrefab;
    public GameObject BUlletPrefab;

    public List<Tank> m_TankList = null;

    private bool m_Training = false;
    private int m_Population_Size = 50;
    private int m_Generation = 0;
    private int[] m_Layers = new int[] { 16, 22, 22, 4 };
    private List<NeuralNetwork> m_Nets;
    private bool m_Bullet_shot = false;

    private int saves;

    GameObject[] Gameobjs;
    
    //reset training after certain amount of time passes
    void Timer()
    {
        m_Training = false;
        foreach (GameObject g in FindObjectsOfType(typeof(GameObject)))
        {
            if (g.layer == 10)
            {
                Object.Destroy(g);
            }
        }

        
    }

   



    private void Update()
    {
        //move these
        if (Input.GetKeyDown("s"))
        {
            Save();
        }

        if (Input.GetKeyDown("l"))
        {
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

            m_Training = true;
            Invoke("Timer", 15f);
            CreateTanks();
            
        }




       //save after 100 generations or press S

        if( m_Generation >= 100 + saves)
        {
                Save();
        }


    }

 
    private void CreateTanks()
    {
        if (m_TankList != null)
        {
            for (int i = 0; i < m_TankList.Count; i++)
            {
                GameObject.Destroy(m_TankList[i].gameObject);
            }

        }

        m_TankList = new List<Tank>();
        
 
   

        for (int i = 0; i < m_Population_Size; i++)
        {
            
            int Enemy = 1;
            Vector3 Pos;
            Pos = new Vector3(-10f, 0f);
            //set start position and enemy value (odd and even tanks vs each other)

            
            if (i % 2 == 0)
            {
                Enemy = i + 1;
                Pos = new Vector3(-30f, 0f);
            }
            else
            {
                Enemy = i - 1;
                Pos = new Vector3(30f, 0f);
            }
            

            //create tank gameobject
           
            Tank Tanky = ((GameObject)Instantiate(TankPrefab, Pos, TankPrefab.transform.rotation)).GetComponent<Tank>();
            Tanky.Init(m_Nets[i], Pos, Enemy);
            m_TankList.Add(Tanky);

        }
        for (int i = 0; i < m_Population_Size; i++)
        {
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

        var data = JsonUtility.ToJson(m_Population_Size);
        
        //population must be even, just setting it to 20 incase it's not
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
        Debug.Log("save");
        saves += 100;
        m_SaveData.GenerationNumber = m_Generation;

        //if(m_SaveData.NetDataList == null)
            m_SaveData.NetDataList = new List<NetData>();

       ///m_SaveData.NetDataList.RemoveAll(tag => tag.m_fitnesss == NetDataList)

        for (int i = 0; i < m_Population_Size; i++) {
            var netdata = new NetData() { m_fitnesss = m_Nets[i].m_fitness, m_layers = m_Nets[i].m_layers, m_neurons = m_Nets[i].m_neurons, m_weights = m_Nets[i].m_weights };


            m_SaveData.NetDataList.Add(netdata);
    }

        //m_Nets[i + (m_Population_Size / 2)] = new NeuralNetwork(m_Nets[i + (m_Population_Size / 2)]);

        var data = JsonUtility.ToJson(m_SaveData);
        PlayerPrefs.SetString("SaveData", data);
        JsonUtility.ToJson(data);

    }

    public void Load()
    {

        Debug.Log("load");
        var data = PlayerPrefs.GetString("SaveData");
        m_SaveData = JsonUtility.FromJson<SaveData>(data);
        //CancelInvoke();
       // m_Nets = m_SaveData.NetDataList;
        Debug.Log(m_SaveData.GenerationNumber);
       // if(m_SaveData.NetDataList.

        Debug.Log(m_SaveData.NetDataList[0].m_fitnesss);
        //Timer();


        
    }


}


  



// TODO -- comment everthing
// get permanent save data
//run testing
//setup playing vs best perfoming AI



