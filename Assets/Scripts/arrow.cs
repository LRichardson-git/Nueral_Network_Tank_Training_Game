using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class arrow : MonoBehaviour
{

    private Rigidbody2D m_rBody;
    GameObject m_Parent;
    // Start is called before the first frame update
    void Start()
    {
        m_rBody = GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pos = new Vector3(0f, 0f, -5f);
        m_rBody.MoveRotation(m_Parent.GetComponent<Tank>().m_Angle);
        transform.position = m_Parent.transform.position + pos;

        Invoke("Timer", 15f);

    }

    public void init(GameObject parent)
    {
       m_Parent = parent;
    }

    private void Timer()
    {
        Object.Destroy(this.gameObject);
    }
}
