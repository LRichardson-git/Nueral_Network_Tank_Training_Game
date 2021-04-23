using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    //References to objects
    private Rigidbody2D m_rbody;
    public GameObject TriggerPrefab;
    private GameObject m_Parent;

    //Gamplay related
    int m_MoveSpeed = 1000;
    int i = 0;
    int m_Enemy;
    float m_Angle = 1f;
    int m_bounces = 0;
    Vector3 LastVelocity;

    //messages to send back to parent
    private bool m_send = true;
    public bool hitt = false;
   
    void Start()
    {
        //spawn hitbox (had trouble and this seemed to work)
        Trigger_Bullet Bulley = ((GameObject)Instantiate(TriggerPrefab, transform.position, TriggerPrefab.transform.rotation)).GetComponent<Trigger_Bullet>();
        Bulley.init(transform.transform, m_Enemy);
    }


    void Update()
    {
        //Keep constant velocity
        LastVelocity = m_rbody.velocity;

        //Die if bounce of wall twice
        if(m_bounces > 2)
        {
            Die();
        }

        //Die if tank dies
        if (m_Parent.GetComponent<Tank>().initilized == false)
        {
            Die();
        }

        //Send information on location
        if(m_send == true)
        {
            m_Parent.GetComponent<Tank>().m_BulletList[i] = transform.position.x;
            m_Parent.GetComponent<Tank>().m_BulletList[i + 1] = transform.position.y;
        }

        
    }
    void Die()
    {
        //Killself and reset data in bulletlist of parent 
        m_send = false;
        m_Parent.GetComponent<Tank>().m_BulletList[i] = 0;
        m_Parent.GetComponent<Tank>().m_BulletList[i + 1] = 0;
        Object.Destroy(this.gameObject);

        
    }

    public void init (float Angle, int Enemy, GameObject Parent)
    {
        //initilze values
        m_Enemy = Enemy;
        m_Angle = Angle;
        m_rbody = GetComponent<Rigidbody2D>();
        m_Parent = Parent;
        
        //change rotation based on angle tank is looking
        m_rbody.MoveRotation(m_Angle);
        Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, Angle) * Vector2.right);
        m_rbody.AddForce( dir * m_MoveSpeed);

        //Die after 7 seconds
        Invoke("Die", 7f);

        //Ignore collisions from basically everything except walls
        Physics2D.IgnoreLayerCollision(10, 8);
        Physics2D.IgnoreLayerCollision(10, 9);
        Physics2D.IgnoreLayerCollision(10, 10);
        Physics2D.IgnoreLayerCollision(10, 11);

        
       
       //Get which bullet it is
       i = Parent.GetComponent<Tank>().m_bulletnumb;

       //Set basic informaiton in bulletlist in parent tank
       Parent.GetComponent<Tank>().m_BulletList[i] = transform.position.x;
       Parent.GetComponent<Tank>().m_BulletList[i+1] = transform.position.y;


       Parent.GetComponent<Tank>().m_bulletnumb += 2;
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        //When collide bounce
        var Speed = LastVelocity.magnitude;
        var direction = Vector3.Reflect(LastVelocity.normalized, coll.contacts[0].normal);

        //Change velocity / direction
        m_rbody.velocity = direction * Mathf.Max(Speed, 0f);
        m_bounces++;


    }

    public void hit() 
    {
        //tell parent he hit enemy 
        hitt = true;
        m_Parent.GetComponent<Tank>().hit_Enemy();
        Die();
    }
}

