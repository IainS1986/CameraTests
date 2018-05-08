using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using MvvmCrossTest.Core.Droid.Services;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class CameraStateCallback : CameraDevice.StateCallback
    {
        private Action<CameraDevice> OnDeviceOpened;
        private Action<CameraDevice> OnDeviceDisconnected;
        private Action<CameraDevice, CameraError> OnDeviceError;
        
        public CameraStateCallback(Action<CameraDevice> onOpened,
                                    Action<CameraDevice> onDisconnected,
                                    Action<CameraDevice, CameraError> onError)
        {
            OnDeviceOpened = onOpened;
            OnDeviceDisconnected = onDisconnected;
            OnDeviceError = onError;
        }

        public override void OnOpened(CameraDevice camera)
        {
            if (OnDeviceOpened != null)
                OnDeviceOpened(camera);
        }

        public override void OnDisconnected(CameraDevice camera)
        {
            if (OnDeviceDisconnected != null)
                OnDeviceDisconnected(camera);
        }

        public override void OnError(CameraDevice camera, CameraError error)
        {
            if (OnDeviceError != null)
                OnDeviceError(camera, error);
        }
    }
}