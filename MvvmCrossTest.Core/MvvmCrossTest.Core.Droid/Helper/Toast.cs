using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MvvmCrossTest.Core.Droid.Helper
{
    public class DroidToastService
    {
        /// <summary>
        /// Shows a {@link Toast} on the UI thread.
        /// </summary>
        /// <param name="text">The message to show.</param>
        public static void ShowToast(string text, Activity activity)
        {
            if (activity != null)
            {
                Toast.MakeText(activity, (string)text, ToastLength.Short).Show();
            }
        }
    }
}