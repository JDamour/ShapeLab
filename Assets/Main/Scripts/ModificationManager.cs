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
    private ComputeShader clearVertexAreaShader;

    private int dimension;
    private float modRange = 5.0f;
    private float modPower = 1.0f;
    private float MAX_RANGE = 50.0f;
    private float MIN_RANGE = 1.0f;
    private float MAX_TOOL_POWER = 1.5f;
    private float MIN_TOOL_POWER = 0.75f;

    public ModificationManager(ComputeShader modShader, ComputeShader clearShader, int N, float scale)
    {
        dimension = N;
        DensityModShader = modShader;
        clearVertexAreaShader = clearShader;

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
    internal void modify(Vector3 modCenter, ACTION modAction, ComputeBuffer vertexBuffer)
    {
        int[] offset = calculateOffset(modCenter, modRange);
        int boundingBoxSize = calculateModifySize(modRange);

        DensityModShader.SetVector("Bounding_offSet", calculateBoundingBox(modCenter, modRange));
        DensityModShader.SetVector("modCenter", new Vector4(modCenter.x, modCenter.y, modCenter.z, 1));
        DensityModShader.SetFloat("toolPower", modPower);
        DensityModShader.SetFloat("modRange", modRange);
        DensityModShader.SetInt("offsetX", offset[0]);
        DensityModShader.SetInt("offsetY", offset[1]);
        DensityModShader.SetInt("offsetZ", offset[2]);

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
        DensityModShader.Dispatch(DensityModShader.FindKernel(kernelName), boundingBoxSize/4, boundingBoxSize/4, boundingBoxSize/4);

        clearVertexArea(offset, boundingBoxSize, vertexBuffer);
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

    private int[] calculateOffset(Vector3 modCenter, float modRange)
    {
        int[] offset = new int[3];
        offset[0] = (int)Mathf.Max(Mathf.Min((float)Math.Floor(modCenter.x - modRange), dimension), 0f);
        offset[1] = (int)Mathf.Max(Mathf.Min((float)Math.Floor(modCenter.y - modRange), dimension), 0f);
        offset[2] = (int)Mathf.Max(Mathf.Min((float)Math.Floor(modCenter.z - modRange), dimension), 0f);
        Debug.Log("modcenter is: "+ modCenter);
        return offset;
    }

    private int calculateModifySize(float modRange)
    {
        int boundingBoxSize = (int)Mathf.Ceil(modRange * 2);
        if (boundingBoxSize%4 == 1){
            return boundingBoxSize + 3;

        }else if (boundingBoxSize%4 == 2){
            return boundingBoxSize + 2;

        }else if(boundingBoxSize%4 == 3){
            return boundingBoxSize + 1;

        }else {
            return boundingBoxSize;
        }
        
    }

    private void clearVertexArea(int[] offset, int size, ComputeBuffer vertexBuffer)
    {
        Debug.Log("offset is: " + offset[0] + " " + offset[1] + " " + offset[2]);
        Debug.Log("range is: " + modRange + " size is: " + size);
        clearVertexAreaShader.SetInt("cubeDimension", dimension);
        clearVertexAreaShader.SetInt("offsetX", offset[0]);
        clearVertexAreaShader.SetInt("offsetY", offset[1]);
        clearVertexAreaShader.SetInt("offsetZ", offset[2]);
        clearVertexAreaShader.SetVector("offset", new Vector3(offset[0], offset[1], offset[2]));
        clearVertexAreaShader.SetBuffer(0, "vertexBuffer", vertexBuffer);
        clearVertexAreaShader.Dispatch(0, size/4, size/4, size/4);
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

    public void SetToolPower(float newPower)
    {
        this.modPower = this.MIN_TOOL_POWER + newPower*0.3f;
        this.modPower = Math.Max(Math.Min(this.modPower, this.MAX_TOOL_POWER), this.MIN_TOOL_POWER);
        // DensityModShader.SetFloat("toolPower", power);
    }

    public void ResetToolRange()
    {
        this.modRange = 5.0f;
        this.modRange = Math.Max(Math.Min(this.modRange, this.MAX_RANGE), this.MIN_RANGE);
        Debug.Log("Range reset to " + this.modRange);

    }

    internal void destroy()
    {
        densityBuffer.Release();
    }
}
