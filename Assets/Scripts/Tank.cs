using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tank : MonoBehaviour
{

    private bool initilized = false;
    private Transform m_hex;

    private NeuralNetwork m_net;
    private Rigidbody2D m_rBody;
    private Material[] m_mats;

    //nerual net related
    private float m_fitness = 0f;

    //tank related
    public float m_MoveSpeed = 100f;

    void Start()
    {
        //create rigidbody
        m_rBody = GetComponent<Rigidbody2D>();

        
    }

    private void FixedUpdate()
    {
          



        
    }

    //todo - fix this
    public void Init(NeuralNetwork net, Transform hex)
    {
       
        initilized = true;
    }
    public void OnServerInitialized(NeuralNetwork net, )
    {
        
    }

}
