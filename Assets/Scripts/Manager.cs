using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Manager : MonoBehaviour
{

    //variables like training or population size
    public GameObject TankPrefab;
    public GameObject EnemyPosition;

    private bool m_Training = false;
    private int m_Population_Size = 50;
    private int m_Generation = 0;
    private int[] m_Layers = new int[] { 1, 12, 12, 1 };
    private List<NeuralNetwork> m_Nets;
    private bool m_Bullet_shot = false;
    private List<Tank> m_TankList = null;

    
    //reset training after certain amount of time passes
    void Timer()
    {
        m_Training = false;
    }

    private void Update()
    {
        
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

       //Todo - UPdate information that the manager will have acces to e.g positions and stuff or maybe we handle that in tanks


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
        
        //TODO - SET Start Position to right place
        // fix initionlation

        for (int i = 0; i < m_Population_Size; i++)
        {
            Tank Tanky = ((GameObject)Instantiate(TankPrefab, new Vector3(UnityEngine.Random.Range(-10f, 10f), UnityEngine.Random.Range(-10f, 10f), 0), TankPrefab.transform.rotation)).GetComponent<Tank>();
            
            //fix initilizations
            Tanky.Init(m_Nets[i], Something);
            m_TankList.Add(Tanky);
        }

    }




    void InitTankNeuralNetworks()
    {
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
}


  






