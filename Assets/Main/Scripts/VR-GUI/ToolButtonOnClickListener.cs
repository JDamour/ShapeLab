using UnityEngine;
using System.Collections;

public class ToolButtonOnClickListener : MonoBehaviour {

    private ToolButtonToggle toolButtonToggle;

    // Use this for initialization
    void Start () {
        toolButtonToggle = this.GetComponent<ToolButtonToggle>();

    }
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnTriggerEnter(Collider other)
    {
        if (toolButtonToggle.ToggleState == true)
        {
            toolButtonToggle.ButtonTurnsOff();
        }
        else
        {
            toolButtonToggle.ButtonTurnsOn();
        }
    }
}
