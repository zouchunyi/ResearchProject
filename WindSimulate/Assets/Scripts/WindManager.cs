using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

namespace WindSimulation
{
    public struct MotorOmni
    {
        public Vector3 m_Position;
        public Vector3 m_Direction;
        public float m_Radius;
        public float m_Force;
    };

    public class WindManager : MonoBehaviour
    {
        private static WindManager m_Instance = null;
        public static WindManager instance
        {
            get
            {
                if (m_Instance == null)
                {
                    GameObject obj = new GameObject("WindManager");
                    GameObject.DontDestroyOnLoad(obj);
                    m_Instance = obj.AddComponent<WindManager>();
                }

                return m_Instance;
            }
        }

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

        private List<WindMotorOmni> m_WindMotorOmniList = new List<WindMotorOmni>();

        public void RegisterWind(WindMotorOmni wind)
        {
            m_WindMotorOmniList.Add(wind);
        }

        public void UnregisterWind(WindMotorOmni wind)
        {
            m_WindMotorOmniList.Remove(wind);
        }

        private void Awake()
        {
            if (m_Instance != null)
            {
                GameObject.DestroyImmediate(this);
            }
            else
            {
                m_Instance = this;
            }
        }

        // Start is called before the first frame update
        void Start()
        {
            m_DynamicWindTexture = new RenderTexture(64, 32, 0, RenderTextureFormat.ARGB64);
            m_DynamicWindTexture.enableRandomWrite = true;
            m_DynamicWindTexture.volumeDepth = 64;
            m_DynamicWindTexture.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            m_DynamicWindTexture.filterMode = FilterMode.Point;
            m_DynamicWindTexture.Create();
            Shader.SetGlobalTexture("_DynamicWindTexture", m_DynamicWindTexture);

            m_DynamicWindTextureCopy = new RenderTexture(64, 32, 0, RenderTextureFormat.ARGB64);
            m_DynamicWindTextureCopy.enableRandomWrite = true;
            m_DynamicWindTextureCopy.volumeDepth = 64;
            m_DynamicWindTextureCopy.dimension = UnityEngine.Rendering.TextureDimension.Tex3D;
            m_DynamicWindTexture.filterMode = FilterMode.Point;
            m_DynamicWindTextureCopy.Create();
            Shader.SetGlobalTexture("_DynamicWindTextureCopy", m_DynamicWindTextureCopy);

            //Init
            int initKernel = m_DynamicCoumputeShader.FindKernel("Init");
            m_DynamicCoumputeShader.SetTextureFromGlobal(initKernel, "_DynamicWindTexture", "_DynamicWindTexture");
            //m_DynamicCoumputeShader.Dispatch(initKernel, 1, 32, 2);
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
            AdjustPosition();
            DiffuseWind();
            ApplyWind();
        }

        private Vector3 m_LastPosition = Vector3.zero;
        private void AdjustPosition()
        {
            if (!m_Target.position.Equals(m_LastPosition))
            {
                Vector4 deltaPosition;
                deltaPosition.x = (int)((m_Target.position.x - m_LastPosition.x) / 0.5f);
                deltaPosition.y = (int)((m_Target.position.y - m_LastPosition.y) / 0.5f);
                deltaPosition.z = (int)((m_Target.position.z - m_LastPosition.z) / 0.5f);
                deltaPosition.w = 1;
                if (deltaPosition.x != 0 || deltaPosition.y != 0 || deltaPosition.z != 0)
                {
                    if (deltaPosition.x != 0)
                    {
                        m_LastPosition.x = m_Target.position.x;
                    }
                    if (deltaPosition.y != 0)
                    {
                        m_LastPosition.y = m_Target.position.y;
                    }
                    if (deltaPosition.z != 0)
                    {
                        m_LastPosition.z = m_Target.position.z;
                    }
                    CopyTexture();
                    m_DynamicCoumputeShader.SetVector("_DeltaPosition", deltaPosition);
                    m_DynamicCoumputeShader.Dispatch(m_DynamicWindPositionAdjustKernel, 1, 32, 2);

                    Vector4 postion = new Vector4();
                    postion.x = m_LastPosition.x;
                    postion.y = m_LastPosition.y;
                    postion.z = m_LastPosition.z;

                    Shader.SetGlobalVector("_PlayerPositon", postion);
                }
            }
        }

        private List<MotorOmni> m_MotorOmniList = new List<MotorOmni>();
        private void ApplyWind()
        {
            m_MotorOmniList.Clear();
            for (int i = 0; i < m_WindMotorOmniList.Count; ++i)
            {
                MotorOmni omni;
                Vector3 center = (m_WindMotorOmniList[i].transform.position - m_Target.position) / 0.5f + new Vector3(32, 16, 32);
                center.x = Mathf.Floor(center.x);
                center.y = Mathf.Floor(center.y);
                center.z = Mathf.Floor(center.z);
                if (center.x >= 0 && center.x <= 63 && center.y >= 0 && center.y <= 63 && center.z >= 0 && center.z <= 63)
                {
                    omni.m_Position = center;
                    omni.m_Direction = m_WindMotorOmniList[i].m_Direction;
                    omni.m_Force = m_WindMotorOmniList[i].m_Force;
                    omni.m_Radius = m_WindMotorOmniList[i].m_Radius;

                    m_MotorOmniList.Add(omni);
                }
            }
            if (m_MotorOmniList.Count > 0)
            {
                ComputeBuffer buffer = new ComputeBuffer(m_MotorOmniList.Count, 4 * 8);
                buffer.SetData(m_MotorOmniList);
                m_DynamicCoumputeShader.SetBuffer(m_DynamicWindApplyWind, "_MotorOmniBuffer", buffer);
                m_DynamicCoumputeShader.SetFloat("_MotorOmniBufferLength", m_MotorOmniList.Count);
                m_DynamicCoumputeShader.Dispatch(m_DynamicWindApplyWind, 1, 32, 2);
                buffer.Release();
            }

        }

        private void DiffuseWind()
        {
            m_DynamicCoumputeShader.Dispatch(m_DynamicDiffuseWindKernel, 1, 32, 2);
        }

        private void CopyTexture()
        {
            m_DynamicCoumputeShader.Dispatch(m_DynamicWindCopyKernel, 1, 32, 2);
        }
    }
}

