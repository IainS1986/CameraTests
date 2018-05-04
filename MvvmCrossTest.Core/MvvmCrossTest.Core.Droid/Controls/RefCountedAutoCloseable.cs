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
using Java.Lang;

namespace MvvmCrossTest.Core.Droid.Controls
{
    public class RefCountedAutoCloseable<T> : Java.Lang.Object, IAutoCloseable where T : Java.Lang.Object
    {
        T mObject;
        long mRefCount = 0;

        /// <summary>
        /// Wrap the given object.
        /// </summary>
        /// <param name="obj">object an object to wrap.</param>
        public RefCountedAutoCloseable(T obj)
        {
            if (obj == null)
                throw new NullPointerException();

            mObject = obj;
        }

        /// <summary>
        /// the reference count and return the wrapped object.
        /// </summary>
        /// <returns>the wrapped object, or null if the object has been released.</returns>
        public T GetAndRetain()
        {
            if (mRefCount < 0)
                return default(T);

            mRefCount++;
            return mObject;
        }

        /// <summary>
        /// Return the wrapped object.
        /// </summary>
        /// <returns>the wrapped object, or null if the object has been released.</returns>
        public T Get()
        {
            return mObject;
        }

        /// <summary>
        /// Decrement the reference count and release the wrapped object if there are no other
        /// users retaining this object.
        /// </summary>
        public void Close()
        {
            if (mRefCount >= 0)
            {
                mRefCount--;
                if (mRefCount < 0)
                {
                    try
                    {
                        var obj = (mObject as IAutoCloseable);
                        if (obj == null)
                            throw new Java.Lang.Exception("unclosable");
                        obj.Close();
                    }
                    catch (Java.Lang.Exception e)
                    {
                        if (e.Message != "unclosable")
                            throw new RuntimeException(e);
                    }
                    finally
                    {
                        mObject = default(T);
                    }
                }
            }
        }
    }
}