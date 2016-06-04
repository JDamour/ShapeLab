using UnityEngine;
using System.Collections;

public class SCManager : MonoBehaviour
{
    public Texture defaultTex;
    public Camera SCCam;
    public MeshRenderer[] SCScreenList;
    public RenderTexture[] SCTextureList;
    public RenderTexture[] SCTextureStartList;

    private int currentIndex = 0;
    // Use this for initialization
    void Start()
    {
        for (int i = 0; i < SCScreenList.Length; i++)
        {
            SCTextureList[i] = new RenderTexture(SCCam.pixelWidth, SCCam.pixelHeight, 16);
            SCTextureList[i].Create();
            SCTextureList[i].isPowerOfTwo = true;
            SCScreenList[i].material.mainTexture = defaultTex;
        }

        /*
    for (int i = 0; i< SCScreenList.Length; i++)
    {
        SCTextureList[i] = new RenderTexture(SCCam.pixelWidth, SCCam.pixelHeight, 16);
        SCTextureList[i].Create();
        SCTextureList[i].isPowerOfTwo = true;
        SCCam.targetTexture = SCTextureList[i];
        SCCam.Render();
        SCScreenList[i].material.SetTexture("", SCTextureList[i]);
    }
    SCTextureStartList = SCTextureList;
    */
    }

    public void TakeScreenShoot()
    {
        currentIndex++;
        if (currentIndex >= SCScreenList.Length)
            currentIndex = 0;
        SCCam.targetTexture = SCTextureList[currentIndex];
        SCCam.Render();
        SCScreenList[currentIndex].material.mainTexture = SCTextureList[currentIndex];
    }
    public void ResetScreenshots()
    {
        SCTextureList = SCTextureStartList;
        currentIndex = 0;
        for (int i = 0; i < SCScreenList.Length; i++)
        {
            SCScreenList[i].material.SetTexture("", SCTextureList[i]);
        }
    }
    public void ResetLastScreenshot()
    {
        //SCScreenList[currentIndex].material.mainTexture = SCTextureStartList[currentIndex];
        SCScreenList[currentIndex].material.mainTexture = defaultTex;
    }
}
