using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    public GameObject m_GrassCopy = null;

    private Material m_Material = null;
    private Mesh m_GrassMesh = null;
    private List<Matrix4x4[]> m_MatricesList = null;

    // Start is called before the first frame update
    void Start()
    {
        GameObject item = GameObject.Instantiate(m_GrassCopy);
        MeshFilter meshFilter = item.GetComponent<MeshFilter>();
        m_GrassMesh = meshFilter.sharedMesh;
        m_Material = item.GetComponent<MeshRenderer>().sharedMaterial;
        item.SetActive(false);

        m_MatricesList = new List<Matrix4x4[]>();
        for (int i = 0; i < 100; ++i)
        {
            Matrix4x4[] matrix4X4s = null;
            RandomMatrix4x4(out matrix4X4s);
            m_MatricesList.Add(matrix4X4s);
        }
    }

    // Update is called once per frame
    void Update()
    {
        DrawGrassFromDirect();
    }

    private void RandomMatrix4x4(out Matrix4x4[] matrix4X4s)
    {
        matrix4X4s = new Matrix4x4[1023];

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
}
