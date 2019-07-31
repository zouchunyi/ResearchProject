using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IndirectDrawBuffer
{
    private Material m_Material = null;
    private Mesh m_GrassMesh = null;
    private List<Matrix4x4[]> m_MatricesList = null;

    private ComputeBuffer m_ArgsBuffer = null;
    private ComputeBuffer m_PositionBuffer = null;
    private ComputeBuffer m_VegetationArgsBuffer = null;
    private ComputeBuffer m_DrawPositionBuffer = null;
    private Bounds m_Bounds = new Bounds(Vector3.zero, Vector3.one * 100);

    //
    //index count per instance
    //instance count
    //start index location
    //base vertex location
    //start instance location
    //
    private uint[] m_Args = new uint[5] { 0, 0, 0, 0, 0 };

    private int m_ListNumber = 0;
    private int m_ListSize = 0;

    public IndirectDrawBuffer(GameObject copyItem, int listNumbler, int listSize)
    {
        m_ListNumber = listNumbler;
        m_ListSize = listSize;

        GameObject item = GameObject.Instantiate(copyItem);
        MeshFilter meshFilter = item.GetComponent<MeshFilter>();
        m_GrassMesh = meshFilter.sharedMesh;
        m_Material = item.GetComponent<MeshRenderer>().material;
        item.SetActive(false);

        m_MatricesList = new List<Matrix4x4[]>();
        for (int i = 0; i < listNumbler; ++i)
        {
            Matrix4x4[] matrix4X4s = null;
            RandomMatrix4x4(out matrix4X4s, listSize);
            m_MatricesList.Add(matrix4X4s);
        }

        m_ArgsBuffer = new ComputeBuffer(5, sizeof(uint), ComputeBufferType.IndirectArguments);

        m_Args[0] = m_GrassMesh.GetIndexCount(0);
        m_Args[1] = (uint)(listNumbler * listSize);
        m_Args[2] = m_GrassMesh.GetIndexStart(0);
        m_Args[3] = m_GrassMesh.GetBaseVertex(0);
        m_ArgsBuffer.SetData(m_Args);

        m_PositionBuffer = new ComputeBuffer(listNumbler * listSize, sizeof(float) * 4);
        m_VegetationArgsBuffer = new ComputeBuffer(listNumbler * listSize, sizeof(float) * 4);
        m_DrawPositionBuffer = new ComputeBuffer(listNumbler * listSize, sizeof(float) * 4, ComputeBufferType.Append);
        Vector4[] positions = new Vector4[listNumbler * listSize];
        Vector4[] vegetationArgs = new Vector4[listNumbler * listSize];
        for (int i = 0; i < m_MatricesList.Count; ++i)
        {
            for (int j = 0; j < listSize; ++j)
            {
                float maxAngle = Random.Range(50f, 85f);
                positions[i * listSize + j] = new Vector4(m_MatricesList[i][j].m03, m_MatricesList[i][j].m13, m_MatricesList[i][j].m23, 1);
                vegetationArgs[i * listSize + j] = new Vector4(maxAngle, 0, 0, 0);
            }
        }
        m_PositionBuffer.SetData(positions);
        m_VegetationArgsBuffer.SetData(vegetationArgs);
        m_Material.SetBuffer("_positionBuffer", m_PositionBuffer);
        m_Material.SetBuffer("_vegetationArgsBuffer", m_VegetationArgsBuffer);
    }

    public int InstanceSize()
    {
        return m_ListNumber * m_ListSize;
    }

    private void RandomMatrix4x4(out Matrix4x4[] matrix4X4s, int size)
    {
        matrix4X4s = new Matrix4x4[size];

        for (int i = 0; i < matrix4X4s.Length; i++)
        {
            var position = new Vector3(Random.Range(-30f, 30f), 0f, Random.Range(-30f, 30f));
            var rotation = Quaternion.Euler(new Vector3(0, Random.Range(0, 360), 0));
            var scale = Vector3.one;
            var matrix = Matrix4x4.TRS(position, rotation, scale);

            matrix4X4s[i] = matrix;
        }
    }

    public void Draw()
    {
        Graphics.DrawMeshInstancedIndirect(m_GrassMesh, 0, m_Material, m_Bounds, m_ArgsBuffer);
    }
}
