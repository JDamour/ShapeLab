using UnityEngine;
using System;
using Leap;
using UnityEngine.UI;
using System.Collections;

/// <summary>
/// Manager of the Sculpting Project
/// manages all input and modification
/// </summary>
public class VoxelManager : MonoBehaviour
{
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
    private float objectScaling;

    //scaling and moving
    private bool moveObject = false;
    private Vector3 startMovePosition = new Vector3(0f, 0f, 0f);
    private Vector3 lastMoveOffset = new Vector3(0f, 0f, 0f);
    private Vector3 posOffset = new Vector3(0f, 0f, 0f);

    private Vector3 boundingBoxOffset;

    //TODO: create a UI Manager
    // UI elements
    public Text radiusText;
    public Text strengthText;
    public Text sessionIDText;
    public GameObject CountdownBox;
    public Text countdownCanvas;
    public float timeMax;
    private float timeRemaining;

    private string sessionID = "placeholder";
    private int userID = 0;

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
        MOVE,
        MOVEINIT,
        NONE
    }

    // initialization before the start() methods are called
    void Awake()
    {
        objectScaling = 1.0f;
        rotation = new Vector3(0f, 0f, 0f);
        if (voxelCubeSize % 8 != 0)
        {
            Debug.Log("The dimension of the voxelCubeField has to be a multiple of 8");
        }
        voxelFieldSize = voxelCubeSize + 1;
        scaling = objectSize * objectScaling / (float)voxelCubeSize;
        voxelObjectGPU.setInitData(voxelCubeSize, scaling);
        cmdQueue = new System.Collections.Generic.Queue<StatefulMain.Command>();
        resetBoundingBoxPosition();
    }

    // initialization
    void Start()
    {
        m_leapController = handController.GetLeapController();

        updateRadiusText();
        updateStrengthText();
        toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
        voxel = new VoxelField(voxelFieldSize);
        voxel.createSphere(voxelFieldSize / 3);
        initMesh(true);
        timeRemaining = timeMax;
    }

    // Update is called once per frame
    void Update()
    {
        updateSessionIDText();
        Frame frame = m_leapController.Frame();
        Vector3 tipPosition = new Vector3(0.5f, 0.5f, 0.0f);
        switch (getIntent())
        {
            #region intend computing
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
            case INTEND.MOVEINIT:
                if (frame.Tools.Count != 0)
                {
                    startMovePosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                    startMovePosition = handController.transform.TransformPoint(startMovePosition);
                    lastMoveOffset = posOffset;
                    
                }
                else
                {
                    moveObject = false;
                    //debug nachricht das die leap sichtbar sein muss zum bewegen
                }
                break;

            case INTEND.MOVE:
                if (moveObject == true)
                {
                    if (frame.Tools.Count == 0)
                    {
                        return;
                    }
                    tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                    tipPosition = handController.transform.TransformPoint(tipPosition);

                    posOffset = lastMoveOffset + (tipPosition - startMovePosition);
                    resetBoundingBoxPosition();
                    updateMesh();
                }
                break;
            case INTEND.MOD:

                if (m_leapController.IsConnected)
                {

                    if (frame.Tools.Count == 0) //only modify if there is a tool
                        return;

                    tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
                    tipPosition = handController.transform.TransformPoint(tipPosition);

                }
                else {
                    // for testing purposes
                    //Debug.Log("modding at: " + tipPosition.x / scaling + ";" + tipPosition.y / scaling + ";" + tipPosition.z / scaling);
                }
                tipPosition -= posOffset;

                //apply modification
                voxelObjectGPU.applyToolAt(getRotatedPosition(tipPosition / scaling), currentTool, objectScaling);
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
                #endregion
        }
        //check, if server has send any commands
        if (cmdQueue.Count > 0)
        {
            #region Webinterface
            StatefulMain.Command newCommand = cmdQueue.Dequeue();

            switch (newCommand)
            {
                case StatefulMain.Command.RESET_ALL:
                    {
                        resetAll(true);
                        voxelObjectGPU.resetTools();
                        Debug.Log("(Servercmd) reseting everything");
                    }
                    break;
                case StatefulMain.Command.RESET_SCREENSHOTS:
                    {
                        scmanager.ResetScreenshots();
                        Debug.Log("(Servercmd) reseting screenshot-storage");
                    }
                    break;
                case StatefulMain.Command.RESET_TOOLS:
                    {
                        voxelObjectGPU.resetTools();
                        Debug.Log("(Servercmd) reseting tool parameter");
                    }
                    break;
                case StatefulMain.Command.NEXT_USER:
                    {
                        userID += 1;
                        resetObjectTransformForScreeshot();
                        //make screenshoot of user generated model
                        scmanager.TakeScreenShoot();
                        //reset all for next user
                        resetAll(true);
                        voxelObjectGPU.resetTools();
                        //reset timer
                        timeRemaining = timeMax;
                        Debug.Log("(Servercmd) preparing programm for next user");
                    }
                    break;
                case StatefulMain.Command.TAKE_SCREENSHOT:
                    {
                        resetObjectTransformForScreeshot();
                        scmanager.TakeScreenShoot();
                        Debug.Log("(Servercmd) rendering screenshot");
                    }
                    break;
                case StatefulMain.Command.DELETE_LAST_SCREENSHOT:
                    {
                        scmanager.ResetLastScreenshot();
                        Debug.Log("(Servercmd) rendering screenshot");
                    }
                    break;
                case StatefulMain.Command.RESET_HMD_LOCATION:
                    {
                        UnityEngine.VR.InputTracking.Recenter();
                        Debug.Log("(Servercmd) recenter HMD");
                    }
                    break;
                case StatefulMain.Command.MAX_TIME_3_MINUTES:
                    {
                        this.timeMax = 180.0f;
                    }
                    break;
                case StatefulMain.Command.MAX_TIME_5_MINUTES:
                    {
                        this.timeMax = 300.0f;
                    }
                    break;
                case StatefulMain.Command.MAX_TIME_UNLIMITED:
                    {
                        this.timeMax = -1.0f;
                    }
                    break;
                default:
                    {
                        Debug.Log("I should do something with this \"" + cmdQueue.Dequeue().ToString() + "\"command...");
                    }
                    break;
            }
#endregion
        }

        if(timeMax > 0 && timeRemaining >= 0)
            this.timeRemaining -= Time.deltaTime;
        if(timeRemaining < 60)
        {
            this.CountdownBox.SetActive(true);
            if(timeRemaining > 1)
                this.countdownCanvas.text = "Countdown:\n"+timeRemaining.ToString("0.00");
            else
                this.countdownCanvas.text = "Countdown:\n"+"0.00";
        }
        if(timeRemaining % 30 == 0)
            Debug.Log("time Left:" + timeRemaining);
        if (timeRemaining == 0)
        {
            // TODO alert user
            Debug.Log("time is up, next user!");
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

    private void resetAll(bool resetByServer)
    {
        if (resetByServer)
        {
            //todo check if current object should be exported
            export(); // transmit current id?
            //todo answer to server "all okay"?
        }
        Debug.Log("Reseting environment");
        updateRadiusText();
        updateStrengthText();
        toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());

        objectScaling = 1.0f;
        scaling = objectSize * objectScaling / (float)voxelCubeSize;
        
        rotation = Vector3.zero;
        posOffset = Vector3.zero;
        resetBoundingBoxPosition();
        voxel = new VoxelField(voxelFieldSize);
        voxel.createSphere(voxelFieldSize / 3);
        initMesh(true);
        voxelObjectGPU.modManager.ResetToolRange();
        
    }

    private void resetTools()
    {
        updateRadiusText();
        updateStrengthText();
        toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
    }

    private void resetObjectTransformForScreeshot()
    {
        objectScaling = 1.0f;
        scaling = objectSize * objectScaling / (float)voxelCubeSize;

        rotation = Vector3.zero;
        posOffset = Vector3.zero;
        resetBoundingBoxPosition();
        updateMesh();
    }


    // get the Intend of the current action
    private INTEND getIntent()
    {
        //controler actions
        #region Controller parsing
        if (Input.GetAxis("PadStickVertical") > 0.6 || Input.GetAxis("PadStickVertical") < -0.6)
        {
            if (Input.GetAxis("PadStickVertical") > 0.6)
            {
                objectScaling = objectScaling + 0.02f;
            }
            else if (Input.GetAxis("PadStickVertical") < -0.6)
            {
                objectScaling = Mathf.Max(objectScaling - 0.02f, 0.3f);
            }
            scaling = objectSize * objectScaling / (float)voxelCubeSize;
            voxelObjectGPU.setScale(scaling);
            boundaries.localScale = new Vector3(objectScaling, objectScaling, objectScaling);
        }

        if (Input.GetAxis("PadStickHorizontal") > 0.5 || Input.GetAxis("PadStickHorizontal") < -0.5)
        {
            if (Input.GetAxis("PadStickHorizontal") > 0.5)
            {
                rotation.y = (rotation.y + 1 + 360) % 360;
            }
            else if (Input.GetAxis("PadStickHorizontal") < -0.5)
            {
                rotation.y = (rotation.y - 1 + 360) % 360;
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
            updateRadiusText();
        }
        if (Input.GetAxis("PadAnalogCrossHorizontal") > 0 ||
            Input.GetKey(KeyCode.RightArrow))
        {
            // increasing tool range
            voxelObjectGPU.getModificationManager().ChangeToolRange(0.1f);
            toolMaterial.SetFloat("_Radius", voxelObjectGPU.modManager.getToolRadius());
            updateRadiusText();
        }
        if (Input.GetAxis("PadAnalogCrossVertical") < 0 ||
            Input.GetKey(KeyCode.DownArrow))
        {
            // reducing tool strength
            voxelObjectGPU.getModificationManager().ChangeToolStrength(-0.005f);
            updateStrengthText();
        }
        if (Input.GetAxis("PadAnalogCrossVertical") > 0 ||
            Input.GetKey(KeyCode.UpArrow))
        {
            // increasing tool strength
            voxelObjectGPU.getModificationManager().ChangeToolStrength(0.005f);
            updateStrengthText();
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
        // updateMesh if rotation is changed during this frame
        if (Input.GetAxis("PadStickHorizontal") != 0 || Input.GetAxis("PadStickVertical") != 0)
        {
            voxelObjectGPU.updateMesh(rotation, posOffset);
        }
        #endregion


        // keyboard shortcuts for debugging

        if (Input.GetButtonDown("ModButton"))
        {
            moveObject = true;
            return INTEND.MOVEINIT;
        }

        if (Input.GetButtonUp("ModButton"))
        {
            moveObject = false;
        }

        if (Input.GetButton("ModButton") || Input.GetButton("Jump"))
        {
            return INTEND.MOVE;
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

    /// <summary>
    /// called to hide or show the modification radius of the toool
    /// </summary>
    /// <param name="show">set to true, if the modification radius should be shown</param>
    public void showToolRadius(bool show)
    {
        if (show)
        {
            toolMaterial.SetFloat("_Transparency", 1.0f);
        }
        else {
            toolMaterial.SetFloat("_Transparency", 0.0f);
        }
    }

    public void setSessionID(string id)
    {
        sessionID = id;
    }

    private void updateBoundaries()
    {
        boundaries.transform.rotation = Quaternion.Euler(new Vector3(rotation.x, -rotation.y, rotation.z));
        resetBoundingBoxPosition();
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
        voxelObjectGPU.updateMesh(rotation, posOffset);
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
        toolMaterial.SetColor("_Color", pushToolColor);
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
    /// called by buttons in VR-UI
    /// </summary>
    public void ChangeToolParameter(ModificationManager.ACTION action)
    {
        switch (action)
        {
            case ModificationManager.ACTION.ADD_POWER:
                {
                    voxelObjectGPU.getModificationManager().ChangeToolStrength(0.01f);
                    updateStrengthText();
                }
                break;
            case ModificationManager.ACTION.SUB_POWER:
                {
                    voxelObjectGPU.getModificationManager().ChangeToolStrength(-0.01f);
                    updateStrengthText();
                }
                break;
            case ModificationManager.ACTION.ADD_RANGE:
                {
                    voxelObjectGPU.getModificationManager().ChangeToolRange(0.01f);
                    updateRadiusText();
                }
                break;
            case ModificationManager.ACTION.SUB_RANGE:
                {
                    voxelObjectGPU.getModificationManager().ChangeToolRange(-0.01f);
                    updateRadiusText();
                }
                break;
        }
    }

    private void updateRadiusText()
    {
        radiusText.text = "Radius: " + ((int)(voxelObjectGPU.getModificationManager().getToolRadius() * 100)) / 100f;
        
    }

    private void updateStrengthText()
    {
        strengthText.text = "Strength: " + ((int)(voxelObjectGPU.getModificationManager().getToolStrength() * 100)) / 100f;
    }

    private void updateSessionIDText()
    {
        sessionIDText.text = "Session ID: " + sessionID.Substring(0,3).ToUpper() + userID.ToString();
    }

    /// <summary>
    /// calls voxel object to export
    /// </summary>
    public void export()
    {
        voxelObjectGPU.exportObject(sessionID + "_" + userID);
    }

    private void resetBoundingBoxPosition()
    {
        boundaries.transform.position = posOffset + new Vector3((objectSize * objectScaling)/2, (objectSize * objectScaling) / 2, (objectSize * objectScaling) / 2);
    }
}
