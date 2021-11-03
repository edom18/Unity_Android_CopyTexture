using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class TestNativeCppRender : MonoBehaviour
{
    [SerializeField] private RawImage _rawImage = null;
    [SerializeField] private RawImage _resultImage = null;
    [SerializeField] private int _width = 512;
    [SerializeField] private int _height = 512;
    [SerializeField] private Camera _camera;

    private byte[] _data = null;
    private RenderTexture _renderTexture;
    private Texture2D _result;
    private NativeArray<byte> _nativeArray;

    // PluginFunction
    [DllImport("nativecpprender")]
    unsafe private static extern bool SetupNativeTextureRender(IntPtr textureId1, void* data, int width, int height);

    [DllImport("nativecpprender")]
    private static extern void FinishNativeTextureRender();

    [DllImport("nativecpprender")]
    private static extern IntPtr GetRenderEventFunc();

    unsafe private void Start()
    {
        _renderTexture = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);
        _renderTexture.Create();

        _result = new Texture2D(_width, _height, TextureFormat.ARGB32, false);
        _resultImage.texture = _result;

        _rawImage.texture = _renderTexture;
        _camera.targetTexture = _renderTexture;

        _data = new byte[_width * _height * 4];
        _nativeArray = new NativeArray<byte>(_width * _height * 4, Allocator.Persistent);

        if (SetupNativeTextureRender(_renderTexture.GetNativeTexturePtr(), _nativeArray.GetUnsafePtr(), _renderTexture.width,
            _renderTexture.height) == false)
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
        _nativeArray.Dispose();
        FinishNativeTextureRender();
    }

    private void IssueEvent(int eventID)
    {
        RenderTexture back = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        GL.IssuePluginEvent(GetRenderEventFunc(), eventID);
        RenderTexture.active = back;

        Debug.Log("!!!!!!!!!!!!!!!!");

        GetData();

        _result.SetPixelData(_nativeArray, 0, 0);
        _result.Apply();

        Debug.Log("==================");
    }

    private void GetData()
    {
        _nativeArray.CopyTo(_data);
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