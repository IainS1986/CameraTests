using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class SurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        private Action<Android.Graphics.SurfaceTexture, int, int> OnAvailable;

        private Action<Android.Graphics.SurfaceTexture> OnDestroyed;

        private Action<Android.Graphics.SurfaceTexture, int, int> OnSizeChanged;

        public SurfaceTextureListener(Action<Android.Graphics.SurfaceTexture, int, int> onAvailable,
                                      Action<Android.Graphics.SurfaceTexture> onDestroyed,
                                      Action<Android.Graphics.SurfaceTexture, int, int> onSizeChanged)
        {
            OnAvailable = onAvailable;
            OnDestroyed = onDestroyed;
            OnSizeChanged = onSizeChanged;
        }

        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            if (OnAvailable != null)
                OnAvailable(surface, width, height);
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            if (OnDestroyed != null)
                OnDestroyed(surface);

            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            if (OnSizeChanged != null)
                OnSizeChanged(surface, width, height);
        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {
        }
    }
}