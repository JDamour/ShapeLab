using UnityEngine;
using System.Collections;
using System;

public class ModificationManager {
    public enum ACTION
    {
        SUBSTRACT,
        ADD
    };

    public ComputeShader DensityModShader;
    private int dimension;

    public ModificationManager(ComputeShader modShader, int N)
    {
        dimension = N;
        DensityModShader = modShader;

        // set up shader vars
        DensityModShader.SetFloat("MIN_DENSITY", -1.0f);
        DensityModShader.SetFloat("MAX_DENSITY", 1.0f);
        DensityModShader.SetFloat("cosStrength", 0.1f);
        DensityModShader.SetFloat("modRange", 20.0f);
        DensityModShader.SetInt("dimension", dimension + 1);
    }

    internal void modify(Vector3 modCenter, float modRange, ComputeBuffer voxelBuffer, ACTION modAction)
    {

        DensityModShader.SetVector("Bounding_offSet", calculateBoundingBox(modCenter, modRange));
        DensityModShader.SetVector("modCenter", new Vector4(modCenter.x, modCenter.y, modCenter.z, 1));
        
        //set up modification specific vars
        switch (modAction)
        {
            case ACTION.SUBSTRACT:
                DensityModShader.SetInt("sign", -1);
                break;
            case ACTION.ADD:
                DensityModShader.SetInt("sign", 1);
                break;
        }
        //setup buffer containing densities
        DensityModShader.SetBuffer(DensityModShader.FindKernel("densityModificator"), "voxel", voxelBuffer);
        //run shader
        DensityModShader.Dispatch(DensityModShader.FindKernel("densityModificator"), dimension / 8, dimension / 8, dimension / 8);
    }

    private Vector4 calculateBoundingBox(Vector3 modCenter, float modRange)
    {
        Vector4 offset = new Vector4(0, 0, 0, 0);
        //todo
        /*
        offset.x = (float)Math.Floor(modCenter.x - modRange);
        offset.y = (float)Math.Floor(modCenter.y - modRange);
        offset.z = (float)Math.Floor(modCenter.z - modRange);
        */
        return offset;
    }

}
