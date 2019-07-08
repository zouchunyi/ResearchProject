using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshTop : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        MeshFilter meshFilter = this.GetComponent<MeshFilter>();
        if (meshFilter != null)
        {
            ComputeMesh(meshFilter.sharedMesh);
        }
    }

    private void ComputeMesh(Mesh mesh)
    {
        List<Vector3> vertexs = new List<Vector3>();
        mesh.GetVertices(vertexs);

        float bottom = float.MaxValue;
        float top = float.MinValue;
        foreach (Vector3 vertex in vertexs)
        {
            bottom = Mathf.Min(bottom, vertex.y);
            top = Mathf.Max(top, vertex.y);
        }

        Debug.Log("Top : " + top + "," + "Bottom : " + bottom);
    }
}
