using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindSimulate : MonoBehaviour
{
    public struct MotorOmni
    {
        public Vector3 m_Position;
        public float m_Radius;
        public float m_Force;
    };

    public Vector3 m_GlobalWindDirection = Vector3.zero;
    public float m_GlobalWindStrength = 0;
    public float m_GlobalWindDirectionChangeAngle = 25;

    public RenderTexture m_DynamicWindTexture = null;
    public RenderTexture m_DynamicWindTextureCopy = null;
    public ComputeShader m_DynamicCoumputeShader = null;

    public Transform m_Target = null;

    private int m_DynamicWindPositionAdjustKernel;
    private int m_DynamicWindCopyKernel;
    private int m_DynamicWindApplyWind;
    private int m_DynamicDiffuseWindKernel;

    // Start is called before the first frame update
    void Start()
    {
        m_DynamicWindTexture = new RenderTexture(64, 32, 0, RenderTextureFormat.ARGB32);
        m_DynamicWindTexture.enableRandomWrite = true;
        m_DynamicWindTexture.volumeDepth = 64;
        m_DynamicWindTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        m_DynamicWindTexture.Create();
        Shader.SetGlobalTexture("_DynamicWindTexture", m_DynamicWindTexture);

        m_DynamicWindTextureCopy = new RenderTexture(64, 32, 0, RenderTextureFormat.ARGB32);
        m_DynamicWindTextureCopy.enableRandomWrite = true;
        m_DynamicWindTextureCopy.volumeDepth = 64;
        m_DynamicWindTextureCopy.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
        m_DynamicWindTextureCopy.Create();
        Shader.SetGlobalTexture("_DynamicWindTextureCopy", m_DynamicWindTextureCopy);

        //Init
        int initKernel = m_DynamicCoumputeShader.FindKernel("Init");
        m_DynamicCoumputeShader.SetTextureFromGlobal(initKernel, "_DynamicWindTexture", "_DynamicWindTexture");
        m_DynamicCoumputeShader.Dispatch(initKernel, 1, 32, 1);
        m_LastPosition = m_Target.position;

        m_DynamicWindPositionAdjustKernel = m_DynamicCoumputeShader.FindKernel("PositionAdjust");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicWindPositionAdjustKernel, "_DynamicWindTexture", "_DynamicWindTexture");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicWindPositionAdjustKernel, "_DynamicWindTextureCopy", "_DynamicWindTextureCopy");


        m_DynamicWindCopyKernel = m_DynamicCoumputeShader.FindKernel("Copy");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicWindCopyKernel, "_DynamicWindTexture", "_DynamicWindTexture");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicWindCopyKernel, "_DynamicWindTextureCopy", "_DynamicWindTextureCopy");

        m_DynamicWindApplyWind = m_DynamicCoumputeShader.FindKernel("ApplyWind");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicWindApplyWind, "_DynamicWindTexture", "_DynamicWindTexture");

        m_DynamicDiffuseWindKernel = m_DynamicCoumputeShader.FindKernel("DiffuseWind");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicDiffuseWindKernel, "_DynamicWindTexture", "_DynamicWindTexture");
        m_DynamicCoumputeShader.SetTextureFromGlobal(m_DynamicDiffuseWindKernel, "_DynamicWindTextureCopy", "_DynamicWindTextureCopy");
    }

    // Update is called once per frame
    void Update()
    {
        Vector4 postion = new Vector4();
        postion.x = m_Target.position.x;
        postion.y = m_Target.position.y;
        postion.z = m_Target.position.z;

        Shader.SetGlobalVector("_PlayerPositon", postion);

        GlobalWindSimulate();
        ExcuteDynamicWind();
    }

    private void OnGUI()
    {
        if (GUI.Button(new Rect(0,0,50,50), ""))
        {
            int initKernel = m_DynamicCoumputeShader.FindKernel("Init");
            m_DynamicCoumputeShader.Dispatch(initKernel, 1, 32, 1);
        }
    }

    private void FixedUpdate()
    {

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
        AdjustPosition();
        DiffuseWind();
        ApplyWind();
    }

    private Vector3 m_LastPosition = Vector3.zero;
    private bool m_JustOnce = false;
    private void AdjustPosition()
    {
        if (!m_Target.position.Equals(m_LastPosition))
        {
            Vector4 deltaPosition;
            deltaPosition.x = Mathf.Floor((m_Target.position.x - m_LastPosition.x) / 0.5f);
            deltaPosition.y = Mathf.Floor((m_Target.position.y - m_LastPosition.y) / 0.5f);
            deltaPosition.z = Mathf.Floor((m_Target.position.z - m_LastPosition.z) / 0.5f);
            deltaPosition.w = 1;
            if (deltaPosition.x != 0 || deltaPosition.y != 0 || deltaPosition.z != 0)
            {
                if (!m_JustOnce)
                {
                    m_LastPosition = m_Target.position;
                    //Debug.Log("x :" + deltaPosition.x + " y : " + deltaPosition.y + " z: " + deltaPosition.z);
                    CopyTexture();
                    m_DynamicCoumputeShader.SetVector("_DeltaPosition", deltaPosition);
                    m_DynamicCoumputeShader.Dispatch(m_DynamicWindPositionAdjustKernel, 1, 32, 1);
                    //m_JustOnce = true;
                }

            }
        }
    }

    private List<MotorOmni> m_MotorOmniList = new List<MotorOmni>();
    private void ApplyWind()
    {
        m_MotorOmniList.Clear();
        ComputeBuffer buffer = new ComputeBuffer(1, 4 * 5);
        MotorOmni omni;
        omni.m_Position = new Vector3(32, 16, 32);
        omni.m_Force = 3f;
        omni.m_Radius = 1.5f;
        m_MotorOmniList.Add(omni);
        buffer.SetData(m_MotorOmniList);
        m_DynamicCoumputeShader.SetBuffer(m_DynamicWindApplyWind, "_MotorOmniBuffer", buffer);
        m_DynamicCoumputeShader.Dispatch(m_DynamicWindApplyWind, 1, 32, 1);
        buffer.Release();
    }

    private void DiffuseWind()
    {
        m_DynamicCoumputeShader.Dispatch(m_DynamicDiffuseWindKernel, 1, 32, 1);
    }

    private void CopyTexture()
    {
        m_DynamicCoumputeShader.Dispatch(m_DynamicWindCopyKernel, 1, 32, 1);
    }
}
