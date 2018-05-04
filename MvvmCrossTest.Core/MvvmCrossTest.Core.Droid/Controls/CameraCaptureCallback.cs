using System;
using System.Collections.Generic;
using System.IO;
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
    public class CameraCaptureCallback : CameraCaptureSession.CaptureCallback
    {
        private Action<CameraCaptureSession, CaptureRequest, long, long> OnStarted;
        private Action<CameraCaptureSession, CaptureRequest, TotalCaptureResult> OnFinished;
        private Action<CameraCaptureSession, CaptureRequest, CaptureFailure> OnFailed;


        public CameraCaptureCallback(Action<CameraCaptureSession, CaptureRequest,long,long> captureStarted,
                                     Action<CameraCaptureSession, CaptureRequest, TotalCaptureResult> captureFinished,
                                     Action<CameraCaptureSession, CaptureRequest, CaptureFailure> captureFailed)
        {
            OnStarted = captureStarted;
            OnFinished = captureFinished;
            OnFailed = captureFailed;
        }

        public override void OnCaptureStarted(CameraCaptureSession session, 
                                                CaptureRequest request,
                                                long timestamp,
                                                long frameNumber)
        {
            if (OnStarted != null)
                OnStarted(session, request, timestamp, frameNumber);

        }

        public override void OnCaptureCompleted(CameraCaptureSession session, 
                                                CaptureRequest request,
                                                TotalCaptureResult result)
        {
            if (OnFinished != null)
                OnFinished(session, request, result);
        }

        public override void OnCaptureFailed(CameraCaptureSession session, CaptureRequest request,
                                              CaptureFailure failure)
        {
            if (OnFailed != null)
                OnFailed(session, request, failure);
        }
    }
}