using UnityEngine;
using System.Collections;

using Leap;
using System;

public class ToolManager : MonoBehaviour
{

    public HandController handController;
    public ToolModel[] toolModels;

    public TOOL currentTool;

    public enum TOOL
    {
        PUSH_TOOL,
        PULL_TOOL,
        SMOOTH_TOOL
    }

    private Controller m_leapController;
    private Vector3 tipPosition;

    // Use this for initialization
    void Start()
    {
        m_leapController = handController.GetLeapController();

        currentTool = TOOL.PUSH_TOOL;
    }

    // Update is called once per frame
    void Update()
    {
        Frame frame = m_leapController.Frame();

        Vector3 tipPosition = frame.Tools[0].TipPosition.ToUnityScaled(false);
        tipPosition *= handController.transform.localScale.x; //scale position with hand movement
        tipPosition += handController.transform.position;
    }

    public void setPushTool()
    {
        currentTool = TOOL.PUSH_TOOL;
        handController.toolModel = toolModels[0];
        //Debug.Log("Push Tool Selected");
    }

    public void setPullTool()
    {
        currentTool = TOOL.PULL_TOOL;
        handController.toolModel = toolModels[1];
        //Debug.Log("Pull Tool Selected");
    }

    public void setSmoothingTool()
    {
        currentTool = TOOL.SMOOTH_TOOL;
        handController.toolModel = toolModels[2];
        //Debug.Log("Smoothing Tool Selected");
    }

}
