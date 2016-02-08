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
    private float MAX_RANGE = 50.0f;
    private float MIN_RANGE = 1.0f;
    private float MAX_TOOL_POWER = 1.25f;
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

    public void SetToolPower(float power)
    {
        DensityModShader.SetFloat("toolPower", power);
    }

    public void ChangeToolRange(float rangeChange)
    {
        //todo UI text with tool range
        Debug.Log("Range changed by "+rangeChange + ", \tnew Value: " + this.modRange);
        this.modRange += rangeChange;
        this.modRange = Math.Max(Math.Min(this.modRange, this.MAX_RANGE), this.MIN_RANGE);
    }

    public float getToolRadius()
    {
        return modRange;
    }

    public void ChangeToolStrength(float powerChange)
    {
        //todo UI text with tool power
        Debug.Log("Strength changed by " + powerChange+", \tnew Value: "+this.modPower);
        this.modPower += powerChange;
        this.modPower = Math.Max(Math.Min(this.modPower, this.MAX_TOOL_POWER), this.MIN_TOOL_POWER);
    }

    public float getToolStrength()
    {
        return modPower;
    }

    internal void destroy()
    {
        densityBuffer.Release();
    }
}
