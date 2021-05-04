using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{


    //Prefabs for creation in game
    public GameObject m_BulletPrefab;
    public GameObject m_ArrowPrefab;
    public GameObject m_directionPreab;
    public GameObject m_directionBulletPreab;

    //Reference to opponent tank in simulation
    public Tank m_Enemy_tank;

    //Gameplay Related
    public int m_enemy = 0;
    public float m_Angle = 1f;
    private float m_accuracy = 0;
    public int m_Enemy_num;
    bool m_shoot = true;
    public int m_bulletnumb = 0;
    public bool initilized = false;
    public float m_MoveSpeed = 0.1f;
    private bool wallB = false;
    private bool moveB = false;
    private bool canwin = true;
    private bool m_left = true;
    public float m_angle_arrow = 0;
    private Vector3 oldpos;

    //Neural network related
    public float[] m_BulletList = new float[6];
    private NeuralNetwork m_net;
    bool touchW = false;
    public bool player = false;

    //nets on right side of screen
    private float m_fitness = 0f;
    Vector3 Starpos;
    float distance;
    public float m_Enemy_angle;

    //backprop
    public int dotc = 0;
 
    //Bullet related
    public Vector3 m_BulletPos;
    public bool BulletY = false;
    public float bulletA;

    //Enemy Tank variables
    public float m_facing_angle = 0;
    private bool won = false;
    private bool once = false;
    public bool pl = false;



    //Bodyrelated
    private Rigidbody2D m_rBody;
    private Material[] m_mats;



    void Start()
    {
        //uniinitlze self so related bodies will kill self

       // if (pl == false){
            Invoke("dead", 30f);
      //  }

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
        arrow ArrowPrefab = ((GameObject)Instantiate(m_ArrowPrefab, transform.position, transform.rotation)).GetComponent<arrow>();
        ArrowPrefab.init(transform.gameObject, m_Enemy_tank);

        direction dirPrefab = ((GameObject)Instantiate(m_directionPreab, transform.position, transform.rotation)).GetComponent<direction>();
        dirPrefab.init(transform.gameObject, m_Enemy_tank);

        

        //if (m_net.best == false)
        //m_mats[0].SetColor("_Color", Color.white);


        Starpos = transform.position;

        oldpos = transform.position;


    }



    private void FixedUpdate()
    {

        //Reset counting of bullets
        if (m_bulletnumb >= 6)
            m_bulletnumb = 0;


        //m_net.delta();

       if (player == false) {

        if (initilized == true) {

            if (once == false)
            {
                m_rBody.rotation = m_Angle;
                once = true;
                m_fitness = 0;
            }

            distance = Vector3.Distance(m_Enemy_tank.transform.position, transform.position);
            //Create Iputs for neural netowrk

            float[] inputs = new float[35];
       
            //Vision of enemy Tank
            float[] temp = Look(0, 0);
            inputs[0] = temp[0];
            temp = Look(1, 0);
            inputs[1] = temp[0];
             temp = Look(2, 0);
            inputs[2] = temp[0];
             temp = Look(3, 0);
            inputs[3] = temp[0];
             temp = Look(4, 0);
            inputs[4] = temp[0];
             temp = Look(5, 0);
            inputs[5] = temp[0];
             temp = Look(6, 0);
            inputs[6] = temp[0];
             temp = Look(7, 0);
            inputs[7] = temp[0];
             temp = Look(8, 0);
            inputs[8] = temp[0];
             temp = Look(9, 0);
            inputs[9] = temp[0];
             temp = Look(10, 0);
            inputs[10] = temp[0];
            temp = Look(11, 0);
            inputs[11] = temp[0];
             temp = Look(12, 0);
            inputs[12] = temp[0];
             temp = Look(13, 0);
            inputs[13] = temp[0];
            temp = Look(14, 0);
            inputs[14] = temp[0];
            temp = Look(15, 0);
            inputs[15] = temp[0];
            
            


            //Distance between tank and enemy tank
            inputs[16] = 1 / distance;

            //vision of any bullets on the screen
            temp = Look(0, 1);
            inputs[17] = temp[0];
            inputs[18] = temp[1];
            inputs[19] = temp[2];
            inputs[20] = temp[3];
            inputs[21] = temp[4];
            inputs[22] = temp[5];
            inputs[23] = temp[6];
            inputs[24] = temp[7];

            //if bullet is on screen or not
            if (m_Enemy_tank.BulletY == true)
                inputs[25] = 1 / Vector3.Distance(m_Enemy_tank.m_BulletPos, transform.position);
            else
                inputs[25] = 0;



            //Distance between walls
            float x = transform.position.x;
            float y = transform.position.y;
            float z = transform.position.z;

            inputs[26] = 1 / Vector3.Distance(new Vector3(-39.5f, y, z), transform.position); 
            inputs[27] = 1 / Vector3.Distance(new Vector3(40f, y, z), transform.position);
            inputs[28] = 1 / Vector3.Distance(new Vector3(x, 17.5f, z), transform.position);
            inputs[29] = 1 / Vector3.Distance(new Vector3(x, -17.5f, z), transform.position);

            //accuracy of tanks in player
            inputs[30] = m_accuracy;
            inputs[31] = m_Enemy_tank.m_accuracy;

            //if can shoot or not
            if (m_shoot == true)
                inputs[32] = 1;
            else
                inputs[32] = 0;

            //rotations / where tanks are looking
            inputs[33] = 1 / transform.rotation.eulerAngles.z;
            inputs[34] = 1 / m_Enemy_tank.transform.rotation.eulerAngles.z;





            if (m_net != null)
            {
                //Feed forward all inputs
                float[] output = m_net.FeedForward(inputs);


                //Best performing tank is cyan
                if (m_net.best == true)
                {
                    m_mats[0].SetColor("_Color", Color.cyan);
                }


                //OUTPUTS FOR MOVEMENT

                //move forward
                if (output[0] < -0.1)
                {

                    m_rBody.velocity = transform.right * m_MoveSpeed * Time.deltaTime;
                    
                    //reward for moving - not used after a few generations
                    if (moveB == false)
                    {
                        m_fitness += 2;
                        moveB = true;
                        Invoke("movee", 3f);

                            if (Vector3.Distance(oldpos, transform.position) < 3)
                            {
                                m_fitness -= 4;
                            }

                            oldpos = transform.position;

                        }


                }
                //backward
                else if (output[0] > 0.1)
                {

                    m_rBody.velocity = transform.right * -m_MoveSpeed * Time.deltaTime;

                    if (moveB == false)
                    {
                        m_fitness += 2;
                        moveB = true;
                        Invoke("movee", 3f);

                            if (Vector3.Distance(oldpos, transform.position) < 3)
                            {
                                m_fitness -= 4;
                            }
                            oldpos = transform.position;
                        }

                        

                    }
                else
                {
                    m_rBody.velocity = transform.right * 0;

                    if (moveB == false)
                    {
                      //  m_fitness += 1;
                        moveB = true;
                        Invoke("movee", 3f);

                            if (Vector3.Distance(oldpos, transform.position) < 3)
                            {
                                m_fitness -= 4;
                            }
                            oldpos = transform.position;
                        }

                        

                    }

                    


                    //Rotate tank or dont rotate
                    if (output[1] < -0.2)
                {



                    if (m_left == true)
                        m_rBody.MoveRotation(m_rBody.rotation + 1);
                    else
                        m_rBody.MoveRotation(m_rBody.rotation - 1);



                }
                else if (output[1] > 0.1)
                {



                    if (m_left == true)
                        m_rBody.MoveRotation(m_rBody.rotation - 1);
                    else
                        m_rBody.MoveRotation(m_rBody.rotation + 1);

   

                }




          
                //punished heavily for walking into a wal
                if (transform.position.x >= 39.2 || transform.position.x <= -39.2 || transform.position.y >= 17.2 || transform.position.y <= -17.2)
                {
                    if (wallB == false)
                    {

                        //Debug.Log("WALL");
                        m_fitness -= 15;
                        Invoke("Wallreset", 1f);
                        wallB = true;

                        if (touchW == false)
                        {
                            touchW = true;
                            m_fitness = m_fitness - 50;
                        }
                    }


                }

                    m_Angle = transform.rotation.eulerAngles.z;





                  //shoot or not
                    if (output[2] > -0.25)
                {
                    

                    //Spawn bullet projectile
                    if (m_shoot == true)
                    {
                        //Spawn bullet
                        Bullet Bulley = ((GameObject)Instantiate(m_BulletPrefab, transform.position, m_BulletPrefab.transform.rotation)).GetComponent<Bullet>();
                        Bulley.init(m_Angle, m_enemy, transform.gameObject);

                        m_shoot = false;
                        //can shoot again in 5 seconds
                        Invoke("timer", 5.1f);

                        //if accurate shot rewarded
                        if (m_accuracy > 0.99)
                        {
                            m_fitness += 15;

                        }

                    }

                }


    
                //set the fitness for the n
                m_net.SetFitness(m_fitness);

            }
        }
        }

        else
        {
            
          //do nothing if not the player


            }

        }


    private void Update()
    {
        //Player tank controls

        if (player == true) {
            
        if (Input.GetButton("up"))
        {

            m_rBody.velocity = transform.right * m_MoveSpeed * (Time.deltaTime * 10);

            
        }
        //backward
        else if (Input.GetButton("down"))
        {

            m_rBody.velocity = transform.right * -m_MoveSpeed * (Time.deltaTime * 10);


        }
        else
        {
            m_rBody.velocity = transform.right * 0;
        }

        if (Input.GetButton("right"))
        {



            m_rBody.MoveRotation(m_rBody.rotation - 1);


        }
        else if (Input.GetButton("left"))
        {


            m_rBody.MoveRotation(m_rBody.rotation + 1);

        }


        if (Input.GetKeyDown("space"))
        {


            //Spawn bullet projectile
            if (m_shoot == true)
            {
                //Spawn bullet
                Bullet Bulley = ((GameObject)Instantiate(m_BulletPrefab, transform.position, m_BulletPrefab.transform.rotation)).GetComponent<Bullet>();
                Bulley.init(m_Angle, m_enemy, transform.gameObject);


                m_shoot = false;

                //Timers for shooting
                Invoke("timer", 5.1f);


            }

            }

            m_Angle = transform.rotation.eulerAngles.z;


        }
    }

    void timer()
    {
        m_shoot = true;

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
            m_fitness += 60;
        else
        {
            m_fitness -= 20;
        }
  
        if (Vector3.Distance(Starpos, transform.position) < 3) ;
        {
            m_fitness -= 20;
        }
        m_net.SetFitness(m_fitness);


        initilized = false;

        //Debug.Log(m_fitness);
    }

    void movee()
    {
        moveB = false;
    }



    public void Init(NeuralNetwork net, Vector3 sTartpos, int Enemy)
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
            m_left = false;

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



    //hit by enemy
    public void Hit()
    {
        m_fitness -= 25;

        m_mats[0].SetColor("_Color", Color.magenta);
        if (won == false)
        {
            canwin = false;
            
        }
        m_net.SetFitness(m_fitness);

    }
    //hit the enemy
    public void hit_Enemy()
    {
        if (canwin == true) {
            won = true;

            m_mats[0].SetColor("_Color", Color.green);
        }

       m_fitness += 20;
        m_net.SetFitness(m_fitness);
    }

    public void arrowUpdate(float Accurate)
    {
        m_accuracy = Accurate;
    }



    private float[] Look(float direction, float type) {

        float[] look = new float[8];

        //16 - looking for enemy tank -- 25.7 degrees per section
        //4 - walls
        // 8 - bullets 48 degress per section

        //find where enemy bullet is
        if (type == 1 && m_Enemy_tank.BulletY == true)
        {
            for (int i = 0; i < 8; i++)
            {
                if (bulletA > 48 * i && bulletA < 48 * (i + 1))
                {
                    look[i] = 1;
                    
                }
                
            }
        }

        //find where enemy tank is
        else {

            if (m_Enemy_angle > 22.5 * direction && m_Enemy_angle < 22.5 * (direction + 1))
            {
                look[0] = 1;
            }


        }


        return look;

        }


}
