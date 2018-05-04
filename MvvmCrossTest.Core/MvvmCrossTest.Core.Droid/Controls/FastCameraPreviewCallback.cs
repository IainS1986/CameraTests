using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using ApxLabs.FastAndroidCamera;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class FastCameraPreviewCallback : Java.Lang.Object, INonMarshalingPreviewCallback
    {
        public Action<IntPtr, Camera> OnFrameReady;

        public FastCameraPreviewCallback (Action<IntPtr, Camera> onFrameReady)
        {
            OnFrameReady = onFrameReady;
        }

        public void OnPreviewFrame(IntPtr data, Camera camera)
        {
            //Manipulate the byte data here to do some image processing
            if (OnFrameReady != null)
                OnFrameReady(data, camera);
        }
    }
}