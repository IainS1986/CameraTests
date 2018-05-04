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

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class CameraPreviewCallback : Java.Lang.Object, Camera.IPreviewCallback
    {
        public Action<byte[], Camera> OnFrameReady;

        public CameraPreviewCallback(Action<byte[], Camera> onFrameReady)
        {
            OnFrameReady = onFrameReady;
        }

        public void OnPreviewFrame(byte[] data, Camera camera)
        {
            //Manipulate the byte data here to do some image processing
            if (OnFrameReady != null)
                OnFrameReady(data, camera);
        }
    }
}