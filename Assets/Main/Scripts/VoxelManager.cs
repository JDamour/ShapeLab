using UnityEngine;
using System;
using Leap;
using UnityEngine.UI;

/// <summary>
/// Manager of the Sculpting Project
/// manages all input and modification
/// </summary>
public class VoxelManager : MonoBehaviour {

    public float objectSize;
    private float scaling;
    public int voxelCubeSize;
    public Transform eyePos;
    public Transform boundaries;

    public VoxelObjectGPU voxelObjectGPU;
    public SCManager scmanager;

    private System.Collections.Generic.Queue<StatefulMain.Command> cmdQueue;

    private int voxelFieldSize;
    private VoxelField voxel;

    // Leap variables
    private Controller m_leapController;
    public HandController handController;
    private ModificationManager.ACTION currentTool;

    public SphereTool tool;
    private SphereTool currentToolObject;
    public Material toolMaterial;

    // Tool colors
    public Color pushToolColor;
    public Color pullToolColor;
    public Color smoothToolColor;

    private Vector3 rotation;

    //TODO: create a UI Manager
    // UI elements
    public Text radiusText;
    public Text strengthText;

    private enum INTEND
    {
        MOD,
        RESETVIEW,
        RESETALL,
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
        cmdQueue = new System.Collections.Generic.Queue<StatefulMain.Command>();
    }

    // initialization
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
                updateMesh();

