using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    // Start is called before the first frame update

    private Rigidbody2D m_rbody;
    int m_MoveSpeed = 1000;

    int m_Enemy;
    float m_Angle = 1f;
    int m_bounces = 0;
    Vector3 LastVelocity;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        LastVelocity = m_rbody.velocity;

        if(m_bounces > 2)
        {
            Object.Destroy(this.gameObject);
        }

       
    }
    void Die()
    {
        Object.Destroy(this.gameObject);
    }

    public void init (float Angle, int Enemy)
    {
        m_Enemy = Enemy;
        m_Angle = Angle;
        ;
        m_rbody = GetComponent<Rigidbody2D>();
        
        
        m_rbody.MoveRotation(m_Angle);
        Vector2 dir = (Vector2)(Quaternion.Euler(0, 0, Angle) * Vector2.right);
        m_rbody.AddForce( dir * m_MoveSpeed);
        Debug.Log(Angle);
        Invoke("Die", 5f);


        Physics2D.IgnoreLayerCollision(10, 8);
            Physics2D.IgnoreLayerCollision(10, 9);
            Physics2D.IgnoreLayerCollision(10, 10);
            Physics2D.IgnoreLayerCollision(10, 11);

    }

    private void OnCollisionEnter2D(Collision2D coll)
    {
        var Speed = LastVelocity.magnitude;
        var direction = Vector3.Reflect(LastVelocity.normalized, coll.contacts[0].normal);

        m_rbody.velocity = direction * Mathf.Max(Speed, 0f);

       

        m_bounces++;
    }
}