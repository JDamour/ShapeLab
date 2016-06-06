using UnityEngine;
using System.Collections;

public class UIButton_ChangeParameter : MonoBehaviour
{
    public VoxelManager mymanager;
    public ModificationManager.ACTION myAction;
    private ToolButtonToggle toolButtonToggle;

    private bool active = false;

    // Use this for initialization
    void Awake()
    {
        toolButtonToggle = this.GetComponent<ToolButtonToggle>();
    }

    // Update is called once per frame
    void Update()
    {
        if (active)
        {
            mymanager.ChangeToolParameter(myAction);
        }
        else
        {
            changeState(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        active = true;
        changeState(true);
    }

    void OnTriggerExit(Collider other)
    {
        changeState(false);
        active = false;
    }

    public void changeState(bool active)
    {
        bool toggleState = toolButtonToggle.ToggleState;

        if (toggleState == active)
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
