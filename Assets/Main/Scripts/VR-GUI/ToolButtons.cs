using UnityEngine;
using System.Collections;

public class ToolButtons : MonoBehaviour {

    public VoxelManager manager;
    public ToolButtonOnClickListener pullToolButton;
    public ToolButtonOnClickListener pushToolButton;
    public ToolButtonOnClickListener smoothToolButton;

    void Start()
    {
        setPullTool();
    }

    public void setPullTool()
    {
        //Debug.Log("Set Pull Tool");
        pullToolButton.changeState(true);
        pushToolButton.changeState(false);
        smoothToolButton.changeState(false);

        manager.setPullTool();
    }

    public void setPushTool()
    {
        pullToolButton.changeState(false);
        pushToolButton.changeState(true);
        smoothToolButton.changeState(false);

        manager.setPushTool();
    }

    public void setSmoothTool()
    {
        pullToolButton.changeState(false);
        pushToolButton.changeState(false);
        smoothToolButton.changeState(true);

        manager.setSmoothTool();
    }

}
