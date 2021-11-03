using System;
using System.Collections;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class TestNativeCppRender : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage = null;
    [SerializeField] private int _width = 512;
    [SerializeField] private int _height = 512;
    [SerializeField] private Camera _camera;

    private byte[] _data = null;
    private RenderTexture _renderTexture;

    // PluginFunction
    [DllImport("nativecpprender")]
    private static extern bool SetupNativeTextureRender(IntPtr textureId1, byte[] data, int width, int height);

    [DllImport("nativecpprender")]
    private static extern void FinishNativeTextureRender();

    [DllImport("nativecpprender")]
    private static extern IntPtr GetRenderEventFunc();

    private void Start()
    {
        _renderTexture = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);
        _renderTexture.Create();
        
        _rawImage.texture = _renderTexture;
        _camera.targetTexture = _renderTexture;

        _data = new byte[_width * _height * 4];
        
        if (SetupNativeTextureRender(_renderTexture.GetNativeTexturePtr(), _data, _renderTexture.width, _renderTexture.height) == false)
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
        _renderTexture.Release();
        FinishNativeTextureRender();
    }

    private void IssueEvent(int eventID)
    {
        RenderTexture back = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        GL.IssuePluginEvent(GetRenderEventFunc(), eventID);
        RenderTexture.active = back;

        Debug.Log(_data[0]);
        Debug.Log(_data[1]);
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