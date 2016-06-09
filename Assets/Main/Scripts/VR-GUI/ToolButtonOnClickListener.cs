using UnityEngine;
using System.Collections;

public class ToolButtonOnClickListener : MonoBehaviour {

    public ToolButtons toolButtons;
    public string tool;
    private ToolButtonToggle toolButtonToggle;

    // Use this for initialization
    void Awake () {
        toolButtonToggle = this.GetComponent<ToolButtonToggle>();
    }

    void Start()
    {
        if (tool == "Pull")
        {
            changeState(true);
        } else
        {
            changeState(false);
        }
    }
	
    void OnTriggerEnter(Collider other)
    {
        if(tool == "Push")
        {
            toolButtons.setPushTool();
        }
        if (tool == "Pull")
        {
            toolButtons.setPullTool();
        }
        if (tool == "Smooth")
        {
            toolButtons.setSmoothTool();
        }
    }

    public void changeState(bool active)
    {
        bool toggleState = toolButtonToggle.ToggleState;

        if(toggleState == active)
        {
            return;
        }
        if (toggleState != active)
        {
            if (toggleState)
            {
                toolButtonToggle.ButtonTurnsOff();
            }
            else
            {
                toolButtonToggle.ButtonTurnsOn();
            }

            toolButtonToggle.ToggleState = !toggleState;
        }
    }
}
