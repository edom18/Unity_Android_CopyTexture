using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class TestNativeCppRender : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage1 = null;
    [SerializeField] private RawImage _rawImage2 = null;
    [SerializeField] private int _width = 512;
    [SerializeField] private int _height = 512;

    // PluginFunction
    [DllImport("nativecpprender")]
    private static extern bool SetupNativeTextureRender(IntPtr textureId1, IntPtr textureId2, int width, int height);

    [DllImport("nativecpprender")]
    private static extern void FinishNativeTextureRender();

    [DllImport("nativecpprender")]
    private static extern IntPtr GetRenderEventFunc();

    private void Start()
    {
        // var texture = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
        var texture1 = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);
        texture1.Create();
        var texture2 = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);
        texture2.Create();
        
        _rawImage1.texture = texture1;
        _rawImage2.texture = texture2;
        
        if (SetupNativeTextureRender(texture1.GetNativeTexturePtr(), texture2.GetNativeTexturePtr(), texture1.width, texture1.height) == false)
        {
            return;
        }

        // StartCoroutine(NativeTextureRenderLoop());
    }

    private void Update()
    {
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            if (touch.phase == TouchPhase.Began)
            {
                IssueEvent(Input.touchCount >= 2 ? 2 : 1);
            }
        }
    }

    private void OnDestroy()
    {
        FinishNativeTextureRender();
    }

    private void IssueEvent(int eventID)
    {
        GL.IssuePluginEvent(GetRenderEventFunc(), eventID);
    }

    // private IEnumerator NativeTextureRenderLoop()
    // {
    //     while (true)
    //     {
    //         yield return new WaitForEndOfFrame();
    //         GL.IssuePluginEvent(GetRenderEventFunc(), 1);
    //     }
    // }
}