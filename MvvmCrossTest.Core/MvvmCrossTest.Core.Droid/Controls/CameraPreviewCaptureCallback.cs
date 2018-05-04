using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Hardware.Camera2;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using MvvmCrossTest.Core.Droid.Helper;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class CameraPreviewCaptureCallback : CameraCaptureSession.StateCallback
    {
        public Action<CameraCaptureSession> OnConfiguredSession;
        public Action<string> OnShowToast;

        public CameraPreviewCaptureCallback(Action<CameraCaptureSession> configuredCallback,
                                            Action<string> onShowToast)
        {
            OnConfiguredSession = configuredCallback;
            OnShowToast = onShowToast;
        }

        public override void OnConfigured(CameraCaptureSession cameraCaptureSession)
        {
            if (OnConfiguredSession != null)
                OnConfiguredSession(cameraCaptureSession);
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            if (OnShowToast != null)
                OnShowToast("Failed to configure camera.");
        }
    }
}