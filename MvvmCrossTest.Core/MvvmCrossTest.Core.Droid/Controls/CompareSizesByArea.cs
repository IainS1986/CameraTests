using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Java.Lang;
using Java.Util;

namespace MvvmCrossTest.Core.Droid.Controls
{
    /// <summary>
    /// Comparator based on area of the given {@link Size} objects.
    /// </summary>
    public class CompareSizesByArea : Java.Lang.Object, IComparator
    {
        public int Compare(Size lhs, Size rhs)
        {
            // We cast here to ensure the multiplications won't overflow
            return Long.Signum((long)lhs.Width * lhs.Height -
            (long)rhs.Width * rhs.Height);
        }

        int IComparator.Compare(Java.Lang.Object lhs, Java.Lang.Object rhs)
        {
            return 0;
        }

        bool IComparator.Equals(Java.Lang.Object @object)
        {
            return false;
        }
    }

}