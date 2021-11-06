#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include <math.h>
#include <stdio.h>
#include <assert.h>
#include <GLES3/gl3.h>

static GLuint  g_textureId = NULL;
static int     g_texWidth;
static int     g_texHeight;
static void*   g_data = NULL;

static void UNITY_INTERFACE_API OnRenderEvent(int eventID);

#define LOG_PRINTF printf

extern "C" bool SetNativeTexture(void* textureId, void* data, int width, int height)
{
    g_textureId = (GLuint)(size_t)textureId;
    g_data = data;
    g_texWidth = width;
    g_texHeight = height;

    LOG_PRINTF("SetNativeTexture:%d, %d, %d", g_textureId, g_texWidth, g_texHeight);

    return true;
}

extern "C" void FinishNativeTexture()
{
    if (g_data != NULL)
    {
        delete[] g_data;
    }
    g_data = NULL;
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return OnRenderEvent;
}

static void ReadPixels()
{
    int currentFBOWrite;
    glGetIntegerv(GL_DRAW_FRAMEBUFFER_BINDING, &currentFBOWrite);
    glBindFramebuffer(GL_READ_FRAMEBUFFER, currentFBOWrite);

    glReadPixels(0, 0, g_texWidth, g_texHeight, GL_RGBA, GL_UNSIGNED_BYTE, g_data);
}

static void UNITY_INTERFACE_API OnRenderEvent(int eventID)
{
    ReadPixels();
}
