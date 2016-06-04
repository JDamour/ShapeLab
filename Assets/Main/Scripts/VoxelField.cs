using UnityEngine;
using System;
using System.Collections;
using Random = UnityEngine.Random;

/// <summary>
/// class for a 3d voxelfield
/// </summary>
public class VoxelField{

    private int[,,] colors;
    private float[,,] voxel;
    private int size;

    //constructor to set the dimension of the field
    public VoxelField(int size)
    {
        voxel = new float[size, size, size];
        colors = new int[size, size, size];
        this.size = size;
    }

    //creates a random voxel field
    public void createRandomGrid()
    {
        voxel = new float[size, size, size];
        int inside = 0;
        int outside = 0;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    colors[x, y, z] = 0xBABABA;
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

    /// <summary>
    /// creates a sphere in the voxel field
    /// </summary>
    /// <param name="radius">rqadius of the voxel sphere, between 0 an voxel-dimension/2</param>
    public void createSphere(int radius)
    {
        voxel = new float[size, size, size];
        int inside = 0;
        int outside = 0;

        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    colors[x, y, z] = 0xBABABA;
                    if (Vector3.Distance(new Vector3(size/2, size/2, size/2), new Vector3(x, y, z)) < radius)
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

    /// <summary>
    /// creates a block in the voxel field
    /// </summary>
    public void createBlock()
    {

        voxel = new float[size, size, size];
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                {
                    colors[x, y, z] = 0xBABABA;
                    if (x == 0 || x == size - 1 || y == 0 || y == size - 1 || z == 0 || z == size - 1)
                    {
                        voxel[x, y, z] = 1f;
                    }
                    else
                    {
                        voxel[x, y, z] = -1f;
                    }
                }
                    
    }

    // returnsd the voxelfield als 3d array
    public float[,,] getField()
    {
        return voxel;
    }

    public int[,,] getColorField()
    {
        return colors;
    }

    //returns the desity value for a given position
    public float getValue(int x, int y, int z)
    {
        return voxel[x, y, z];
    }
    //returns the desity value for a given Vector3 position
    public float getValue(Vector3 vec)
    {

        return voxel[(int)vec.x,(int)vec.y, (int)vec.z];
    }

    //TODO: remove unused code
    public int getCubeIndex(int xPos, int yPos, int zPos, float isolevel)
    {
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

    public void changeDensityAt(Vector3 position, float radius, Func<float, float, float> manipulationFunction)
    {
        // 1. get bounding box around position
        int[,] boundingIndices = {
                    {(int)-radius, (int)radius},
                    {(int)-radius, (int)radius},
                    {(int)-radius, (int)radius}
                };
        // 1.1: check outOfBounds for indices
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 2; j++)
            {
                if (boundingIndices[i, j] < 0)
                {
                    boundingIndices[i, j] = 0;
                }
                if (boundingIndices[i, j] >= size)
                {
                    boundingIndices[i, j] = size - 1;
                }
            }
        }

        //2. for each point in box, apply function
        for (int x = boundingIndices[0, 0]; x < boundingIndices[0, 1]; x++)
        {
            for (int y = boundingIndices[1, 0]; y < boundingIndices[1, 1]; y++)
            {
                for (int z = boundingIndices[2, 0]; z < boundingIndices[2, 1]; z++)
                {
                    float dist = Vector3.Distance(new Vector3(x,y,z), position);
                    if (dist <= radius)
                    {
                        voxel[x,y,z] = manipulationFunction(voxel[x, y, z], dist);
                    }
                }
            }
        }
    }
}
