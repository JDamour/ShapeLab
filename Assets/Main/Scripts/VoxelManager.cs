using UnityEngine;
using System;
using System.Collections;
using Leap;

public class VoxelManager : MonoBehaviour {

    public float scaling;
    public int size;
    public Transform eyePos;

    public VoxelObject voxelObject;
    public PointCloudObject pointCloudObject;
    public VoxelObjectGPU voxelObjectGPU;

    private VoxelField voxel;
    private LookUpTables lookUpTables;
    private float isolevel = 0f;


    private Controller m_leapController;
    public HandController handController;


    // Use this for initialization
    void Start () {
        //m_leapController = handController.GetLeapController();

        lookUpTables = new LookUpTables();
        voxel = new VoxelField(size);
        voxel.createSphere(size / 3);
        initMesh();
    }

    // Update is called once per frame
    void Update () {
        if (Input.GetKeyUp(KeyCode.Space))
        {
            /*
            Frame frame = m_leapController.Frame();
            //Get TipPosition of the Tool
            Vector3 tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
            tipPosition *= handController.transform.localScale.x; //scale position with hand movement
            tipPosition += handController.transform.position;
            */
            //voxelObjectGPU.setModPosition(tipPosition);
            Vector3 position = new Vector3(1, 1, 1);
            float modRange = 10.0f;
            //modManager.modify(position, modRange, voxelObjectGPU.getVoxelBuffer(), ModificationManager.ACTION.ADD);
            voxelObjectGPU.newModification(position, ModificationManager.ACTION.ADD);
            updateMesh();
        }
        if (Input.GetKeyUp(KeyCode.LeftAlt))
        {
            /*
            Frame frame = m_leapController.Frame();
            //Get TipPosition of the Tool
            Vector3 tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
            tipPosition *= handController.transform.localScale.x; //scale position with hand movement
            tipPosition += handController.transform.position;
            */
            //voxelObjectGPU.setModPosition(tipPosition);
            voxelObjectGPU.newModification(new Vector3(1, 1, 1), ModificationManager.ACTION.SUBSTRACT);
            updateMesh();
        }
        if (Input.GetKeyUp("s"))
        {
            voxel.createSphere(size / 2);
            updateMesh();
        }
        if (Input.GetKeyUp("b"))
        {
            voxel.createBlock();
            updateMesh();
        }
        if (Input.GetKeyUp("r"))
        {
            voxel.createRandomGrid();
            updateMesh();
        }
        if (Input.GetKeyUp("1"))
        {
            int radius = 10;
            Func<float, float, float> currentDeformFunction = delegate (float curRadius, float curDensity) { return Mathf.Cos(curRadius/(float)radius) * curDensity; };
            voxel.changeDensityAt(new Vector3(0,0,0), radius, currentDeformFunction);
            updateMesh();
        }
        if (Input.GetKeyUp("2"))
        {
            int radius = 10;
            Func<float, float, float> currentDeformFunction = delegate (float curRadius, float curDensity) { return (1+Mathf.Cos(curRadius / (float)radius)) * curDensity; };
            voxel.changeDensityAt(new Vector3(0, 0, 0), radius, currentDeformFunction);
            updateMesh();
        }
        if (Input.GetKeyUp("3"))
        {
            int radius = 10;
            Func<float, float, float> currentDeformFunction = delegate (float curRadius, float curDensity) { return Mathf.Cos(curRadius / (float)radius) + curDensity; };
            voxel.changeDensityAt(new Vector3(0, 0, 0), radius, currentDeformFunction);
            updateMesh();
        }
        if (Input.GetKeyUp("4"))
        {
            int radius = 10;
            Func<float, float, float> currentDeformFunction = delegate (float curRadius, float curDensity) { return curDensity - Mathf.Cos(curRadius / (float)radius); };
            voxel.changeDensityAt(new Vector3(0, 0, 0), radius, currentDeformFunction);
            updateMesh();
        }
    }

    private void initMesh()
    {
        voxelObjectGPU.initMesh(voxel);
    }

    public void updateMesh()
    {
        //pointCloud();
        //marchingCubes();
        //marchingCubesGPU();
        voxelObjectGPU.updateMesh(voxel);
    }

