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
    class CameraSurfaceTextureListener : Java.Lang.Object, TextureView.ISurfaceTextureListener
    {
        Activity Activity { get; set; }

        public Action<int, int, Activity> OnConfigureTransform;

        public Action OnDestroyed;

        public CameraSurfaceTextureListener(Activity activity)
        {
            Activity = activity;
        }

        public void OnSurfaceTextureAvailable(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            if (OnConfigureTransform != null)
                OnConfigureTransform(width, height, Activity);
        }

        public bool OnSurfaceTextureDestroyed(Android.Graphics.SurfaceTexture surface)
        {
            if (OnDestroyed != null)
                OnDestroyed();
            //lock (mCameraStateLock)
            //{
            //    mPreviewSize = null;
            //}
            return true;
        }

        public void OnSurfaceTextureSizeChanged(Android.Graphics.SurfaceTexture surface, int width, int height)
        {
            if (OnConfigureTransform != null)
                OnConfigureTransform(width, height, Activity);
        }

        public void OnSurfaceTextureUpdated(Android.Graphics.SurfaceTexture surface)
        {
        }
    }

}