using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject m_GrassDirectCopy = null;
    public GameObject m_GrassIndirectCopy = null;
    public int m_CircleTimes = 1;
    public int m_InstanceNumber = 1023;
    public ComputeShader m_CullingComputeShader = null;

    public bool m_Indirect = false;

    private Material m_Material = null;
    private Mesh m_GrassMesh = null;
    private List<Matrix4x4[]> m_MatricesList = null;

    private ComputeBuffer m_ArgsBuffer = null;
    private ComputeBuffer m_PositionBuffer = null;
    private ComputeBuffer m_VegetationArgsBuffer = null;
    private ComputeBuffer m_DrawPositionBuffer = null;
    private Bounds m_Bounds = new Bounds(Vector3.zero, Vector3.one * 100);

    //GPU culling
    private int m_CullingKernel = 0;

    //
    //index count per instance
    //instance count
    //start index location
    //base vertex location
    //start instance location
    //
    private uint[] m_Args = new uint[5] { 0, 0, 0, 0, 0 };

    private void Start()
    {
        GameObject item = null;
        if (m_Indirect)
        {
            item = GameObject.Instantiate(m_GrassIndirectCopy);
        }
        else
        {
            item = GameObject.Instantiate(m_GrassDirectCopy);
        }
        MeshFilter meshFilter = item.GetComponent<MeshFilter>();
        m_GrassMesh = meshFilter.sharedMesh;
        m_Material = item.GetComponent<MeshRenderer>().sharedMaterial;
        item.SetActive(false);

        m_MatricesList = new List<Matrix4x4[]>();
        for (int i = 0; i < m_CircleTimes; ++i)
        {
            Matrix4x4[] matrix4X4s = null;
            RandomMatrix4x4(out matrix4X4s);
            m_MatricesList.Add(matrix4X4s);
        }
        
        if (m_Indirect)
        {
            m_ArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

            m_Args[0] = m_GrassMesh.GetIndexCount(0);
            m_Args[1] = (uint)(m_CircleTimes * m_InstanceNumber);
            m_Args[2] = m_GrassMesh.GetIndexStart(0);
            m_Args[3] = m_GrassMesh.GetBaseVertex(0);
            m_ArgsBuffer.SetData(m_Args);

            m_PositionBuffer = new ComputeBuffer(m_CircleTimes * m_InstanceNumber, sizeof(float) * 4);
            m_VegetationArgsBuffer = new ComputeBuffer(m_CircleTimes * m_InstanceNumber, sizeof(float) * 4);
            m_DrawPositionBuffer = new ComputeBuffer(m_CircleTimes * m_InstanceNumber, sizeof(float) * 4, ComputeBufferType.Append);
            Vector4[] positions = new Vector4[m_CircleTimes * m_InstanceNumber];
            Vector4[] vegetationArgs = new Vector4[m_CircleTimes * m_InstanceNumber];
            for (int i = 0; i < m_MatricesList.Count; ++i)
            {
                for (int j = 0; j < m_InstanceNumber; ++j)
                {
                    float maxAngle = Random.Range(10f, 80f);
                    positions[i * m_InstanceNumber + j] = new Vector4(m_MatricesList[i][j].m03, m_MatricesList[i][j].m13, m_MatricesList[i][j].m23, 1);
                    vegetationArgs[i * m_InstanceNumber + j] = new Vector4(maxAngle, 0, 0, 0);
                }
            }
            m_PositionBuffer.SetData(positions);
            m_VegetationArgsBuffer.SetData(vegetationArgs);
            m_Material.SetBuffer("_positionBuffer", m_PositionBuffer);
            m_Material.SetBuffer("_vegetationArgsBuffer", m_VegetationArgsBuffer);

            m_CullingKernel = m_CullingComputeShader.FindKernel("CSMain");
        }
    }

    private void Update()
    {
        if (m_Indirect)
        {
            //GPUCulling();
            DrawGrassFromIndirect();
        }
        else
        {
            DrawGrassFromDirect();
        }
    }

    private void RandomMatrix4x4(out Matrix4x4[] matrix4X4s)
    {
        matrix4X4s = new Matrix4x4[m_InstanceNumber];

        for (int i = 0; i < matrix4X4s.Length; i++)
        {
            var position = new Vector3(Random.Range(-30f, 30f), 0f, Random.Range(-30f, 30f));
            var rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            var scale = Vector3.one;
            var matrix = Matrix4x4.TRS(position, rotation, scale);

            matrix4X4s[i] = matrix;
        }
    }

    private void DrawGrassFromDirect()
    {
        for (int i = 0; i < m_MatricesList.Count; ++i)
        {
            Graphics.DrawMeshInstanced(m_GrassMesh, 0, m_Material, m_MatricesList[i]);
        }
    }

    private void DrawGrassFromIndirect()
    {
        Graphics.DrawMeshInstancedIndirect(m_GrassMesh, 0, m_Material, m_Bounds, m_ArgsBuffer);
    }

    private void GPUCulling()
    {
        m_DrawPositionBuffer.SetCounterValue(0);
        int pitch = m_CircleTimes * m_InstanceNumber / 512;
        m_CullingComputeShader.SetBuffer(m_CullingKernel, "_positionBuffer", m_PositionBuffer);
        m_CullingComputeShader.SetBuffer(m_CullingKernel, "_drawPositionBuffer", m_DrawPositionBuffer);
        m_CullingComputeShader.SetVector("_cameraPosition", new Vector4(transform.position.x, transform.position.y, transform.position.z, 1));
        m_CullingComputeShader.Dispatch(m_CullingKernel, pitch, 1, 1);
        ComputeBuffer.CopyCount(m_DrawPositionBuffer, m_ArgsBuffer, 4);
    }
}