                break;
            case INTEND.RESETVIEW:
                UnityEngine.VR.InputTracking.Recenter();
                break;
            case INTEND.RESETALL:
                //todo call function from server via websocket
                resetAll(false);
                break;
            case INTEND.NONE:
                break;
        }

        //check, if server has send any commands
        if(cmdQueue.Count > 0)
        {
            StatefulMain.Command newCommand = cmdQueue.Dequeue();

            switch (newCommand)
            {
                case StatefulMain.Command.RESET_ALL:
                    {
                        resetAll(true);
                        Debug.Log("(Servercmd) reseting everything");
                    }
                    break;
                case StatefulMain.Command.RESET_SCREENSHOTS:
                    {
                        resetAll(true);
                        Debug.Log("(Servercmd) reseting screenshot-storage");
                    }
                    break;
                case StatefulMain.Command.RESET_TOOLS:
                    {
                        resetAll(true);
                        Debug.Log("(Servercmd) reseting tool parameter");
                    }
                    break;
                case StatefulMain.Command.NEXT_USER:
                    {
                        resetAll(true);
                        Debug.Log("(Servercmd) preparing programm for next user");
                    }
                    break;
                case StatefulMain.Command.TAKE_SCREENSHOT:
                    {
                        scmanager.TakeScreenShoot();
                        Debug.Log("(Servercmd) rendering screenshot");
                    }
                    break;
                default:
                    {
                        Debug.Log("I should do something with this \"" + cmdQueue.Dequeue().ToString() + "\"command...");
                    }
                    break;
            }
            
        }
        
        // change tools manualy with keyboard for testing
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

    public void queueServerCommand(StatefulMain.Command cmd)
    {
        //Debug.Log("Server said: "+cmd.ToString()+", queueing");
        cmdQueue.Enqueue(cmd);
    }

    private void resetAll( bool resetByServer)
    {
        if (resetByServer)
        {
            //todo check if current object should be exported
            export(); // transmit current id?
            //todo answer to server "all okay"?
        }
        Debug.Log("Reseting environment");
        radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius() * 100)) / 100f;
        strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
        toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());

        rotation = Vector3.zero;
        voxel = new VoxelField(voxelFieldSize);
        voxel.createSphere(voxelFieldSize / 3);
        initMesh(true);
        voxelObjectGPU.modManager.ResetToolRange();
    }

    // get the Intend of the current action
    private INTEND getIntent()
    {
        
        //TODO erkennung, wann objekt berührt wird
        if (Input.GetAxis("PadStickVertical") != 0)
        {
            if (Input.GetAxis("PadStickVertical") > 0) { 
                rotation.x = (rotation.x + 1 + 360) %360;
            }
            else
            {
                rotation.x = (rotation.x - 1 + 360) %360;
            }
            //Debug.Log("Rotation X: " + rotation.x);
        }
        if (Input.GetAxis("PadStickHorizontal") != 0)
        {
            if (Input.GetAxis("PadStickHorizontal") > 0)
            {
                rotation.y = (rotation.y + 1 + 360)%360;
            }
            else
            {
                rotation.y = (rotation.y - 1 + 360) %360;
            }
            //Debug.Log("Rotation Y: " + rotation.y);
        }
        
            updateBoundaries();
        if (Input.GetAxis("PadAnalogCrossHorizontal") < 0 ||
            Input.GetKey(KeyCode.LeftArrow))
        {
            // reducing tool range
            voxelObjectGPU.getModificationManager().ChangeToolRange(-0.1f);
            toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
            radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius()*100))/100f;
        }
        if (Input.GetAxis("PadAnalogCrossHorizontal") > 0 ||
            Input.GetKey(KeyCode.RightArrow))
        {
            // increasing tool range
            voxelObjectGPU.getModificationManager().ChangeToolRange(0.1f);
            toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
            radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius() * 100)) / 100f;
        }
        if (Input.GetAxis("PadAnalogCrossVertical") < 0 ||
            Input.GetKey(KeyCode.DownArrow))
        {
            // reducing tool strength
            voxelObjectGPU.getModificationManager().ChangeToolStrength(-0.005f);
            strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
        }
        if (Input.GetAxis("PadAnalogCrossVertical") > 0 ||
            Input.GetKey(KeyCode.UpArrow))
        {
            // increasing tool strength
            voxelObjectGPU.getModificationManager().ChangeToolStrength(0.005f);
            strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
        }
        if (Input.GetAxis("PadTrigger") != 0)
        {
            // activate modding and set strength
            // overrides previous toolpower, but once you go trigger, you never go back ;)
            voxelObjectGPU.getModificationManager().SetToolPower(Input.GetAxis("PadTrigger"));
            return INTEND.MOD;
        }
        if (Input.GetButton("PadResetButton"))
        {
            return INTEND.RESETALL;
        }
        if (Input.GetButton("PadResetViewButton"))
        {
            return INTEND.RESETVIEW;
        }
        

        // keyboard shortcuts for debugging
        
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

        // updateMesh if rotation is changed during this frame
        if (Input.GetAxis("PadStickVertical") != 0 || Input.GetAxis("PadStickHorizontal") != 0)
        {
            voxelObjectGPU.updateMesh(rotation);
        }
        return INTEND.NONE;
    }

    /// <summary>
    /// called to hide or show the modification radius of the toool
    /// </summary>
    /// <param name="show">set to true, if the modification radius should be shown</param>
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

    /// <summary>
    /// Call to init the startmesh. 
    /// </summary>
    /// <param name="withSmooth">Set to true,if the mesh should be smoothed from the beginning.</param>
    private void initMesh(bool withSmooth)
    {
        voxelObjectGPU.initMesh(voxel, rotation, withSmooth);
    }

    //TODO: find the flickering problem, to only call updateMesh once
    /// <summary>
    /// update of the Mesh. Needs to be called to times to avoid Mesh flickering
    /// </summary>
    public void updateMesh()
    {
        voxelObjectGPU.updateMesh(rotation);
        //voxelObjectGPU.updateMesh(rotation);
    }

    /// <summary>
    /// Rotate a given Vector3 based on the Rotation of the Object.
    /// </summary>
    /// <param name="position">The Vector3 that shoul dbe rotated./param>
    /// <returns></returns>
         
    protected Vector3 getRotatedPosition(Vector3 position)
    {
        Vector3 tempPos = position - new Vector3(voxelCubeSize / 2, voxelCubeSize / 2, voxelCubeSize / 2);

        //degree >> radians
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

    /// <summary>
    /// Set currentTool to Substract Tool. Sets the color in the material.
    /// </summary>
    public void setPushTool()
    {
        toolMaterial.SetColor("_Color",pushToolColor);
        currentTool = ModificationManager.ACTION.SUBSTRACT;
        Debug.Log("Current tool now is: SUBSTRACT");
    }

    /// <summary>
    /// Set current Tool to Add Tool.
    /// </summary>
    public void setPullTool()
    {
        toolMaterial.SetColor("_Color", pullToolColor);
        currentTool = ModificationManager.ACTION.ADD;
        Debug.Log("Current tool now is: ADD");
    }

    /// <summary>
    /// Set current Tool to Smooth Tool
    /// </summary>
    public void setSmoothTool()
    {
        toolMaterial.SetColor("_Color", smoothToolColor);
        currentTool = ModificationManager.ACTION.SMOOTH;
        Debug.Log("Current tool now is: SMOOTH"); 
    }

    /// <summary>
    /// calls voxel object to export
    /// </summary>
    public void export()
    {
        voxelObjectGPU.exportObject();
    }
}
