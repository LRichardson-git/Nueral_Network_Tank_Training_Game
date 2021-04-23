using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{

   
    //Prefabs for creation in game
    public GameObject m_BulletPrefab;
    public GameObject m_ArrowPrefab;

    //Reference to opponent tank in simulation
    public Tank m_Enemy_tank;

    //Gameplay Related
    public int m_enemy = 0;
    public float m_Angle = 1f;
    private float m_accuracy = 0;
    private int m_ammo = 3;
    public int m_Enemy_num;
    bool m_shoot = true;
    public int m_bulletnumb = 0;
    public bool initilized = false;
    public float m_MoveSpeed = 0.1f;
    private bool wallB = false;
    private bool moveB = false;
    private bool canwin = true;

    //Neural network related
    public float[] m_BulletList = new float[6];
    private NeuralNetwork m_net;
    //nets on right side of screen
    
    private float m_fitness = 0f;
    Vector3 Starpos;

    //Enemy Tank variables
    private float m_enemy_accuracy = 0;
    private float m_facing_angle = 0;
    private bool won = false;
    
  
    //wall heights
    int height = 19;
    int Wall = 40;
   
    //Bodyrelated
    private Rigidbody2D m_rBody;
    private Material[] m_mats;



    void Start()
    {
        //uniinitlze self so related bodies will kill self
        Invoke("dead", 30f);

        //Create and fill bullet list
        for (int i = 0; i < 6; i++)
            m_BulletList[i] = 0;

        //Ignorecollisions with own bullets
        Physics2D.IgnoreLayerCollision(9, 10);
        Physics2D.IgnoreLayerCollision(8, 10);

        //create rigidbody
        m_rBody = GetComponent<Rigidbody2D>();
        m_mats = new Material[transform.childCount];
       
        for (int i = 0; i < m_mats.Length; i++) { 
            m_mats[i] = transform.GetChild(i).GetComponent<Renderer>().material; 
        
    }
        
        //Create Arrow (red arrow where you see direction aiming)
        arrow ArrowPrefab = ((GameObject)Instantiate(m_ArrowPrefab, transform.position , transform.rotation)).GetComponent<arrow>();
        ArrowPrefab.init(transform.gameObject, m_Enemy_tank);

        //if (m_net.best == false)
        //m_mats[0].SetColor("_Color", Color.white);
        

        Starpos = transform.position;

    }

    

    private void FixedUpdate()
    {
       
        //Reset counting of bullets
        if (m_bulletnumb >= 6)
            m_bulletnumb = 0;

        


        if (initilized == true) {


            if (m_net.best == true)
            {
                m_mats[0].SetColor("_Color", Color.cyan);
            }
            //Create Iputs for neural netowrk

            float[] inputs = new float[18];

            //This tank
            inputs[0] = transform.position.x;
            inputs[1] = transform.position.y;
            inputs[2] = m_facing_angle;
            inputs[3] = m_Angle;
            inputs[4] = m_ammo;

            //enemy Tank
            inputs[5] = m_Enemy_tank.transform.position.x;
            inputs[6] = m_Enemy_tank.transform.position.y;
            inputs[7] = m_Enemy_tank.m_facing_angle;
            inputs[8] = m_Enemy_tank.m_Angle;
            inputs[8] = m_Enemy_tank.m_ammo;

            //bullets_enemy_possitions
            inputs[10] = m_Enemy_tank.m_BulletList[0];
            inputs[11] = m_Enemy_tank.m_BulletList[1];
            inputs[12] = m_Enemy_tank.m_BulletList[2];
            inputs[13] = m_Enemy_tank.m_BulletList[3];
            inputs[14] = m_Enemy_tank.m_BulletList[4];
            inputs[15] = m_Enemy_tank.m_BulletList[5];

            //Current accuracy vs Enemy accury
            inputs[16] = m_accuracy;
            inputs[17] = m_enemy_accuracy;

            
           



            //put all inputs into feed forward
            if (m_net != null)
            {
                float[] output = m_net.FeedForward(inputs);
            
            



            //punish if walk into walls
            if (transform.position.x >= Wall -0.2  || transform.position.x <= -Wall + 0.5 || transform.position.y >= height - 1.5 || transform.position.y <= -height + 2)
            {
                if (wallB == false) { 
                    m_fitness -= 10;
                    Invoke("Wallreset", 5f);
                        wallB = true;
                    }


                }

            //Move forward,backward or not at all
            else
            {
                //forward
                if (output[0] > 0)
                {

                    m_rBody.velocity = transform.right * m_MoveSpeed * Time.deltaTime;

                        if (moveB == false)
                        {
                            m_fitness += 10;
                            moveB = true;
                            Invoke("movee", 5f);
                        }


                    }
                //backward
                else if (output[0] > -0.5)
                {

                    m_rBody.velocity = transform.right * -m_MoveSpeed * Time.deltaTime;

                        if (moveB == false)
                        {
                            m_fitness += 7;
                            moveB = true;
                            Invoke("movee", 5f);
                        }
                    }
                else
                {
                    m_rBody.velocity = transform.right * 0;

                        if (moveB == false)
                        {
                            m_fitness += 4;
                            moveB = true;
                            Invoke("movee", 5f);
                        }
                    }

            }
            



            //Rotate tank or dont rotate
            if (output[1] > 0)
            {
                m_rBody.MoveRotation(m_rBody.rotation + 1);
                m_facing_angle++;
                
            }
                else if (output[1] > -0.5)
            {
                m_rBody.MoveRotation(m_rBody.rotation - 1);
                m_facing_angle--;
            }


            //Change direction of aim or dont at all
            if (output[2] > 0.25)
            {
                m_Angle++;
                
            }
            else if (output[2] < 0.25 && output[2] > -0.50)
            {
                m_Angle--;
                
                
            }


            //Shoot or not to shoot
            if (output[3] > 0)
            {

                //Spawn bullet projectile
                if (m_ammo > 0 && m_shoot == true)
                {
                    //Spawn bullet
                    Bullet Bulley = ((GameObject)Instantiate(m_BulletPrefab, transform.position, m_BulletPrefab.transform.rotation)).GetComponent<Bullet>();
                    Bulley.init(m_Angle,m_enemy,transform.gameObject);

                   //Change ammo
                    m_ammo = m_ammo - 1;
                    m_shoot = false;

                    //Timers for ammo refresh
                    Invoke("refresh_ammo", 9.1f);
                    Invoke("timer", 3f);

                }
            }
            //Set neurel net fitness to tank fitness
            m_net.SetFitness(m_fitness);

                
            
            }
        }

    }

    void timer ()
    {
        m_shoot = true;        
        if (Starpos == transform.position)
        {
            m_fitness -= 20;
        }
    }

    void Wallreset()
    {
        wallB = false;
    }

    public void addFitness(int fit)
    {
        m_fitness += fit;
    }

    void dead()
    {
        if (won == true)
            m_fitness += 120;

        initilized = false;
        m_net.SetFitness(m_fitness);
        //Debug.Log(m_fitness);
    }

    void movee()
    {
        moveB = false;
    }

    


    public void Init(NeuralNetwork net,Vector3 sTartpos, int Enemy)
    {
        m_enemy = Enemy;
        
        m_net = net;

        

        //Set defaults based on wheather or not is on left side or right side of screen
        if (m_enemy % 2 == 0)
        {
            transform.gameObject.layer = 8;
            Physics2D.IgnoreLayerCollision(8, 9);
            Physics2D.IgnoreLayerCollision(8, 8);
            Physics2D.IgnoreLayerCollision(8, 10);
            m_Angle = 180;
        }
        else
        {
            transform.gameObject.layer = 9;
            Physics2D.IgnoreLayerCollision(9, 8);
            Physics2D.IgnoreLayerCollision(9, 9);
            Physics2D.IgnoreLayerCollision(9, 11);
            
        }

        


        initilized = true;

        


    }

    public void refresh_ammo()
    {
        m_ammo++;
    }

    public void Hit()
    {
        m_fitness -= 30;
      
        m_mats[0].SetColor("_Color", Color.magenta);
        if (won == false)
            canwin = false;
        m_net.SetFitness(m_fitness);

    }

    public void hit_Enemy()
    {
        if (canwin == true) { 
            won = true;

        m_mats[0].SetColor("_Color", Color.green);
        }

        m_fitness += 60;
        m_net.SetFitness(m_fitness);
    }

    public void arrowUpdate(float Accurate)
    {
        m_accuracy = Accurate;
    }

}
