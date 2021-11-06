using System;
using System.Collections;
using System.Runtime.InteropServices;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class CopyTextureData : MonoBehaviour
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

    [DllImport("copytexturedata")]
    unsafe private static extern bool SetNativeTexture(IntPtr textureId1, void* data, int width, int height);

    [DllImport("copytexturedata")]
    private static extern void FinishNativeTexture();

    [DllImport("copytexturedata")]
    private static extern IntPtr GetRenderEventFunc();

    unsafe private void Start()
    {
        _width = Screen.width;
        _height = Screen.height;
        
        _renderTexture = new RenderTexture(_width, _height, 0, RenderTextureFormat.ARGB32);
        _renderTexture.Create();

        _result = new Texture2D(_width, _height, TextureFormat.RGBA32, false);
        _resultImage.texture = _result;

        _rawImage.texture = _renderTexture;
        _camera.targetTexture = _renderTexture;

        _data = new byte[_width * _height * 4];
        _nativeArray = new NativeArray<byte>(_width * _height * 4, Allocator.Persistent);

        if (SetNativeTexture(_renderTexture.GetNativeTexturePtr(), _nativeArray.GetUnsafePtr(), _renderTexture.width,
            _renderTexture.height) == false)
        {
            return;
        }

        StartCoroutine(NativeTextureRenderLoop());
    }

    private void OnDestroy()
    {
        _renderTexture.Release();
        _nativeArray.Dispose();
        FinishNativeTexture();
    }

    private void IssueEvent()
    {
        RenderTexture back = RenderTexture.active;
        RenderTexture.active = _renderTexture;
        GL.IssuePluginEvent(GetRenderEventFunc(), 1);
        RenderTexture.active = back;

        _nativeArray.CopyTo(_data);

        _result.SetPixelData(_nativeArray, 0, 0);
        _result.Apply();
    }

    private IEnumerator NativeTextureRenderLoop()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            IssueEvent();
        }
    }
}