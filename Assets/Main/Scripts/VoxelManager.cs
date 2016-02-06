using UnityEngine;
using System;
using System.Collections;
using Leap;

public class VoxelManager : MonoBehaviour {

    public float objectSize;
    private float scaling;
    public int voxelCubeSize;
    public Transform eyePos;

    public VoxelObjectGPU voxelObjectGPU;

    private int voxelFieldSize;
    private VoxelField voxel;
    private float isolevel = 0f;

    //Leap variables
    private Controller m_leapController;
    public HandController handController;
    private ModificationManager.ACTION currentTool;

    public ToolModel[] toolModels;

    private enum INTEND
    {
        MOD,
        CREATERND,
        CREATEBLOCK,
        CREATESPHERE,
        NONE
    }

    // initialization before the start() methods are called
    void Awake()
    {
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

        voxel = new VoxelField(voxelFieldSize);
        voxel.createSphere(voxelFieldSize / 3);
        initMesh();
    }

    // Update is called once per frame
    void Update () {
        

        switch (getIntent()) {
            case INTEND.CREATESPHERE:
                voxel.createSphere(voxelFieldSize / 3);
                initMesh();
                break;
            case INTEND.CREATERND:
                voxel.createRandomGrid();
                initMesh();
                break;
            case INTEND.CREATEBLOCK:
                voxel.createBlock();
                initMesh();
                break;
            case INTEND.MOD:
                Frame frame = m_leapController.Frame();
                Vector3 tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                //tipPosition *= handController.transform.localScale.x; //scale position with hand movement
                //tipPosition += handController.transform.position;
                tipPosition = handController.transform.TransformPoint(tipPosition);
                //voxelObjectGPU.setModPosition(tipPosition);
                //tipPosition = new Vector3(1, 1, 1); //TODO, for debugging
               
                //only modify if there is a tool
                if (frame.Tools[0] != null)
                {
                    Debug.Log("modding at: " + tipPosition.x + ";" + tipPosition.y + ";" + tipPosition.z);

                    //apply modification
                    voxelObjectGPU.updateMesh(tipPosition/scaling, currentTool);
                    //render new vertices
                    updateMesh();
                }
                break;
        }
        
        if (Input.GetKeyUp("1"))
        {
            currentTool = ModificationManager.ACTION.ADD;
            handController.toolModel = toolModels[0];
            Debug.Log("Current tool now is: ADD");
            handController.destroyCurrentTools();
        }
        if (Input.GetKeyUp("2"))
        {
            currentTool = ModificationManager.ACTION.SUBSTRACT;
            handController.toolModel = toolModels[1];
            Debug.Log("Current tool now is: SUBSTRACT");
            handController.destroyCurrentTools();
        }
        if (Input.GetKeyUp("3"))
        {
            currentTool = ModificationManager.ACTION.SMOOTH;
            handController.toolModel = toolModels[2];
            Debug.Log("Current tool now is: SMOOTH");
            handController.destroyCurrentTools();
        }
    }

    private INTEND getIntent()
    {
        //TODO erkennung, wann objekt berührt wird
        if (Input.GetButton("Jump"))
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

    private void initMesh()
    {
        voxelObjectGPU.initMesh(voxel);
    }

    public void updateMesh()
    {
        voxelObjectGPU.updateMesh(new Vector3(0,0,0), ModificationManager.ACTION.NONE);
    }
}
