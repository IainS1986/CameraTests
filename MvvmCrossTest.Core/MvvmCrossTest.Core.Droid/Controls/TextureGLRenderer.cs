using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Nio;
using Javax.Microedition.Khronos.Egl;
using Javax.Microedition.Khronos.Opengles;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class GLSquare
    {
        // Our vertices.
        private float[] vertices = {
              -1.0f,  1.5f, 0.0f,  // 0, Top Left
		      -1.0f, -1.5f, 0.0f,  // 1, Bottom Left
		       1.0f, -1.5f, 0.0f,  // 2, Bottom Right
		       1.0f,  1.5f, 0.0f,  // 3, Top Right
		};

        // UVs
        float[] uvs = { 0.0f, 0.0f, //
				0.0f, 1.0f, //
				1.0f, 1.0f, //
				1.0f, 0.0f, //
		};

        // The order we like to connect them.
        private short[] indices = { 0, 1, 2, 0, 2, 3 };

        // Our vertex buffer.
        private FloatBuffer vertexBuffer;

        // Our index buffer.
        private ShortBuffer indexBuffer;

        // Our uv buffer.
        private FloatBuffer textureBuffer;

        // Our texture id.
        private int mTextureId = -1;

        // The bitmap we want to load as a texture.
        private Bitmap mBitmap;

        private bool mShouldLoadTexture;

        public GLSquare()
        {
            // a float is 4 bytes, therefore we multiply the number if
            // vertices with 4.
            ByteBuffer vbb = ByteBuffer.AllocateDirect(vertices.Length * 4);
            vbb.Order(ByteOrder.NativeOrder());
            vertexBuffer = vbb.AsFloatBuffer();
            vertexBuffer.Put(vertices);
            vertexBuffer.Position(0);

            // short is 2 bytes, therefore we multiply the number if
            // vertices with 2.
            ByteBuffer ibb = ByteBuffer.AllocateDirect(indices.Length * 2);
            ibb.Order(ByteOrder.NativeOrder());
            indexBuffer = ibb.AsShortBuffer();
            indexBuffer.Put(indices);
            indexBuffer.Position(0);

            ByteBuffer byteBuf = ByteBuffer.AllocateDirect(uvs.Length * 4);
            byteBuf.Order(ByteOrder.NativeOrder());
            textureBuffer = byteBuf.AsFloatBuffer();
            textureBuffer.Put(uvs);
            textureBuffer.Position(0);
        }

        /**
         * Set the bitmap to load into a texture.
         *
         * @param bitmap
         */
        public void LoadBitmap(Bitmap bitmap)
        {
            mBitmap = bitmap;
            mShouldLoadTexture = true;
        }

        public void UpdateBitmap()
        {
            mShouldLoadTexture = true;
        }

        /**
         * Loads the texture.
         *
         * @param gl
         */
        private void LoadGLTexture(IGL10 gl)
        {
            // Generate one texture pointer...
            int[] textures = new int[1];
            gl.GlGenTextures(1, textures, 0);
            mTextureId = textures[0];

            // ...and bind it to our array
            gl.GlBindTexture(GL10.GlTexture2d, mTextureId);

            // Create Nearest Filtered Texture
            gl.GlTexParameterf(GL10.GlTexture2d, GL10.GlTextureMinFilter,
                    GL10.GlNearest);
            gl.GlTexParameterf(GL10.GlTexture2d, GL10.GlTextureMagFilter,
                    GL10.GlNearest);

            // Different possible texture parameters, e.g. GL10.GL_CLAMP_TO_EDGE
            gl.GlTexParameterf(GL10.GlTexture2d, GL10.GlTextureWrapS,
                    GL10.GlClampToEdge);
            gl.GlTexParameterf(GL10.GlTexture2d, GL10.GlTextureWrapT,
                    GL10.GlRepeat);

            // Use the Android GLUtils to specify a two-dimensional texture image
            // from our bitmap
            GLUtils.TexImage2D(GL10.GlTexture2d, 0, mBitmap, 0);
        }


        /**
	     * This function draws our square on screen.
	     * @param gl
	     */
        public void Draw(IGL10 gl)
        {

            // Counter-clockwise winding.
            gl.GlFrontFace(GL10.GlCcw);
            // Enable face culling.
            gl.GlEnable(GL10.GlCullFaceCapability);
            // What faces to remove with the face culling.
            gl.GlCullFace(GL10.GlBack);

            // Enabled the vertices buffer for writing and to be used during
            // rendering.
            gl.GlEnableClientState(GL10.GlVertexArray);
            // Specifies the location and data format of an array of vertex
            // coordinates to use when rendering.
            gl.GlVertexPointer(3, GL10.GlFloat, 0,
                                     vertexBuffer);

            gl.GlColor4f(1.0f, 1.0f, 1.0f, 1.0f); 

            // New part...
            if (mShouldLoadTexture)
            {
                LoadGLTexture(gl);
                mShouldLoadTexture = false;
            }
            if (mTextureId != -1 && textureBuffer != null)
            {
                gl.GlEnable(GL10.GlTexture2d);
                // Enable the texture state
                gl.GlEnableClientState(GL10.GlTextureCoordArray);

                // Point to our buffers
                gl.GlTexCoordPointer(2, GL10.GlFloat, 0, textureBuffer);
                gl.GlBindTexture(GL10.GlTexture2d, mTextureId);
            }

            gl.GlDrawElements(GL10.GlTriangles, indices.Length,
                      GL10.GlUnsignedShort, indexBuffer);

            // Disable the vertices buffer.
            gl.GlDisableClientState(GL10.GlVertexArray);

            if (mTextureId != -1 && textureBuffer != null)
            {
                gl.GlDisableClientState(GL10.GlTextureCoordArray);
            }

            // Disable face culling.
            gl.GlDisable(GL10.GlCullFaceCapability);
        }
    }

    public class SimpleGLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        private bool mFirstDraw;
        private bool mSurfaceCreated;
        private DateTime mLastTime;
        private int mFPS;

        private GLSquare m_square;

        public int FPS
        {
            get { return mFPS; }
        }

        public SimpleGLRenderer()
        {
            mFirstDraw = true;
            mSurfaceCreated = false;
            mLastTime = DateTime.Now;
            mFPS = 0;

            // Initialize our square.
            m_square = new GLSquare();
        }

        public void LoadTexture(Bitmap tex)
        {
            m_square.LoadBitmap(tex);
        }

        public void UpdateTexture()
        {
            m_square.UpdateBitmap();
        }

        public void OnDrawFrame(IGL10 gl)
        {
            mFPS++;
            DateTime currentTime = DateTime.Now;
            if (currentTime - mLastTime >= TimeSpan.FromMilliseconds(1000))
            {
                mFPS = 0;
                mLastTime = currentTime;
            }

            if (mFirstDraw)
            {
                mFirstDraw = false;
            }

            // Clears the screen and depth buffer.
            gl.GlClear(GL10.GlColorBufferBit | GL10.GlDepthBufferBit);

            m_square.Draw(gl);
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            mSurfaceCreated = false;

            // Sets the current view port to the new size.
            gl.GlViewport(0, 0, width, height);
            // Select the projection matrix
            gl.GlMatrixMode(GL10.GlProjection);
            // Reset the projection matrix
            gl.GlLoadIdentity();
            // Calculate the aspect ratio of the window
            GLU.GluPerspective(gl, 45.0f, (float)width / (float)height, 0.1f, 100.0f);
            // Select the modelview matrix
            gl.GlMatrixMode(GL10.GlModelview);
            // Reset the modelview matrix
            gl.GlLoadIdentity();

            // Replace the current matrix with the identity matrix
            gl.GlLoadIdentity();

            // Translates 4 units into the screen.
            gl.GlTranslatef(0, 0, -4);
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            mSurfaceCreated = true;

            // Set the background color to black ( rgba ).
            gl.GlClearColor(0.0f, 0.0f, 0.0f, 0.5f);
            // Enable Smooth Shading, default not really needed.
            gl.GlShadeModel(GL10.GlSmooth);
            // Depth buffer setup.
            gl.GlClearDepthf(1.0f);
            // Enables depth testing.
            gl.GlEnable(GL10.GlDepthTest);
            // The type of depth testing to do.
            gl.GlDepthFunc(GL10.GlLequal);
            // Really nice perspective calculations.
            gl.GlHint(GL10.GlPerspectiveCorrectionHint,
                              GL10.GlNicest);
        }

        public int getFPS()
        {
            return mFPS;
        }
    }

    public class TextureGLRenderer : Java.Lang.Object, GLSurfaceView.IRenderer
    {
        private ByteBuffer buf;
        private int cwidth, cheight;
        private FloatBuffer vertexBuffer, texelBuffer;
        private ShortBuffer indexBuffer;
        int[] textures = new int[1];

        float[] vertices = new float[]{
               0.0f,  1.0f, 0.0f,
               0.0f,  0.0f, 0.0f,
               1.0f,  0.0f, 0.0f,
               1.0f,  1.0f, 0.0f
        };
        private float[] texels = {
            0.0f, 1.0f,
            0.0f, 0.0f,
            1.0f, 0.0f,
            1.0f, 1.0f
        };
        private short[] indices = { 0, 1, 2, 0, 2, 3 };


        public void OnDrawFrame(IGL10 gl)
        {
            gl.GlClear(GL10.GlColorBufferBit | GL10.GlDepthBufferBit);
            UpdateTexture(gl);
        }

        public void OnSurfaceChanged(IGL10 gl, int width, int height)
        {
            gl.GlViewport(0, 0, width, height);
            gl.GlMatrixMode(GL10.GlProjection);
            gl.GlLoadIdentity();
            GLU.GluOrtho2D(gl, 0, width, 0, height);
            gl.GlMatrixMode(GL10.GlModelview);
            gl.GlLoadIdentity();

            buf = ByteBuffer.AllocateDirect(256 * 256 * 3).Order(ByteOrder.NativeOrder());
            cwidth = width;
            cheight = height;

            for (int i = 0; i < vertices.Length; i += 3)
            {
                vertices[i] *= cwidth;
                vertices[i + 1] *= cheight;
            }
            gl.GlEnable(GL10.GlTexture2d);
            gl.GlGenTextures(1, textures, 0);
            gl.GlBindTexture(GL10.GlTexture2d, textures[0]);
            gl.GlTexParameterf(GL10.GlTexture2d, GL10.GlTextureMagFilter, GL10.GlNearest);
            gl.GlTexParameterf(GL10.GlTexture2d, GL10.GlTextureMinFilter, GL10.GlNearest);
            gl.GlTexImage2D(GL10.GlTexture2d, 0, 3, 128, 128, 0, GL10.GlRgb, GL10.GlUnsignedByte, buf);

            ByteBuffer vbb = ByteBuffer.AllocateDirect(vertices.Length * 4);
            vbb.Order(ByteOrder.NativeOrder());
            vertexBuffer = vbb.AsFloatBuffer();
            vertexBuffer.Put(vertices);
            vertexBuffer.Position(0);

            ByteBuffer tbb = ByteBuffer.AllocateDirect(texels.Length * 4);
            tbb.Order(ByteOrder.NativeOrder());
            texelBuffer = tbb.AsFloatBuffer();
            texelBuffer.Put(texels);
            texelBuffer.Position(0);

            ByteBuffer ibb = ByteBuffer.AllocateDirect(indices.Length * 2);
            ibb.Order(ByteOrder.NativeOrder());
            indexBuffer = ibb.AsShortBuffer();
            indexBuffer.Put(indices);
            indexBuffer.Position(0);
        }

        public void OnSurfaceCreated(IGL10 gl, Javax.Microedition.Khronos.Egl.EGLConfig config)
        {
            gl.GlClearColor(1.0f, 0.0f, 0.0f, 0.5f);
            gl.GlShadeModel(GL10.GlSmooth);
            gl.GlClearDepthf(1.0f);
            gl.GlEnable(GL10.GlDepthTest);
            gl.GlDepthFunc(GL10.GlLequal);
            gl.GlHint(GL10.GlPerspectiveCorrectionHint, GL10.GlNicest);
        }

        private void UpdateTexture(IGL10 gl)
        {
            // Update pixels
            // write random r g or b values to random locations
            for (int y = 0; y < 256; ++y)
                for (int x = 0; x < 256; ++x)
                    buf.Put(x + y * 256, (sbyte)(Java.Lang.Math.Random() * 255));

            buf.Position(0);
            gl.GlEnable(GL10.GlTexture2d);
            gl.GlBindTexture(GL10.GlTexture2d, textures[0]);

            gl.GlEnableClientState(GL10.GlVertexArray);
            gl.GlVertexPointer(3, GL10.GlFloat, 0, vertexBuffer);

            gl.GlTexSubImage2D(GL10.GlTexture2d, 0, 0, 0, 128, 128, GL10.GlRgb, GL10.GlUnsignedByte, buf);
            gl.GlEnableClientState(GL10.GlTextureCoordArray);
            gl.GlTexCoordPointer(2, GL10.GlFloat, 0, texelBuffer);
            gl.GlBindTexture(GL10.GlTexture2d, textures[0]);

            gl.GlDrawElements(GL10.GlTriangles, indices.Length, GL10.GlUnsignedShort, indexBuffer);

            gl.GlDisableClientState(GL10.GlVertexArray);
            gl.GlDisableClientState(GL10.GlTextureCoordArray);

        }
    }
}