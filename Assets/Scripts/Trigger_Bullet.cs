using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Bullet : MonoBehaviour
{
    int m_Enemy = 0;

    private void Start()
    {
        //ignore collisions with other triggers
        Physics2D.IgnoreLayerCollision(10, 12);
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = transform.parent.position;
    }

    public void init(Transform Parent2, int enemy)
    {
        transform.parent = Parent2;

        //
        if (enemy % 2 == 0)
        {
            m_Enemy = enemy + 1;
        }
        else
        {
            m_Enemy = enemy - 1;
        }

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //Check if collided with tank
        if(collision.transform.name == "Tank(Clone)")
        {
            //Check if tank is the assigned enemy of parent tank
            if(collision.gameObject.GetComponent<Tank>().m_enemy == m_Enemy)
            {
                //Tell tank it has hit the enemy
                transform.parent.GetComponent<Bullet>().hit();
                collision.gameObject.GetComponent<Tank>().Hit();
                
            }
            
        } 
    }
}
