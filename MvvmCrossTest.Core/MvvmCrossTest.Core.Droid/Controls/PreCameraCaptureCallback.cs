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

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class PreCameraCaptureCallback : CameraCaptureSession.CaptureCallback
    {
        private Action<CaptureResult> OnProcess;

        public PreCameraCaptureCallback(Action<CaptureResult> onProcess)
        {
            OnProcess = onProcess;
        }

        void Process(CaptureResult result)
        {
            if (OnProcess != null)
                OnProcess(result);
        }

        public override void OnCaptureProgressed(CameraCaptureSession session, CaptureRequest request,
                                                  CaptureResult partialResult)
        {
            Process(partialResult);
        }

        public override void OnCaptureCompleted(CameraCaptureSession session, CaptureRequest request,
                                                 TotalCaptureResult result)
        {
            Process(result);
        }
    }
}