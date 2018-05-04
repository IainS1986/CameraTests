using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Media;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class MediaScannerClient : Java.Lang.Object, MediaScannerConnection.IOnScanCompletedListener
    {
        public void OnMediaScannerConnected()
        {
            //do nothing
        }

        public void OnScanCompleted(string path, Android.Net.Uri uri)
        {
            Log.Info("MediaScannerClient", "Scanned " + path + ":");
            Log.Info("MediaScannerClient", "-> uri=" + uri);
        }
    }
}