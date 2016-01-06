using UnityEngine;
using System.Collections;

public class Voxel{

    private float[,,] voxel;
    private int size;

    public Voxel(int size)
    {
        voxel = new float[size, size, size];
        this.size = size;
    }

    public void createRandomGrid()
    {
        voxel = new float[size, size, size];
        int inside = 0;
        int outside = 0;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1 || z == 0 || z == size - 1)
                    {
                        voxel[x, y, z] = 1f;
                    }
                    else
                    {
                        voxel[x, y, z] = Random.Range(-1f, 1f);
                    }

                    if(voxel[x, y, z] >= 0)
                    {
                        outside += 1;
                    }
                    else
                    {
                        inside += 1;
                    }
                }

        Debug.Log("VOXEL: Generated Random Grid");
    }

    public void createSphere(int radius)
    {
        voxel = new float[size, size, size];
        int inside = 0;
        int outside = 0;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    if(Vector3.Distance(new Vector3(size/2, size/2, size/2), new Vector3(x, y, z)) < radius)
                    {
                        voxel[x, y, z] = -1f;
                        inside += 1;
                    }
                    else
                    {
                        voxel[x, y, z] = 1f;
                        outside += 1;
                    }
                }

        Debug.Log("VOXEL: Generated (Almost) Sphere");
    }

    public float getValue(Vector3 vec)
    {

        return voxel[(int)vec.x,(int)vec.y, (int)vec.z];
    }

    public int getCubeIndex(int xPos, int yPos, int zPos, float isolevel)
    {
        /*
        int cubeindex = 0;
        if (voxel[xPos, yPos, zPos+1] <= isolevel) cubeindex |= 1;
        if (voxel[xPos+1, yPos, zPos+1] <= isolevel) cubeindex |= 2;
        if (voxel[xPos+1, yPos, zPos] <= isolevel) cubeindex |= 4;
        if (voxel[xPos, yPos, zPos] <= isolevel) cubeindex |= 8;
        if (voxel[xPos, yPos+1, zPos+1] <= isolevel) cubeindex |= 16;
        if (voxel[xPos+1, yPos+1, zPos+1] <= isolevel) cubeindex |= 32;
        if (voxel[xPos+1, yPos+1, zPos] <= isolevel) cubeindex |= 64;
        if (voxel[xPos, yPos+1, zPos] <= isolevel) cubeindex |= 128;
        */

        int binaryIndex = 0;
        if (voxel[xPos, yPos, zPos + 1] <= 0) binaryIndex += 1;
        if (voxel[xPos + 1, yPos, zPos + 1] <= 0) binaryIndex += 2;
        if (voxel[xPos + 1, yPos, zPos] <= 0) binaryIndex += 4;
        if (voxel[xPos, yPos, zPos] <= 0) binaryIndex += 8;
        if (voxel[xPos, yPos + 1, zPos + 1] <= 0) binaryIndex += 16;
        if (voxel[xPos + 1, yPos + 1, zPos + 1] <= 0) binaryIndex += 32;
        if (voxel[xPos + 1, yPos + 1, zPos] <= 0) binaryIndex += 64;
        if (voxel[xPos, yPos + 1, zPos] <= 0) binaryIndex += 128;

        return binaryIndex;
    }
}
