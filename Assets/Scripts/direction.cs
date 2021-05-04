using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class direction : MonoBehaviour
{
    // Start is called before the first frame update
    Tank Enemy;
    private Rigidbody2D m_rBody;
    GameObject m_Parent;
    void Start()
    {
        m_rBody = GetComponent<Rigidbody2D>();
        Invoke("Timer", 30f);
    }

    // Update is called once per frame
    void Update()
    {

        //find where the enemy bullet is if the is one
        if (m_Parent.GetComponent<Tank>().BulletY == true)
        {
            transform.position = m_Parent.transform.position;

            Vector3 targg = m_Parent.GetComponent<Tank>().m_Enemy_tank.m_BulletPos;
            targg.z = 0f;

            //calculate angle of enemy bullet
            Vector3 objectPoss = transform.position;
            targg.x = targg.x - objectPoss.x;
            targg.y = targg.y - objectPoss.y;

            float anglee = Mathf.Atan2(targg.y, targg.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.Euler(new Vector3(0, 0, anglee));

            //send tank the angle of the bullet
            m_Parent.GetComponent<Tank>().bulletA = transform.rotation.eulerAngles.z;

        }


        transform.position = m_Parent.transform.position;

        Vector3 targ = Enemy.transform.position;
        targ.z = 0f;

        Vector3 objectPos = transform.position;
        targ.x = targ.x - objectPos.x;
        targ.y = targ.y - objectPos.y;

        float angle = Mathf.Atan2(targ.y, targ.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(new Vector3(0, 0, angle));

        
        m_Parent.GetComponent<Tank>().m_Enemy_angle = transform.rotation.eulerAngles.z;

    }

    public void init(GameObject parent, Tank Enemy_Tank)
    {
        Enemy = Enemy_Tank;
        m_Parent = parent;
    }
    private void Timer()
    {
        Object.Destroy(this.gameObject);
    }
}
