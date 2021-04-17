using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{

    //gameplay data

    public GameObject BulletPrefab;
    public Tank m_Enemy_tank;
    public GameObject m_arrow;
   
    public float[] m_BulletList = new float[6] ;
    public Vector2 m_Position;
    public int m_enemy = 0;
    private Vector2 m_Enemy_pos;
    Vector3 startpos;
    private int m_enemy_accuracy = 0;
    private float m_facing_angle = 0;
    private bool won = false;
    private int m_accuracy = 0;
    public float m_Angle = 1f;
    private int m_ammo = 3;
    public int m_Enemy_num;
    float[] m_LastOutput;
    bool m_shoot = true;
    GameObject mang;
    bool Switch = false;
    public int m_bulletnumb = 0;
    //wall heights
    int height = 19;
    int Wall = 40;
   
    public bool initilized = false;


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
        for (int i = 0; i < 6; i++)
            m_BulletList[i] = 0;


        Physics2D.IgnoreLayerCollision(9, 10);
        Physics2D.IgnoreLayerCollision(8, 10);
        //create rigidbody
        m_rBody = GetComponent<Rigidbody2D>();
        m_mats = new Material[transform.childCount];
       
        for (int i = 0; i < m_mats.Length; i++) { 
            m_mats[i] = transform.GetChild(i).GetComponent<Renderer>().material;
            
        
    }
        
        arrow m_Arrow = ((GameObject)Instantiate(m_arrow, transform.position , BulletPrefab.transform.rotation)).GetComponent<arrow>();
        m_Arrow.init(transform.gameObject);


        m_mats[0].SetColor("_Color", Color.white);

    }

    private void FixedUpdate()
    {
        /*
        switch (m_bulletnumb){

            case 0:
                for (int i = 0; i < 6; i++)
                    m_BulletList[i] = 0;
                break;
            case 2:
                for (int i = 2; i < 6; i++)
                    m_BulletList[i] = 0;
                break;
            case 4:
                for (int i = 4; i < 6; i++)
                    m_BulletList[i] = 0;
                break;
            default:
                break;

        } 
        */

        if (m_bulletnumb >= 6)
            m_bulletnumb = 0;

        

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



            float[] inputs = new float[16];
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
            //bullets_enemy
            inputs[10] = m_Enemy_tank.m_BulletList[0];
            inputs[11] = m_Enemy_tank.m_BulletList[1];
            inputs[12] = m_Enemy_tank.m_BulletList[2];
            inputs[13] = m_Enemy_tank.m_BulletList[3];
            inputs[14] = m_Enemy_tank.m_BulletList[4];
            inputs[15] = m_Enemy_tank.m_BulletList[5];
            
            if (m_enemy == 1)
            {
                //Debug.Log(m_Enemy_tank.transform.position.x);
               // Debug.Log(m_Enemy_tank.transform.position.y);
                //Debug.Log(m_Enemy_tank.m_BulletList[0]);
               // Debug.Log(inputs.Length);
            }
            
            float[] output = m_net.FeedForward(inputs);



            //Movement stops walking into walls and punishes
            if (transform.position.x >= Wall -0.2  || transform.position.x <= -Wall + 0.5 || transform.position.y >= height - 1.5 || transform.position.y <= -height + 2)
            {

                m_fitness--;
                //Debug.Log("Dead");
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
                m_facing_angle++;
                
            }
                else if (output[1] > -0.5)
            {
                m_rBody.MoveRotation(m_rBody.rotation - 1);
                m_facing_angle--;
            }


            //Aim

            if (output[2] > 0.25)
            {
                m_Angle++;
                
            }
            else if (output[2] < 0.25 && output[2] > -0.50)
            {
                m_Angle--;
                
                
            }








            //TODO BULLETS
            if (output[3] > 0)
            {

                //Spawn bullet projectile
                if (m_ammo > 0 && m_shoot == true)
                {
                    Bullet Bulley = ((GameObject)Instantiate(BulletPrefab, transform.position, BulletPrefab.transform.rotation)).GetComponent<Bullet>();
                    Bulley.init(m_Angle,m_enemy,transform.gameObject);
                   // Debug.Log(m_ammo);
                    m_ammo = m_ammo - 1;
                    m_shoot = false;
                    Invoke("refresh_ammo", 9.1f);
                    Invoke("timer", 3f);
                }
                else
                {

                }
            }
           

               if (won == false)
            {
                m_fitness -= 100;
            }


            m_net.SetFitness(m_fitness);

        }

    }

    void timer ()
    {
        m_shoot = true;
    }

    //todo - fix this
    public void Init(NeuralNetwork net,Vector3 sTartpos, int Enemy)
    {
        m_enemy = Enemy;
        startpos = sTartpos;
        this.m_net = net;

        initilized = true;



        if (m_enemy % 2 == 0)
        {
            transform.gameObject.layer = 8;
            Physics2D.IgnoreLayerCollision(8, 8);
            Physics2D.IgnoreLayerCollision(8, 10);
        }
        else
        {
            transform.gameObject.layer = 9;
            Physics2D.IgnoreLayerCollision(9, 9);
            Physics2D.IgnoreLayerCollision(9, 11);
        }

        Debug.Log( m_enemy);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // 
    }



    public void refresh_ammo()
    {
        m_ammo++;
    }

    public void Hit()
    {
        m_fitness -= 50;
        //Debug.Log("hitytt");
        m_mats[0].SetColor("_Color", Color.magenta);
        initilized = false;
        
    }

    public void hit_Enemy()
    {
        won = true;

        m_mats[0].SetColor("_Color", Color.green);

        m_fitness += 50;
    }

}
