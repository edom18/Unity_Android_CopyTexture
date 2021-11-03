#include "IUnityInterface.h"
#include "IUnityGraphics.h"
#include <math.h>
#include <stdio.h>
#include <assert.h>
#include <GLES2/gl2.h>

static GLuint  g_textureId = NULL;
static int     g_texWidth;
static int     g_texHeight;
static GLuint  g_textureId2 = NULL;
static u_char* g_pBytes = NULL;

#define LOG_PRINTF printf

extern "C" bool SetupNativeTextureRender( void* textureId, void* textureId2, int width, int height )
{
    g_textureId = (GLuint)(size_t)textureId;
    g_textureId2 = (GLuint)(size_t)textureId2;
    g_texWidth = width;
    g_texHeight = height;

    LOG_PRINTF( "SetupNativeTextureRender:%d, %d, %d", g_textureId, g_texWidth, g_texHeight );

    g_pBytes = new u_char[ g_texWidth * g_texHeight * 4 ];

    return true;
}

extern "C" void FinishNativeTextureRender()
{
    if( g_pBytes != NULL )
        delete[] g_pBytes;
    g_pBytes = NULL;
}

static void UNITY_INTERFACE_API OnRenderEvent( int eventID )
{
    if (eventID == 1)
    {
        glBindTexture( GL_TEXTURE_2D, g_textureId );
    }
    else
    {
        glBindTexture( GL_TEXTURE_2D, g_textureId2 );
    }

    static u_char s_r = 0;
    u_char* bytes = g_pBytes;

    for( int y = 0; y < g_texHeight; y++ )
    {
        for( int x = 0; x < g_texWidth; x++ )
        {
            int offset = ( ( y * g_texWidth ) + x ) * 4;
            bytes[ offset + 0 ] = s_r;
            bytes[ offset + 1 ] = 0;
            bytes[ offset + 2 ] = 0;
            bytes[ offset + 3 ] = 255;
        }
    }

    glTexSubImage2D( GL_TEXTURE_2D, 0, 0, 0, g_texWidth, g_texHeight, GL_RGBA, GL_UNSIGNED_BYTE, bytes );
    s_r ++;
    s_r %= 255;
}

extern "C" UnityRenderingEvent UNITY_INTERFACE_EXPORT UNITY_INTERFACE_API GetRenderEventFunc()
{
    return OnRenderEvent;
}