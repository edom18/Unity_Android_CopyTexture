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

    //PluginFunction
    [DllImport ("nativecpprender")]
    private static extern bool SetupNativeTextureRender( IntPtr textureId, int width, int height );
    [DllImport ("nativecpprender")]
    private static extern void FinishNativeTextureRender();
    [DllImport ("nativecpprender")]
    private static extern IntPtr GetRenderEventFunc();

    private void Start()
    {
        var texture = new Texture2D( _width, _height, TextureFormat.ARGB32, false );
        _rawImage.texture = texture;
        if( SetupNativeTextureRender( texture.GetNativeTexturePtr(), texture.width, texture.height ) == false )
            return;

        StartCoroutine( NativeTextureRenderLoop() );
    }

    private void OnDestroy()
    {
        FinishNativeTextureRender();
    }

    private IEnumerator NativeTextureRenderLoop()
    {
        while( true )
        {
            yield return new WaitForEndOfFrame();
            GL.IssuePluginEvent( GetRenderEventFunc(), 1 );
        }
    }
}