    private void pointCloud()
    {
        Debug.Log("Create new Point Mesh");
        pointCloudObject.resetCloud();

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    if(voxel.getValue(x,y,z) < 0f)
                    {
                        Vector3.Distance(eyePos.position, new Vector3(x,y,z));
                        float color = Mathf.Max(0f,Mathf.Min(1f,1-(Vector3.Distance(eyePos.position, new Vector3(x, y, z))/ Vector3.Distance(eyePos.position, new Vector3(size, size, size))))); 
                        pointCloudObject.addVertices(new Vector3(x,y,z), color);
                    }
                }
        pointCloudObject.updateCloud();
    }

    private void marchingCubes()
    {
        voxelObject.resetMesh();

        for (int x = 0; x < size-1; x++){
            for(int y = 0; y < size-1; y++){
                for (int z = 0; z < size - 1; z++){
                    
                    int cubeIndex = voxel.getCubeIndex(x, y, z, isolevel);

                    if (lookUpTables.edgeTable[cubeIndex] == 0)
                    {
                        continue;
                    }
                    

                    Vector3 v0 = new Vector3(x, y, z + 1);
                    Vector3 v1 = new Vector3(x + 1, y, z + 1);
                    Vector3 v2 = new Vector3(x + 1, y, z);
                    Vector3 v3 = new Vector3(x, y, z);
                    Vector3 v4 = new Vector3(x, y + 1, z + 1);
                    Vector3 v5 = new Vector3(x + 1, y + 1, z + 1);
                    Vector3 v6 = new Vector3(x + 1, y + 1, z);
                    Vector3 v7 = new Vector3(x, y + 1, z);

                    Vector3[] vertList = new Vector3[12];
                    /* Find the vertices where the surface intersects the cube */
                    //if ((lookUpTables.edgeTable[cubeIndex] & 1) != 0 )
                        vertList[0] =
                           vertexInterpolate(isolevel, v0, v1, voxel.getValue(v0), voxel.getValue(v1));
                   // if ((lookUpTables.edgeTable[cubeIndex] & 2) != 0)
                        vertList[1] =
                           vertexInterpolate(isolevel, v1, v2, voxel.getValue(v1), voxel.getValue(v2));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 4) != 0)
                        vertList[2] =
                           vertexInterpolate(isolevel, v2, v3, voxel.getValue(v2), voxel.getValue(v3));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 8)!= 0)
                        vertList[3] =
                           vertexInterpolate(isolevel, v3, v0, voxel.getValue(v3), voxel.getValue(v0));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 16) != 0)
                        vertList[4] =
                           vertexInterpolate(isolevel, v4, v5, voxel.getValue(v4), voxel.getValue(v5));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 32) != 0)
                        vertList[5] =
                           vertexInterpolate(isolevel, v5, v6, voxel.getValue(v5), voxel.getValue(v6));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 64) != 0)
                        vertList[6] =
                           vertexInterpolate(isolevel, v6, v7, voxel.getValue(v6), voxel.getValue(v7));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 128) != 0)
                        vertList[7] =
                           vertexInterpolate(isolevel, v7, v4, voxel.getValue(v7), voxel.getValue(v4));
                   // if ((lookUpTables.edgeTable[cubeIndex] & 256) != 0)
                        vertList[8] =
                           vertexInterpolate(isolevel, v0, v4, voxel.getValue(v0), voxel.getValue(v4));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 512) != 0)
                        vertList[9] =
                           vertexInterpolate(isolevel, v1, v5, voxel.getValue(v1), voxel.getValue(v5));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 1024) != 0)
                        vertList[10] =
                           vertexInterpolate(isolevel, v2, v6, voxel.getValue(v2), voxel.getValue(v6));
                    //if ((lookUpTables.edgeTable[cubeIndex] & 2048) != 0)
                        vertList[11] =
                           vertexInterpolate(isolevel, v3, v7, voxel.getValue(v3), voxel.getValue(v7));

                    for (int i = 0; lookUpTables.triTable[cubeIndex, i] != -1; i += 3)
                    {
                        //voxelObject.addTriangle(vertList[lookUpTables.triTable[cubeIndex, i]], vertList[lookUpTables.triTable[cubeIndex, i + 1]], vertList[lookUpTables.triTable[cubeIndex, i + 2]]);
                        voxelObject.addTriangle(vertList[lookUpTables.triTable[cubeIndex, i+2]], vertList[lookUpTables.triTable[cubeIndex, i + 1]], vertList[lookUpTables.triTable[cubeIndex, i]]);
                    }

                }
            }
        }

        voxelObject.updateMesh();
        Debug.Log("VOXELMANAGER: Finished Mesh creation");
    }

    /// <summary>
    /// interpolates a new Vertex at the surface position on the edge
    /// </summary>
    /// <param name="isolevel">surface value</param>
    /// <param name="p1">vertices 1</param>
    /// <param name="p2">vertices 2</param>
    /// <param name="v1">voxel value from vertices 1</param>
    /// <param name="v2">voxel value from vertices 2</param>
    /// <returns></returns>
    private Vector3 vertexInterpolate(float isolevel, Vector3 p1, Vector3 p2, float v1, float v2)
    {

        if (Mathf.Abs(isolevel - v1) < 0.00001)
            return (p1);
        if (Mathf.Abs(isolevel - v2) < 0.00001)
            return (p2);
        if (Mathf.Abs(v1 - v2) < 0.00001)
            return (p1);
        float mu = (isolevel - v1) / (v2 - v1);
        Vector3 p = new Vector3();
        p.x = p1.x + mu * (p2.x - p1.x);
        p.y = p1.y + mu * (p2.y - p1.y);
        p.z = p1.z + mu * (p2.z - p1.z);

        return p;
    }
}
