using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class VoxelObject : MonoBehaviour {

    private Mesh mesh;
    private MeshFilter meshFilter;

    private List<Vector2> uv;
    private List<int> triangles;

    private Dictionary<Vector3, int> verticesDic;

    // Use this for initialization
    void Start()
    {
        meshFilter = GetComponent<MeshFilter>();
        mesh = new Mesh();
        meshFilter.mesh = mesh;
        resetMesh();
    }

    public void addTriangle(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        if(verticesDic.ContainsKey(v1))
        {
            triangles.Add(verticesDic[v1]);
        }
        else
        {
            verticesDic.Add(v1, verticesDic.Count);
            triangles.Add(verticesDic[v1]);
            uv.Add(new Vector2(0f, 0f));
        }

        if (verticesDic.ContainsKey(v2))
        {
            triangles.Add(verticesDic[v2]);
        }
        else
        {
            verticesDic.Add(v2, verticesDic.Count);
            triangles.Add(verticesDic[v2]);
            uv.Add(new Vector2(0f, 0f));
        }

        if (verticesDic.ContainsKey(v3))
        {
            triangles.Add(verticesDic[v3]);
        }
        else
        {
            verticesDic.Add(v3, verticesDic.Count);
            triangles.Add(verticesDic[v3]);
            uv.Add(new Vector2(0f, 0f));
        }
    }

    public void resetMesh()
    {
        uv = new List<Vector2>();
        triangles = new List<int>();
        mesh = new Mesh();
        verticesDic = new Dictionary<Vector3, int>();
    }

    public void updateMesh()
    {
        Vector3[] verticesArray = new Vector3[verticesDic.Count];
        foreach(KeyValuePair<Vector3, int> entry in verticesDic)
        {
            verticesArray[entry.Value] = entry.Key; 
        }

        //mesh.vertices = vertices.ToArray();
        mesh.vertices = verticesArray;
        mesh.uv = uv.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateBounds();
        mesh.RecalculateNormals();

        Debug.Log("MESH VERTICES: " + mesh.vertexCount);
        Debug.Log("TRIANGLES: " + triangles.Count / 3);
        meshFilter.mesh = mesh;

    }
	
}
