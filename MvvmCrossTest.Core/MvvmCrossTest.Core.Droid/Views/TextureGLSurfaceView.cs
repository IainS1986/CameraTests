using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.Opengl;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Lang;
using MvvmCrossTest.Core.Droid.Controls;

namespace MvvmCrossTest.Core.Droid.Views
{
    [Register("mvvmcrosstest.core.droid.views.TextureGLSurfaceView")]
    public class TextureGLSurfaceView : GLSurfaceView
    {
        public SimpleGLRenderer mRenderer;

        public TextureGLSurfaceView(Activity activity) : base(activity)
        {
            SetEGLConfigChooser(8, 8, 8, 8, 16, 0);

            // Create an OpenGL ES 2.0 context.
            //SetEGLContextClientVersion(2);
            PreserveEGLContextOnPause = true;

            // Set the Renderer for drawing on the GLSurfaceView
            mRenderer = new SimpleGLRenderer();
            SetRenderer(mRenderer);

            // Render the RenderView only when there is a change in the drawing data
            this.RenderMode = Rendermode.Continuously;
        }

        public async Task QueueEventAsync(Action action)
        {
            SemaphoreSlim locking = new SemaphoreSlim(0, 1);
            QueueEvent(() =>
            {
                new Runnable(action).Run();
                locking.Release();
            });
            await locking.WaitAsync();
        }
    }
}