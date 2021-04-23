using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrow : MonoBehaviour
{

    //References to objects
    Tank Enemy;
    private Rigidbody2D m_rBody;
    GameObject m_Parent;

    //currect accuracy
    float dot;
    bool accurate = false;

    void Start()
    {
        m_rBody = GetComponent<Rigidbody2D>();
        Invoke("Timer", 30f);
    }

    void Update()
    {
        //Set rotation and location based on parent tank
        Vector3 pos = new Vector3(0f, 0f, -5f);
        m_rBody.MoveRotation(m_Parent.GetComponent<Tank>().m_Angle);
        transform.position = m_Parent.transform.position + pos;

        //Caluculate accuracy (if looking at enemy)
        dot = Vector3.Dot(transform.right, (Enemy.transform.position - transform.position).normalized);
        m_Parent.GetComponent<Tank>().arrowUpdate(dot);

        if (dot > 0.95 && accurate == false)
        {
            m_Parent.GetComponent<Tank>().addFitness(20);
            accurate = true;
            Invoke("resetaccurate", 5f);
        }

    }

    void resetaccurate ()
    {
        accurate = false;
    }

    public void init(GameObject parent, Tank Enemy_Tank)
    {
        m_Parent = parent;
        Enemy = Enemy_Tank;
    }

    private void Timer()
    {
        Object.Destroy(this.gameObject);
    }
}
