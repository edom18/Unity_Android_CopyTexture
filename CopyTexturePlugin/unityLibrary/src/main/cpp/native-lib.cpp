#include "IUnityInterface.h"
#include "IUnityGraphics.h"
//#include "RenderAPI.h"
#include <math.h>
#include <stdio.h>
#include <assert.h>
#include <GLES3/gl3.h>

static GLuint  g_textureId = NULL;
static int     g_texWidth;
static int     g_texHeight;
static u_char* g_pBytes = NULL;
static void*   g_data = NULL;

#define LOG_PRINTF printf

extern "C" bool SetupNativeTextureRender( void* textureId, void* data, int width, int height )
{
    g_textureId = (GLuint)(size_t)textureId;
    g_data = data;
    g_texWidth = width;
    g_texHeight = height;

    LOG_PRINTF( "SetupNativeTextureRender:%d, %d, %d", g_textureId, g_texWidth, g_texHeight );

    g_pBytes = new u_char[ g_texWidth * g_texHeight * 4 ];

    return true;
}

extern "C" void FinishNativeTextureRender()
{
    if( g_pBytes != NULL )
    {
        delete[] g_pBytes;
    }
    g_pBytes = NULL;

    if (g_data != NULL)
    {
        delete[] g_data;
    }
    g_data = NULL;
}

static void ReadPixels(void* data)
{
//    int currentFBO;
//    glGetIntegerv(GL_FRAMEBUFFER, &currentFBO);
//
//    glBindFramebuffer(GL_FRAMEBUFFER, currentFBO);

    int currentFBORead;
    int currentFBOWrite;
    glGetIntegerv(GL_READ_FRAMEBUFFER_BINDING, &currentFBORead);
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &currentFBOWrite);

    glBindFramebuffer(GL_READ_FRAMEBUFFER, currentFBOWrite);

    glReadPixels(0, 0, g_texWidth, g_texHeight, GL_RGBA, GL_UNSIGNED_BYTE, g_pBytes);

    glBindFramebuffer(GL_READ_FRAMEBUFFER, currentFBORead);
}

static void UNITY_INTERFACE_API OnRenderEvent( int eventID )
{
    ReadPixels(g_data);
}

//static void UNITY_INTERFACE_API OnRenderEvent( int eventID )
//{
//    glBindTexture( GL_TEXTURE_2D, g_textureId );
//
//    static u_char s_r = 0;
//    u_char* bytes = g_pBytes;
//    u_char* result = (u_char*)g_data;
//
//    for( int y = 0; y < g_texHeight; y++ )
//    {
//        for( int x = 0; x < g_texWidth; x++ )
//        {
//            int offset = ( ( y * g_texWidth ) + x ) * 4;
//            bytes[ offset + 0 ] = s_r;
//            bytes[ offset + 1 ] = 0;
//            bytes[ offset + 2 ] = 0;
//            bytes[ offset + 3 ] = 255;
//
//            result[ offset + 0 ] = s_r;
//            result[ offset + 1 ] = s_r;
//            result[ offset + 2 ] = s_r;
//            result[ offset + 3 ] = 255;
//        }
//    }
//
//    glTexSubImage2D( GL_TEXTURE_2D, 0, 0, 0, g_texWidth, g_texHeight, GL_RGBA, GL_UNSIGNED_BYTE, bytes );
//    s_r ++;
//    s_r %= 255;
//}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return OnRenderEvent;
}

// --------------------------------------------------
// Unity Set Interfaces
static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType);

static IUnityInterfaces* s_UnityInterfaces = NULL;
static IUnityGraphics* s_Graphics = NULL;

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginLoad(IUnityInterfaces* unityInterfaces)
{
    LOG_PRINTF("Called a load callback.");

    s_UnityInterfaces = unityInterfaces;
    s_Graphics = s_UnityInterfaces->Get<IUnityGraphics>();
    s_Graphics->RegisterDeviceEventCallback(OnGraphicsDeviceEvent);

    OnGraphicsDeviceEvent(kUnityGfxDeviceEventInitialize);
}

extern "C" void UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API UnityPluginUnlaod()
{
    s_Graphics->UnregisterDeviceEventCallback(OnGraphicsDeviceEvent);
}

// -----------------------------------------------------
// Graphics Device Event

//static RenderAPI* s_CurrentAPI = NULL;
static UnityGfxRenderer s_DeviceType = kUnityGfxRendererNull;

static void UNITY_INTERFACE_API OnGraphicsDeviceEvent(UnityGfxDeviceEventType eventType)
{
    LOG_PRINTF("OnGraphicsDeviceEvent!!!");

    // Create graphics API implementation upon initialization
    if (eventType == kUnityGfxDeviceEventInitialize)
    {
//        assert(s_CurrentAPI == NULL);
        s_DeviceType = s_Graphics->GetRenderer();
//        s_CurrentAPI = CreateRenderAPI(s_DeviceType);
    }

//    if (s_CurrentAPI)
//    {
//        s_CurrentAPI->ProcessDeviceEvent(eventType, s_UnityInterfaces);
//    }

    if (eventType == kUnityGfxDeviceEventShutdown)
    {
//        delete s_CurrentAPI;
//        s_CurrentAPI = NULL;
        s_DeviceType = kUnityGfxRendererNull;
    }
}