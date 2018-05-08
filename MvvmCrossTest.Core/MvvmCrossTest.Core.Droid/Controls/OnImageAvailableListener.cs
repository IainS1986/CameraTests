using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class OnImageAvailableListener : Java.Lang.Object, ImageReader.IOnImageAvailableListener
    {
        private Action<ImageReader> OnAvailable;

        public OnImageAvailableListener(Action<ImageReader> onAvailable)
        {
            OnAvailable = onAvailable;
        }

        public void OnImageAvailable(ImageReader reader)
        {
            if (OnAvailable != null)
                OnAvailable(reader);
        }
    }
}