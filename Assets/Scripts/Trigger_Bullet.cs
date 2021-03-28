using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Trigger_Bullet : MonoBehaviour
{
    // Start is called before the first frame update
    
    int m_Enemy = 0;
    private void Start()
    {
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
        //Debug.Log("here");
        


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
        if(collision.transform.name == "Tank(Clone)")
        {
            //Debug.Log("working");
            if(collision.gameObject.GetComponent<Tank>().m_enemy == m_Enemy)
            {
                transform.parent.GetComponent<Bullet>().hit();
                collision.gameObject.GetComponent<Tank>().Hit();
                
            }
            
        }

       // Debug.Log(collision.transform.name);

        
    }
}
