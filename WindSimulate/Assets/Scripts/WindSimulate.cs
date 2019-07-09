using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSimulate : MonoBehaviour
{
    public Vector3 m_GlobalWindDirection = Vector3.zero;
    public float m_GlobalWindStrength = 0;
    public float m_GlobalWindDirectionChangeAngle = 25;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        GlobalWindSimulate();
    }

    private void GlobalWindSimulate()
    {
        Vector4 curGlobalWind = Vector4.zero;
        Vector3 dir = m_GlobalWindDirection.normalized;
        dir = Quaternion.AngleAxis(OneToOneFromTime(2.5f) * m_GlobalWindDirectionChangeAngle, Vector3.up) * dir;
        dir = dir.normalized;
        curGlobalWind.x = dir.x;
        curGlobalWind.y = dir.y;
        curGlobalWind.z = dir.z;
        curGlobalWind.w = m_GlobalWindStrength * 0.8f + m_GlobalWindStrength * OneToOneFromTime(2) * 0.2f;
        Shader.SetGlobalVector("_GlobalWind", curGlobalWind);
    }

    private float ZeroToOneFromTime(float speed = 1)
    {
        return Mathf.Abs(Mathf.Sin(Time.fixedTime * speed));
    }

    private float OneToOneFromTime(float speed = 1)
    {
        return Mathf.Sin(Time.fixedTime * speed);
    }
}
