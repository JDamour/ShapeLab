using UnityEngine;
using System.Collections;

public class Boundaries : MonoBehaviour {

    public VoxelManager manager;

    void OnTriggerEnter(Collider other)
    {
        manager.showToolRadius(true);
    }

    void OnTriggerExit(Collider other)
    {
        manager.showToolRadius(false);
    }
}
