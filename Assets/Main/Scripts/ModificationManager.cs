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

    public ComputeShader DensityModShader;
    private ComputeBuffer densityBuffer;
    private ComputeBuffer boolReturnBuffer;
    private int dimension;

    private float modRange = 20.0f;
    private float modPower = 1.0f;

    public ModificationManager(ComputeShader modShader, int N, float scale)
    {
        dimension = N;
        DensityModShader = modShader;
        boolReturnBuffer = new ComputeBuffer(1, sizeof(bool));

        // set up shader vars
        DensityModShader.SetFloat("toolPower", 1.0f);
        DensityModShader.SetFloat("MIN_DENSITY", -1.0f);
        DensityModShader.SetFloat("MAX_DENSITY", 1.0f);
        DensityModShader.SetFloat("cosStrength", 0.08f);
        DensityModShader.SetFloat("modRange", 5.0f);
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

        //set up modification specific vars
        String kernelName = "densityModificator";
        switch (modAction)
        {
            case ACTION.SUBSTRACT:
                DensityModShader.SetInt("sign", -1);
                break;
            case ACTION.ADD:
                DensityModShader.SetInt("sign", 1);
                break;
            case ACTION.SMOOTH:
                kernelName = "smoothModificator";
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
        Debug.Log("Range changed by "+rangeChange + ", \tnew Value: " + this.modRange);
        this.modRange += rangeChange;
    }
    public void ChangeToolStrength(float powerChange)
    {
        Debug.Log("Strength changed by " + powerChange+", \tnew Value: "+this.modPower);
        this.modPower += powerChange;
    }
    /*
    internal bool isPositionInObject(Vector3 tipPosition)
    {
        bool[] retVal = new bool[1];
        retVal[0] = false;
        DensityModShader.SetVector("tipPosition", new Vector4(tipPosition.x, tipPosition.y, tipPosition.z, 1));
        //boolReturnBuffer.Dispose();

        boolReturnBuffer.SetData(retVal);
        DensityModShader.SetBuffer(DensityModShader.FindKernel("isPositionInObject"), "boolReturn", boolReturnBuffer);
        //run shader
        DensityModShader.Dispatch(DensityModShader.FindKernel("isPositionInObject"), dimension / 8, dimension / 8, dimension / 8);
        boolReturnBuffer.GetData(retVal);
        //boolReturnBuffer.Release();

        if (retVal[0])
            Debug.Log("retVal is:"+retVal[0].ToString());
        return retVal[0];
    }*/

    internal void destroy()
    {
        densityBuffer.Release();
        boolReturnBuffer.Release();
    }
}
