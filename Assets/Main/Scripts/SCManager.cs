using UnityEngine;
using System.Collections;

public class SCManager : MonoBehaviour
{
    public Texture defaultTex;
    public Camera SCCam;
    public MeshRenderer[] SCScreenList;
    private RenderTexture[] SCTextureList;
    private RenderTexture[] SCTextureStartList;

    private int currentScreenIndex = 0;
    // Use this for initialization
    void Start()
    {
        SCTextureList = new RenderTexture[SCScreenList.Length];
        for (int i = 0; i < SCScreenList.Length; i++)
        {
            SCTextureList[i] = createRenderTexture(SCCam.pixelWidth, SCCam.pixelHeight);
            SCScreenList[i].material.mainTexture = defaultTex;
        }
    }

    public RenderTexture createRenderTexture(int width, int height)
    {
        RenderTexture tmp = new RenderTexture(width, height, 16);
        tmp.Create();
        tmp.isPowerOfTwo = true;
        return tmp;
    }

    public void TakeScreenShoot()
    {
        currentScreenIndex++;
        if (currentScreenIndex >= SCScreenList.Length)
            currentScreenIndex = 0;
        SCCam.targetTexture = SCTextureList[currentScreenIndex];
        SCCam.Render();
        SCScreenList[currentScreenIndex].material.mainTexture = SCTextureList[currentScreenIndex];
    }
    public void ResetScreenshots()
    {
        // delete old texture list
        SCTextureList = new RenderTexture[SCScreenList.Length];
        for (int i = 0; i < SCScreenList.Length; i++)
        {
            SCTextureList[i] = createRenderTexture(SCCam.pixelWidth, SCCam.pixelHeight);
        }
        currentScreenIndex = 0;
        for (int i = 0; i < SCScreenList.Length; i++)
        {
            SCScreenList[i].material.SetTexture("", SCTextureList[i]);
        }
    }
    public void ResetLastScreenshot()
    {
        //SCScreenList[currentIndex].material.mainTexture = SCTextureStartList[currentIndex];
        SCScreenList[currentScreenIndex].material.mainTexture = defaultTex;
    }
}
