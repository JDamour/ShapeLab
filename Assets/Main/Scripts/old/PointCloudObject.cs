using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PointCloudObject : MonoBehaviour {

    private Mesh mesh;
    private MeshFilter meshFilter;

    private List<Vector3> vertices;
    private List<int> indices;
    private List<Color> colors;

    void Awake()
    {
        initMesh();
        meshFilter = GetComponent<MeshFilter>();
        meshFilter.mesh = mesh;
    }

    public void initMesh()
    {
        vertices = new List<Vector3>();
        colors = new List<Color>();
        mesh = new Mesh();
        indices = new List<int>();
    }

    public void addVertices(Vector3 vec, float color)
    {
        indices.Add(vertices.Count);
        vertices.Add(vec);
        colors.Add(new Color(color, color, color, 1));
    }

    public void updateCloud()
    {
        mesh.vertices = vertices.ToArray();
        mesh.colors = colors.ToArray();
        mesh.SetIndices(indices.ToArray(), MeshTopology.Points,0);
        mesh.RecalculateBounds();

        //Debug.Log("MESH VERTICES: " + mesh.vertexCount);
        meshFilter.mesh = mesh;
    }

    public void resetCloud()
    {
        initMesh();
    }
}
