using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    int i = 0;
    private Rigidbody2D m_rbody;
    int m_MoveSpeed = 1000;
    
    public GameObject TriggerPrefab;
    private GameObject m_Parent;
    int m_Enemy;
    float m_Angle = 1f;
    int m_bounces = 0;
    private bool m_send = true;
    public bool hitt = false;
    Vector3 LastVelocity;
    void Start()
    {


        Trigger_Bullet Bulley = ((GameObject)Instantiate(TriggerPrefab, transform.position, TriggerPrefab.transform.rotation)).GetComponent<Trigger_Bullet>();
        Bulley.init(transform.transform, m_Enemy);
    }

    // Update is called once per frame
    void Update()
    {
        LastVelocity = m_rbody.velocity;

        if(m_bounces > 2)
        {
            Die();
        }

        if (m_Parent.GetComponent<Tank>().initilized == false)
        {
            Die();
        }
        if(m_send == true)
        {
            m_Parent.GetComponent<Tank>().m_BulletList[i] = transform.position.x;
            m_Parent.GetComponent<Tank>().m_BulletList[i + 1] = transform.position.y;
        }
        
    }
    void Die()
    {
        m_send = false;
        m_Parent.GetComponent<Tank>().m_BulletList[i] = 0;
        m_Parent.GetComponent<Tank>().m_BulletList[i + 1] = 0;
        Object.Destroy(this.gameObject);

        
    }

    public void init (float Angle, int Enemy, GameObject Parent)
    {
        m_Enemy = Enemy;
        m_Angle = Angle;
        m_rbody = GetComponent<Rigidbody2D>();
        m_Parent = Parent;
        
        m_rbody.MoveRotation(m_Angle);
        Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, Angle) * Vector2.right);
        m_rbody.AddForce( dir * m_MoveSpeed);
        //Debug.Log(Angle);
        Invoke("Die", 7f);


        Physics2D.IgnoreLayerCollision(10, 8);
            Physics2D.IgnoreLayerCollision(10, 9);
            Physics2D.IgnoreLayerCollision(10, 10);
            Physics2D.IgnoreLayerCollision(10, 11);

        
       
       i = Parent.GetComponent<Tank>().m_bulletnumb; 
        
       Parent.GetComponent<Tank>().m_BulletList[i] = transform.position.x;
       Parent.GetComponent<Tank>().m_BulletList[i+1] = transform.position.y;


        Parent.GetComponent<Tank>().m_bulletnumb += 2;
    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        var Speed = LastVelocity.magnitude;
        var direction = Vector3.Reflect(LastVelocity.normalized, coll.contacts[0].normal);

        m_rbody.velocity = direction * Mathf.Max(Speed, 0f);

       

        m_bounces++;


    }

    public void hit() 
    {
        hitt = true;
        m_Parent.GetComponent<Tank>().hit_Enemy();
        Die();
    }
}

