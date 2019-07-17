using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSimulate : MonoBehaviour
{
    public Vector3 m_GlobalWindDirection = Vector3.zero;
    public float m_GlobalWindStrength = 0;
    public float m_GlobalWindDirectionChangeAngle = 25;

    public RenderTexture m_DynamicWindTexture = null;
    public ComputeShader m_DynamicCoumputeShader = null;

    //public Transform m_Target

    private int m_DynamicWindKernel;

    // Start is called before the first frame update
    void Start()
    {
        m_DynamicWindTexture = new RenderTexture(64, 32, 0, RenderTextureFormat.ARGB32);
        m_DynamicWindTexture.enableRandomWrite = true;
        m_DynamicWindTexture.volumeDepth = 64;
        m_DynamicWindTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        m_DynamicWindTexture.Create();
        //Color[] colors = new Color[32 * 16 * 32];
        //for (int x = 0; x < 32; ++x)
        //{
        //    for (int y = 0; y < 16; ++y)
        //    {
        //        for (int z = 0; z < 32; ++z)
        //        {
        //            colors[x * 32 * 16 + y * 16 + z] = Color.red;
        //        }
        //    }
        //}
        //m_DynamicWindTexture.SetPixels(colors);
        //m_DynamicWindTexture.Apply();
        Shader.SetGlobalTexture("_DynamicWindTexture", m_DynamicWindTexture);

        m_DynamicWindKernel = m_DynamicCoumputeShader.FindKernel("CSMain");
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate()
    {
        GlobalWindSimulate();
        ExcuteDynamicWind();
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

    private void ExcuteDynamicWind()
    {
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicWindKernel, "_DynamicWindTexture", "_DynamicWindTexture");
        m_DynamicCoumputeShader.Dispatch(m_DynamicWindKernel, 1, 32, 1);
    }
}
