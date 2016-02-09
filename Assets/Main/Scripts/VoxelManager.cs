﻿using UnityEngine;
using System;
using System.Collections;
using Leap;
using UnityEngine.UI;

public class VoxelManager : MonoBehaviour {

    public float objectSize;
    private float scaling;
    public int voxelCubeSize;
    public Transform eyePos;
    public Transform boundaries;

    public VoxelObjectGPU voxelObjectGPU;

    private int voxelFieldSize;
    private VoxelField voxel;
    //private float isolevel = 0f;

    //Leap variables
    private Controller m_leapController;
    public HandController handController;
    private ModificationManager.ACTION currentTool;

    public SphereTool tool;
    private SphereTool currentToolObject;
    public Material toolMaterial;

    public Color pushToolColor;
    public Color pullToolColor;
    public Color smoothToolColor;

    private Vector3 rotation;

    public Text radiusText;
    public Text strengthText;

    private enum INTEND
    {
        MOD,
        CREATERND,
        CREATEBLOCK,
        CREATESPHERE,
        REDUCETOOLRANGE,
        INCREASETOOLRANGE,
        REDUCETOOLSTRENGTH,
        INCREASETOOLSTRENGTH,
        NONE
    }

    // initialization before the start() methods are called
    void Awake()
    {
        rotation = new Vector3(0f,0f,0f);
        if(voxelCubeSize%8 != 0)
        {
            Debug.Log("The dimension of the voxelCubeField has to be a multiple of 8");
        }
        voxelFieldSize = voxelCubeSize + 1;
        scaling = objectSize / (float)voxelCubeSize;
        voxelObjectGPU.setInitData(voxelCubeSize, scaling);
    }

