using UnityEngine;
using System.Collections;
using System;

public class ModificationManager {
    public enum ACTION
    {
        SUBSTRACT,
        ADD,
        SMOOTH,
        NONE
    };

    private ComputeShader DensityModShader;
    private ComputeBuffer densityBuffer;

    private int dimension;
    private float modRange = 5.0f;
    private float modPower = 1.0f;
    private float MAX_RANGE = 20.0f;
    private float MIN_RANGE = 1.0f;
    private float MAX_TOOL_POWER = 1.5f;
    private float MIN_TOOL_POWER = 0.75f;

    public ModificationManager(ComputeShader modShader, int N, float scale)
    {
        dimension = N;
        DensityModShader = modShader;

        // set up shader vars
        DensityModShader.SetFloat("toolPower", modPower);
        DensityModShader.SetFloat("MIN_DENSITY", -1.0f);
        DensityModShader.SetFloat("MAX_DENSITY", 1.0f);
        DensityModShader.SetFloat("cosStrength", 20.0f);
        DensityModShader.SetFloat("modRange", modRange);
        DensityModShader.SetInt("dimension", dimension + 1);
    }

    internal void setDensityBuffer(ComputeBuffer voxelBuffer)
    {
        densityBuffer = voxelBuffer;
    }

    /// <summary>
    /// Modification of the object
    /// </summary>
    /// <param name="modCenter">center of the modification</param>
    /// <param name="modAction">type of modification</param>
    internal void modify(Vector3 modCenter, ACTION modAction)
    {

        DensityModShader.SetVector("Bounding_offSet", calculateBoundingBox(modCenter, modRange));
        DensityModShader.SetVector("modCenter", new Vector4(modCenter.x, modCenter.y, modCenter.z, 1));
        DensityModShader.SetFloat("toolPower", modPower);
        DensityModShader.SetFloat("modRange", modRange);

        //set up modification specific vars and kernel name
        String kernelName = "";
        switch (modAction)
        {
            case ACTION.ADD:
                DensityModShader.SetInt("sign", -1);
                kernelName = "densityModificator";
                break;
            case ACTION.SUBSTRACT:
                DensityModShader.SetInt("sign", 1);
                kernelName = "densityModificator";
                break;
            case ACTION.SMOOTH:
                //kernelName = "smoothModificator";
                kernelName = "smooth3x3Modificator";
                break;
        }
        //setup buffer containing densities
        DensityModShader.SetBuffer(DensityModShader.FindKernel(kernelName), "voxel", densityBuffer);
        //run shader
        DensityModShader.Dispatch(DensityModShader.FindKernel(kernelName), dimension / 8, dimension / 8, dimension / 8);
    }

    //TODO: Calculate a bounding box
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

    // 
    internal void InitialSmooth(int smoothQuality)
    {
        DensityModShader.SetVector("Bounding_offSet", new Vector4(0, 0, 0, 0));
        DensityModShader.SetVector("modCenter", new Vector4(this.dimension/2.0f, this.dimension / 2.0f, this.dimension / 2.0f, 1));
        DensityModShader.SetFloat("toolPower", MIN_TOOL_POWER);
        DensityModShader.SetFloat("modRange", 500.0f);
        //setup buffer containing densities
        DensityModShader.SetBuffer(DensityModShader.FindKernel("smooth3x3Modificator"), "voxel", densityBuffer);
        for(int i = 0; i < smoothQuality; i++)
        {
            //run shader
            DensityModShader.Dispatch(DensityModShader.FindKernel("smooth3x3Modificator"), dimension / 8, dimension / 8, dimension / 8);
        }
        
    }

    //
    public void ChangeToolRange(float rangeChange)
    {
        //Debug.Log("Range changed by "+rangeChange + ", \tnew Value: " + this.modRange);
        this.modRange += rangeChange;
        this.modRange = Math.Max(Math.Min(this.modRange, this.MAX_RANGE), this.MIN_RANGE);
    }

    public void ChangeToolStrength(float powerChange)
    {
        //Debug.Log("Strength changed by " + powerChange+", \tnew Value: "+this.modPower);
        this.modPower += powerChange;
        this.modPower = Math.Max(Math.Min(this.modPower, this.MAX_TOOL_POWER), this.MIN_TOOL_POWER);
    }

    public float getToolRadius()
    {
        return modRange;
    }

    public float getToolStrength()
    {
        return modPower;
    }

    public void SetToolPower(float power)
    {
        DensityModShader.SetFloat("toolPower", power);
    }

    internal void destroy()
    {
        densityBuffer.Release();
    }
}
