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
    public class CameraSessionStateCallback : CameraCaptureSession.StateCallback
    {
        public Action<CameraCaptureSession> m_onConfigured;

        public Action<CameraCaptureSession> m_onConfigureFailed;

        public CameraSessionStateCallback(Action<CameraCaptureSession> onConfigured, Action<CameraCaptureSession> onConfigureFailed)
        {
            m_onConfigured = onConfigured;
            m_onConfigureFailed = onConfigureFailed;
        }

        public override void OnConfigured(CameraCaptureSession session)
        {
            if (m_onConfigured != null)
                m_onConfigured(session);
        }

        public override void OnConfigureFailed(CameraCaptureSession session)
        {
            if (m_onConfigureFailed != null)
                m_onConfigureFailed(session);
        }
    }
}