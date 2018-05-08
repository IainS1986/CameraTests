using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Java.Nio;

namespace MvvmCrossTest.Core.Droid.Helper
{
    public class JNIUtils
    {
        [DllImport("libMobileLib", EntryPoint = "MvvmCrossTest_Core_Droid_Helper_JNIUtils_AndroidInfo")]
        public static extern string AndroidInfo(
            IntPtr env,
            IntPtr jniClass);

        [DllImport("libMobileLib", EntryPoint = "MvvmCrossTest_Core_Droid_Helper_JNIUtils_GrayscaleDisplay")]
        public static extern void GrayscaleDisplay(
            IntPtr env,
            IntPtr jniClass,
            int srcWidth,
            int srcHeight,
            int rowStride,
            IntPtr srcBuffer,
            IntPtr surface);

        [DllImport("libMobileLib", EntryPoint = "MvvmCrossTest_Core_Droid_Helper_JNIUtils_RGBADisplay")]
        public static extern void RGBADisplay(
            IntPtr env,
            IntPtr jniClass,
            int srcWidth,
            int srcHeight, 
            int Y_rowStride,
            IntPtr Y_Buffer,
            int UV_rowStride,
            IntPtr U_Buffer,
            IntPtr V_Buffer,
            IntPtr surface);

        [DllImport("libMobileLib", EntryPoint = "MvvmCrossTest_Core_Droid_Helper_JNIUtils_RGBADisplay2")]
        public static extern void RGBADisplay2(
            IntPtr env,
            IntPtr jniClass,
            int srcWidth,
            int srcHeight, 
            int Y_rowStride,
            IntPtr Y_Buffer,
            IntPtr U_Buffer,
            IntPtr V_Buffer,
            IntPtr surface);

    }
}