    // Use this for initialization
    void Start () {
        m_leapController = handController.GetLeapController();

        radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius() * 100)) / 100f;
        strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
        toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
        voxel = new VoxelField(voxelFieldSize);
        voxel.createSphere(voxelFieldSize / 3);
        initMesh(true);

    }

    // Update is called once per frame
    void Update () {
        switch (getIntent()) {
            case INTEND.CREATESPHERE:
                voxel.createSphere(voxelFieldSize / 3);
                initMesh(false);
                break;
            case INTEND.CREATERND:
                voxel.createRandomGrid();
                initMesh(false);
                break;
            case INTEND.CREATEBLOCK:
                voxel.createBlock();
                initMesh(false);
                break;
            
            case INTEND.MOD:
                Frame frame = m_leapController.Frame();
                Vector3 tipPosition = new Vector3(0.5f, 0.5f, 0.0f);
                if (m_leapController.IsConnected) {
                    
                    if (frame.Tools.Count == 0) //only modify if there is a tool
                        return;

                    tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                    tipPosition = handController.transform.TransformPoint(tipPosition);
                    
                } else {
                    // for testing purposes
                    Debug.Log("modding at: " + tipPosition.x / scaling + ";" + tipPosition.y / scaling + ";" + tipPosition.z / scaling);
                }

                //apply modification
                voxelObjectGPU.applyToolAt(getRotatedPosition(tipPosition / scaling), currentTool);
                //voxelObjectGPU.updateMesh(getRotatedPosition(tipPosition / scaling), currentTool, rotation);
                //render new vertices
                updateMesh();

                break;
            case INTEND.NONE:
                updateMesh();
                break;
        }
        
        if (Input.GetKeyUp("1"))
        {
            setPullTool(); 
        }
        if (Input.GetKeyUp("2"))
        {
            setPushTool();
        }
        if (Input.GetKeyUp("3"))
        {
            setSmoothTool();     
        }
    }

    private INTEND getIntent()
    {
        //TODO erkennung, wann objekt berührt wird
        if (Input.GetAxis("StickVertical") != 0)
        {
            if (Input.GetAxis("StickVertical") > 0) { 
                rotation.x = (rotation.x + 1 + 360) %360;
            }
            else
            {
                rotation.x = (rotation.x - 1 + 360) %360;
            }
            //todo rotate object Around X axis
            Debug.Log("StickVertical: " + Input.GetAxis("StickVertical"));
            Debug.Log("Rotation X: " + rotation.x);
        }
        if (Input.GetAxis("StickHorizontal") != 0)
        {
            if (Input.GetAxis("StickHorizontal") > 0)
            {
                rotation.y = (rotation.y + 1 + 360)%360;
            }
            else
            {
                rotation.y = (rotation.y - 1 + 360) %360;
            }
            //todo rotate object around Y axis
            Debug.Log("StickHorizontal: " + Input.GetAxis("StickHorizontal"));
            Debug.Log("Rotation Y: " + rotation.y);
        }
        updateBoundaries();
        if (Input.GetAxis("AnalogCrossHorizontal") < 0 ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            // reducing tool range
            voxelObjectGPU.getModificationManager().ChangeToolRange(-0.1f);
            toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
            //radiusText.text = "Radius: " + voxelObjectGPU.getModificationManager().getToolRadius(true) + "%";
            radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius()*100))/100f;
        }
        if (Input.GetAxis("AnalogCrossHorizontal") > 0 ||
            Input.GetKey(KeyCode.RightArrow))
        {
            // increasing tool range
            voxelObjectGPU.getModificationManager().ChangeToolRange(0.1f);
            toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
            radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius() * 100)) / 100f;
        }
        if (Input.GetAxis("AnalogCrossVertical") < 0 ||
            Input.GetKey(KeyCode.DownArrow))
        {
            // reducing tool strength
            voxelObjectGPU.getModificationManager().ChangeToolStrength(-0.005f);
            strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
        }
        if (Input.GetAxis("AnalogCrossVertical") > 0 ||
            Input.GetKey(KeyCode.UpArrow))
        {
            // increasing tool strength
            voxelObjectGPU.getModificationManager().ChangeToolStrength(0.005f);
            strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
        }
        if (Input.GetButton("ModButton") || Input.GetButton("Jump"))
        {
            return INTEND.MOD;
        }
        if (Input.GetKeyUp("s"))
        {
            return INTEND.CREATESPHERE;
        }
        if (Input.GetKeyUp("b"))
        {
            return INTEND.CREATEBLOCK;
        }
        if (Input.GetKeyUp("r"))
        {
            return INTEND.CREATERND;
        }
        return INTEND.NONE;
    }

    public void showToolRadius(bool show)
    {
        if (show){
            toolMaterial.SetFloat("_Transparency", 1.0f);
        }else{
            toolMaterial.SetFloat("_Transparency", 0.0f);
        }
    }

    private void updateBoundaries()
    {
        boundaries.transform.rotation = Quaternion.Euler(new Vector3(rotation.x, -rotation.y, rotation.z));
    }

    private void initMesh(bool withSmooth)
    {
        voxelObjectGPU.initMesh(voxel, rotation, withSmooth);
    }

    public void updateMesh()
    {
        voxelObjectGPU.updateMesh(new Vector3(0,0,0), ModificationManager.ACTION.NONE, rotation);
    }

    protected Vector3 getRotatedPosition(Vector3 position)
    {
        //Debug.Log("position before"+ position);
        Vector3 tempPos = position - new Vector3(voxelCubeSize / 2, voxelCubeSize / 2, voxelCubeSize / 2);

        float rotationX = -rotation.x / 180 * (float)Math.PI;
        float rotationY = rotation.y / 180 * (float)Math.PI;
        
        //Y-axis rotation
        Vector3 rotationYpos;
        rotationYpos.x = Mathf.Cos(rotationY) * tempPos.x + Mathf.Sin(rotationY) * tempPos.z;
        rotationYpos.y = tempPos.y;
        rotationYpos.z = -Mathf.Sin(rotationY) * tempPos.x + Mathf.Cos(rotationY) * tempPos.z;
        
        //X-axis rotation
        Vector3 rotationXpos;
        rotationXpos.x = rotationYpos.x;
        rotationXpos.y = Mathf.Cos(rotationX) * rotationYpos.y - Mathf.Sin(rotationX) * rotationYpos.z;
        rotationXpos.z = Mathf.Sin(rotationX) * rotationYpos.y + Mathf.Cos(rotationX) * rotationYpos.z;

        tempPos = rotationXpos + new Vector3(voxelCubeSize / 2, voxelCubeSize / 2, voxelCubeSize / 2);

        return tempPos;
    }

    public void setPushTool()
    {
        toolMaterial.SetColor("_Color",pushToolColor);
        currentTool = ModificationManager.ACTION.SUBSTRACT;
        Debug.Log("Current tool now is: SUBSTRACT");
    }

    public void setPullTool()
    {
        toolMaterial.SetColor("_Color", pullToolColor);
        currentTool = ModificationManager.ACTION.ADD;
        Debug.Log("Current tool now is: ADD");
    }

    public void setSmoothTool()
    {
        toolMaterial.SetColor("_Color", smoothToolColor);
        currentTool = ModificationManager.ACTION.SMOOTH;
        Debug.Log("Current tool now is: SMOOTH"); 
    }
}
