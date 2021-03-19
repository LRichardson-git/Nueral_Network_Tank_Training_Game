using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{

    //gameplay data

    public Vector2 m_Position;
    private int m_enemy;
    private Vector2 m_Enemy_pos;
    Vector3 startpos;
    private int m_enemy_accuracy = 0;

    private bool won = false;
    private int m_accuracy = 0;
    private float m_Angle = 0f;
    private int m_ammo = 3;
    private int m_Enemy_num;
    float[] m_LastOutput;
    GameObject mang;
    bool Switch = false;

    //wall heights
    int height = 19;
    int Wall = 40;

    private bool initilized = false;
   

    private NeuralNetwork m_net;
    private Rigidbody2D m_rBody;
    private Material[] m_mats;
    private Vector3 m_EulerAngleVelocity;
    //nerual net related
    private float m_fitness = 0f;
    
    //tank related
    public float m_MoveSpeed = 0.1f;

    void Start()
    {
        //create rigidbody
        m_rBody = GetComponent<Rigidbody2D>();
        m_mats = new Material[transform.childCount];
       
        for (int i = 0; i < m_mats.Length; i++) { 
            m_mats[i] = transform.GetChild(i).GetComponent<Renderer>().material;
            
        
    }
        
       // m_rBody.isKinematic = false;
        m_rBody.gravityScale = 0;
        
}

    private void FixedUpdate()
    {

        //update enemy position
        if (initilized == true) {

            m_Position = transform.position;

            mang = GameObject.Find("Manager");
            m_Enemy_pos = mang.GetComponent<Manager>().m_TankList[m_enemy].m_Position;

            //x and y values to go in
            float e_x = m_Enemy_pos.x;
            float e_y = m_Enemy_pos.y;

            //TODO -add bullets and accuracy

            //update likelyhood of being hit and likelyhood to hit opponent
            m_enemy_accuracy = mang.GetComponent<Manager>().m_TankList[m_enemy].m_accuracy;



            float[] inputs = new float[8];
            m_Angle += 1;
            //current position
            inputs[0] = transform.position.x;
            inputs[1] = transform.position.y;
            //Gun facing 
            inputs[2] = m_Angle;
            inputs[3] = m_accuracy;
            inputs[4] = m_ammo;
            inputs[5] = m_Enemy_pos.x;
            inputs[6] = m_Enemy_pos.y;
            inputs[7] = m_enemy_accuracy;
            


            float[] output = m_net.FeedForward(inputs);



            //Movement stops walking into walls and punishes
            if (transform.position.x >= Wall || transform.position.x <= -Wall || transform.position.y >= height || transform.position.y <= -height)
            {

                m_fitness--;
                Debug.Log("Dead");
                m_rBody.position = startpos;
                won = false;
            }
            //Move forward,backward or not at all
            else
            {
                //forward
                if (output[0] > 0)
                {

                    m_rBody.velocity = transform.right * m_MoveSpeed * Time.deltaTime;
                }
                //backward
                else if (output[0] > -0.5)
                {

                    m_rBody.velocity = transform.right * -m_MoveSpeed * Time.deltaTime;
                }
                else
                {
                    m_rBody.velocity = transform.right * 0;
                }

            }




            //Rotate tank
            if (output[1] > 0)
            {
                m_rBody.MoveRotation(m_rBody.rotation + 1);
            }
                else if (output[1] > -0.5)
            {
                m_rBody.MoveRotation(m_rBody.rotation - 1);
            }


            //TODO BULLETS
                if (output[2] > 0)
            {
                //Spawn bullet projectile
                if (m_ammo > 0)
                {



                    if(e_x > transform.position.x -1 && e_x < transform.position.x + 1 && e_y > transform.position.y)
                    {
                        m_fitness += 1000;
                        won = true;
                        for (int i = 0; i < m_mats.Length; i++)
                            m_mats[i].color = new Color(0f,0f,20f);

                        for (int i = 0; i < m_mats.Length; i++)
                            mang.GetComponent<Manager>().m_TankList[m_enemy].m_mats[i].color = new Color(255f, 0f, 20f);
                        
                    }
                    //fire
                    m_fitness++;
                    Invoke("refresh_ammo", 3f);
                }
                else if (m_ammo ==0) {
                    m_fitness--;
                }
            }
           

               if (won == false)
            {
                m_fitness -= 100;
            }

                if (m_rBody.position == startpos)
            {
                m_fitness -= 10;
            }


            m_net.AddFitness(m_fitness);

        }
    }

    //todo - fix this
    public void Init(NeuralNetwork net,Vector3 sTartpos, int Enemy)
    {
        m_enemy = Enemy;
        startpos = sTartpos;
        this.m_net = net;

        initilized = true;
    }


    public void refresh_ammo()
    {
        m_ammo++;
    }

}
