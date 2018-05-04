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
    public class DeviceOrientationListener : OrientationEventListener
    {
        private Action<int> OrientationChanged;

        public DeviceOrientationListener(Context context, SensorDelay delay, Action<int> orientationChanged) : base(context, delay)
        {
            OrientationChanged = orientationChanged;
        }

        public override void OnOrientationChanged(int orientation)
        {
            if (OrientationChanged != null)
                OrientationChanged(orientation);
        }
    }
}