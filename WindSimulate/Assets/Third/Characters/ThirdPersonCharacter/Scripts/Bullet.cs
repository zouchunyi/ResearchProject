using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using WindSimulation;

public class Bullet : MonoBehaviour
{
    public Vector3 m_Speed;

    private float m_Countdown = 0;

    private WindMotorOmni m_WindMotorOmni = null;

    private void Awake()
    {
        m_WindMotorOmni = this.GetComponent<WindMotorOmni>();
    }

    private void Start()
    {
        m_Countdown = 3f;
    }

    private void Update()
    {
        m_WindMotorOmni.m_Direction = m_Speed.normalized;
    }

    private void FixedUpdate()
    {
        transform.position += m_Speed;
        m_Countdown -= Time.deltaTime;
        if (m_Countdown <= 0)
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
