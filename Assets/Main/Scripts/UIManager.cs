using UnityEngine;
using System.Collections;

/// <summary>
/// class that manages all UI Input
/// </summary>
public class UIManager : MonoBehaviour {

    public VoxelManager manager;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {

        if (Input.GetKeyUp("e"))
        {
            manager.export();
        }

    }
}
